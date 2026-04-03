using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    [Header("PhysicsBody")]
    [SerializeField] protected float verticalLength;
    [SerializeField] protected float horizontalLength;
    [SerializeField] protected float depthLength;
    [SerializeField] protected float coefDrag;
    [SerializeField] protected float freefallMeander;//free horizontal mult
    [SerializeField] protected float freefallReduceUpMult;//free up mult
    [SerializeField] protected float horForceMult;
    [SerializeField] protected float fallForceMult;
    [SerializeField] protected Transform cam;
    [SerializeField] protected Transform orientation;
    [SerializeField] protected Transform playerBody;

    protected Quaternion desiRotation;
    protected Rigidbody rb;
    protected InputHandler ih;
    protected FreeFall ff;
    protected float angFriction;
    protected float spdMult_ang; // set in subclass script
    private Vector3 lastPoint;
    private Vector3 v_dir;
    private float area1;//front
    private float area2;//side
    private float area3;//top
    private Vector3 player_fwd;
    private Vector3 player_right;
    private Vector3 player_up;
    private Vector3 fplayer_fwd;
    private Vector3 fplayer_right;
    private Vector3 fplayer_up;
    private Vector3 air_reactF;

    protected void movementForces(float strength) // forces to point vector of up and vector of desi in same dir
    {
        ////%%%%%%% Note : Don't use playerbody as rotation is controlled by cam

        Quaternion swing, twist;
        Vector3 transUp = rb.rotation * Vector3.up;
        Vector3 transFwd = rb.rotation * Vector3.forward;
        Quaternion difQua = desiRotation * Quaternion.Inverse(rb.rotation);
        DecomposeSwingTwist(difQua, transUp, out swing, out twist);
        //Debug.DrawRay(rb.position, swing * Vector3.up * 2.5f, Color.magenta);
        //Debug.DrawRay(rb.position, twist * Vector3.up * 2.5f, Color.orange);

        Vector3 fullDesiDir = desiRotation * Vector3.up;
        Vector3 fullDesiRot = desiRotation * Vector3.forward;

        float fullTheta1 = Vector3.Angle(transUp, fullDesiDir) * Mathf.Deg2Rad;
        float fullTheta2 = Vector3.Angle(swing * transFwd, fullDesiRot) * Mathf.Deg2Rad;

        Quaternion lerpSwing = Quaternion.Lerp(Quaternion.identity, swing, 1 / fullTheta1 * spdMult_ang * Time.fixedDeltaTime * strength);// t has to be between 0 - 1
        Quaternion lerpTwist = Quaternion.Lerp(Quaternion.identity, twist, 1 / fullTheta2 * spdMult_ang * Time.fixedDeltaTime * strength);
        //Debug.Log(1 / w.magnitude + "time");
        Quaternion segmentRot = lerpTwist * lerpSwing * rb.rotation;
        Debug.DrawRay(rb.position, transUp * 2.9f, Color.blue);
        Debug.DrawRay(rb.position, transFwd * 1.4f, Color.blue);

        //Debug.DrawRay(rb.position, segmentRot * Vector3.up * 3f, Color.red);
        //Debug.DrawRay(rb.position, segmentRot * Vector3.forward * 1.5f, Color.red);

        Vector3 desiDir = segmentRot * Vector3.up;
        var x1 = Vector3.Cross(transUp, desiDir.normalized);
        float theta1 = Vector3.Angle(transUp, desiDir.normalized) * Mathf.Deg2Rad;//remaning swing angle
        Vector3 w1 = x1.normalized * fullTheta1 / Time.fixedDeltaTime * Mathf.Pow(spdMult_ang * strength, 0.3f); // d/t = ang v
        //Vector3 currentSwing = Vector3.Project(rb.angularVelocity, fullDesiRot);

        Vector3 desiRot = segmentRot * Vector3.forward;
        var x2 = Vector3.Cross(transFwd, desiRot.normalized);
        float theta2 = Vector3.Angle(lerpSwing * transFwd, desiRot) * Mathf.Deg2Rad;//remaning twist angle
        Vector3 w2 = x2.normalized * fullTheta2 / Time.fixedDeltaTime * Mathf.Pow(spdMult_ang * strength, 0.3f); // d/t = ang v
        //Vector3 currentTwist = Vector3.Project(rb.angularVelocity, fullDesiDir);

        Quaternion q = rb.rotation * rb.inertiaTensorRotation;

        Vector3 t1 = q * Vector3.Scale(rb.inertiaTensor, Quaternion.Inverse(q) * w1);//scale
        Vector3 t2 = q * Vector3.Scale(rb.inertiaTensor, Quaternion.Inverse(q) * w2);


        Vector3 t = t1 + t2;

        t -= Vector3.ProjectOnPlane(rb.angularVelocity, desiDir) + Vector3.ProjectOnPlane(rb.angularVelocity, desiDir);
        //t -= rb.angularVelocity;

        Vector3 tempCross = Vector3.Cross(desiDir, transUp);
        if (Vector3.ProjectOnPlane(rb.angularVelocity, tempCross).magnitude > fullTheta1 * Time.fixedDeltaTime)
            t -= Vector3.ProjectOnPlane(rb.angularVelocity, tempCross);

        tempCross = Vector3.Cross(desiRot, transFwd);
        if (Vector3.ProjectOnPlane(rb.angularVelocity, tempCross).magnitude > fullTheta2 * Time.fixedDeltaTime)
            t -= Vector3.ProjectOnPlane(rb.angularVelocity, tempCross);

        Debug.DrawRay(rb.position, fullDesiRot * 1.5f, Color.yellow);
        Debug.DrawRay(rb.position, fullDesiDir * 2.3f, Color.yellow);

        /*
        //determine torque required to move
        Vector3 desiDir = desiRotation * Vector3.up;
        var x = Vector3.Cross(playerBody.up, desiDir.normalized);
        float theta = Mathf.Asin(x.magnitude);
        Vector3 w = x.normalized * theta / Time.fixedDeltaTime; // d/t = ang v
        Quaternion q = playerBody.rotation * rb.inertiaTensorRotation;
        Vector3 t1 = q * Vector3.Scale(rb.inertiaTensor, Quaternion.Inverse(q) * w) * strength;
        Vector3 tempVel = Vector3.ProjectOnPlane(rb.angularVelocity, desiDir);
        float tempAng = Vector3.Angle(desiDir, playerBody.up) * Mathf.Deg2Rad;
        //Mathf.SmoothDampAngle(tempAng, 0, ref tempVel, 1);
        */

        //if (tempVel.magnitude > tempAng)//reverse force if velocity excedes distance remaning
        //    t -= tempVel;
        //add torque
        if (!float.IsNaN(t.x))
            rb.AddTorque(t, ForceMode.Force);
    }

    /*
        protected void pitchForces(float strength) // forces to allign the pitch(up down rotation) and yaw with desi
        {
            //determine torque to get desi angle
            Vector3 t2 = Vector3.Cross(playerBody.forward, desiRotation * Vector3.forward).normalized * Mathf.Sin(Vector3.Angle(playerBody.forward, desiRotation * Vector3.forward) * Mathf.Deg2Rad / 2) * spdMult_ang * strength;
            if (!float.IsNaN(t2.x))
                rb.AddTorque(t2, ForceMode.Force);
            //Debug.DrawRay(playerBody.position, playerBody.forward, Color.magenta);
            //Debug.DrawRay(playerBody.position, desiRotation * Vector3.forward, Color.orange);
        }
    */

    protected void resultingForces(float forceMult, float yPlaneVelInfluence, float xzBonusCons, float resistancePercent)
    {
        Vector3 v_fixed = new Vector3(rb.linearVelocity.x * yPlaneVelInfluence, rb.linearVelocity.y, rb.linearVelocity.z * yPlaneVelInfluence);
        v_dir = rb.linearVelocity.normalized;//velocity direction
        player_fwd = playerBody.forward;//get fwd normal of player against air
        if (Vector3.Dot(v_dir, playerBody.forward) < 0)
            player_fwd *= -1;
        player_right = playerBody.right;//get right normal of player against air
        if (Vector3.Dot(v_dir, playerBody.right) < 0)
            player_right *= -1;
        player_up = playerBody.up;//get up normal of player against air
        if (Vector3.Dot(v_dir, playerBody.up) < 0)
            player_up *= -1;
        areaCalc();

        //force vectors
        Vector3 applyForce;
        fplayer_fwd = -player_fwd * area1 * v_fixed.sqrMagnitude * forceMult;// normal + shear force(friction with air)
        fplayer_fwd -= Vector3.ProjectOnPlane(v_fixed, player_fwd).normalized * coefDrag * fplayer_fwd.magnitude;// Ff = coef * Fn
        fplayer_right = -player_right * area2 * v_fixed.sqrMagnitude * forceMult;
        fplayer_right -= Vector3.ProjectOnPlane(v_fixed, player_right).normalized * coefDrag * fplayer_right.magnitude;
        fplayer_up = -player_up * area3 * v_fixed.sqrMagnitude * forceMult;
        fplayer_up -= Vector3.ProjectOnPlane(v_fixed, player_up).normalized * coefDrag * fplayer_up.magnitude;
        air_reactF = fplayer_fwd + fplayer_right + fplayer_up; //air reaction force on player 
        Vector3 vertical_force = Vector3.zero;
        if (air_reactF.y < 0) //if react force is down
        {//if forced down
            air_reactF.y = -Mathf.Pow(-air_reactF.y, 0.8f);//reduce down force #
        }
        else
        {//if forced up
            vertical_force = Vector3.up * Mathf.Clamp(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude, 0, 5);//# increase up forces when move sideways substantialy
            if (ff.state == FreeFall.FallState.free)
            {
                // reduce area up resistance, reduce all meanandering
                air_reactF = new Vector3(air_reactF.x * freefallMeander, Mathf.Pow(air_reactF.y, 1 / freefallReduceUpMult) ,air_reactF.z * freefallMeander);
            }
            else
            {
                // mult on up force (fall faster or slower) & mult apply to horizontal
                air_reactF = new Vector3(air_reactF.x * horForceMult, air_reactF.y * fallForceMult ,air_reactF.z * horForceMult);
            }
        }
        
        Vector3 reactFlat = new Vector3(air_reactF.x, 0, air_reactF.z);
        float reactDot = Vector3.Dot(reactFlat, ih.baseInputDir);
        if (reactDot < 0) //if reducing vel in ih prevent vel loss
        {
            air_reactF -= Vector3.Project(reactFlat,ih.lastInputDir) * xzBonusCons;
        }


        applyForce = air_reactF + vertical_force;

        //AddForce
        rb.AddForce(applyForce, ForceMode.Force);
        Debug.DrawRay(transform.position, applyForce * 2, Color.darkOrange, 3);

        //resulting torque
        resistanceCalc(resistancePercent);
    }

    protected void areaCalc()//rework#
    {
        //calculate frontal area
        float angle_v = Vector3.Angle(-v_dir, playerBody.up) * Mathf.Deg2Rad;//angle between horizontal side and normal
        float angle_h = Vector3.Angle(-v_dir, playerBody.right) * Mathf.Deg2Rad;//angle between vertical side and normal
        float angle_f = Vector3.Angle(-v_dir, playerBody.forward) * Mathf.Deg2Rad;
        float side_v = verticalLength * Mathf.Sin(angle_v);//calc projected side length
        float side_h = horizontalLength * Mathf.Sin(angle_h);
        float side_f = depthLength * Mathf.Sin(angle_f);
        Vector3 tempUp = Vector3.ProjectOnPlane(playerBody.up, v_dir);//project sides of rhombus onto v_dir normal
        Vector3 tempRight = Vector3.ProjectOnPlane(playerBody.right, v_dir);
        Vector3 tempFwd = Vector3.ProjectOnPlane(playerBody.forward, v_dir);

        //frontal
        float tempAngle1 = Vector3.Angle(tempUp, tempRight) * Mathf.Deg2Rad;//angle of rhombus projection
        float tempHeight = side_v * Mathf.Sin(tempAngle1);
        area1 = tempHeight * side_h;

        //side
        float tempAngle2 = Vector3.Angle(tempUp, tempFwd) * Mathf.Deg2Rad;//angle of rhombus projection
        tempHeight = side_v * Mathf.Sin(tempAngle2);
        area2 = tempHeight * side_f;

        //top
        float tempAngle3 = Vector3.Angle(tempRight, tempFwd) * Mathf.Deg2Rad;//angle of rhombus projection
        tempHeight = side_f * Mathf.Sin(tempAngle3);
        area3 = tempHeight * side_h;

    }

    protected void angularResistance()
    {
        //spin drag
        Vector3 tempDrag = -rb.angularVelocity * ((float)(1 / (1 + math.pow(100, -angFriction / 100)) - 0.5)) * 2;
        if (!float.IsNaN(tempDrag.x))
            rb.AddTorque(tempDrag, ForceMode.Force);
    }
    
    protected void resistanceCalc(float effect)
    {
        if (air_reactF.sqrMagnitude < Mathf.Epsilon)
            return;
            
        // get torque force on a falling object 
        Vector3 sumMoments = Vector3.Cross(fplayer_fwd, depthLength / 2 * player_fwd) + Vector3.Cross(fplayer_right, horizontalLength / 2 * player_right) + Vector3.Cross(fplayer_up, verticalLength / 2 * player_up);//#possibly update?
        //calculate center of preasure
        Vector3 centerOfPressure = Vector3.Cross(air_reactF, sumMoments) / air_reactF.sqrMagnitude; //weighted average, sum of moments / F net
        Vector3 torque = Vector3.Cross(centerOfPressure, air_reactF); // tau = r * Fd
        if (!float.IsNaN((torque * effect).x))
        rb.AddTorque(torque * effect, ForceMode.Force);
    }

    //lean centripetal force
    protected void leanForces(Vector3 tangent, Vector3 upSpline, Vector3 leftSpline, Vector3 pointSpline, float angMult, float mouse)
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); //remove y component
        Vector3 disPoint = pointSpline - lastPoint;
        float angleBetween = Vector3.SignedAngle(disPoint, tangent, Vector3.up);//angle between lastvel and flatVel
        //Debug.DrawRay(transform.position, (flatVel.normalized - lastVel.normalized) * 8, Color.red, 2);
        float chord = disPoint.magnitude / 2;//chord length
        float radius = chord / Mathf.Sin(angleBetween * Mathf.Deg2Rad);//find radius of circular motion

        //Debug.Log(flatVel.sqrMagnitude / (Physics.gravity.magnitude * radius));
        float leanAngle = Mathf.Atan(flatVel.sqrMagnitude * angMult / (Physics.gravity.magnitude * radius)) * Mathf.Rad2Deg;//θ = arctan(v²/gr)
        leanAngle *= angMult;//exageration

        Quaternion tempNormal = Quaternion.FromToRotation(Vector3.up, upSpline) * orientation.rotation;//set normal of rail as up
        Debug.DrawRay(transform.position, upSpline * 4, Color.white);

        desiRotation = tempNormal * Quaternion.AngleAxis(leanAngle, flatVel);//apply angle

        //determine torque required to move
        Vector3 desiDir = desiRotation * Vector3.up;
        Vector3 modedDir = desiDir - cam.forward * mouse;
        var x = Vector3.Cross(playerBody.up, modedDir.normalized);
        float theta = Mathf.Asin(x.magnitude);
        Vector3 w = x.normalized * theta / Time.fixedDeltaTime;
        Quaternion q = playerBody.rotation * rb.inertiaTensorRotation;
        Vector3 t1 = q * Vector3.Scale(rb.inertiaTensor, Quaternion.Inverse(q) * w);
        Vector3 tempVel = Vector3.ProjectOnPlane(rb.angularVelocity, modedDir);
        float tempAng = Vector3.Angle(modedDir, playerBody.up) * Mathf.Deg2Rad;
        //Mathf.SmoothDampAngle(tempAng, 0, ref tempVel, 1);
        if (tempVel.magnitude > tempAng)//reverse force if velocity excedes distance remaning
            t1 -= tempVel;
        //add torque
        if (!float.IsNaN(t1.x))
            rb.AddTorque(t1, ForceMode.Force);
        Debug.DrawRay(transform.position, t1 * 4, Color.cyan);

        //determine torque to get desi angle
        Vector3 t2 = Vector3.Cross(playerBody.forward, desiRotation * Vector3.forward) * spdMult_ang;
        if (!float.IsNaN(t2.x))
            rb.AddTorque(t2, ForceMode.Force);

        Debug.DrawRay(transform.position, modedDir * 4, Color.red);
        Debug.DrawRay(transform.position, Vector3.up * 5, Color.green);

        //addjust vertical angle
        Vector3 vertDif = Vector3.SignedAngle(playerBody.up, upSpline, leftSpline) * Mathf.Deg2Rad * leftSpline - Vector3.Project(rb.GetAccumulatedTorque(), leftSpline);
        if (!float.IsNaN(vertDif.x))
            rb.AddTorque(vertDif / 2, ForceMode.Force);//readjustmentforce
        //save last vel
        lastPoint = pointSpline;
    }
    
    public static void DecomposeSwingTwist(Quaternion q, Vector3 twistAxis, out Quaternion swing, out Quaternion twist)
    {
        Vector3 r = new Vector3(q.x, q.y, q.z);

        // singularity: rotation by 180 degree
        if (r.sqrMagnitude < Mathf.Epsilon)
        {
            Vector3 rotatedTwistAxis = q * twistAxis;
            Vector3 swingAxis = 
            Vector3.Cross(twistAxis, rotatedTwistAxis);

            if (swingAxis.sqrMagnitude > Mathf.Epsilon)
            {
            float swingAngle = 
                Vector3.Angle(twistAxis, rotatedTwistAxis);
            swing = Quaternion.AngleAxis(swingAngle, swingAxis);
            }
            else
            {
            // more singularity: 
            // rotation axis parallel to twist axis
            swing = Quaternion.identity; // no swing
            }

            // always twist 180 degree on singularity
            twist = Quaternion.AngleAxis(180.0f, twistAxis);
            return;
        }

        // meat of swing-twist decomposition
        Vector3 p = Vector3.Project(r, twistAxis);
        twist = new Quaternion(p.x, p.y, p.z, q.w);
        twist = twist.normalized;
        swing = q * Quaternion.Inverse(twist);
    }
}
