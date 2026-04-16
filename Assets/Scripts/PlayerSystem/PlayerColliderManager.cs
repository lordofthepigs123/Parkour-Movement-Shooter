using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderManager : MonoBehaviour
{
    [Header("Variables")]
    public LayerMask whatIsGround;
    [SerializeField] float groundCheckDis;
    [SerializeField] public float insideCheckDis;
    public bool grounded;
    public bool touchingWall;
    public bool touchingGround;
    [HideInInspector] public Vector3 wallNormal;
    [HideInInspector] public Vector3 tangentWall;
    [HideInInspector] public Vector3 upWall;
    [HideInInspector] public Vector3 colImpulse;
    [HideInInspector] public Vector3 lowesPoint;
    public Collider activeCol; //holds collider object reference
    public float ActiveHeight;
    public float ActiveRadius;
    [SerializeField] private int numCols = 0;
    private Vector3 totalNormal;
    private Vector3 wallPoint;
    private int colObjectPointer;

    [Header("Components")]
    static private CapsuleCollider ColM;
    static private SphereCollider ColS;
    private PlayerStateMachine sm;
    private Collider[] colliders;//player cols
    [HideInInspector] public Collider wallCollider;//wall col
    public ActiveCol state;
    public enum ActiveCol
    {
        Capsule,
        Sphere
    }
    private void Start()
    {
        sm = GetComponent<PlayerStateMachine>();
        ColM = GetComponent<CapsuleCollider>();
        ColS = GetComponent<SphereCollider>();
        Collider[] temp = { ColM, ColS };
        colliders = temp;

        changeCol(ActiveCol.Capsule);
    }

    private void Update()
    {
        //resets
        colObjectPointer = 0;
        totalNormal = Vector3.zero;
        wallPoint = Vector3.positiveInfinity;
        //ground raycast
        //Debug.DrawRay(sm.rotAdjustPos, Vector3.down * groundCheckDis, Color.blue,1);
        if (Physics.Raycast(sm.rotAdjustPos, Vector3.down, groundCheckDis, whatIsGround) || touchingGround)
            grounded = true;
        else
            grounded = false;
    }

    public void changeCol(ActiveCol targetCol) //change the current targeted colider
    {
        numCols = 0;
        state = targetCol;
        foreach (Collider collider in colliders)//disable all coliders
        {
            collider.enabled = false;
        }

        if (state == ActiveCol.Capsule)
        {
            activeCol = ColM;
            ActiveHeight = ColM.height;
            ActiveRadius = ColM.radius;

            ColM.enabled = true;
            numCols = 0;
            resetVars();
            return;
        }
        if (state == ActiveCol.Sphere)
        {
            activeCol = ColS;
            ActiveHeight = ColS.radius * 2;
            ActiveRadius = ColS.radius;

            ColS.enabled = true;
            numCols = 0;
            resetVars();
            return;
        }
    }

    public void enableCol(bool enabled, Collider other)
    {
        foreach (Collider collider in colliders)//disable all coliders
        {
            Physics.IgnoreCollision(collider, other, !enabled);
        }
    }

    //Detect collisions with all Wall
    private void OnCollisionEnter(Collision collision)
    {
        colImpulse = collision.impulse;
        Invoke("resetImpulse",0.0001f);
        //Debug.DrawRay(transform.position, colImpulse, Color.blue, 1);
        if (collision.gameObject.layer == Mathf.Log(whatIsGround.value, 2))
        {
            if (numCols < 0)
                numCols = 0;
            numCols++;

            touchingWall = true;
            //Debug.Log("enter Col");
        }
    }
    //Get info when touching wall    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == Mathf.Log(whatIsGround.value, 2))
        {
            if (colObjectPointer <= 0)
            {
                touchingGround = false;
            }
            colObjectPointer ++;

            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Angle(Vector3.up, contact.normal) < 30)
                {
                    touchingGround = true;
                }
                totalNormal += contact.normal;// average wallNormal # possible trigger distance weighting
                
                if (contact.point.y < wallPoint.y)
                    wallPoint = contact.point;// lowest wall point
            }
            //Debug.DrawRay(transform.position, wallNormal * 2, Color.blue,1);
            wallCollider = collision.collider; // #

            if (colObjectPointer >= numCols)//calculate vars after last collision
            {
                wallNormal = totalNormal.normalized;
                lowesPoint = wallPoint;
                wallVars();
            }
        }
    }

    //Stop Collision
    private void OnCollisionExit(Collision collision)
    {
        if (touchingWall && collision.gameObject.layer == Mathf.Log(whatIsGround.value, 2))
        {
            numCols--;

            if (numCols <= 0)
            {
                resetVars();
                //Debug.Log("exit Col");
            }
        }
    }
    
    private void resetVars()
    { 
        touchingGround = false;
        touchingWall = false;
    }

    private void resetImpulse()
    { 
        colImpulse = Vector3.zero;
    }

    private void wallVars()
    {
        //get vars
        tangentWall = Vector3.Cross(wallNormal, Vector3.up);//(left / right) tangent of wall
        upWall = Quaternion.AngleAxis(90, tangentWall) * wallNormal;
    }
}
