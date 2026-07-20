using Unity.Mathematics;
using UnityEngine;

public class PlayerBodyDirector : MonoBehaviour
{
    [Header("PlayerBodyDirector")]
    [SerializeField] float minVelocityOveride;
    [SerializeField] float forwardExtraBuf;
    
    [Header("Components")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform player;
    [SerializeField] Rigidbody rb;

    private void Update()
    {
        CalcRotation();
    }

    private void CalcRotation()
    {
        //blend between velocity dir (flipped) and orientation
        Vector3 upDir = player.up;
        Vector3 faceDir = Vector3.ProjectOnPlane(rb.linearVelocity, upDir).normalized;
        Vector3 flatOri = Vector3.ProjectOnPlane(orientation.forward, upDir).normalized;
        float dirOriDot = Vector3.Dot(faceDir, flatOri);
        if (dirOriDot < -forwardExtraBuf)
        { // forward vs backward
            faceDir *= -1;
        }
        if (rb.linearVelocity.magnitude < minVelocityOveride)
        {
            float tempRatio = rb.linearVelocity.magnitude / minVelocityOveride;
            faceDir = Vector3.Lerp(flatOri, faceDir, tempRatio);
        } 

        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(faceDir, Vector3.up), Vector3.up);
    }
}
