using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerStateMachine : StateManager<PlayerStateMachine.MovementState>
{
    [Header("Adjustable Variables")]
    [SerializeField] float groundDrag;
    [SerializeField] float slideDrag;
    [SerializeField] float wallRunDrag;
    [SerializeField] float upRunDrag;
    [SerializeField] float downRunDrag;
    [SerializeField] float grindDrag;
    [SerializeField] float airDrag;
    [SerializeField] float dragFF;
    [SerializeField] float movSpeed;
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

    public MovementState state;  // # remove

    public enum MovementState
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

    private void Awake()
    {
        CurrentState = States[MovementState.freeze];
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
            state = MovementState.freeze; // ##
            rb.linearVelocity = Vector3.zero;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
        }
        //mode to Wedge Grabing
        else if (wedgeGrabing)
        {
            state = MovementState.wedgegrabing;
            rb.linearDamping = airDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to Wedge Grabing
        else if (swinging)
        {
            state = MovementState.swinging;
            rb.linearDamping = airDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = false;
        }
        else if (inHop)
        {
            state = MovementState.inhop;
            rb.linearDamping = airDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to grinding
        else if (grinding)
        {
            state = MovementState.grinding;
            rb.linearDamping = grindDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to grinding
        else if (accelRail)
        {
            state = MovementState.accelrail;
            rb.linearDamping = groundDrag;
            pm.moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            rb.linearDamping = airDrag;
            pm.moveSpeed = movSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running Up
        else if (wallRunningUp)
        {
            state = MovementState.wallrunningup;
            rb.linearDamping = upRunDrag;
            pm.moveSpeed = vupSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running Down
        else if (wallRunningDown)
        {
            state = MovementState.wallrunningdown;
            rb.linearDamping = downRunDrag;
            pm.moveSpeed = vdownSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Resist Down
        else if (wallResistDown)
        {
            state = MovementState.wallresistdown;
            rb.linearDamping = downRunDrag;
            pm.moveSpeed = vdownSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running
        else if (wallRunning)
        {
            state = MovementState.wallrunning;
            rb.linearDamping = wallRunDrag;
            pm.moveSpeed = movSpeed;
            pc.fwdLocked = true;
        }
        //Mode to Rolling
        else if (rolling)
        {
            state = MovementState.rolling;
            rb.linearDamping = groundDrag;
            pm.moveSpeed = movSpeed;
            pc.fwdLocked = true;
        }
        //Mode to Slide
        else if (sliding)
        {
            state = MovementState.sliding;
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
            state = MovementState.standingup;
            float tempPercent = ff.standUpRatio; // lerp mods from prone to walk
            float tempComp = 1 - tempPercent;
            rb.linearDamping = proneDrag * tempComp + groundDrag * tempPercent;
            pm.moveSpeed = proneSpeed * tempComp + movSpeed * tempPercent;
            pc.fwdLocked = true;
        }
        else if (ih.heldX && cm.grounded)
        {
            //Mode to prone
            state = MovementState.prone;
            rb.linearDamping = proneDrag;
            pm.moveSpeed = proneSpeed;
            pc.fwdLocked = true;
        }
        //Mode to freefall
        else if (freeFalling)
        {
            //Mode to freefall
            state = MovementState.freefall;
            rb.linearDamping = dragFF;
            pm.moveSpeed = movSpeed;
            pc.fwdLocked = false;
        }
        //Mode to running
        else if (cm.grounded && !pm.exitingSlope)
        {
            state = MovementState.walking;
            rb.linearDamping = groundDrag;
            pm.moveSpeed = movSpeed;
            pc.fwdLocked = true;
        }
        //Mode to air
        else
        {
            state = MovementState.air;
            rb.linearDamping = airDrag;
            pm.moveSpeed = movSpeed;
            inAir = true;
            pc.fwdLocked = true;
        }

        if (state != MovementState.air)
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
        if (state == MovementState.prone)
        {
            rb.AddForce(proneStickForce * Vector3.down, ForceMode.Force);//stick to ground force
        }
    }
}
