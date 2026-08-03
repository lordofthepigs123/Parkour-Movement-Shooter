using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class MainRagdollHandeler : PhysicsBody
{
    [Header("MainRagdollHandeler")]

    [SerializeField] float baseSpdAng;
    [SerializeField] float baseFricAng;
    [SerializeField] float speedMult_Grind;
    [SerializeField] float friction_Grind;
    [SerializeField] float sSpeedMult_standUp;
    [SerializeField] float sFriction_standUp;
    [SerializeField] float speedMult_standUp;
    [SerializeField] float friction_standUp;
    [SerializeField] float speedMult_WallRun;
    [SerializeField] float friction_WallRun;
    [SerializeField] float speedMult_Prone;
    [SerializeField] float friction_Prone;
    [SerializeField] float spreadMult;
    [SerializeField] float percentMod;
    [SerializeField] float degreeMax; // degrees 0 - 90
    [SerializeField] float deflectionMult;
    public float AngularSet;
    [HideInInspector] public float AngularDif;
    [HideInInspector] public Vector3 FixedRbNetAccel {get; private set;}

    [Header("Components")]
    private PlayerMovement pm;
    private WallRunning wr;
    private PlayerGrind pg;
    [HideInInspector] public PlayerStateMachine sm;
    [HideInInspector] public PlayerColliderManager cm;
    private HeatHandler hh;
    [SerializeField] private Transform bodyDirection;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        wr = GetComponent<WallRunning>();
        pg = GetComponent<PlayerGrind>();
        ff = GetComponent<FreeFall>();
        sm = GetComponent<PlayerStateMachine>();
        cm = GetComponent<PlayerColliderManager>();
        ih = GetComponent<InputHandler>();
        hh = GetComponent<HeatHandler>();
        rb.freezeRotation = false; //enabled full rb ragdoll
        rb.maxAngularVelocity = AngularSet;
    }

    private void Update()
    {
        AngularDif = Vector3.Angle(desiRotation * Vector3.up, transform.up);
    }

    private void FixedUpdate()
    {
        //
        FixedRbNetAccel = CalcRbNetAccel(Time.fixedDeltaTime);
        //
        if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.wedgegrabing)
        {

        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.swinging)
        {

        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.inhop)
        {
            //detector needs rework#
            /*
            desiRotation = orientation.rotation * Quaternion.FromToRotation(transform.rotation * Vector3.up, pg.triggerNormal);
            movementForces(0.5f);
            angularResistance();
            */

        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.grinding)
        {
            var (tangentSpline, upSpline, leftSpline, worldPos) = pg.getVarsRail(pg.currentRailScript);
            leanForces(tangentSpline, upSpline, leftSpline, worldPos, pg.angMult, pg.mouseDirStr);
            angFriction = friction_Grind;
            spdMult_ang = speedMult_Grind;
            angularResistance();
            //frictionAngDeflect() * #
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.accelrail)
        {
            var (tangentSpline, upSpline, leftSpline, worldPos) = pg.getVarsRail(pg.currentRailScript);
            leanForces(tangentSpline, upSpline, leftSpline, worldPos, pg.angMult, pg.mouseDirStr);
            angFriction = friction_Grind;
            spdMult_ang = speedMult_Grind;
            angularResistance();
            //frictionAngDeflect() * #
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.standingup)
        {
            angFriction = Mathf.Lerp(sFriction_standUp, friction_standUp, sm.standUpRatio);
            spdMult_ang = Mathf.Lerp(sSpeedMult_standUp, speedMult_standUp, sm.standUpRatio);
            desiRotation = bodyDirection.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.wallrunningup)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempSide = Vector3.Cross(Vector3.up, wr._wallNormal);
            desiRotation = frictionAngDeflect() * Quaternion.AngleAxis(20, tempSide) * bodyDirection.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.wallresistdown)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempSide = Vector3.Cross(Vector3.up, wr._wallNormal);
            desiRotation = frictionAngDeflect() * Quaternion.AngleAxis(10, tempSide) * bodyDirection.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.wallrunningdown)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempSide = Vector3.Cross(Vector3.up, wr._wallNormal);
            Quaternion flipRot = Quaternion.FromToRotation(Vector3.Reflect(bodyDirection.rotation * Vector3.forward, wr._wallNormal), bodyDirection.rotation * Vector3.forward);
            desiRotation = frictionAngDeflect() * flipRot * Quaternion.AngleAxis(-135, tempSide) * bodyDirection.rotation;
            movementForces(1);
            angularResistance();
            //add camera restrictions#
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.wallrunning)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempForward = Vector3.Cross(Vector3.up, wr._wallNormal);
            desiRotation = frictionAngDeflect() * Quaternion.AngleAxis(30, tempForward) * bodyDirection.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.rolling)
        {

        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.sliding)
        {
            // possibly frictionAngDeflect() #
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.prone)
        {
            // possibly frictionAngDeflect() #
            spdMult_ang = friction_Prone;
            angFriction = speedMult_Prone;
            desiRotation = Quaternion.AngleAxis(90, bodyDirection.right) * Quaternion.FromToRotation(Vector3.up, pm.slopeNormal) * bodyDirection.rotation;
            movementForces(0.6f);
            angularResistance();
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.freefall)
        {

        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.walking)
        {
            spdMult_ang = baseSpdAng;
            angFriction = baseFricAng;
            if (pm.OnSlope())
            {
                
            }
            desiRotation = frictionAngDeflect() * bodyDirection.rotation;
            movementForces(1);
            //pitchForces(1);
            angularResistance();
        }
        //
        else if (sm.CurrentStateKey == PlayerStateMachine.EMovementState.air)
        {
            spdMult_ang = baseSpdAng;
            angFriction = 0;
            float tempYvel = rb.linearVelocity.y / spreadMult;
            float deflectionMult = rb.linearVelocity.magnitude / spreadMult * (2 * tempYvel / (Mathf.Pow(tempYvel, 2) + 1));
            float mixDegree = 2 / (1 + Mathf.Pow(1 + 1 / percentMod, -deflectionMult)) - 1;
            Vector3 velCross = Vector3.Cross(rb.linearVelocity, Vector3.up);
            desiRotation = Quaternion.AngleAxis(-mixDegree * degreeMax, velCross) * orientation.rotation;
            //Debug.DrawRay(rb.worldCenterOfMass, Quaternion.AngleAxis(mixDegree * degreeMax, velCross) * Vector3.up * 3, Color.maroon);
            movementForces(0.2f);
        }
    }

    private Quaternion frictionAngDeflect() 
    {
        //tilt opposite way when slowing down
        float accFriction = hh.heatNegative.magnitude / Time.fixedDeltaTime; //a = v delta / t delta
        float deflectionAngle = Mathf.Atan(accFriction * deflectionMult / Physics.gravity.magnitude) * Mathf.Rad2Deg;//θ = arctan(a/g)
        //deflectionAngle *= deflectionMult;
        //Debug.Log(deflectionAngle);
        Vector3 tempAxis = Vector3.Cross(cm.wallNormal, hh.heatNegative.normalized).normalized;
        Quaternion deflectQuaternion = Quaternion.AngleAxis(deflectionAngle, tempAxis);
        return deflectQuaternion;
    }
}
