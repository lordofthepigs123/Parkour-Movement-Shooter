using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float jumpForce;
    [SerializeField] float SPboostForce;
    [SerializeField] float exitTime;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    [SerializeField] float fallMultiplier;
    [SerializeField] float minAngVel;
    [SerializeField] float angBoostMult;
    [SerializeField] float maxAngBoost;
    public float maxSlopeAngle;
    public float minSlopeAngle;
    [SerializeField] float koyoteTime;

    [Header("Publics")]
    public bool canJump;
    public float moveSpeed;
    [HideInInspector] public Vector3 slopeNormal;
    
    private RaycastHit slopeHit;
    public bool exitingSlope;
    private float exitTimer;
    private bool readyToJump;
    private bool SPready;
    private Vector3 flatVel;
    private Vector3 moveDirection;

    [Header("Components")]
    private PlayerStats ps;
    private InputHandler ih;
    private PlayerColliderManager cm;
    private PlayerStateMachine sm;
    private Rigidbody rb;
    [SerializeField] AngleJumpParticleManager aj;

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        ih = GetComponent<InputHandler>();
        cm = GetComponent<PlayerColliderManager>();
        sm = GetComponent<PlayerStateMachine>();
        readyToJump = true;
    }

    private void FixedUpdate()
    {
        if (sm.restricted || sm.rolling || sm.dashing || sm.freeFalling || sm.swinging || sm.grinding || sm.accelRail)
        {
            return;
        }
        MovePlayer();
    }

    private void Update()
    {
        //koyote time
        if (exitTimer > 0 || !cm.touchingWall)
            exitTimer -= Time.deltaTime;
        else
        {
            exitTimer = 0;
            exitingSlope = false;
        }
        
        if (cm.grounded && readyToJump)
        {
            canJump = true;
        }
        else if (canJump)
        {
            Invoke(nameof(KoyoteOff), koyoteTime);
        }

        //Turn off gravity while on slope
        if (!sm.wallRunning)
            rb.useGravity = !OnSlope();

        MyInput();
    }

    private void MyInput()
    {
        flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        //when to jump
        if (ih.ikeySPACE && readyToJump && canJump)
        {
            ih.activatedSPACE = true;
            readyToJump = false;
            canJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if (SPready && ih.rkeySPACE)
        {
            SPready = false;
            ih.rkeySPACE = false;
            //Bonus SP force
            rb.AddForce(Vector3.up * SPboostForce * ih.acuSPACE, ForceMode.Impulse);
        }
    }

    //WASD movement
    private void MovePlayer()
    {
        moveDirection = ih.baseInputDir;
        if (cm.touchingWall)
            moveDirection = ih.planeInputDir(cm.wallNormal, false);

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * ps.SPD.Value / 11, ForceMode.Force);

            if (moveDirection.y < 0)
                rb.AddForce(Vector3.down * 2f * flatVel.magnitude, ForceMode.Force);
            return;
        }

        //Gravity while falling / short jump
        if (sm.inAir)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * ps.SPD.Value / 11 * airMultiplier, ForceMode.Force);
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Physics.gravity * (fallMultiplier - 1) * Time.deltaTime;
            }
            return;
        }
        if (sm.wallRunningUp || sm.wallRunningDown)
        {
            rb.AddForce(Vector3.Project(moveDirection, cm.tangentWall) * moveSpeed * ps.SPD.Value / 11, ForceMode.Force);
            return;
        }

        if (cm.grounded)
        {
            rb.AddForce(moveDirection * moveSpeed * ps.SPD.Value / 11, ForceMode.Force);
            return;
        }
    }

    public void Jump()
    {
        exitingSlope = true;
        exitTimer = exitTime;
        SPready = true;

        Vector3 netforce = Vector3.up * jumpForce;
        //reset y velocity;
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = flatVel;
            netforce += Vector3.up * Mathf.Pow(-rb.linearVelocity.y, 0.5f);
        }
        if (rb.angularVelocity.magnitude > minAngVel)
        {
            float push = getAngularPush(cm.wallNormal, -transform.up, cm.lowesPoint) * angBoostMult;
            Debug.Log(push);
            push = maxAngBoost * (1 - Mathf.Pow((float)System.Math.E, -push / maxAngBoost)); // slow as aproach max
            netforce += cm.wallNormal * push;
        }
        rb.AddForce(netforce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void KoyoteOff()  
    {
        canJump = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(sm.rotAdjustPos, Vector3.down, out slopeHit, 2))
        {
            slopeNormal = slopeHit.normal;
            float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);
            return slopeAngle < maxSlopeAngle && slopeAngle > minSlopeAngle; //within range
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeNormal).normalized;
    }

    public float getAngularPush(Vector3 normal, Vector3 contactLength, Vector3 contact)
    {
        Vector3 diff = rb.position - contact;
        float size = cm.activeCol.bounds.size.y / 2;
        float travelDis = (Vector3.Project(diff, normal) - normal * size).magnitude;// get distance between wall and rb center then compare to fully straightened radius
        travelDis = Mathf.Clamp(travelDis, 0, size);
        Vector3 projectAng = Vector3.ProjectOnPlane(rb.angularVelocity, normal);
        Vector3 tranLength = Quaternion.AngleAxis(90, normal) * Vector3.ProjectOnPlane(contactLength, normal).normalized;

        if (Vector3.Dot(tranLength, projectAng) <= 0)
            return 0;
        float inertia = 0.5f * rb.mass * Mathf.Pow(contactLength.magnitude,2);//I = 0.5 mr^2
        float angEnergy = 0.5f * inertia * projectAng.magnitude;//E = 0.5 Iw^2
        float force = angEnergy / travelDis; // estimate force -  F = W/d

        aj.createParticle(force);
        return force;
    }
}