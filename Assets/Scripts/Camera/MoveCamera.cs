using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public PlayerStateMachine sm;
    public Transform cameraPosition;
    public Transform PlayerTransform;
    public float cameraForwardDis;
    public float rollRatio;

    // Update is called once per frame
    void Update()
    {
        Vector3 desiPos;
        if (sm.state == PlayerStateMachine.EMovementState.rolling)
            desiPos = cameraPosition.position * rollRatio + PlayerTransform.position * (1 - rollRatio);
        else
            desiPos = cameraPosition.position;
        desiPos += transform.forward * cameraForwardDis;
        transform.position = desiPos;
    }
}
