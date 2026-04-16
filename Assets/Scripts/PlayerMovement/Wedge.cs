using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Wedge : MonoBehaviour
{
    [SerializeField] ComboCounter cc;
    [SerializeField] Transform cam;
    [SerializeField] PlayerCam playerCam;
    [SerializeField] WallRunning wr;
    [SerializeField] float wedgeRange;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float maxWedgeDistance;
    [SerializeField] float wedgeSpeed;
    [SerializeField] float maxMoveTime;
    [SerializeField] float minTimeOnWall;
    [SerializeField] float exitWedgeTime;
    [SerializeField] float meleeboostForce;
    [HideInInspector] public bool raycasting = false;
    [HideInInspector] public bool exitingWedge = false;
    [HideInInspector] public bool hangLockIn = false;
    public bool holding;
    [HideInInspector] public float slipAmount;
    [HideInInspector] public Vector3 WedgeDirection;

    [SerializeField] float wallDashForce;
    [SerializeField] float dashUpwardsForce;
    [SerializeField] float dashDuration;
    [SerializeField] float removeVelEndDash;

    private float angleWedge;

    private PlayerStats ps;
    private Vector3 holdSpot;
    private float exitWedgeTimer;
    private float timeOnWall;
    private float moveTime = 0;
    private bool holdDetecting;
    private Rigidbody rb;
    private PlayerStateMachine sm;
    private PlayerColliderManager cm;
    private RaycastHit Hit;
    private RaycastHit SphereHit;
    private RaycastHit RayHit;
    private Vector3 direction;
    private InputHandler ih;
    private bool SPadded;
    private float dashTimer;

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        sm = GetComponent<PlayerStateMachine>();
        cm = GetComponent<PlayerColliderManager>();
        ih = GetComponent<InputHandler>();
    }

    private void Update()
    {
        SubStateMachine();
        if (raycasting && !holding)
            WallDetection();
    }

    private void SubStateMachine()
    {
        if (holding)
        {
            FreezeRigidBodyOnWall();

            timeOnWall += Time.deltaTime;

            if (timeOnWall > minTimeOnWall && ih.keySHIFT)
                ExitHold();

            if (ih.ikeySPACE)
                StartWallDash();

            if (cm.grounded)
                ExitHold();

            Vector3 OriginPos = new Vector3(rb.position.x, rb.position.y + 1.5f, rb.position.z);
            direction = (Hit.point - OriginPos);
            WedgeDirection = direction.normalized;
            if (!Physics.SphereCast(OriginPos, 0.2f, direction, out SphereHit, 1.5f, wallLayer))
            {
                ExitHold();
            }

        }
        else if (exitingWedge)
        {
            if (exitWedgeTimer > 0)
                exitWedgeTimer -= Time.deltaTime;
            else
                exitingWedge = false;
        }

        if (sm.dashing)
        {
            if (ih.rkeySPACE && !SPadded)
            {
                ih.rkeySPACE = false;
                SPadded = true;
                //Add bonus time
                dashTimer += ih.acuSPACE * 0.2f;
                rb.linearVelocity = rb.linearVelocity * (1 + ih.acuSPACE * 0.2f);
            }
            dashTimer -= Time.deltaTime;
            if (dashTimer < 0)
            {
                EndWallDash();
            }
        }
    }

    private void WallDetection()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out Hit, wedgeRange, wallLayer))
        {
            angleWedge = Vector3.Angle(Vector3.down, Hit.normal);
            Vector3 OriginPos = new Vector3(rb.position.x, rb.position.y + 2f, rb.position.z);

            //detect angle of wall/ground
            if (angleWedge <= 91)
            {
                direction = (Hit.point - OriginPos);

                if (moveTime > 0)
                {
                    moveTime -= Time.deltaTime;
                    holdDetecting = true;

                    if (Vector3.Distance(OriginPos, Hit.point) < maxWedgeDistance)
                        EnterHang();
                }
                else if (!holdDetecting)
                {
                    rb.AddForce(direction * wedgeSpeed, ForceMode.Impulse); // dash towards wall when charged attack
                    moveTime = maxMoveTime;
                }
                else
                {
                    ExitHold();
                }

                if (sm.wallRunning)
                {
                    wr.exitingWall = true;
                    wr.exitWallTimer = wr.exitWallTime;
                }
            }
            else if (Physics.Raycast(cam.transform.position, cam.transform.forward, out Hit, maxWedgeDistance + 0.1f, wallLayer) && !cm.grounded)
                GroundBoost();
            else
                raycasting = false;
        }
        else
            raycasting = false;
    }

    private void EnterHang()
    {
        holding = true;
        raycasting = false;
        sm.unlimited = true;
        sm.restricted = true;
        sm.wedgeGrabing = true;

        if (-rb.linearVelocity.y > 0.05f)
            slipAmount = -rb.linearVelocity.y * 0.07f;
        else
            slipAmount = 0.1f;
    }

    private void FreezeRigidBodyOnWall()
    {
        rb.useGravity = false;

        Vector3 OriginPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
        Vector3 directionToWedge = Hit.point - OriginPos;
        float distanceToWedge = Vector3.Distance(OriginPos, Hit.point);

        if (distanceToWedge > 0.8f)
        {
            if (rb.linearVelocity.magnitude < wedgeSpeed)
                rb.AddForce(directionToWedge.normalized * wedgeSpeed * 1000f * Time.deltaTime, ForceMode.Force);
        }
        else
        {
            if (!hangLockIn)
            {
                hangLockIn = true;
                Vector3 newPos = new Vector3(rb.transform.position.x, rb.transform.position.y - slipAmount, rb.transform.position.z);
                rb.transform.DOMove(newPos, slipAmount * 0.3f + 0.1f);
            } 

            if (!sm.freeze)
                sm.freeze = true;
            if (sm.unlimited)
                sm.unlimited = false;
            rb.linearVelocity = Vector3.zero;
        }

        if (distanceToWedge > maxWedgeDistance + 0.2f)
        {
            ExitHold();
        }

    }

    private void StartWallDash()
    {
        ih.activatedSPACE = true;
        dashTimer = dashDuration;
        SPadded = false;
        ExitHold();
        cc.onWallDash();

        rb.linearVelocity = Vector3.zero;

        Invoke(nameof(delayedDash), 0.025f);

    }

    private void delayedDash()
    {
        sm.dashing = true;
        rb.useGravity = false;
        direction = cam.forward.normalized;
        rb.AddForce(direction * wallDashForce * 100f + Vector3.up * dashUpwardsForce, ForceMode.Force);
    }


    private void EndWallDash()
    {
        StopAllCoroutines();
        rb.useGravity = true;
        sm.dashing = false;
        if (!cm.touchingWall && !wr.proximWall) // remove some velocity at end of dash unless tranistioned
            rb.linearVelocity = rb.linearVelocity * removeVelEndDash;
    }

    private void GroundBoost()
    {
        cc.onWedgeBoost();
        Vector3 OriginPos = new Vector3(rb.position.x, rb.position.y + 2f, rb.position.z);
        Vector3 direction = -(Hit.point - OriginPos);

        rb.AddForce(direction * meleeboostForce, ForceMode.Impulse);


        if (sm.wallRunning)
        {
            wr.exitingWall = true;
            wr.exitWallTimer = wr.exitWallTime;
        }
    }

    private void ExitHold()
    {
        DOTween.Kill(transform);
        hangLockIn = false;
        holdDetecting = false;
        raycasting = false;
        exitingWedge = true;
        exitWedgeTimer = exitWedgeTime;
        holding = false;
        timeOnWall = 0f;
        moveTime = 0f;

        sm.wedgeGrabing = false;
        sm.unlimited = false;
        sm.restricted = false;
        sm.freeze = false;

        rb.useGravity = true;
    }
}
