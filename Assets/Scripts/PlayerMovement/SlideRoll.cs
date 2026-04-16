using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class SlideRoll : PhysicsBody
{
    [Header("SlideRoll")]
    [SerializeField] float rollForce;
    [SerializeField] float upForceRemMult;// 0 - 1
    [SerializeField] float StickForce;
    [SerializeField] float exitTimeBase;
    [SerializeField] float maxRollTime;
    [SerializeField] float minRollTime;
    [SerializeField] float detachTime;
    [SerializeField] float friction_slide;
    [SerializeField] float turnMult_slide;
    [SerializeField] float resistRemovMult;// 0 - 1
    [SerializeField] float slideFuncBase;// 1 -
    [SerializeField] float slideFuncCrit;
    [SerializeField] float gravMult;
    [SerializeField] float slopeMult;
    [SerializeField] float rollCamPull;// 0 - 1

    private bool canSlideRoll;
    private float detachTimer;
    private float exitTimer;
    private float exitTime;
    private float rollTimer;
    private float applyRatio;
    [HideInInspector] public Vector3 moveDirection;
    private Vector3 spin_Axis;
    [SerializeField] AnimationCurve rollCurve;

    [Header("Components")]
    [SerializeField] ComboCounter cc;
    private PlayerStats ps;
    private PlayerStateMachine sm;
    private PlayerCam pc;
    private PlayerColliderManager cm;
    private PlayerMovement pm;
    private WallRunning wr;

    public SRState state;

    public enum SRState
    {
        idle,
        rolling,
        sliding,
        exiting
    }

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        sm = GetComponent<PlayerStateMachine>();
        pc = cam.GetComponent<PlayerCam>();
        ih = GetComponent<InputHandler>();
        ff = GetComponent<FreeFall>();
        cm = GetComponent<PlayerColliderManager>();
        pm = GetComponent<PlayerMovement>();
        wr = GetComponent<WallRunning>();
    }

    private void Update()
    {
        StateMachine();
    }

    private void StateMachine()
    {

        //checking if on surface
        if (cm.grounded || cm.touchingWall)
        {
            detachTimer = 0;
            canSlideRoll = true;
        }
        else if (canSlideRoll && detachTimer < detachTime)
        {
            detachTimer += Time.deltaTime;
        }
        else
        {
            canSlideRoll = false;
        }

        if (state == SRState.idle)
        {
            //enter checks
            if ((ih.keySHIFT || ih.heldSHIFT) && canSlideRoll)
            {
                if (sm.freeFalling || sm.standingUp)
                {
                    return;
                }
                ih.activatedSHIFT = true;
                StartSlide();
            }
            return;
        }
        if (state == SRState.exiting)
        {
            //exit timer
            if (exitTimer > 0)
                exitTimer -= Time.deltaTime;
            else
            {
                state = SRState.idle;
            }

            if (ih.keySHIFT && !ih.activatedSHIFT && canSlideRoll)
            {
                ih.activatedSHIFT = true;
                StartRoll(1);
            }

            return;
        }
        if (state == SRState.rolling)
        {

            if (!cm.grounded && !cm.touchingWall)//check if in air
            {
                sm.rolling = false;
                state = SRState.idle;
                //stop crouch
                cm.changeCol(PlayerColliderManager.ActiveCol.Capsule);
                //playerBody.localScale = new Vector3(playerBody.localScale.x, sm.startYScale, playerBody.localScale.z);

                ff.enterFreeFall();
            }

            if (rollTimer > 0)
                rollTimer -= Time.deltaTime;
            else
            {
                StopRoll();
            }

            return;
        }
        if (state == SRState.sliding)
        {
            if (wr.proximWall && !cm.grounded && ih.ikeySPACE && !ih.activatedSPACE)//slide wall jump
            {
                ih.activatedSPACE = true;
                wr.wallJump();
            }

            if (!ih.heldSHIFT || !canSlideRoll)
            {
                StopSliding();
            }
            return;
        }
    }

    private void FixedUpdate()
    {
        //roll movement
        if (state == SRState.rolling)
            RollMovement();
        else if (state == SRState.sliding)
        {
            SlideMovement();
        }
    }

    //add HUD invincibility indicator for all rolls
    public void StartRoll(float ratio)
    {
        Debug.Log("sRoll");
        cm.changeCol(PlayerColliderManager.ActiveCol.Sphere);//shrink
        sm.rolling = true;
        ih.activatedSHIFT = true;
        state = SRState.rolling;

        ratio = Mathf.Clamp(ratio,0,1);//how much already complete#
        moveDirection = ih.planeInputDir(cm.wallNormal, false);//set direction
        spin_Axis = Quaternion.AngleAxis(90, cm.wallNormal) * moveDirection.normalized;
        Debug.DrawRay(transform.position, moveDirection * 5, Color.red, 1);
        //calculate time
        float currentRollTime = Mathf.Lerp(minRollTime, maxRollTime, ratio);
        rollTimer = currentRollTime;
        Debug.Log(rollTimer + " seconds");
        applyRatio = (maxRollTime - rollTimer) / maxRollTime;
        DOTween.To(() => applyRatio, x => applyRatio = x, 1, currentRollTime).SetEase(rollCurve);

        //camera effects
        pc.DoRoll(spin_Axis, currentRollTime);
    }

    private void RollMovement()
    {
        //calc spin
        Vector3 orth_forward = playerBody.forward;
        Vector3.OrthoNormalize(ref spin_Axis, ref orth_forward);

        Vector3 netForce = moveDirection.normalized * rollForce;//base boost force
        netForce.y -= netForce.y * upForceRemMult;
        netForce -= cm.wallNormal * StickForce;//force towards surface
        //addforce
        rb.AddForce(netForce, ForceMode.Force);
        desiRotation = Quaternion.AngleAxis(360 * applyRatio, spin_Axis) * orientation.rotation;
        movementForces(0.5f);
    }

    private void StopRoll()
    {
        Debug.Log("nRoll");
        if (ih.heldSHIFT)
        {
            StartSlide();
        }
        else
        {
            cm.changeCol(PlayerColliderManager.ActiveCol.Capsule);

            exitTime = exitTimeBase / ps.BLNC.Value * 70;
            exitTimer = exitTime;
            //rb.angularVelocity = Vector3.zero;
            state = SRState.exiting;
        }
        sm.rolling = false;
    }

    public void StartSlide()
    {
        Debug.Log("sSlide");
        //camera effects add #

        rb.angularVelocity *= 0.2f;
        rb.linearVelocity *= 1.2f;//speed boost
        moveDirection = ih.planeInputDir(cm.wallNormal, false);//save direction
        cm.changeCol(PlayerColliderManager.ActiveCol.Sphere);//shrink
        state = SRState.sliding;
        sm.sliding = true;
        ih.activatedSHIFT = true;
    }

    private void SlideMovement()
    {
        angFriction = friction_slide;
        spdMult_ang = turnMult_slide;

        //determine body tilt back
        Vector3 tempNormal = rb.linearVelocity;
        Vector3 tempCorrection = moveDirection - tempNormal;
        float tempMag = tempCorrection.magnitude;
        Vector3.OrthoNormalize(ref tempNormal, ref tempCorrection);
        tempCorrection *= tempMag;
        Vector3 tempAxis = Vector3.Cross(transform.rotation * Vector3.forward, cm.wallNormal).normalized;
        desiRotation = Quaternion.AngleAxis(45, tempAxis) * Quaternion.FromToRotation(Vector3.up, cm.wallNormal) * orientation.rotation;//set lean back when sliding
        //Debug.DrawRay(transform.position, desiRotation * Vector3.up * 5, Color.black, 0.1f);

        //apply forces
        Vector3 netForce = Vector3.zero;
        
        Vector3 resistForce = -rb.linearVelocity * rb.linearDamping + Physics.gravity;
        if (Vector3.Dot(resistForce.normalized, moveDirection) < 0)//anti resist force
        {
            float velMult = Vector3.Project(rb.linearVelocity, moveDirection).magnitude;
            velMult = 1 / (1 + Mathf.Pow(slideFuncBase, -velMult + slideFuncCrit));// 1 / (1 + c ^ -(x - b))
            resistForce = -Vector3.Project(resistForce, Quaternion.AngleAxis(90, cm.wallNormal) * moveDirection) * velMult;
            resistForce -= Vector3.Project(rb.linearVelocity, resistForce.normalized) * velMult;
            resistForce += Physics.gravity * (1 - velMult) * gravMult; //punish force

            if (resistForce.magnitude > 500)
            {
                resistForce = resistForce/resistForce.magnitude * 500;
            }
            netForce += resistForce * resistRemovMult;

            Debug.DrawRay(transform.position, resistForce, Color.blue, 2);
        }

        float tempAngle = SlopeAngle();
        if (tempAngle < pm.maxSlopeAngle && tempAngle > pm.minSlopeAngle)//slope accel
        {
            Vector3 slopeNormal2d = new Vector3(pm.slopeNormal.x, 0, pm.slopeNormal.z).normalized;
            Vector3 slopeForce = Vector3.ProjectOnPlane(slopeNormal2d, cm.wallNormal).normalized * slopeMult * Mathf.Sin(tempAngle * 2 * Mathf.Deg2Rad);
            netForce += slopeForce;
            Debug.Log(slopeForce * 2);
            //Debug.DrawRay(transform.position, slopeForce * 2, Color.blue);
            //Debug.DrawRay(transform.position, Vector3.ProjectOnPlane(slopeNormal2d, cm.wallNormal).normalized * 2, Color.green);
        }
        netForce -= cm.wallNormal * StickForce;
        rb.AddForce(netForce, ForceMode.Force);//force towards surface

        movementForces(1);
        angularResistance();
    }

    private void StopSliding()
    {
        Debug.Log("nSlide");
        cm.changeCol(PlayerColliderManager.ActiveCol.Capsule);
        sm.sliding = false;
        exitTime = exitTimeBase / ps.BLNC.Value * 40;
        exitTimer = exitTime;
        state = SRState.exiting;

        //suck to wall
        rb.AddForce(-cm.wallNormal, ForceMode.Impulse);
    }
/*
    private void freeze()
    {
        if (!sm.accelRail && !sm.grinding)//if not on rail
        {
            rb.angularVelocity = Vector3.zero;//remove spin
            rb.maxAngularVelocity = angularMaxOG;//default max angular velocity
            rb.MoveRotation(Quaternion.identity);
        }
        //stop crouch
        cm.changeCol(PlayerColliderManager.ActiveCol.Capsule);
        //playerBody.localScale = new Vector3(playerBody.localScale.x, sm.startYScale, playerBody.localScale.z);
    }
    */

    public float SlopeAngle()
    {
        return Vector3.Angle(Vector3.up, cm.wallNormal);
    }
}
