using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBounce : MonoBehaviour
{
    [SerializeField] PlayerCam cam;
    [SerializeField] float bounceCooldown;
    [SerializeField] float enterMaxTime;
    [SerializeField] float maxTimingMultiplier;
    [SerializeField] LayerMask bounceSurface;

    private Vector3 ogDir;
    private float tempVel;

    private Vector3 contactDir;
    private Vector3 contactNorm;
    private float contactDis;
    public float exitTimer;
    private float enterTimer;
    private bool exitingBounce;
    public bool hasCollided;
    private RaycastHit wallHit;

    private PlayerStats ps;
    private PlayerColliderManager cm;
    private Rigidbody rb;
    private Wedge wedge;
    private InputHandler ih;
    private WallRunning wr;

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        cm = GetComponent<PlayerColliderManager>();
        wedge = GetComponent<Wedge>();
        ih = GetComponent<InputHandler>();
        wr = GetComponent<WallRunning>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        tempVel = collision.relativeVelocity.magnitude;

        contactDir = Vector3.zero;
        contactNorm = Vector3.zero;
        contactDis = 2;
        if (!hasCollided)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (collision.gameObject.layer == Mathf.Log(bounceSurface.value, 2))
                {
                    hasCollided = true;
                    Vector3 temp = contact.point - transform.position;
                    float dis = Vector3.Distance(contact.point, transform.position);

                    if (dis < contactDis)
                    {
                        contactDir = temp;
                        contactNorm = contact.normal;
                        contactDis = dis;
                    }
                }
            }
            //Debug.DrawRay(transform.position, contactNorm * 3, Color.blue, 1);
        }
    }

    private void Update()
    {
        StateMachine();
    }

    private void StateMachine()
    {
        //Enter timers
        if (hasCollided)
        {
            enterTimer -= Time.deltaTime;
            if (enterTimer <= 0)
                hasCollided = false;
        }
        else
        {
            ogDir = rb.linearVelocity.normalized;
            enterTimer = enterMaxTime;
        }
        //Exit timers
        if (exitTimer > 0)
        {
            exitingBounce = true;
            exitTimer -= Time.deltaTime;
        }

        if (exitTimer <= 0)
            exitingBounce = false;


        if (ih.ikeySPACE && !cm.grounded && !exitingBounce && wr.bounce && hasCollided)
        {
            //Activate wallBounce
            if (Physics.Raycast(transform.position, contactDir, out wallHit, 1.5f))
            {
                ih.forceUpSpace = true;
                exitTimer = bounceCooldown;//BLNC
                //Invoke(nameof(bounceForce), 0.01f);
                bounceForce();
            }
        }
    }
/*
    private Vector3 getBounceVector(Vector3 camD, Vector3 ogD)
    {
        float newX, newY, newZ, numS, numL;
        Vector3 reflectD = Vector3.Reflect(ogD, wallHit.normal);
        if (reflectD.x > -ogD.x)
        {
            numS = -ogD.x;
            numL = reflectD.x;
        }
        else
        {
            numL = -ogD.x;
            numS = reflectD.x;
        }
        newX = bounce1d(numS, numL, camD.x);
        if (reflectD.y > -ogD.y)
        {
            numS = -ogD.y;
            numL = reflectD.y;
        }
        else
        {
            numL = -ogD.y;
            numS = reflectD.y;
        }
        newY = bounce1d(numS, numL, camD.y);
        if (reflectD.z > -ogD.z)
        {
            numS = -ogD.z;
            numL = reflectD.z;
        }
        else
        {
            numL = -ogD.z;
            numS = reflectD.z;
        }
        newZ = bounce1d(numS, numL, camD.z);


        return new Vector3(newX, newY, newZ).normalized;
    }

    private float bounce1d(float numS, float numL, float _cam)
    {
        float midD = (numL + numS) / 2;
        if (Mathf.Abs(numL - numS) < 1)
        {
            if (midD < 0)
            {
                midD++;
            }
            else
            {
                midD--;
            }
        }
        if (_cam < numL && _cam > numS)
        {
            return _cam;
        }
        else if (numS > midD)
        {
            if (_cam < midD || _cam > numL)
            {
                return numL;
            }
            else
            {
                return numS;
            }

        }
        else if (midD > numL)
        {
            if (_cam < midD && _cam > numL)
            {
                return numL;
            }
            else
            {
                return numS;
            }
        }
        else
        {
            if (_cam > midD && _cam < numL)
            {
                return numL;
            }
            else
            {
                return numS;
            }
        }
    }
*/

    private void bounceForce()
    {
        Debug.Log("Bounce");
        //set wallrun
        wr.state = WallRunning.WallState.exitingWall;
        wr.exitWallTimer = 0.2f;
        //set cooldown
        ih.activatedSPACE = false;

        Vector3 reflectedDir = Vector3.Reflect(ogDir, contactNorm);
        //Vector3 newDir = getBounceVector(cam.transform.forward, ogDir);
        rb.linearVelocity = Vector3.zero;
        //Add bounce force
        rb.AddForce(reflectedDir * tempVel * (1 + ih.acuSPACE * 0.4f), ForceMode.Impulse);
    }
}


