using System.Collections.Generic;
using UnityEngine;

public class FootDetectorHandler : MonoBehaviour
{
    [Header("FootDetectorHandler")]
    [SerializeField] private LayerMask checkLayer;
    [SerializeField] private float capsuleCheckRadius;
    [SerializeField] private float minCheckRadius;
    [SerializeField] private float capsuleCheckLength;
    [SerializeField] private float velRadModMax;
    [SerializeField] private float velLengthModMax;
    [SerializeField] private float camStrength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float sphereCastLength;
    [SerializeField] private float rayCastLength;
    private Collider[] lapping = new Collider[1];
    private bool overlap;
    private Vector3[] directions;
    [HideInInspector] public List<Vector3> TargetStore = new List<Vector3>();
    [HideInInspector] public bool StateHasEnabled;
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cam;
    private void Start()
    {
        directions = new Vector3[26];
        // add directions
        Vector3[] tempDirs = new Vector3[6] {Vector3.forward,Vector3.left,Vector3.up,Vector3.back,Vector3.right,Vector3.down};
        int index = 0;
        for (int i = 0; i < 6; i++) // all combinations of three directions
        {
            directions.SetValue(tempDirs[i], index);
            index++;
            for (int j = i+1; j < 6; j++)
            {
                if (i+3==j) // skip opposite direction
                    continue;
                directions.SetValue(tempDirs[i] + tempDirs[j], index);
                index++;
                for (int k = j+1; k < 6; k++)
                {
                    if (i+3==k||j+3==k) // skip opposite direction
                        continue;
                    directions.SetValue(tempDirs[i] + tempDirs[j] + tempDirs[k], index);
                    index++;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (!StateHasEnabled) // only allow detect when leg in air search
        {
            overlap = false;
            return;
        }
        float lengthLerp = Mathf.Lerp(0, capsuleCheckLength, rb.linearVelocity.magnitude / velLengthModMax);
        Vector3 displace = rb.linearVelocity.normalized * lengthLerp;
        float radLerp = Mathf.Lerp(minCheckRadius,capsuleCheckRadius,rb.linearVelocity.magnitude / velRadModMax);
        int hit = Physics.OverlapCapsuleNonAlloc(transform.position, transform.position + displace, radLerp, lapping, checkLayer);

        overlap = hit != 0;
    }

    private void Update()
    {
        TargetStore.Clear();
        if (overlap)
        {
            RayChecks();
        }
    }

    private void RayChecks()
    {
        List<Vector3> hitsPoint = new List<Vector3>();
        List<Vector3> hitsNormal = new List<Vector3>();
        //frontal spherecast
        Vector3 fdir = (cam.forward * camStrength + rb.linearVelocity).normalized;
        if (Physics.SphereCast(transform.position, sphereCastRadius, fdir, out RaycastHit hit, sphereCastLength, checkLayer))
        {
            hitsPoint.Add(hit.point);
            hitsNormal.Add(hit.normal);
        }

        // Side Raycasts
        Quaternion fdirRot = Quaternion.FromToRotation(Vector3.forward, fdir);
        foreach (Vector3 vec in directions)
        {
            if (Physics.Raycast(transform.position, fdirRot * vec, out RaycastHit _hit, rayCastLength, checkLayer))
            {
                hitsPoint.Add(_hit.point);
                hitsNormal.Add(_hit.normal);
            }
        }

        //Recast succesful hits
        List<Vector3> uniqNormals = new List<Vector3>();
        for (int i = 0; i < hitsPoint.Count; i++)
        {
            //prevent duplicate normals testing
            if (uniqNormals.Contains(hitsNormal[i]))
                continue;
            uniqNormals.Add(hitsNormal[i]);

            float checkDis = (transform.position - hitsPoint[i]).magnitude;
            if (Physics.Raycast(transform.position, -hitsNormal[i], out RaycastHit _hit, checkDis, checkLayer)) // check normal from transform position to get closest point
            {
                hitsPoint.Add(_hit.point);
                hitsNormal.Add(_hit.normal);
            }
        }

        //Add final hits to list
        TargetStore = hitsPoint;

        /* debug
        for (int i = 0; i < hitsPoint.Count; i++)
        {
            Debug.DrawRay(hitsPoint[i],hitsNormal[i],Color.purple);
        }
        */
    }
}
