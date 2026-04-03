using UnityEngine;

[System.Serializable]
public struct Edge
{
    public Vector3 start;
    public Vector3 end;

    public Edge(Vector3 startPoint, Vector3 endPoint)
    {
        start = startPoint;
        end = endPoint;
    }

    public Vector3 ClosestPoint(Vector3 refer)
    {
        Vector3 AB = end - start;
        Vector3 Arefer = refer - start;

        float dot = Vector3.Dot(Arefer, AB) / AB.sqrMagnitude; //get dot on line segment and normalise
        dot = Mathf.Clamp(dot, 0 , 1);

        return start + AB * dot;
    }
}
