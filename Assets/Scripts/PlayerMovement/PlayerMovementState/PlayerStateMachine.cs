using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerStateMachine : StateManager<PlayerStateMachine.EMovementState>
{
    public EMovementState state;  // # remove

    public enum EMovementState
    {
        freeze,
        unlimited,
        wedgegrabing,
        swinging,
        inhop,
        accelrail,
        grinding,
        wallrunningup,
        wallrunningdown,
        wallresistdown,
        wallrunning,
        walking,
        dashing,
        rolling,
        sliding,
        standingup,
        freefall,
        prone,
        air
    }
    
    [Header("Adjustable Variables")]
    [SerializeField] float groundDrag;
    [SerializeField] float slideDrag;
    [SerializeField] float wallRunDrag;
    [SerializeField] float upRunDrag;
    [SerializeField] float downRunDrag;
    [SerializeField] float grindDrag;
    [SerializeField] float airDrag;
    [SerializeField] float dragFF;
    [SerializeField] float defaultMovSpeed;
    [SerializeField] float proneStickForce;
    [SerializeField] float proneSpeed;
    [SerializeField] float proneDrag;
    [SerializeField] float vdownSpeed;
    [SerializeField] float vupSpeed;

    [Header("State Variables")]
    public bool unlimited;
    public bool restricted;
    public bool rolling;
    public bool sliding;
    public bool standingUp;
    public bool freeFalling;
    public bool wallRunning;
    public bool wallRunningUp;
    public bool wallRunningDown;
    public bool wallResistDown;
    public bool freeze;
    public bool dashing;
    public bool wedgeGrabing;
    public bool swinging;
    public bool inHop;
    public bool accelRail;
    public bool grinding;
    public bool inAir;

    [Header("Components")]
    [SerializeField] protected Transform cam;
    public Vector3 rotAdjustPos;
    private PlayerStats ps;
    private InputHandler ih;
    private FreeFall ff;
    private SlideRoll sr;
    private PlayerColliderManager cm;
    private PlayerCam pc;
    private PlayerMovement pm;
    private Rigidbody rb;
    
    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        ih = GetComponent<InputHandler>();
        ff = GetComponent<FreeFall>();
        sr = GetComponent<SlideRoll>();
        cm = GetComponent<PlayerColliderManager>();
        pm = GetComponent<PlayerMovement>();
        pc = cam.GetComponent<PlayerCam>();
    }


    private void Update()
    {
        StateMachine();
        RotAdjustPosition();
    }

    private void FixedUpdate()
    {
        StateHandler();
    }

    private void StateMachine()
    {
        //mode to freeze
        if (freeze)
        {
            state = EMovementState.freeze; // ##
            rb.linearVelocity = Vector3.zero;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to unlimited
        else if (unlimited)
        {
            state = EMovementState.unlimited;
        }
        //mode to Wedge Grabing
        else if (wedgeGrabing)
        {
            state = EMovementState.wedgegrabing;
            rb.linearDamping = airDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to Wedge Grabing
        else if (swinging)
        {
            state = EMovementState.swinging;
            rb.linearDamping = airDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = false;
        }
        else if (inHop)
        {
            state = EMovementState.inhop;
            rb.linearDamping = airDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to grinding
        else if (grinding)
        {
            state = EMovementState.grinding;
            rb.linearDamping = grindDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to grinding
        else if (accelRail)
        {
            state = EMovementState.accelrail;
            rb.linearDamping = groundDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to Dashing
        else if (dashing)
        {
            state = EMovementState.dashing;
            rb.linearDamping = airDrag;
            pm.moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running Up
        else if (wallRunningUp)
        {
            state = EMovementState.wallrunningup;
            rb.linearDamping = upRunDrag;
            pm.moveSpeed = vupSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running Down
        else if (wallRunningDown)
        {
            state = EMovementState.wallrunningdown;
            rb.linearDamping = downRunDrag;
            pm.moveSpeed = vdownSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Resist Down
        else if (wallResistDown)
        {
            state = EMovementState.wallresistdown;
            rb.linearDamping = downRunDrag;
            pm.moveSpeed = vdownSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running
        else if (wallRunning)
        {
            state = EMovementState.wallrunning;
            rb.linearDamping = wallRunDrag;
            pm.moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //Mode to Rolling
        else if (rolling)
        {
            state = EMovementState.rolling;
            rb.linearDamping = groundDrag;
            pm.moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //Mode to Slide
        else if (sliding)
        {
            state = EMovementState.sliding;
            if (sr.SlopeAngle() > pm.minSlopeAngle)
            {
                rb.linearDamping = airDrag;
            }
            else
            {
                rb.linearDamping = slideDrag;
            }

            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to standUp
        else if (standingUp && cm.grounded)
        {
            state = EMovementState.standingup;
            float tempPercent = ff.standUpRatio; // lerp mods from prone to walk
            float tempComp = 1 - tempPercent;
            rb.linearDamping = proneDrag * tempComp + groundDrag * tempPercent;
            pm.moveSpeed = proneSpeed * tempComp + defaultMovSpeed * tempPercent;
            pc.fwdLocked = true;
        }
        else if (ih.heldX && cm.grounded)
        {
            //Mode to prone
            state = EMovementState.prone;
            rb.linearDamping = proneDrag;
            pm.moveSpeed = proneSpeed;
            pc.fwdLocked = true;
        }
        //Mode to freefall
        else if (freeFalling)
        {
            //Mode to freefall
            state = EMovementState.freefall;
            rb.linearDamping = dragFF;
            pm.moveSpeed = defaultMovSpeed;
            pc.fwdLocked = false;
        }
        //Mode to running
        else if (cm.grounded && !pm.exitingSlope)
        {
            state = EMovementState.walking;
            rb.linearDamping = groundDrag;
            pm.moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //Mode to air
        else
        {
            state = EMovementState.air;
            rb.linearDamping = airDrag;
            pm.moveSpeed = defaultMovSpeed;
            inAir = true;
            pc.fwdLocked = true;
        }

        if (state != EMovementState.air)
            inAir = false;
    }

    private void RotAdjustPosition()// call able Position that shifts towards ground and center when tilted
    {
        float tempHeight = 1 - Mathf.Cos(Vector3.Angle(Vector3.up, transform.up) * Mathf.Deg2Rad);
        Vector3 tempFwd = Vector3.ProjectOnPlane(transform.up, Vector3.up);
        rotAdjustPos = transform.position + 0.5f * 0.99f * tempHeight * Vector3.down + cm.ActiveHeight / 2.01f * tempHeight * tempFwd + Vector3.up * cm.insideCheckDis;
    }

    private void StateHandler()
    {
        if (state == EMovementState.prone)
        {
            rb.AddForce(proneStickForce * Vector3.down, ForceMode.Force);//stick to ground force
        }
    }
}
