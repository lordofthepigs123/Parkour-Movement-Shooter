using System.Collections.Generic;
using UnityEngine;

public class GrabEdgeDetector : MonoBehaviour
{
    [Header("GrabEdgeDetector")]
    public LayerMask whatIsEdgeable;
    [SerializeField] Collider checkCol; 
    [SerializeField] Transform checkFromPos;
    [SerializeField] float checkFromMaxDis;
    [SerializeField] float triangleNunLimiter;
    [SerializeField] float maxMergeDis;
    [SerializeField] float maxAngle;
    public bool Near;

    private int numCols = 0;
    private int colObjectPointer;
    //outputs
    [HideInInspector] public Edge edge;
    [HideInInspector] public Vector3 edgePos;
    [HideInInspector] public Vector3 edgeAxis;// includes length

    private List<Vector3> combinedVert;
    private List<int> closeTri;
    private List<float> triDis; // closeTri minmum distances to sort by

    private void Start()
    {
        combinedVert = new List<Vector3>();
        closeTri = new List<int>();
        triDis = new List<float>();
    }
    private void Update()
    {
        //resets
        colObjectPointer = 0;
        combinedVert.Clear();
        closeTri.Clear();
        triDis.Clear();
    }

    //Detect tigger with Wall
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Mathf.Log(whatIsEdgeable.value, 2))
        {
            if (numCols < 0)
                numCols = 0;
            numCols++;

            Near = true;
        }
    }

    //Get info when trigger is in object
    private void OnTriggerStay(Collider other)
    {
        //Debug.Log(gameObject.name + " obj ");
        if (other.gameObject.layer == Mathf.Log(whatIsEdgeable.value, 2))
        {
            colObjectPointer ++;

            //add all close triangles and related unique vertices
            disciminateFaces(other);
            if (colObjectPointer >= numCols)//calculate vars after last other
            {
                edgeDetect();
            }
        }
    }

    //End trigger
    private void OnTriggerExit(Collider other)
    {
        if (Near && other.gameObject.layer == Mathf.Log(whatIsEdgeable.value, 2))
        {
            numCols--;

            if (numCols <= 0)
            {
                resetVars();
                //Debug.Log("exit Col");
            }
        }
    }

    private void resetVars()
    { 
        Near = false;
        edge = new Edge(Vector3.zero, Vector3.zero);
        edgePos = Vector3.zero;
        edgeAxis = Vector3.zero;
    }



    private void disciminateFaces(Collider other)
    {
        Mesh mesh = (other as MeshCollider).sharedMesh;
        if (mesh == null)
            return;

        Vector3[] verticies = convertWorldVerticies(mesh.vertices, other.transform);
        int[] triangles = mesh.triangles; // triangles represented as groups of three pointers to corresponding vertex
        //combinedVert.AddRange(verticies); // add all verticies to combined list
        Vector3 checkF = checkFromPos.position;

        for (int i = 0; i < Mathf.Min(triangles.Length, triangleNunLimiter * 3); i += 3)
        {
            int v1 = triangles[i];//get all verticies
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];
            
            //add triangle with close edge
            float d1 = (new Edge(verticies[v1], verticies[v2]).ClosestPoint(checkF) - checkF).magnitude;
            float d2 = (new Edge(verticies[v2], verticies[v3]).ClosestPoint(checkF) - checkF).magnitude;
            float d3 = (new Edge(verticies[v3], verticies[v1]).ClosestPoint(checkF) - checkF).magnitude;
            float min = Mathf.Min(d1, d2, d3);
            //Debug.DrawLine()
            if (min < checkFromMaxDis)
            {
                //add triangle
                sortAdd(min, addVerticie(verticies[v1]), addVerticie(verticies[v2]), addVerticie(verticies[v3]));
            }
        }
    }

    private Vector3[] convertWorldVerticies(Vector3[] array, Transform other)
    {
        List<Vector3> worldSpaceVertices = new List<Vector3>();
        foreach (Vector3 localVertex in array)
        {
            // TransformPoint applies the objects position, rotation, and scale
            Vector3 worldVertex = other.TransformPoint(localVertex);
            worldSpaceVertices.Add(worldVertex);
        }
        return worldSpaceVertices.ToArray();
    }

    private int addVerticie(Vector3 v) // outputs new index of vertex
    {
        //check if exists
        int temp = combinedVert.IndexOf(v);
        if (temp != -1)
            return temp;
        
        //check if can merge
        int mergeIndex = mergeVerticie(combinedVert, v, maxMergeDis);
        if (mergeIndex != -1)
            return mergeIndex;

        //add as new vertex
        combinedVert.Add(v);
        return combinedVert.Count - 1;
    }

    private int mergeVerticie(List<Vector3> verts, Vector3 target, float threshold)
    {
        for (int i = 0; i < verts.Count; i++)
        {
            // Check if the current vertex is within the threshold distance of an existing vertex
            if (Vector3.Distance(verts[i], target) < threshold)
                return i;
        }
        return -1;
    }

    private void sortAdd(float dis, int p1, int p2, int p3)// add to array by distance, shortest to longest
    {
        //binary search
        int start = 0;
        int end = triDis.Count - 1;
        int position; // final position
        while (true)
        {
            if (start > end) // exit
            {
                position = start;
                break;
            }

            int checkInd = (start + end) / 2;
            if (triDis[checkInd] == dis) // if repeat dis
            {
                position = checkInd + 1;// right after
                break;
            }
            if (triDis[checkInd] < dis)
            {
                start = checkInd + 1;
            }
            else
            {
                end = checkInd - 1;
            }
        }

        triDis.Insert(position, dis);

        closeTri.Insert(position * 3, p3);
        closeTri.Insert(position * 3, p2);
        closeTri.Insert(position * 3, p1);
    }

    private void edgeDetect()
    {
        List<Vector2Int> edgeHold = new List<Vector2Int>(); // all gone through edges
        List<Vector2Int> repeatEdge = new List<Vector2Int>(); // only edges that are shared

        List<int> edgeTriangles = new List<int>(); // groups of two triangle indexes that share an edge
        //get closest points and corresponding edge
        for (int i = 0; i < closeTri.Count; i += 3)
        {
            int v1 = closeTri[i];//get all verticies
            int v2 = closeTri[i + 1];
            int v3 = closeTri[i + 2];

            Vector2Int e1 = new Vector2Int(v1, v2);
            Vector2Int e2 = new Vector2Int(v2, v3);
            Vector2Int e3 = new Vector2Int(v3, v1);

            addEdge(e1, edgeHold, repeatEdge, i, edgeTriangles);
            addEdge(e2, edgeHold, repeatEdge, i, edgeTriangles);
            addEdge(e3, edgeHold, repeatEdge, i, edgeTriangles);
        }

        // check against crit angle
        for (int i = 0; i < edgeTriangles.Count; i += 2)
        {
            Vector3 n1 = calcNormal(i, edgeTriangles); // by closeTri (small to large)
            Vector3 n2 = calcNormal(i + 1, edgeTriangles);

            //Debug
            Vector3 pos1 = (combinedVert[closeTri[edgeTriangles[i] * 3]] + combinedVert[closeTri[edgeTriangles[i] * 3 + 1]] + combinedVert[closeTri[edgeTriangles[i] * 3 + 2]]) / 3;
            Vector3 pos2 = (combinedVert[closeTri[edgeTriangles[i + 1] * 3]] + combinedVert[closeTri[edgeTriangles[i + 1] * 3 + 1]] + combinedVert[closeTri[edgeTriangles[i + 1] * 3 + 2]]) / 3;
            Debug.DrawRay(pos1, n1 * 3, Color.aquamarine);
            Debug.DrawRay(pos2 , n2 * 3, Color.darkOrange);
            //End Debug


            float angle = Vector3.Angle(n1, n2);
           
            if (angle >= maxAngle)
            {
                //Success
                Vector3 p1 = combinedVert[repeatEdge[i / 2].x];
                Vector3 p2 = combinedVert[repeatEdge[i / 2].y];
                edge = new Edge(p1, p2);
                edgePos = edge.ClosestPoint(checkFromPos.position);
                edgeAxis = p2 - p1;
                return;
            }
        }
        //fail
        edge = new Edge(Vector3.zero, Vector3.zero);
        edgePos = Vector3.zero;
        edgeAxis = Vector3.zero;
    }

    private void addEdge(Vector2Int e, List<Vector2Int> hold, List<Vector2Int> rep, int tri, List<int> et) // outputs new index of vertex
    {
        //check if edge is repeated
        int temp = hold.IndexOf(e);
        if (temp == -1)//try again - reversed
            temp = hold.IndexOf(new Vector2Int(e.y, e.x));

        if (temp != -1)
        {
            rep.Add(e);
            et.Add(tri / 3);//add two triangle indexes of shared edge - 
            et.Add(temp / 3);//rounds down
        }
        
        hold.Add(e);
    }

    private Vector3 calcNormal(int index, List<int> et)
    {
        Vector3 e1 = combinedVert[closeTri[et[index] * 3 + 1]] - combinedVert[closeTri[et[index] * 3]]; // p2 - p1
        Vector3 e2 = combinedVert[closeTri[et[index] * 3 + 2]] - combinedVert[closeTri[et[index] * 3]]; // p3 - p1
        return Vector3.Cross(e1, e2).normalized; // as long as points are in mesh order clockwise order will result in correct normal
    }
}
