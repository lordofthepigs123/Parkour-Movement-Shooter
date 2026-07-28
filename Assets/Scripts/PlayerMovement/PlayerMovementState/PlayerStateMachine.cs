using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerStateMachine : StateManager<PlayerStateMachine.EMovementState>
{
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
    public bool slidingOnSlope;
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
    public bool exitingSlope;
    public float standUpRatio;
    public float moveSpeed;

    [Header("Components")]
    [SerializeField] protected Transform cam;
    private MovementContext _context;
    private PlayerColliderManager cm;
    private PlayerCam pc;
    private InputHandler ih;
    private Rigidbody rb;
    
    private void Start()
    {
        _context = new MovementContext();

        rb = GetComponent<Rigidbody>();
        cm = GetComponent<PlayerColliderManager>();
        ih = GetComponent<InputHandler>();
        pc = cam.GetComponent<PlayerCam>();
    }


    private void Update()
    {
        StateMachine();
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
            CurrentStateKey = EMovementState.freeze; // ##
            rb.linearVelocity = Vector3.zero;
            moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to unlimited
        else if (unlimited)
        {
            CurrentStateKey = EMovementState.unlimited;
        }
        //mode to Wedge Grabing
        else if (wedgeGrabing)
        {
            CurrentStateKey = EMovementState.wedgegrabing;
            rb.linearDamping = airDrag;
            moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to Wedge Grabing
        else if (swinging)
        {
            CurrentStateKey = EMovementState.swinging;
            rb.linearDamping = airDrag;
            moveSpeed = 0;
            pc.fwdLocked = false;
        }
        else if (inHop)
        {
            CurrentStateKey = EMovementState.inhop;
            rb.linearDamping = airDrag;
            moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to grinding
        else if (grinding)
        {
            CurrentStateKey = EMovementState.grinding;
            rb.linearDamping = grindDrag;
            moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to grinding
        else if (accelRail)
        {
            CurrentStateKey = EMovementState.accelrail;
            rb.linearDamping = groundDrag;
            moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to Dashing
        else if (dashing)
        {
            CurrentStateKey = EMovementState.dashing;
            rb.linearDamping = airDrag;
            moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running Up
        else if (wallRunningUp)
        {
            CurrentStateKey = EMovementState.wallrunningup;
            rb.linearDamping = upRunDrag;
            moveSpeed = vupSpeed;
            pc.fwdLocked = true; 
        }
        //mode to Wall Running Down
        else if (wallRunningDown)
        {
            CurrentStateKey = EMovementState.wallrunningdown;
            rb.linearDamping = downRunDrag;
            moveSpeed = vdownSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Resist Down
        else if (wallResistDown)
        {
            CurrentStateKey = EMovementState.wallresistdown;
            rb.linearDamping = downRunDrag;
            moveSpeed = vdownSpeed;
            pc.fwdLocked = true;
        }
        //mode to Wall Running
        else if (wallRunning)
        {
            CurrentStateKey = EMovementState.wallrunning;
            rb.linearDamping = wallRunDrag;
            moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //Mode to Rolling
        else if (rolling)
        {
            CurrentStateKey = EMovementState.rolling;
            rb.linearDamping = groundDrag;
            moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //Mode to Slide
        else if (sliding)
        {
            CurrentStateKey = EMovementState.sliding;
            if (slidingOnSlope)
            {
                rb.linearDamping = airDrag;
            }
            else
            {
                rb.linearDamping = slideDrag;
            }

            moveSpeed = 0;
            pc.fwdLocked = true;
        }
        //mode to standUp
        else if (standingUp && cm.grounded)
        {
            CurrentStateKey = EMovementState.standingup;
            // lerp mods from prone to walk
            float tempComp = 1 - standUpRatio;
            rb.linearDamping = proneDrag * tempComp + groundDrag * standUpRatio;
            moveSpeed = proneSpeed * tempComp + defaultMovSpeed * standUpRatio;
            pc.fwdLocked = true;
        }
        else if (ih.heldX && cm.grounded)
        {
            //Mode to prone
            CurrentStateKey = EMovementState.prone;
            rb.linearDamping = proneDrag;
            moveSpeed = proneSpeed;
            pc.fwdLocked = true;
        }
        //Mode to freefall
        else if (freeFalling)
        {
            //Mode to freefall
            CurrentStateKey = EMovementState.freefall;
            rb.linearDamping = dragFF;
            moveSpeed = defaultMovSpeed;
            pc.fwdLocked = false;
        }
        //Mode to running
        else if (cm.grounded && !exitingSlope)
        {
            CurrentStateKey = EMovementState.walking;
            rb.linearDamping = groundDrag;
            moveSpeed = defaultMovSpeed;
            pc.fwdLocked = true;
        }
        //Mode to air
        else
        {
            CurrentStateKey = EMovementState.air;
            rb.linearDamping = airDrag;
            moveSpeed = defaultMovSpeed;
            inAir = true;
            pc.fwdLocked = true;
        }

        if (CurrentStateKey != EMovementState.air)
            inAir = false;
    }

    private void StateHandler()
    {
        if (CurrentStateKey == EMovementState.prone)
        {
            rb.AddForce(proneStickForce * Vector3.down, ForceMode.Force);//stick to ground force
        }
    }
}
