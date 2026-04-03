using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDraw : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] Transform cam;
    [SerializeField] PlayerCam playerCam;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float sphereCastRad;
    [SerializeField] float aboveCheckDis;
    [SerializeField] float sphereRadius;
    [SerializeField] float tolerateTopAngle;
    [SerializeField] float FwdDownRange;
    [SerializeField] float ledgeCooldown;
    [SerializeField] float stickFwdMult;
    [SerializeField] float ledgeUpMult;
    [SerializeField] float FwdRange;
    [SerializeField] float FwdCheckOffset;
    [SerializeField] float ledgeFwdForce;
    [SerializeField] float ledgeTime;

    private float ledgeTimer;
    [SerializeField]private bool ledgeClimbing;
    private bool canForwardBoost;
    private bool canBoost = true;
    private RaycastHit rayHit;
    private RaycastHit _rayHit;

    private Rigidbody rb;
    private PlayerColliderManager cm;
    private InputHandler ih;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cm = GetComponent<PlayerColliderManager>();
        ih = GetComponent<InputHandler>();
    }
    private void Update()
    {
        SubStateMachine();
    }

    private void SubStateMachine()
    {
        if (ledgeClimbing) //
        {
            if (ledgeTimer > 0)
                ledgeTimer -= Time.deltaTime;   
            else
            {
                Invoke("resetLedgeBoost", ledgeCooldown);//reset being able to forward boost and next boost
                ledgeClimbing = false;
                canBoost = false;
            }
        }

        if (canForwardBoost)
        {
            if (!Physics.Raycast(transform.position + transform.up * FwdCheckOffset, -cm.wallNormal, out rayHit, FwdRange, obstacleLayer)) //# position? // secondary check#
            {
                Debug.Log("Fwd Ledge Boost");
                canForwardBoost = false;
                canBoost = false;
                //add forward force over the top
                rb.AddForce(-cm.wallNormal * ledgeFwdForce, ForceMode.Impulse);
            }
        }

        if (!canBoost)//cooldown
            return;

        if (!cm.touchingWall || ih.keySHIFT || cm.grounded || rb.linearVelocity.y < -0.2f)
            return;

        Debug.DrawRay(transform.position, -cm.wallNormal * FwdRange, Color.yellowNice);
        if (!Physics.Raycast(transform.position, -cm.wallNormal, out _rayHit, FwdRange, obstacleLayer))
            return;

        Vector3 checkPos = new Vector3 (transform.position.x, cam.position.y + aboveCheckDis, transform.position.z);
        float heightDif = checkPos.y - transform.position.y;

        Debug.DrawRay(checkPos - cm.wallNormal * FwdDownRange/2, Vector3.down * (heightDif - sphereRadius), Color.purple);

        if (!Physics.Raycast(checkPos - cm.wallNormal * FwdDownRange/2, Vector3.down, out _rayHit, heightDif - sphereRadius, obstacleLayer))//check down if platform exists
            return;
        if (_rayHit.normal != Vector3.zero && Vector3.Angle(_rayHit.normal, Vector3.up) > tolerateTopAngle)
            return;

        if (!ledgeClimbing)
        {
            Debug.Log("Start ledgeBoost");
            ledgeClimbing = true;
            canForwardBoost = true;
            ledgeTimer = ledgeTime;
        }

        //upward boost to ledge height
        canForwardBoost = true;
        Vector3 netForce = (rayHit.point - transform.position).normalized * stickFwdMult;

        float disMult = (_rayHit.point.y - transform.position.y) * ledgeUpMult + 0.1f;
        if (disMult < 0)
            return;
        float upForce = Physics.gravity.magnitude - Mathf.Pow(rb.linearVelocity.y, 2) / disMult / 2;//v² = u² + 2as
        if (upForce > 0)//add up force
            netForce += upForce * Vector3.up;
        rb.AddForce(netForce, ForceMode.Force);
        //Debug.DrawRay(transform.position, netForce, Color.yellowNice, 2);
        //Debug.Log(netForce.magnitude);
    }

    private void resetLedgeBoost()
    {
        Debug.Log("Reset ledgeBoost");
        canBoost = true;
        ledgeClimbing = false;
        canForwardBoost = false;
    }
}
