using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WedgeMovement : MonoBehaviour
{
    [SerializeField] Wedge wd;
    [SerializeField] Rigidbody prb;

    private Rigidbody rb;
    private bool activated = false;
    private bool lockIn = false;
    private Vector3 Velocity;
    private Vector3 offset;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        StateHandler();
    }

    private void StateHandler()
    {
            
        if (wd.holding)
        {
            if (!activated)
            {
                activated = true;
                
                rb.position = prb.position + new Vector3(wd.WedgeDirection.x * 0.3f, 1.7f, wd.WedgeDirection.z * 0.3f);
                offset = prb.position - rb.position;
                rb.linearVelocity = prb.linearVelocity;
                prb.position = rb.position + offset;

            }
            else
            {
                prb.position = rb.position + offset;
                if (wd.hangLockIn && !lockIn)
                    Forces();
                //use grappling hook script
            }

        }
        else
        {
            resetWedge();

            Velocity = new Vector3(prb.linearVelocity.x, 0, prb.linearVelocity.z);
        }
    }

    private void Forces()
    {
        rb.linearVelocity = Vector3.zero;
        lockIn = true;
        rb.AddForce(Vector3.down * wd.slipAmount + Velocity * 0.8f);
    }


    private void resetWedge()
    {
        lockIn = false;
        activated = false;
        rb.position = prb.position;
    }

}
