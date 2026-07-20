using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [Header("MoveCamera")]
    public float cameraUpDis;
    public float cameraForwardDis;

    [HideInInspector] public bool rolling;

    [Header("Components")]
    public Transform headBone;

    void Update()
    {
        Vector3 desiPos= headBone.position;
        desiPos += -headBone.forward * cameraUpDis + headBone.up * cameraForwardDis;
        transform.position = desiPos;
    }
}
