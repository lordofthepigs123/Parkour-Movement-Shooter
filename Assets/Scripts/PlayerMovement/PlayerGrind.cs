using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerGrind : Attacher
{
    [Header("PlayerGrind")]
    [SerializeField] float grindSpeed;
    [SerializeField] float decelRate;
    [SerializeField] float playerAccel;
    [SerializeField] float accelTimeMult;
    [SerializeField] float heightOffset;
    [SerializeField] float adjustMult; // 0 - 1
    [SerializeField] float exitTime;
    [SerializeField] float jumpOffGrindForce;
    [SerializeField] public  float angMult;
    [SerializeField] float swingMaxDis;
    [SerializeField] float swingMinDis;
    [SerializeField] float exitSpdMult;
    [SerializeField] float exitBoost;
    [SerializeField] float swingingForce;
    [SerializeField] float extraProximityTime;
    [SerializeField] float enterVelWarpMult; // 1 or > 
    [SerializeField] float enterSwingTime;
    [SerializeField] float enterGrindTime;
    [SerializeField] float maxBoost;
    [SerializeField] float hopMovMult;
    [SerializeField] float hopPullForce;
    [SerializeField] float hopVertMult;
    [SerializeField] public float mouseDirStr;

    [SerializeField] float kSpring; // k value
    [SerializeField] float damper; // loss of energy
    [SerializeField] float massScale; // 
    [SerializeField] LayerMask grindable;
    [SerializeField] LayerMask isObstacle;
    private float exitTimer;
    private float enterSwingTimer;
    private float enterGrindTimer;
    private float hopTimer;
    private float accelTimer;
    [HideInInspector] public bool lockedWithRail;
    private bool SPready;
    public bool aboveRail;// triggered by RailDetectorHandler
    public bool curntDir;
    private Vector3 inputDir;
    private float3 tangentSpline;
    private Vector3 flatTangent;
    private float currentPos; // ratio t of spline
    private float3 posSpline;
    private Vector3 worldPos;
    [HideInInspector] public float3 upSpline;

    private Vector3 leftSpline;
    private Vector3 initialUp;
    public bool inProximity;// triggered by RailDetectorHandler
    private float extraProximityTimer;
    private bool semiProximity;
    [HideInInspector] public Vector3 triggerNormal;// obtained by RailDetectorHandler
    [HideInInspector] public Vector3 swingPoint;
    private RaycastHit hopPoint;

    [Header("Components")]
    public RailScript currentRailScript;// obtained by RailDetectorHandler
    public Collider currentRailCol;// obtained by RailDetectorHandler
    private StateManager sm;
    private PlayerColliderManager cm;
    private PlayerMovement pm;

    [Header("States")]
    public GrindState state;
    private GrindState delayedState;
    private GrindState lastState;
    public enum GrindState
    {
        idle,
        exiting,
        inHop,
        enterSwing,
        enterGrind,
        swinging,
        onRail,
        grindRail,
        slideRail
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sm = GetComponent<StateManager>();
        ih = GetComponent<InputHandler>();
        cm = GetComponent<PlayerColliderManager>();
        pm = GetComponent<PlayerMovement>();
    }
    /*
    public void HandleJump(InputAction.CallbackContext context)
    {
        jump = Convert.ToBoolean(context.ReadValue<float>());
    }
    public void HandleMovement(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        input.x = rawInput.x;
    }
    */
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
        //update last state
        if (state != delayedState)
        {
            lastState = delayedState;
            delayedState = state;
        }

        if (state == GrindState.idle)
        {
            if (!inProximity)
                return;
            if (!ih.heldE && !ih.keyE)
            {
                state = GrindState.inHop;
                sm.inHop = true;
                hopTimer = 0;
                extraProximityTimer = extraProximityTime;
                semiProximity = true;

                initialUp = triggerNormal;
                return;
            }
            checkEnterSwing();
            return;
        }

        if (state == GrindState.exiting)
        {
            lockedWithRail = false;

            if (ih.rkeySPACE && SPready)
            {
                SPready = false;
                ih.rkeySPACE = false;
                //Bonus SP force
                Debug.Log(jumpOffGrindForce * ih.acuSPACE + "sp boost");
                rb.AddForce(Vector3.up * jumpOffGrindForce * Mathf.Clamp(ih.acuSPACE, 0, 100), ForceMode.Impulse);//possibly add stats - ps.weight.Value / 60
            }
            exitChecks();
            return;
        }

        //bonus proximity time
        if (extraProximityTimer > 0)
        {
            extraProximityTimer -= Time.deltaTime;
            semiProximity = true;
        }
        else
            semiProximity = inProximity;

        if (state == GrindState.inHop)
        {
            lockedWithRail = true;
            hopTimer += Time.deltaTime;

            //check exit condition
            if (!semiProximity)
            {
                exitStandard();
                exitTimer = exitTime;
                return;
            }
            if ((ih.heldE || !ih.keyE) && !ih.activatedE && currentPos != 1 && currentPos != 0)
            {
                sm.inHop = false;
                checkEnterSwing();
                return;
            }

            if (ih.ikeySPACE)//Physics.Raycast(transform.position, -transform.up, out hopPoint, 1.5f, grindable)
            {
                ih.activatedSPACE = true;
                Debug.Log("hop up");
                railHopUp(1);

                exitStandard();
                exitTimer = exitTime / 2;
                return;
            }

            if (ih.ikeySPACE && hopTimer >= 0.14f)//
            {
                ih.activatedSPACE = true;
                Debug.Log("hop forward");
                railHopFwd(1.2f);

                exitStandard();
                exitTimer = exitTime / 2;
                return;
            }

            return;
        }

        if (state == GrindState.enterSwing)
        {
            lockedWithRail = true;

            if (ih.ikeySPACE && !ih.activatedSPACE) //ready up fast hop
            {
                removeJoint();
                state = GrindState.inHop;
                sm.inHop = true;
                hopTimer = 0;

                initialUp = triggerNormal;
                return;
            }

            if (enterSwingTimer > enterSwingTime)
            {
                joint.spring = kSpring;
                joint.damper = damper;
                state = GrindState.swinging;
            }
            else
                enterSwingTimer += Time.deltaTime;
            
            if (!semiProximity)
            {
                Debug.Log(semiProximity + " " + lockedWithRail);
                exitSwing();
                exitTimer = exitTime / 2;
            }

            checkExitSwing();
            return;
        }

        if (state == GrindState.enterGrind)
        {
            lockedWithRail = true;

            if (ih.ikeySPACE && !ih.activatedSPACE) //ready up fast hop
            {
                stepUp(toAboveRailDis().y);
                state = GrindState.inHop;
                sm.inHop = true;
                hopTimer = 0;

                initialUp = triggerNormal;
                return;
            }

            if (ih.keyE && !ih.activatedE) //drop off
            {
                ih.activatedE = true;
                exitRail();
                return;
            }

            if (!semiProximity)
            {
                exitStandard();
                exitTimer = exitTime / 2;
                return;
            } 

            lockOnCheck();

            if (enterGrindTimer > enterGrindTime) //lock once time up
            {
                lockRail();
            }
            else
                enterGrindTimer += Time.deltaTime;

            return;
        }

        if (state == GrindState.swinging)
        {
            Debug.DrawRay(transform.position,rb.GetAccumulatedForce() * 2, Color.magenta);
            checkExitSwing();
            return;
        }

        if (state == GrindState.onRail)
        {
            //exit
            if (ih.ikeySPACE && !ih.activatedSPACE)
            {
                ih.activatedSPACE = true;
                //jump force
                Vector3 jumpForce = transform.up * jumpOffGrindForce;
                rb.AddForce(jumpForce, ForceMode.Impulse);

                exitRail();
                return;
            }
            if ((ih.keyE && !ih.activatedE) || !aboveRail)
            {
                ih.activatedE = true;
                exitRail();
                return;
            }
            if (rb.linearVelocity.magnitude > grindSpeed)
            {
                sm.accelRail = false;
                sm.grinding = true;
                state = GrindState.grindRail;
                Debug.Log("enter grinder");
            }
            return;
        }

        if (state == GrindState.grindRail)
        {
            //exit
            if (ih.ikeySPACE && !ih.activatedSPACE)
            {
                ih.activatedSPACE = true;
                //jump force
                Vector3 jumpForce = transform.up * jumpOffGrindForce;
                rb.AddForce(jumpForce, ForceMode.Impulse);

                exitRail();
                return;
            }
            if ((ih.keyE && !ih.activatedE) || !aboveRail)
            {
                ih.activatedE = true;
                exitRail();
                return;
            }
            if (Vector3.Dot(tangentSpline, inputDir) <= 0 && rb.linearVelocity.magnitude < grindSpeed * 0.8f)
            {
                //Debug.Log("switch to onRail");
                sm.grinding = false;
                sm.accelRail = true;
                state = GrindState.onRail;
            }
            return;
        }

        if (state == GrindState.slideRail)
        {

            return;
        }
    }

    private void StateHandler()
    {
        //input direction
        inputDir = ih.planeInputDir(upSpline, false);

        if (state == GrindState.inHop)
        {
            hopMovement();
            return;
        }

        if (state == GrindState.enterGrind)
        {
            enterGrindMovement();
            return;
        }
        if (state == GrindState.enterSwing)
        {
            enterSwingMovement();
            return;
        }

        if (!inProximity)
            return;
        
        if (aboveRail) //if on the rail, restirct player along the rail
        {
            if (state == GrindState.onRail)
            {
                accelMovement();
                PlayerRailMovement();//restrictor
                return;
            }
            if (state == GrindState.grindRail)
            {
                grindRailMovement();
                PlayerRailMovement();//restrictor
                return;
            }
            if (state == GrindState.slideRail)
            {

                return;
            }
        }
        if (state == GrindState.swinging)
        {
            PlayerSwingMovement();
            Debug.DrawRay(rb.worldCenterOfMass, rb.linearVelocity / 3, Color.blue, 1);
            return;
        }
    }

    public (Vector3 tangentSpline, Vector3 upSpline, Vector3 leftSpline, Vector3 worldPos) getVarsRail(RailScript railScript)  //called only when trigger is in rail
    {
        //Calculating the local positions of the player's current position and next position
        currentPos = railScript.CalculateTargetRailPoint(transform.position, out swingPoint);
        SplineUtility.Evaluate(railScript.railSpline.Spline, currentPos, out posSpline, out tangentSpline, out upSpline);
        tangentSpline = railScript.transform.rotation * tangentSpline;
        upSpline = railScript.transform.rotation * upSpline;
        worldPos = railScript.LocalToWorldConversion(posSpline);//Converting the local positions into world positions.
        tangentSpline = math.normalize(tangentSpline);
        Debug.DrawRay(worldPos, tangentSpline * 4, Color.green,2);
        upSpline = math.normalize(upSpline);
        Debug.DrawRay(swingPoint,upSpline * 2, Color.darkRed,2);
        leftSpline = Vector3.Cross(upSpline, tangentSpline);

        //Calculate the direction the player is going down the rail
        curntDir = Vector3.Dot(tangentSpline, rb.linearVelocity.normalized) < 0;
        tangentSpline *= curntDir ? -1 : 1;//set tangentSpline to fwd dir
        flatTangent = Vector3.ProjectOnPlane(tangentSpline, Vector3.up).normalized;//create a projected tangent dir
        //Debug.DrawRay(transform.position, tangentSpline * 2, Color.cyan);
        return (tangentSpline, upSpline, leftSpline, worldPos);
    }

    private void PlayerRailMovement()
    {
        //addjust on curve force
        float angleBetween = Vector3.Angle(tangentSpline, rb.linearVelocity);//angle between tangent and vel
        float chord = rb.linearVelocity.magnitude / 2;//chord length
        float radius = chord / Mathf.Sin(angleBetween * Mathf.Deg2Rad);//find radius of circular motion
        Vector3 planeNorml = Vector3.Cross(tangentSpline, rb.linearVelocity).normalized;
        //Debug.DrawRay(transform.position, planeNorml , Color.black, 2);
        Vector3 accDir = Vector3.Cross(tangentSpline, planeNorml).normalized;
        Vector3 tangentVel = Vector3.Project(rb.linearVelocity, tangentSpline);
        Vector3 centripetalForce = rb.mass * accDir * (tangentVel.sqrMagnitude / radius) / Time.fixedDeltaTime;//ac = v²/r

        //addjust to center force
        Vector3 adjustDist = toAboveRailDis();
        adjustDist = Vector3.ProjectOnPlane(adjustDist, tangentSpline);//remove forward/backward error
        Vector3 tempVel = Vector3.Project(rb.linearVelocity, adjustDist.normalized);
        Vector3 adjustForce = (adjustDist - tempVel * Time.fixedDeltaTime) * adjustMult * rb.mass;
        /*
        if (Vector3.Dot(tempVel, adjustForce) > 0)//subtract from adjust force existing velocity
        {
            if (tempVel.magnitude > adjustForce.magnitude)
                adjustForce = Vector3.zero;
            else
                adjustForce -= tempVel;
        }
        */

        //net force
        Vector3 netForce = centripetalForce + adjustForce;

        //subtract centrepetal force
        if (Vector3.Dot(adjustForce, accDir) > 0)
        {
            Vector3 negative = Vector3.Project(adjustForce, accDir);
            if (negative.magnitude < centripetalForce.magnitude)
            {
                netForce -= negative;
            }
            else
            {
                netForce = adjustForce;
            }
        }

        //add force
        if (!float.IsNaN(netForce.x))
            rb.AddForce(netForce, ForceMode.Force);
    }

    private Vector3 toAboveRailDis()
    {
        RaycastHit hitPoint;
        if (Physics.Raycast(worldPos + transform.up * heightOffset, -transform.up, out hitPoint, heightOffset, grindable))//possibly change not transform#
            return hitPoint.point - transform.position;
        return worldPos - transform.position;
    }

    private void accelMovement()
    {
        //as long as not input dir backwards keep grinding
        float tempDot = Vector3.Dot(tangentSpline, inputDir);
        if (tempDot <= 0)
            accelTimer = 0;
        else
            accelTimer += Time.fixedDeltaTime;
        //Debug.Log(tangentSpline);
            float tempMult = tempDot < 0 ? decelRate : playerAccel; // decelerating or accelerating?
        Vector3 applyForce = tempDot * tempMult * tangentSpline * (1 + accelTimer * accelTimeMult);
        rb.AddForce(applyForce, ForceMode.Force);
        //Debug.DrawRay(transform.position, flatTangent * 2, Color.blue);
        //Debug.DrawRay(transform.position, tangentSpline * 2, Color.green);
        //possibly gravity force
        //Debug.Log(rb.GetAccumulatedForce().magnitude);
    }

    private void grindRailMovement()
    {
        //as long as not input dir backwards keep grinding
        float tempDot = Vector3.Dot(tangentSpline, inputDir);
        if (tempDot < 0)
        {
            rb.AddForce(tempDot * tangentSpline * decelRate);
        }
        else if (rb.linearVelocity.magnitude < grindSpeed)
        {
            //Debug.Log(rb.velocity.magnitude);
            rb.AddForce(tangentSpline * (grindSpeed - rb.linearVelocity.magnitude), ForceMode.Force);
            //Debug.Log((rb.velocity.normalized * (grindSpeed - rb.velocity.magnitude)).magnitude);
        }

        //possibly gravity force
    }

    private void CalculateAndSetRailPosition()
    {
        //This is going to be the world position of where the player is going to start on the rail.
        //The 0 to 1 value of the player's position on the spline. We also get the world position of where that
        //point is.
        getVarsRail(currentRailScript);
        currentPos = currentRailScript.CalculateTargetRailPoint(transform.position, out swingPoint);
        //elapsedTime = timeForFullSpline * normalisedTime;
        //Multiply the full time for the spline by the normalised time to get elapsed time. This will be used in
        //the movement code.

        //Spline evaluate takes the 0 to 1 normalised time above, 
        //and uses it to give you a local position, a tangent (forward), and up
        float3 pos, forward, up;
        SplineUtility.Evaluate(currentRailScript.railSpline.Spline, currentPos, out pos, out forward, out up);
        //Set player's initial position on the rail before starting the movement code.
        RaycastHit hitPoint;
        Physics.Raycast(swingPoint + transform.up * heightOffset, -transform.up, out hitPoint, heightOffset, grindable);
        transform.position = hitPoint.point;
    }

    private void lockRail()
    {
        aboveRail = true;
        sm.accelRail = true;
        state = GrindState.onRail;
        cm.enableCol(false, currentRailCol);//disable collisions between player and rail
        //set velocity projected and warped along tangent
        Vector3 tangentVel = Vector3.Project(rb.linearVelocity, tangentSpline);
        float3 desiVel = tangentVel + tangentVel.normalized * Mathf.Pow(rb.linearVelocity.magnitude - tangentVel.magnitude, 1 / enterVelWarpMult); // add part of lost velocity to tangent vel

        if (float.IsNaN(desiVel.x))
            desiVel.x = 0;
        if (float.IsNaN(desiVel.y))
            desiVel.y = 0;
        if (float.IsNaN(desiVel.z))
            desiVel.z = 0;

        rb.linearVelocity = desiVel;
        CalculateAndSetRailPosition();
    }

    private void exitChecks()
    {
        if (exitTimer > exitTime)
        {
            pm.exitingSlope = false;
            state = GrindState.idle;
        }
        else
            exitTimer += Time.deltaTime;
    }

    private void enterRail()
    {
        Debug.Log("entering Rail");
        state = GrindState.enterGrind;
        enterGrindTimer = 0;

        stepUp(toAboveRailDis().y);
    }

    private void lockOnCheck()
    {
        Vector3 tempDis = toAboveRailDis();
        tempDis = new Vector3(tempDis.x, tempDis.y * 0.5f, tempDis.z); //strech distance to increase y bias
        if (tempDis.magnitude < 0.2f)
        {
            lockRail();
        }
    }

    private void exitRail()
    {
        //Set aboveRail to false, clear the rail script, and push the player off the rail.
        Debug.Log("exitRail");
        state = GrindState.exiting;
        aboveRail = false;
        sm.grinding = false;
        sm.accelRail = false;
        pm.exitingSlope = true;
        SPready = true;

        float tempAng = Mathf.Clamp(Vector3.Angle(rb.linearVelocity,inputDir),0,90);// returns dif in degrees, reduce if towards vel
        rb.AddForce(inputDir * (1 + Mathf.Sin(tempAng * Mathf.Deg2Rad) / rb.linearVelocity.magnitude) / (1 + 1 / rb.linearVelocity.magnitude) * exitBoost, ForceMode.Impulse);//Exit force
        exitTimer = 0;
    }

    private void exitStandard()
    {
        //Set aboveRail to false, clear the rail script, and push the player off the rail.
        Debug.Log("exit Standard");
        state = GrindState.exiting;
        aboveRail = false;
        sm.grinding = false;
        sm.accelRail = false;
        sm.swinging = false;
        sm.inHop = false;
        pm.exitingSlope = true;

        exitTimer = 0;
    }
    private void checkEnterSwing()
    {
        //search for swingable when E key
        Vector3 tempDir;
        currentRailScript.CalculateTargetRailPoint(transform.position, out swingPoint); // evaluate joint location

        tempDir = swingPoint - transform.position;
        RaycastHit tempHit;
        if (Physics.Raycast(transform.position, tempDir, out tempHit, tempDir.magnitude, isObstacle | grindable)) //do swingable axis and angle check #
        {
            if (tempHit.transform.gameObject.layer == isObstacle)
                return;
            getVarsRail(currentRailScript);
            if (Vector3.Angle(tempHit.normal, upSpline) <= 90 && Vector3.Angle(Vector3.ProjectOnPlane(rb.linearVelocity, upSpline), tangentSpline) < 40)//if ontop and following railo dir skip to grind
            {
                ih.activatedE = true;
                enterRail();
                return;
            }

            if (currentPos != 1 && currentPos != 0)
            {
                ih.activatedE = true;
                Debug.DrawRay(swingPoint, tangentSpline * 4, Color.magenta, 5);
                Debug.DrawRay(swingPoint, rb.linearVelocity * 4, Color.greenYellow, 5);
                
                enterSwing();
            }
        }
    }

    private void enterSwing()
    {
        Debug.Log("entering Swing");
        state = GrindState.enterSwing;
        sm.swinging = true;

        enterSwingTimer = 0;
        extraProximityTimer = extraProximityTime;
        semiProximity = true;

        createJoint(this.gameObject, swingPoint, swingMaxDis, swingMinDis, massScale);
    }

    private void transitionToGrind()
    {
        enterRail();
        sm.swinging = false;
        removeJoint();
    }

    private void stepUp(float upY)
    {
        float inertia = 2 * Physics.gravity.magnitude * (upY + 0.1f);//2as
        Vector3 upForce = rb.mass * (Mathf.Abs(inertia) / inertia * Mathf.Pow(Mathf.Abs(inertia), 0.5f) - rb.linearVelocity.y) / Time.fixedDeltaTime * Vector3.up;//v² = u² + 2as
        if (!float.IsNaN(upForce.y))
            rb.AddForce(upForce);
        //Debug.Log(upForce.y + ", " + upY);
    }

    private void enterGrindMovement()
    {
        Vector3 tempDis = Vector3.Project(toAboveRailDis(), leftSpline);
        float dotVel = Vector3.Dot(rb.linearVelocity, tempDis.normalized);
        float remainTime = enterGrindTime - enterGrindTimer;
        //compare if velocity is enough to reach pos xy
        if (tempDis.magnitude > dotVel * remainTime)
        {
            //apply adjustment force xy
            Vector3 adjForce = rb.mass * (tempDis / remainTime - tempDis.normalized * dotVel) / Time.fixedDeltaTime;
            rb.AddForce(adjForce);
        }

    }

    private void enterSwingMovement()
    {
        //lerp function to increase sping strength
        joint.spring = kSpring * enterSwingTimer / enterSwingTime;
        joint.damper = damper * enterSwingTimer / enterSwingTime;

    }

    private void PlayerSwingMovement()
    {
        Vector3 radius = swingPoint - rb.worldCenterOfMass;

        //gravity counter balance
        Vector3 counterAcc = Vector2.zero;
        float tempAng = Vector3.Angle(radius, Vector3.up); // determine players distance from Down in degrees
        float tempAcc = Mathf.Sin(tempAng * Mathf.Deg2Rad) * Physics.gravity.magnitude / 2;
        if (tempAcc > 0)
            counterAcc = Vector3.up * tempAcc;
        //forward back movement from inputs
        Vector3 tangent = Quaternion.AngleAxis(90, tangentSpline) * radius;
        Vector3 tangentialAcc = Vector3.ProjectOnPlane(inputDir, transform.up).normalized * swingingForce;
        Debug.DrawRay(rb.worldCenterOfMass, tangentialAcc, Color.green);
        Vector3 applyForce = (tangentialAcc + counterAcc) * rb.mass;
        rb.AddForce(applyForce, ForceMode.Force);

    }

    private void checkExitSwing()
    {
        //exit when SPACE key
        if (ih.ikeySPACE)
        {
            ih.activatedSPACE = true;
            exitSwing();
            return;
        }

        if (!ih.keyE || ih.activatedE)
            return;
        ih.activatedE = true;
        if (semiProximity && Physics.Raycast(transform.position, Vector3.down, 0.9f, grindable)) //allow grinding once above rail
        {
            transitionToGrind();
            return;
        }

        exitSwing();
    }

    private void exitSwing()
    {
        Debug.Log("exiting Swing");
        state = GrindState.exiting;
        sm.swinging = false;
        pm.exitingSlope = true;
        removeJoint();
        SPready = true;
        //add boost that is semi proportional to velocity but curves out to maxboost
        float applyMag = (float)Math.Pow(1 / (1 + Math.Pow(1 + 1 / maxBoost, -rb.linearVelocity.magnitude)) - 0.5f, 0.5f) * maxBoost;
        rb.AddForce(rb.linearVelocity.normalized * applyMag, ForceMode.Impulse);//Exit force   #
        Debug.Log(applyMag + "base");

        exitTimer = 0;//Exiting time
    }

    private void hopMovement()
    {
        Vector3 centripetal = -triggerNormal * hopPullForce;
        float dotInput = Vector3.Dot(centripetal.normalized, ih.baseInputDir);
        if (dotInput < 0)
        {
            centripetal *= 1 + dotInput;
        }
        rb.AddForce(centripetal);//slight centiripetal
    }

    private void railHopUp(float strengtMult)
    {
        Vector3 tempCross = Vector3.Cross(rb.linearVelocity, triggerNormal);
        float tempAngle = Vector3.SignedAngle(rb.linearVelocity, triggerNormal, tempCross);
        // gives vector 90 degree above velocity in similar dir as point normal
        Vector3 velocityNormal = Quaternion.AngleAxis(90 * tempAngle / Math.Abs(tempAngle), tempCross) * rb.linearVelocity;
        //Debug.DrawRay(transform.position, velocityNormal.normalized * 2, Color.red, 2);
        //Debug.DrawRay(transform.position,  rb.velocity.normalized * 2, Color.blue, 2);

        Vector3 applyForce = Vector3.zero;
        Vector3 applyDir = ih.planeInputDir(velocityNormal.normalized, false) + initialUp * 2f;
        if (Vector3.Dot(rb.linearVelocity, triggerNormal) < 0)//if there is velocity countering intended dir
            applyForce -= Vector3.Project(rb.linearVelocity, triggerNormal) * 0.7f;//apply partially negating force
        Vector3 newVel = rb.linearVelocity + applyForce;
        if (newVel.y < 0)
            applyForce += Vector3.up * newVel.y * 0.7f;
        applyForce += applyDir.normalized * hopVertMult * strengtMult;// + adjust force for existing velocity 
        rb.AddForce(applyForce, ForceMode.Impulse);
        Debug.DrawRay(rb.worldCenterOfMass, applyForce, Color.black, 3);
    }

    private void railHopFwd(float strengtMult)
     {
        Vector3 tempCross = Vector3.Cross(rb.linearVelocity, triggerNormal);
        float tempAngle = Vector3.SignedAngle(rb.linearVelocity, triggerNormal, tempCross);
        // gives vector 90 degree above velocity in similar dir as point normal
        Vector3 velocityNormal = Quaternion.AngleAxis(90 * tempAngle / Math.Abs(tempAngle), tempCross) * rb.linearVelocity;

        Vector3 applyForce = Vector3.zero;
        Vector3 applyDir = ih.planeInputDir(velocityNormal.normalized,false) + Vector3.Dot(rb.linearVelocity.normalized, triggerNormal) * triggerNormal * 2;
        Vector3 projectVel = -Vector3.Project(rb.linearVelocity, triggerNormal);
        if (Vector3.Dot(rb.linearVelocity, triggerNormal) < 0)//if there is velocity countering intended dir
            applyForce += projectVel * 0.7f;//apply partially negating force
        Vector3 newVel = rb.linearVelocity + applyForce;
        if (newVel.y < 0)
            applyForce -= Vector3.down * newVel.y * 0.7f;
        applyForce += applyDir.normalized * hopVertMult * strengtMult;// + adjust force for existing velocity 
        rb.AddForce(applyForce, ForceMode.Impulse);
        Debug.DrawRay(rb.worldCenterOfMass, applyForce, Color.blue, 3);
    }
}