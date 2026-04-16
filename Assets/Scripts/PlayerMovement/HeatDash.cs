using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeatDash : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldownTime;
    private Vector3 setInputDir;
    public bool canHeatDash = true;
    private bool heatDashing;
    private bool onCooldown;
    private float dashTimer;
    private float dashCooldownTimer;

    [Header("Components")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform cam;

    private PlayerStateMachine sm;
    private HeatHandler hh;
    private InputHandler ih;
    private Rigidbody rb;
    private PlayerStats ps;
    private SlideRoll sr;
    private void Awake()
    {
        sm = GetComponent<PlayerStateMachine>();
        hh = GetComponent<HeatHandler>();
        ih = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody>();
        ps = GetComponent<PlayerStats>();
        sr = GetComponent<SlideRoll>();
    }

    private void Update()
    {
        Inputs();
        if (heatDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer < 0)
                exitDash();
            return;
        }
        if (onCooldown)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer < 0)
            {
                canHeatDash = true;
                onCooldown = false;
            }
            return;
        }
           
    }
    private void FixedUpdate()
    {
        if (heatDashing)
            dashMovement();
    }


    private void Inputs()
    {
        if (ih.keyF && !ih.activatedF && canHeatDash)//activate dash
        {
            ih.activatedF = true;
            enterDash();
        }
    }

    private void enterDash()
    {
        Debug.Log("enter dash");
        //add sm and other stuff #
        //cam effects
        heatDashing = true;
        canHeatDash = false;
        dashTimer = dashTime;
        if (sm.sliding)
            setInputDir = Vector3.Project(sr.moveDirection * 0.01f + ih.baseInputDir, sr.moveDirection).normalized;
        else
            setInputDir = Vector3.ProjectOnPlane(cam.forward * (ih.keyVertical + 0.01f), transform.up).normalized;//change cam -> body facing dir #

        //calculate energy usage
        Vector3 tempVel = Vector3.Project(rb.linearVelocity, setInputDir);
        float tempEnergyK = 0.5f * rb.mass * (tempVel.magnitude * Vector3.Dot(rb.linearVelocity, setInputDir) * rb.linearVelocity.magnitude);
        // E = 1/2 * m * (vi - vf)^2
        Vector3 boostForce = Mathf.Pow(2 * hh.heat / hh.heatOverallMult / rb.mass, 0.5f) / Time.fixedDeltaTime * setInputDir; // vi - vf = (2E/m)^0.5 | a = (vf - vi) / t
        hh.heat = 0;
        Debug.Log(boostForce + " f");
        rb.AddForce(boostForce, ForceMode.Force);
    }

    private void dashMovement()
    {
        //calculate energy usage
        Vector3 tempVel = Vector3.ProjectOnPlane(rb.linearVelocity, setInputDir);
        Vector3 boostForce = -tempVel * (1 - dashTimer / dashTime); // increase adjust force as reach end of dash
        rb.AddForce(boostForce, ForceMode.Force);
    }

    private void exitDash()
    {
        //Debug.Log("exit dash");
        //add sm and other stuff #
        //cam effects
        heatDashing = false;
        onCooldown = true;
        dashCooldownTimer = dashCooldownTime;
    }
}
