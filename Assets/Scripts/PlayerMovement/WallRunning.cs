using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Variables")]
    [Header("Conditions")]
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float wallCheckDistance;
    [SerializeField] float enterMaximumVel;
    [SerializeField] float maximumPostiVel;
    [SerializeField] float minStateVel;
    [SerializeField] float stateBoundAng;
    [SerializeField] float stateBoundBuffers;
    [Header("Constants")]
    [SerializeField] float enterTime;//bounce time
    [SerializeField] float gravityCounterForce;
    [SerializeField] float wallStickForce; // stick when 90 degrees
    [SerializeField] float stickAngleMult; // base add of wallStickForce
    [SerializeField] float minAngleDif;
    [SerializeField] float fullAngMult;
    [SerializeField] float lesserAngMult;
    [SerializeField] float wallRunStrengthMax; // new # make mult depend off wall normal angle - consider "falling off" angled wall when 0 strength
    [SerializeField] float maxStrengthDecay; // max decay
    [SerializeField] float sideStrengthDecay; // minimum input decay
    [SerializeField] float strengthRegen;
    [SerializeField] float strengthFuncBase;
    [SerializeField] float sideToUpThold; // 0 -
    [SerializeField] float forceUpMult; //master up force
    [SerializeField] float forceDownMult; //master down force
    [SerializeField] float forceHoriMult; //master side force
    [SerializeField] float wallHoriStrech; //decrease vert mult dot, increase hori dot  1 -
    [SerializeField] float bevelGrav; // bevel gravity effective area 1 - 
    [SerializeField] float uniDragPow;
    [SerializeField] float dragUpWall;// resistance -
    [SerializeField] float dragHoriWall;
    [SerializeField] float dragDownMult;
    [SerializeField] float dragMinimumMult;

    [Header("Impulses")]
    [SerializeField] float forceRemoverMod;
    [SerializeField] float lowVelChainMod;
    [SerializeField] float lowVelBoostMax;
    [SerializeField] float wallJumpUpForce;
    [SerializeField] float wallJumpOffForce;
    [SerializeField] float wallJumpAmount;
    [SerializeField] float neutralJumpMult; // 0 - 1
    [SerializeField] float enterBoostVert;
    [SerializeField] float enterBoosthori;
    [SerializeField] float enterUpMult;
    [SerializeField] float velocityRemoverMod;
    [Header("other")]
    [SerializeField] float maxCamTilt;
    public float exitWallTime;
    [HideInInspector] public float exitWallTimer;
    [SerializeField] bool useGravity;
    public bool bounce;

    [SerializeField] private float wallRunStrength;
    [SerializeField] private float overallStrengthDecay;
    [SerializeField] private float resultingStrengthMult;
    private float enterTimer;
    private float jumpRemaining;
    private float wallDot;
    private float inputDot;

    private float oriDot;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private RaycastHit forwardWallHit;
    private RaycastHit backWallHit;

    private Vector3 inputDir;
    private Vector3 wallInputDir;
    [HideInInspector] public Vector3 _wallNormal;
    private Vector3 wallVelocity;
    private Vector3 horizontalWV;

    private bool wallForward;
    private bool wallBack;
    private bool wallLeft;
    private bool wallRight;
    private bool SPvertical;
    private bool SPhorizontal;
    private bool hittingWall;
    private bool strengthRegenable;

    [Header("Components")]
    [SerializeField] Transform orientation;
    [SerializeField] ComboCounter cc;
    [SerializeField] PlayerCam cam;

    private Collider currentWall;
    private Collider lastWall;

    private PlayerStats ps;
    private StateManager sm;
    private Rigidbody rb;
    private Wedge wedge;
    private InputHandler ih;
    private PlayerColliderManager cm;
    private MainRagdollHandeler rh;

    public bool proximWall;
    public WallState state;
    public bool exitingWall;

    public enum WallState
    {
        idle,
        neutral,
        upRunning,
        downRunning,
        horizontalRunning,
        exitingWall
    }

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        sm = GetComponent<StateManager>();
        wedge = GetComponent<Wedge>();
        ih = GetComponent<InputHandler>();
        cm = GetComponent<PlayerColliderManager>();
        rh = GetComponent<MainRagdollHandeler>();
    }

    private void Update()
    {
        CheckForWall();
        if (proximWall)
            wallVars();
        StateMachine();

        resultingStrengthMult = 2 / (1 + Mathf.Pow(strengthFuncBase, -wallRunStrength)) - 1; // 2 / (1 + c ^ -x) - 1
    }

    private void FixedUpdate()
    {
        if (state == WallState.idle || state == WallState.exitingWall)
            return;

        rb.useGravity = useGravity;

        if (state == WallState.upRunning || state == WallState.downRunning || state == WallState.horizontalRunning)
            wallRunMovement();
    }

    private void LateUpdate()
    {
        StrengthMultCalc();
    }

    private void CheckForWall()
    {
        Vector3 WallCheckPos = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z); //feet # change to feet colider

        wallForward = Physics.Raycast(WallCheckPos, orientation.forward, out forwardWallHit, wallCheckDistance, whatIsGround);
        wallBack = Physics.Raycast(WallCheckPos, -orientation.forward, out backWallHit, wallCheckDistance, whatIsGround);
        wallRight = Physics.Raycast(WallCheckPos, orientation.right, out rightWallHit, wallCheckDistance, whatIsGround);
        wallLeft = Physics.Raycast(WallCheckPos, -orientation.right, out leftWallHit, wallCheckDistance, whatIsGround);

        hittingWall = wallForward || wallBack || wallRight || wallLeft;

        if (hittingWall)
        {
            proximWall = true;

            float orientationDif = 2;
            Vector3 tempNormal = Vector3.zero;
            List<Vector3> normals = new List<Vector3>();
            Collider tempCols = null;
            List<Collider> cols = new List<Collider>();
            if (wallForward)
            {
                normals.Add(forwardWallHit.normal);
                cols.Add(forwardWallHit.collider);
            }
            if (wallBack)
            {
                normals.Add(backWallHit.normal);
                cols.Add(backWallHit.collider);
            }
            if (wallRight)
            {
                normals.Add(rightWallHit.normal);
                cols.Add(rightWallHit.collider);
            }
            if (wallLeft)
            {
                normals.Add(leftWallHit.normal);
                cols.Add(leftWallHit.collider);
            }

            for (int i = 0; i < normals.Count; i++)//check for case if multiple walls at once
            {
                float tempDif = Mathf.Abs(Vector3.Dot(orientation.forward, normals[i]));//the closer to 0 the more perpendicular
                //Debug.Log(tempDif);
                if (tempDif < orientationDif)
                {
                    orientationDif = tempDif;
                    tempNormal = normals[i];
                    tempCols = cols[i];
                }
            }
            _wallNormal = tempNormal;
            currentWall = tempCols;
            return;
        }
        else if (cm.touchingWall)
        {
            proximWall = true;
            _wallNormal = cm.wallNormal;
            currentWall = cm.wallCollider;
            return;
        }
        else
        {
            proximWall = false;
        }
    }
    private void wallVars()
    {
        //get vars
        wallInputDir = ih.planeInputDir(_wallNormal, false);//inputs projected on wall

        wallDot = Vector3.Dot(_wallNormal, Vector3.up); // higher angle larger drain
        inputDot = Vector3.Dot(inputDir, Vector3.ProjectOnPlane(_wallNormal, Vector3.up)); // dot of input to downWall
        float velDot = Vector3.Dot(orientation.forward, wallVelocity);
        oriDot = Vector3.Dot(wallVelocity.normalized, cm.tangentWall) * Mathf.Abs(velDot) / velDot; // dot of input to wall velocity

        //Debug.DrawRay(transform.position, wallInputDir * 5, Color.red);
        //Debug.DrawRay(transform.position, cm.upWall * 4, Color.blueViolet);
    }

    private void StateMachine()
    {
        //reset counts
        if (cm.grounded || sm.wedgeGrabing)
        {
            jumpRemaining = wallJumpAmount;
            lastWall = null;
        }

        //get inputs direction
        inputDir = ih.baseInputDir;
        
        if (state == WallState.neutral)
        {
            if (ih.ikeySPACE && !ih.activatedSPACE && hittingWall) // check inputs and if feet in proximity raycast#
            {
                ih.activatedSPACE = true;
                wallJump();
                return;
            }
            leaveCheck();
            return;
        }

        if (state == WallState.upRunning || state == WallState.downRunning || state == WallState.horizontalRunning)
        {
            //strength
            if (strengthRegenable)
                regenStrength();
            stateCheck();

            if (wallRunStrength > 0)
                wallRunStrength -= Time.deltaTime * overallStrengthDecay;
            else
            {
                wallRunStrength = 0;
            }

            //camera effect
            cam.DoTilt(-maxCamTilt * oriDot);


            if (ih.ikeySPACE && !ih.activatedSPACE && hittingWall) // check inputs and if feet in proximity raycast#
            {
                ih.activatedSPACE = true;
                wallJump();
                return;
            }

            if (state != WallState.downRunning)
                return;

            if (Vector3.Dot(wallInputDir, cm.upWall) < 0)
            {
                sm.wallRunningDown = true;
                sm.wallResistDown = false;
            }
            else
            {
                sm.wallResistDown = true;
                sm.wallRunningDown = false;
            }

        }
        else
        {
            Invoke(nameof(regenStrength), 0.2f);
        }

        if (state == WallState.exitingWall)
        {
            //additonal SP forces
            if (ih.rkeySPACE)
            {
                if (SPhorizontal)
                {
                    SPhorizontal = false;
                    ih.rkeySPACE = false;
                    //add force
                    if (rb.linearVelocity.y > 0)
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * (1 + ih.acuSPACE * 0.2f), rb.linearVelocity.z);
                    rb.AddForce(inputDir * ih.acuSPACE * 5, ForceMode.Impulse);
                }
                else if (SPvertical)
                {
                    SPvertical = false;
                    ih.rkeySPACE = false;
                    //add force
                    rb.linearVelocity *= 1 + ih.acuSPACE * 0.2f;

                }
            }

            ExitChecks();
        }
        else
        {
            if (cm.grounded || sm.sliding || sm.rolling || wedge.holding)
            {
                enterTimer = enterTime;
                return;
            }

            if (proximWall)
            {
                //delay on enter
                if (ih.ikeySPACE)
                {
                    bounce = true;
                }
                else if (!bounce && jumpRemaining > 0)
                {
                    stateCheck();
                }
                if (jumpRemaining > 0)
                {
                    enterTimer -= Time.deltaTime;
                    if (bounce && enterTimer < 0)
                    {
                        stateCheck();
                    }
                }
                else// no jumps left
                {
                    state = WallState.neutral;
                }
            }
            else
                enterTimer = enterTime;
        }
    }
    private void stateCheck()
    {
        //wall running state
        if (state == WallState.exitingWall)
            return;
        if (leaveCheck())
            return;

        wallVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, _wallNormal); // project last velocity onto wall

        Vector3 checkVelocity = wallVelocity + wallInputDir * minStateVel;
        float tempAngle = 90 - Vector3.Angle(checkVelocity, cm.upWall); // negative is down
        float extraAng = stateBoundAng;
        if (state == WallState.idle)
            extraAng = 0;
        
        float horiVel = Vector3.Project(checkVelocity, cm.tangentWall).magnitude; // if significant enough vel

        if (state != WallState.downRunning && tempAngle < -(extraAng + stateBoundBuffers))
            startDownRun();
        else if (state != WallState.upRunning && tempAngle > extraAng + stateBoundBuffers)
            startUpRun();
        else if (state != WallState.horizontalRunning && tempAngle > -extraAng + stateBoundBuffers && tempAngle < extraAng - stateBoundBuffers  && horiVel > minStateVel)
            startWallRun();
        return;
    }

    private bool leaveCheck()
    {
        if (!proximWall || cm.grounded || wedge.holding || wedge.exitingWedge || sm.rolling || sm.sliding)
        {
            state = WallState.exitingWall;
            return true;
        }
        return false;
    }

    private Vector3 stickAntiTorque()
    {
        //get mag torque and calc force from wall to counter
        Vector3 angVel = rb.angularVelocity;
        float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.up, angVel.normalized), cm.wallNormal, angVel.normalized) * (90 + minAngleDif) / 90 * Mathf.Deg2Rad;
        float magnit = Mathf.Cos(angle) * angVel.magnitude * fullAngMult;
        if (angle < 0)
            magnit *= lesserAngMult;
        if (magnit < 0)
            magnit = 0;
        Vector3 addVector = -_wallNormal * magnit;
        return addVector;
    }
    private void regenStrength()
    {
        if (((proximWall && !cm.grounded) || wallRunStrength >= wallRunStrengthMax) && !strengthRegenable)
            return;

        wallRunStrength += strengthRegen * Time.deltaTime;
        wallRunStrength = Mathf.Clamp(wallRunStrength, 0, wallRunStrengthMax);
    }

    private void StrengthMultCalc()
    {
        //use wall angle and player input dir angle
        float inputMult = 1 - inputDot;
        if (inputMult < sideToUpThold)
        {
            overallStrengthDecay = 0;
            strengthRegenable = true;
            return;
        }

        inputMult *= (inputMult - sideToUpThold) / (2 - sideToUpThold); // 1 - 0 convert
        inputMult = sideStrengthDecay * (1 - inputMult) + maxStrengthDecay * inputMult;  // lerp btw max and min
        strengthRegenable = false;
        overallStrengthDecay = (1 - wallDot) / 2 * inputMult;

    }
    private void ExitChecks()
    {
        exitingWall = true;
        bounce = false;
        stopWallRun();

        stopVerticalRun();

        if (exitWallTimer > 0)
            exitWallTimer -= Time.deltaTime;

        if (exitWallTimer <= 0)
        {
            exitingWall = false;
            state = WallState.idle;
        }
    }
    private void startWallRun()
    {
        state = WallState.horizontalRunning;

        sm.wallRunningDown = false;
        sm.wallResistDown = false;
        sm.wallRunningUp = false;
        sm.wallRunning = true;

        enterBoost();

        //if same wall penalty #
        lastWall = currentWall;

        //extra speed
        if (wallVelocity.y > 0)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 1.1f, rb.linearVelocity.z);
    }

    private void stopWallRun()
    {
        sm.wallRunning = false;
        cam.DoTilt(0f);
    }

    private void startUpRun()
    {
        state = WallState.upRunning;

        sm.wallRunningDown = false;
        sm.wallResistDown = false;
        sm.wallRunning = true;
        sm.wallRunningUp = true;

        //boost#
        enterBoost();

        //if same wall penalty#
        lastWall = currentWall;
        //camera effect#
    }

    private void startDownRun()
    {
        Debug.Log("ent downrun");

        state = WallState.downRunning;

        sm.wallRunningUp = false;
        sm.wallRunning = true;

        //camera effect#
    }
    private void wallRunMovement()
    {
        Vector3 netForce = Vector3.zero;

        //stick forces
        Vector3 stickForce = -_wallNormal * (1 + Vector3.Dot(Vector3.up, _wallNormal) * stickAngleMult * (1 - resultingStrengthMult)) * wallStickForce;
        if (rh.angularDif > minAngleDif)
            stickForce += stickAntiTorque();
        netForce += stickForce;


        // resistance
        Vector3 netResist;
        Vector3 velVert = Vector3.Project(rb.linearVelocity, cm.upWall);
        float tempDot = Vector3.Dot(wallInputDir, velVert.normalized);
        // resistance vertical
        float drag = dragUpWall;
        if (velVert.y < 0)
            drag = dragDownMult;
        float applyDragMult;
        float modApply = -Mathf.Pow(1 - resultingStrengthMult, uniDragPow);
        applyDragMult = (tempDot < 0) ? tempDot : modApply;
        netResist = velVert.normalized * drag * applyDragMult * (Mathf.Pow(velVert.magnitude, 2) + dragMinimumMult);

        Vector3 velHori = Vector3.Project(rb.linearVelocity, cm.tangentWall);
        tempDot = Vector3.Dot(wallInputDir, velHori.normalized);
        applyDragMult = (tempDot < 0) ? tempDot : modApply;
        // resistance horizontal
        netResist += velHori.normalized * dragHoriWall * applyDragMult * (velHori.magnitude + dragMinimumMult);

        netForce += netResist;
        // end resistance
        //Debug.DrawRay(transform.position, netResist, Color.blueViolet, 3);


        if (wallInputDir.y > -0.25f) //up input
        {
            //gravity reduce
            float yBase = (wallInputDir.y + 0.25f) / 1.25f;
            yBase = Mathf.Pow(yBase, 1 / bevelGrav); //bevel mult
            netForce += cm.upWall * resultingStrengthMult * gravityCounterForce * yBase;
        }

        // wall run force
        float _abs = Mathf.Abs(inputDot);
        float strechDot = _abs / inputDot * Mathf.Pow(_abs, wallHoriStrech);
        float totalMult = (1 - Mathf.Abs(strechDot)) * forceHoriMult;
        if (strechDot > 0)
            totalMult += strechDot * forceDownMult;
        else
            totalMult += -strechDot * forceUpMult;
        netForce += wallInputDir * resultingStrengthMult * totalMult;
        //Debug.DrawRay(transform.position, Vector3.ProjectOnPlane(wallInputDir * resultingStrengthMult * totalMult, _wallNormal) * 0.5f, Color.red, 3);
        //apply
        if (!float.IsNaN(netForce.x))
            rb.AddForce(netForce, ForceMode.Force);
    }
    /*
        private void wallRunningMovement()
        {
            Vector3 forceVec = Vector3.zero;
            //stick to wall force
            Vector3 stickForce = -_wallNormal * (1 + Vector3.Dot(transform.up, _wallNormal) * stickAngleMult) * wallStickForce;
            forceVec += stickForce ;

            float inputVelDot = Vector3.Dot(wallInputDir, wallVelocity.normalized);//are we traveling in same dir as projected inputs
            float wallVelDot = Vector3.Dot(inputDir, (wallVelocity.y * cm.upWall).normalized);//if negative input opposes vert velocity

            if ((inputDir - cm.tangentWall).magnitude > (inputDir + cm.tangentWall).magnitude)//make tangent of wall in same quad as inputDir
                cm.tangentWall = -cm.tangentWall;

            if (inputVelDot < 0)
            {
                //backward force against velocity
                forceVec += dragUpWall * horizontalWV.magnitude * inputDot * (0.1f + ps.BLNC.Value / 100) * -cm.tangentWall;
            }
            else
            {
                //forward force
                forceVec += wallRunForce * resultingStrengthMult * ((1 - inputDot) * cm.tangentWall + wallVelocity.normalized * inputDot);
                //Debug.DrawRay(transform.position, cm.tangentWall * wallRunForce * resultingStrengthMult * 5, Color.red);
            }

            if (wallVelDot < 0)//force against vertical velocity on wall: resistance against wall
                forceVec += dragUpWall * wallVelocity.y * Mathf.Abs(wallVelocity.y) * (0.1f + ps.BLNC.Value / 100) * wallVelDot * 2 * Vector3.down;

            forceVec += cm.upWall * gravityCounterForce;
            // Add force
            rb.AddForce(forceVec,ForceMode.Force);
        }

        private void upRunMovement()
        {
            //stick to wall force
            Vector3 stickForce = -_wallNormal * (1 + Vector3.Dot(transform.up, _wallNormal) * stickAngleMult) * wallStickForce;
            rb.AddForce(stickForce, ForceMode.Force);

            //force wall running up
            if (Vector3.Dot(wallInputDir, cm.upWall) > 0)
            {
                rb.AddForce(cm.upWall * Mathf.Clamp(forceUpMult * resultingStrengthMult, 0, float.PositiveInfinity), ForceMode.Force);
            }
            else
            {
                //downwards force against upward velocity on wall: resistance up wall
                rb.AddForce(Vector3.down * dragUpWall * Mathf.Pow(wallVelocity.y, 2) * (0.1f + ps.BLNC.Value / 100), ForceMode.Force);
            }
        }
        private void downRunMovement()
        {
            //stick to wall force
            Vector3 stickForce = -_wallNormal * (1 + Vector3.Dot(transform.up, _wallNormal) * stickAngleMult) * wallStickForce;
            rb.AddForce(stickForce, ForceMode.Force);

            if (Vector3.Dot(wallInputDir, cm.upWall) < 0)
            {
                //force wall running down
                sm.wallRunningDown = true;
                sm.wallResistDown = false;

                rb.AddForce(-cm.upWall * (0.25f + ps.SPD.Value / 100), ForceMode.Force);
            }
            else
            {
                sm.wallResistDown = true;
                sm.wallRunningDown = false;

                //upward force against fall on wall: resistance down wall
                rb.AddForce(cm.upWall * dragUpWall * Mathf.Pow(wallVelocity.y, 2) * (0.1f + ps.BLNC.Value / 100) * dragDownMult, ForceMode.Force);
                //Debug.Log(cm.upWall * dragUpWall * Mathf.Pow(wallVelocity.y, 2) + " yes");
            }
        }
        */
    private void stopVerticalRun()
    {
        sm.wallRunningDown = false;
        sm.wallResistDown = false;
        sm.wallRunningUp = false;
        sm.wallRunning = false;

        //reset camera effects#
    }

    public void wallJump()
    {
        SPhorizontal = true;//engage additional upward forces
        if (jumpRemaining > 0)
            jumpRemaining -= 1;
        state = WallState.exitingWall;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = Vector3.up * wallJumpUpForce + _wallNormal * wallJumpOffForce; //#
        //Debug.Log(forceToApply.magnitude);
        if (jumpRemaining > 0)
        {
            jumpRemaining -= 1;
            forceToApply *= neutralJumpMult;
        }
        //add force
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
    /*
        private void chainJump()
        {
            jumpRemaining -= 1;

            Vector3 netForce = Vector3.zero;
            netForce += wallInputDir * chainJumpMult;//base directional forces
            Vector3 tempVel = Vector3.ProjectOnPlane(rb.linearVelocity, wallInputDir);
            Vector3 delForce = tempVel.normalized / (1 + velocityRemoverMod / Mathf.Pow(tempVel.magnitude, 0.8f));
            netForce -= delForce;//remove some of other velocities
            netForce += wallInputDir * delForce.magnitude;//boost from removed velocity

            Vector3 tempProject = Vector3.Project(rb.linearVelocity, wallInputDir);
            float velMult = 0;// ranges from lowVelBoostMax to -1
            if (Vector3.Dot(tempProject.normalized, wallInputDir) < 0)
                velMult = Mathf.Pow(tempProject.magnitude, 0.5f) + 1;
            else if (tempProject.magnitude < maximumPostiVel)
                velMult = 1 / Mathf.Pow(lowVelChainMod, tempProject.magnitude) * lowVelBoostMax;
            else
                velMult = -1 / (1 + forceRemoverMod / Mathf.Pow(tempProject.magnitude - maximumPostiVel, 0.75f));
            netForce += wallInputDir * velMult;//bonus boost when low velocity or negative with quick fall off
            netForce += Vector3.up * chainJumpUpForce * (1 + Mathf.Clamp(Vector3.Dot(Vector3.up, tempProject), 0, 1) * velMult);//additional up force
            Debug.Log(tempProject.magnitude);

            //Add Force
            rb.AddForce(netForce, ForceMode.Impulse);
        }
        */

    private void enterBoost()
    {
        Vector3 netForce = Vector3.zero;

        Vector3 boostDir = ih.planeInputDir(_wallNormal, true);
        float boostMult = Mathf.Lerp(enterBoosthori, enterBoostVert, Mathf.Abs(inputDot));
        netForce += boostDir * boostMult;
        
        Vector3 tempVel = Vector3.ProjectOnPlane(rb.linearVelocity, boostDir);
        Vector3 delForce = tempVel.normalized / (1 + velocityRemoverMod / Mathf.Pow(tempVel.magnitude, 0.8f)) * boostDir.magnitude;
        netForce -= delForce;//remove some of other velocities
        netForce += boostDir * delForce.magnitude;//boost from removed velocity

        Vector3 tempProject = Vector3.Project(rb.linearVelocity, boostDir); // wall velocity
        float velMult;// ranges from lowVelBoostMax to -1
        if (Vector3.Dot(tempProject.normalized, boostDir) < 0)
            velMult = Mathf.Pow(tempProject.magnitude, 0.5f) + 1;
        else if (tempProject.magnitude < enterMaximumVel)
            velMult = 1 / Mathf.Pow(lowVelChainMod, tempProject.magnitude) * lowVelBoostMax;
        else
            velMult = -1 / (1 + forceRemoverMod / Mathf.Pow(tempProject.magnitude - enterMaximumVel, 0.75f));
        netForce += boostDir * velMult;//bonus boost when low velocity or negative with quick fall off
        netForce += cm.upWall * enterUpMult * (1 + Mathf.Clamp(Vector3.Dot(cm.upWall, tempProject), 0, 1) * velMult);//additional up force
        netForce *= resultingStrengthMult;
        Debug.Log("ent boost " + netForce.magnitude + state);

        //Add Force
        rb.AddForce(netForce, ForceMode.Impulse);
    }
}
