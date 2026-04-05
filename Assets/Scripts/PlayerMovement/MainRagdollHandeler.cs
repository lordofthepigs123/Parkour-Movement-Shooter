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
    public float angularSet;
    private float angularMaxOG;
    [HideInInspector] public float angularDif;

    [Header("Components")]
    private PlayerMovement pm;
    private WallRunning wr;
    private PlayerGrind pg;
    private StateManager sm;
    private PlayerColliderManager cm;
    private HeatHandler hh;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        wr = GetComponent<WallRunning>();
        pg = GetComponent<PlayerGrind>();
        ff = GetComponent<FreeFall>();
        sm = GetComponent<StateManager>();
        cm = GetComponent<PlayerColliderManager>();
        ih = GetComponent<InputHandler>();
        hh = GetComponent<HeatHandler>();
        rb.freezeRotation = false; //enabled full rb ragdoll
        angularMaxOG = rb.maxAngularVelocity;//default max angular velocity
        rb.maxAngularVelocity = angularSet;
    }

    private void Update()
    {
        angularDif = Vector3.Angle(desiRotation * Vector3.up, transform.up);
    }

    private void FixedUpdate()
    {
        //
        if (sm.state == StateManager.MovementState.wedgegrabing)
        {

        }
        //
        else if (sm.state == StateManager.MovementState.swinging)
        {

        }
        //
        else if (sm.state == StateManager.MovementState.inhop)
        {
            //detector needs rework#
            /*
            desiRotation = orientation.rotation * Quaternion.FromToRotation(transform.rotation * Vector3.up, pg.triggerNormal);
            movementForces(0.5f);
            angularResistance();
            */

        }
        //
        else if (sm.state == StateManager.MovementState.grinding)
        {
            var (tangentSpline, upSpline, leftSpline, worldPos) = pg.getVarsRail(pg.currentRailScript);
            leanForces(tangentSpline, upSpline, leftSpline, worldPos, pg.angMult, pg.mouseDirStr);
            angFriction = friction_Grind;
            spdMult_ang = speedMult_Grind;
            angularResistance();
            //frictionAngDeflect() * #
        }
        //
        else if (sm.state == StateManager.MovementState.accelrail)
        {
            var (tangentSpline, upSpline, leftSpline, worldPos) = pg.getVarsRail(pg.currentRailScript);
            leanForces(tangentSpline, upSpline, leftSpline, worldPos, pg.angMult, pg.mouseDirStr);
            angFriction = friction_Grind;
            spdMult_ang = speedMult_Grind;
            angularResistance();
            //frictionAngDeflect() * #
        }
        //
        else if (sm.state == StateManager.MovementState.standingup)
        {
            angFriction = Mathf.Lerp(sFriction_standUp, friction_standUp, ff.standUpRatio);
            spdMult_ang = Mathf.Lerp(sSpeedMult_standUp, speedMult_standUp, ff.standUpRatio);
            desiRotation = orientation.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.state == StateManager.MovementState.wallrunningup)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempSide = Vector3.Cross(Vector3.up, wr._wallNormal);
            desiRotation = frictionAngDeflect() * Quaternion.AngleAxis(20, tempSide) * orientation.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.state == StateManager.MovementState.wallresistdown)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempSide = Vector3.Cross(Vector3.up, wr._wallNormal);
            desiRotation = frictionAngDeflect() * Quaternion.AngleAxis(10, tempSide) * orientation.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.state == StateManager.MovementState.wallrunningdown)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempSide = Vector3.Cross(Vector3.up, wr._wallNormal);
            Quaternion flipRot = Quaternion.FromToRotation(Vector3.Reflect(orientation.rotation * Vector3.forward, wr._wallNormal), orientation.rotation * Vector3.forward);
            desiRotation = frictionAngDeflect() * flipRot * Quaternion.AngleAxis(-135, tempSide) * orientation.rotation;
            movementForces(1);
            angularResistance();
            //add camera restrictions#
        }
        //
        else if (sm.state == StateManager.MovementState.wallrunning)
        {
            spdMult_ang = speedMult_WallRun;
            angFriction = friction_WallRun;
            Vector3 tempForward = Vector3.Cross(Vector3.up, wr._wallNormal);
            desiRotation = frictionAngDeflect() * Quaternion.AngleAxis(30, tempForward) * orientation.rotation;
            movementForces(1);
            angularResistance();
        }
        //
        else if (sm.state == StateManager.MovementState.rolling)
        {

        }
        //
        else if (sm.state == StateManager.MovementState.sliding)
        {
            // possibly frictionAngDeflect() #
        }
        //
        else if (sm.state == StateManager.MovementState.prone)
        {
            // possibly frictionAngDeflect() #
            spdMult_ang = friction_Prone;
            angFriction = speedMult_Prone;
            desiRotation = Quaternion.AngleAxis(90, orientation.right) * Quaternion.FromToRotation(Vector3.up, pm.slopeNormal) * orientation.rotation;
            movementForces(0.6f);
            angularResistance();
        }
        //
        else if (sm.state == StateManager.MovementState.freefall)
        {

        }
        //
        else if (sm.state == StateManager.MovementState.walking)
        {
            spdMult_ang = baseSpdAng;
            angFriction = baseFricAng;
            if (pm.OnSlope())
            {
                
            }
            desiRotation = frictionAngDeflect() * orientation.rotation;
            movementForces(1);
            //pitchForces(1);
            angularResistance();
        }
        //
        else if (sm.state == StateManager.MovementState.air)
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
