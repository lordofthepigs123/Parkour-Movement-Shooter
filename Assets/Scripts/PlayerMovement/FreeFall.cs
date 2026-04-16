using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class FreeFall : PhysicsBody
{
    [Header("FreeFall")]
    [SerializeField] float exitFFTime;
    [SerializeField] float enterVelBoundary;
    [SerializeField] float friction_rot;
    [SerializeField] float speedMult_rot;
    [SerializeField] float reversTime;
    [SerializeField] float reversCD;
    [SerializeField] float friction_Reversed;
    [SerializeField] float speedMult_Reversed;
    [SerializeField] float angBiasTilt;
    [SerializeField] float opposeAngBiasTilt;
    [SerializeField] float angBiasRoll;
    [SerializeField] float opposeAngBiasRoll;
    [SerializeField] float boundaryBuff;
    [SerializeField] float resistTorqueRoot; // 1 -  tranition root on proportion of resultingTorque used
    [SerializeField] int xtransitionSmooth; // rate at which freefall will bend between states
    [SerializeField] public float standUpTime;
    [SerializeField] float maxRollTime;
    [SerializeField] float minRollTime;
    [SerializeField] float maxVelRollTimeMod;
    [SerializeField] float rollTimeMult;
    [SerializeField] float rollForce;
    [SerializeField] float rollDesiWeighting; // 0 - 1 where larger is more roll
    [SerializeField] float rollCamPull; // 0 - 1
    [SerializeField] float kickForce;

    private Quaternion savedRotation;
    private Vector3 savedRollAxis;//global roll
    private Vector3 xboundaryNormal;
    private Vector3 inputDir;
    private float exitFFTimer;
    private float reversTimer;
    private float rollDirx;//bool
    private float rollDiry;//bool
    private float rollTimer;
    private float currentRollTime;
    [HideInInspector] public float standUpRatio;
    private float standUpTimer;
    private float totalx;
    private float resistancePercent;
    private bool _freeFalling;
    private bool Rolling;
    [SerializeField] private bool faceUp;
    private bool faceInversed;
    private bool canReverse;
    private bool barrelRolling;

    [Header("physics control")]
    [SerializeField] float normalForceMult;// multiplies all areas
    [SerializeField] float yPlaneVelInfluence; // 0 -  how strongly xz vel is weighted
    [SerializeField] float xzBonusCons;// 0 - 1 how much xz force is reduced when opposing
    [SerializeField] float angleOfAtk;// 0 - 90


    [Header("Components")]
    private PlayerStateMachine sm;
    private PlayerColliderManager cm;
    private PlayerCam pc;
    private SlideRoll sr; 
    private PlayerGrind pg;
    
    public FallState state;

    public enum FallState
    {
        idle,
        free,
        upward,
        reversed, // foot kick
        forward,
        backward,
        forwardInv,
        backwardInv,
        rolling,
        barrelRolling,
        exiting,
        standingUp
        
    }
    private void Start()
    {
        ih = GetComponent<InputHandler>();
        sm = GetComponent<PlayerStateMachine>();
        cm = GetComponent<PlayerColliderManager>();
        rb = GetComponent<Rigidbody>();
        pc = cam.GetComponent<PlayerCam>();
        sr = GetComponent<SlideRoll>();
        pg = GetComponent<PlayerGrind>();

        ff = GetComponent<FreeFall>();
    }

    private void Update()
    {
        StateMachine();
    }
    private void FixedUpdate()
    {
        Movement();
    }

    private void StateMachine()
    {
        bool otherMode = (cm.grounded || sm.rolling || sm.sliding || pg.lockedWithRail || sm.wallRunning || cm.touchingWall);

        if (state == FallState.idle)
        {
            if ( _freeFalling || otherMode)
                return;
            if (ih.keySHIFT)
            {
                ih.activatedSHIFT = true;
                enterFreeFall();
            }
            if (ih.keyX)
            {
                enterReverse();

                rb.AddForce(cam.forward * kickForce, ForceMode.Impulse);
                enterFreeFall();
            }
            if (Mathf.Abs(rb.linearVelocity.y) > enterVelBoundary)
                enterFreeFall();
            return;
        }

        if (_freeFalling && !otherMode)
        {
            if (ih.keySHIFT && !ih.activatedSHIFT && !Rolling)//air rolls
            {
                currentRollTime = maxRollTime - Mathf.Pow(Mathf.Clamp(rb.linearVelocity.magnitude / maxVelRollTimeMod ,0,1), rollTimeMult) * (maxRollTime - minRollTime); //decrease roll time with higher vel with clamp

                ih.activatedSHIFT = true;
                desiRotation = calcDesiRot(false);
                savedRotation = desiRotation;
                faceInversed = !faceInversed;
                if (Math.Abs(ih.keyHorizontal) > 0.3f)//start barrel roll
                {
                    pc.DoRoll(playerBody.up * ih.keyHorizontal, currentRollTime);
                    pc.inverse_y = !pc.inverse_y;
                    calcAngleX();
                    rollDirx = (int)ih.keyHorizontal;//what dir will roll spin?
                    rollDiry = (int)ih.keyVertical;

                    enterBarrelRoll();
                }
                else //flip forward
                {
                    rollDiry = (int)ih.keyVertical;//what dir will roll spin?

                    enterRoll();
                }
                pulseRoll();
            }
            
            //air kick
            if (ih.keyX && !ih.activatedX && canReverse)
                enterReverse();
        }

        if (state == FallState.rolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer < 0)
                exitRoll();
            return;
        }

        if (state == FallState.barrelRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer < 0)
                exitRoll();
            return;
        }

        if (state == FallState.exiting)
        {
            if (exitFFTimer > 0)
                exitFFTimer -= Time.deltaTime;
            else
                state = FallState.idle;
            return;
        }

        if (state == FallState.standingUp)
        {
            if (Rolling)
                exitRoll();

            standUpTimer -= Time.deltaTime;
            standUpRatio = 1 - standUpTimer / standUpTime;
            if (standUpTimer < 0)
            {
                sm.standingUp = false;
                exitFreeFall();
            }
            return;
        }
        //exit when ground or other
        if (otherMode)
        { 
            //start standing up
            sm.freeFalling = false;
            _freeFalling = false;

            if (pg.lockedWithRail)
            {
                exitFFTimer = exitFFTime;
                state = FallState.exiting;
                
                return;
            }
            if (Rolling)
            {
                exitFreeFall();
                if (ih.keyHorizontal == 0 && ih.keyVertical == 0)
                {
                    sr.StartSlide();
                }
                else
                {
                    sr.StartRoll(rollTimer / currentRollTime);//continue rolling on ground
                }
                return;
            }
            if (Mathf.Abs(rb.linearVelocity.y) < enterVelBoundary)//if low falling speed allow smooth transitions without rolling
            {
                if (ih.heldSHIFT)
                {
                    sr.StartSlide();
                    exitFFTimer = exitFFTime/2;
                    state = FallState.exiting;
                    return;
                }
                if (ih.heldX)
                {
                    exitFFTimer = exitFFTime;
                    state = FallState.exiting;
                    return;
                }
            }

            //adjust for other exits ex.Wallrunning #
            standUpTimer = standUpTime;
            state = FallState.standingUp;
            sm.standingUp = true;
            
            return;
        }

        if (reversTimer > 0)
        {
            state = FallState.reversed;

            reversTimer -= Time.deltaTime;
            return;
        }
        if (ih.heldSHIFT)
        {
            state = FallState.upward;
            return;
        }
        if (ih.keyVertical == 0 && ih.keyHorizontal == 0)
        {
            state = FallState.free;
            return;
        }
        calcAngleX();//replace xRotation with totalx

        //Transitions between 5 freefall states
        if (!faceUp && !faceInversed)
        {
            state = FallState.forward;
        }
        else if (faceUp && !faceInversed)
        {
            state = FallState.backward;
        }
        else if (!faceUp && faceInversed)
        {
            state = FallState.forwardInv;
        }
        else if (faceUp && faceInversed)
        {
            state = FallState.backwardInv;
        }
    }
    private void Movement()
    {
        if (_freeFalling)
        {
            freeFallMovement();//sets desiRot
            
            if (Rolling)//roll calc
            {
                if (state == FallState.rolling || state == FallState.barrelRolling)
                {
                    RollMovement();
                }
            }

            if (state == FallState.free)
            {
                //
            }
            else
            {//apply desiRot
                movementForces(1);
            }

            resultingForces(normalForceMult, yPlaneVelInfluence, xzBonusCons, resistancePercent);
            //angularResistance();
            return;
        }

        //exiting movement

        
    }

    public void enterFreeFall()
    {
        state = FallState.free;
        sm.freeFalling = true;
        _freeFalling = true;

        Debug.Log("enterFF");
    }

    private void exitFreeFall()
    {
        if (Rolling)
        {
            exitFFTimer = 0.3f;
            exitRoll();
        }
        else
        {
            exitFFTimer = exitFFTime;
        }
        sm.standingUp = false;

        Debug.Log("exitFF");
        state = FallState.exiting;
    }

    //WASD movement when FreeFalling
    private void freeFallMovement()
    {
        angFriction = friction_rot;
        spdMult_ang = speedMult_rot;

        if (state == FallState.reversed)
        {
            angFriction = friction_Reversed;
            spdMult_ang = speedMult_Reversed;
        }

        //INPUTS
        desiRotation = calcDesiRot(false);
        //Debug.DrawRay(transform.position, desiRotation * Vector3.up * 2, Color.purple, 3);
        //Debug.DrawRay(transform.position, playerBody.up * 3, Color.yellow, 3);
    }

    private void RollMovement()
    {
        Quaternion rollRotation;

        //calc spin
        Vector3 spin_Axis;

        float tempRatio = rollTimer / currentRollTime;

        if (barrelRolling)//determine current axis of roll
        {
            spin_Axis = savedRotation * Vector3.up;
            Vector3 axisP = playerBody.rotation * Vector3.forward;
            Vector3 axisS = savedRotation * Vector3.forward;
            Quaternion backAdjust = Quaternion.FromToRotation(axisP, axisS);

            rollRotation = Quaternion.AngleAxis(-180 * rollDirx * (1 - tempRatio), spin_Axis) * savedRotation;//backAdjust#
        }
        else
        {
            spin_Axis = savedRollAxis;//saved orientation.right
            /*
            Vector3 orth_forward = playerBody.up;
            Vector3.OrthoNormalize(ref spin_Axis, ref orth_forward);
            spin_Axis = Vector3.Lerp(savedRollAxis, spin_Axis, rollOrthogonalWeight);

            Debug.DrawRay(transform.position, savedRotation * Vector3.up * 2.5f, Color.red);
            */

            rollRotation = Quaternion.AngleAxis(-360 * rollDiry * tempRatio, spin_Axis) * savedRotation;
        }

        //weighted average
        desiRotation = Quaternion.Slerp(desiRotation, rollRotation, rollDesiWeighting);
        //Debug.DrawRay(transform.position, desiRotation * Vector3.up * 3, Color.purple, 3);
        //Debug.DrawRay(transform.position, desiRotation * Vector3.right * 2, Color.green, 1);
    }


    private float angleMultCalc()//Return angleOfAtk bent towards other angle when aproaching boundry between states
    {
        float xRot = (pc.xRotation + 270) % 180; // + 90
        return (1 - Mathf.Pow(xRot / 90 - 1, xtransitionSmooth*2)) * angleOfAtk;//#
    }

    private void calcAngleX()//replace xRotation with totalx & get boundary angle for flipping
    {
        totalx = pc.xRotation;// + ih.keyVertical * -20;
        /*
        //mod totalx
        if (totalx > 180)
            totalx -= 360;
        else if (totalx < -180)
            totalx += 360;
        */

        xboundaryNormal = Vector3.ProjectOnPlane(rb.linearVelocity, orientation.right);
        Quaternion lookRot = Quaternion.AngleAxis(totalx, orientation.right);
        faceUp = Vector3.Angle(xboundaryNormal, lookRot * orientation.forward) > 90 + (faceUp? -boundaryBuff : boundaryBuff);
        faceInversed = Mathf.Abs(totalx) > 90 + (faceInversed? -boundaryBuff : boundaryBuff);
    }

    private void pulseRoll()
    {
        //Roll force
        inputDir = ih.baseInputDir;//force direction
        rb.AddForce(inputDir * rollForce, ForceMode.Impulse);
    }

    private void enterRoll()
    {
        rollTimer = currentRollTime;

        Rolling = true;
        state = FallState.rolling;

        savedRollAxis = orientation.right;
        Debug.Log("enterR");

        //camera effects
        pc.DoRoll(orientation.right * rollDiry, currentRollTime);
    }

    private void enterBarrelRoll()
    {
        rollTimer = currentRollTime;

        Rolling = true;
        barrelRolling = true;
        state = FallState.barrelRolling;

        Debug.Log("enterBaR");
    }

    private void exitRoll()
    {
        Rolling = false;
        barrelRolling = false;
        state = FallState.free;
        Debug.Log("exitR");
    }

    private void enterReverse() // foot kick
    {
        reversTimer = reversTime;
        ih.activatedX = true;
        canReverse = false;

        Invoke(nameof(cooldownReverse), reversCD + reversTime);
    }
    private void cooldownReverse()
    {
        canReverse = true;
    }

    private Quaternion calcDesiRot(bool saveRot)
    {
        float desiTilt;

        inputDir = ih.baseInputDir;
                    
        resistancePercent = 1; // torque mult set
        if (ih.heldSHIFT)//upward
        {
            return Quaternion.LookRotation(-cam.up, cam.forward);
        }
        if (state == FallState.reversed)//reversed
        {
            return Quaternion.LookRotation(cam.up, -cam.forward);
        }
        if (inputDir == Vector3.zero)//free
        {
            return Quaternion.FromToRotation(Vector3.up, rb.linearVelocity.normalized) * orientation.rotation;
        }

        if (!faceUp && !faceInversed)//fwd
        {
            desiTilt = 90;
        }
        else if (faceUp && !faceInversed)//bwd
        {
            desiTilt = -90;
        }
        else if (!faceUp && faceInversed)//fwdInv
        {
            desiTilt = -90;
        }
        else//bwdInv
        {
            desiTilt = 90;
        }
        Vector3 rnormalVel = Vector3.ProjectOnPlane(rb.linearVelocity, orientation.right).normalized;
        float transAngle = Vector3.Angle(Vector3.down, rnormalVel) - 90;//angle -90 to 90
        float inputWeightV = Mathf.Abs(ih.keyVertical) + 0.1f;
        if (Vector3.Dot(rnormalVel, orientation.forward * (faceInversed ? -1 : 1) * ih.keyVertical) >= 0) //vertical ih opposes vel
        {//bias angle towards straight
            //acceleration
            //Debug.Log(transAngle);
            transAngle = Mathf.Abs(transAngle)/transAngle * Mathf.Pow(Mathf.Abs(transAngle) / 90, angBiasTilt / inputWeightV); 
            //Debug.Log("yup" + transAngle * (90 - angleOfAtk));
        }
        else
        {//bias angle towards vertical
            //deceleration
            transAngle = -Mathf.Abs(transAngle)/transAngle * Mathf.Pow(Mathf.Abs(transAngle) / 90, 1 / opposeAngBiasTilt / inputWeightV);
            //Debug.Log("nup" + transAngle * (90 - angleOfAtk));
        }
        transAngle = transAngle * (90 - angleOfAtk);

        //calculate effectivness of result torque
        float resistVert = Mathf.Pow(1 - Mathf.Abs(transAngle/90), 1 / resistTorqueRoot);

        Vector3 fnormalVel = Vector3.ProjectOnPlane(rb.linearVelocity, orientation.forward).normalized;
        float rollAngle = Vector3.Angle(Vector3.down, fnormalVel) - 90;//angle -90 to 90
        float inputWeightH = Mathf.Abs(ih.keyHorizontal) + 0.1f;
        if (Vector3.Dot(fnormalVel, orientation.right * ih.keyHorizontal) >= 0) //vertical ih opposes vel
        {//bias angle towards side-on
            //acceleration
            rollAngle = Mathf.Abs(rollAngle)/rollAngle * Mathf.Pow(Mathf.Abs(rollAngle) / 90, angBiasRoll / inputWeightH);
            //Debug.Log("ryup" + rollAngle * (90 - angleOfAtk));
        }
        else
        {//bias angle towards flat
            //deceleration
            rollAngle = -Mathf.Abs(rollAngle)/rollAngle * Mathf.Pow(Mathf.Abs(rollAngle) / 90, 1 / opposeAngBiasRoll / inputWeightH);
            //Debug.Log("rnup" + rollAngle * (90 - angleOfAtk));
        }
        rollAngle = rollAngle * (90 - angleOfAtk);

        //calculate effectivness of result torque
        float resistHori = Mathf.Pow(1 - Mathf.Abs(rollAngle/90), 1 / resistTorqueRoot);
        resistancePercent = (resistVert + resistHori) / 2; // torque mult modify #

        Quaternion warpedVel = Quaternion.AngleAxis(transAngle,Vector3.Cross(Vector3.down, rnormalVel)) * Quaternion.AngleAxis(rollAngle,Vector3.Cross(Vector3.down, fnormalVel));//apply tilt & roll such that maximum effect against vel

        //Debug.DrawRay(transform.position, warpedVel * orientation.rotation * Quaternion.AngleAxis(desiTilt, Vector3.right) * Vector3.up * 3, Color.red, 1);
        //Debug.DrawRay(transform.position, warpedVel * orientation.rotation * Quaternion.AngleAxis(desiTilt, Vector3.right) * Vector3.forward * 2, Color.yellow, 1);
        
        return warpedVel * orientation.rotation * Quaternion.AngleAxis(desiTilt, Vector3.right);
    }
}
