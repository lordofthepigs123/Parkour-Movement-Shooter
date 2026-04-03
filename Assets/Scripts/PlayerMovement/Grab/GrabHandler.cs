using UnityEngine;

public class GrabHandler : Attacher
{
    [Header("GrabHandler")]
    //purpose# : pull from detectors info to begin swing, grind - on ledge, rail, melee
    [SerializeField] float temp;
    
    [Header("Components")]
    [SerializeField] GrabEdgeDetector geFeet;
    [SerializeField] GrabEdgeDetector geCam;
    private StateManager sm;
    private PlayerColliderManager cm;
    private PlayerMovement pm;

    [Header("States")]
    public GrabState state;

    public enum GrabState
    {
        idle,
        exiting,
        entering,
        lockedSwing,
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sm = GetComponent<StateManager>();
        ih = GetComponent<InputHandler>();
        cm = GetComponent<PlayerColliderManager>();
        pm = GetComponent<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        StateHandler();
    }

    private void Update()
    {
        StateMachine();
    }

    private void StateMachine()
    {
        if (state == GrabState.idle)
        {
            Debug.DrawLine(geCam.edge.start + Vector3.one * 0.01f, geCam.edge.end, Color.darkOliveGreen);
            Debug.DrawLine(geFeet.edge.start, geFeet.edge.end + Vector3.one * 0.01f, Color.burlywood);
        }

        if (state == GrabState.exiting)
        {
            
        }

        if (state == GrabState.entering)
        {
            
        }

        if (state == GrabState.lockedSwing)
        {
            
        }
    }

    private void StateHandler()
    {
        if (state == GrabState.idle)
        {
            
        }

        if (state == GrabState.exiting)
        {
            
        }

        if (state == GrabState.entering)
        {
            
        }

        if (state == GrabState.lockedSwing)
        {
            
        }
    }
}
