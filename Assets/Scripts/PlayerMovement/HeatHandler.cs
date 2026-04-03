using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatHandler : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] public float heat; //0-100   //##################################### add hud meter
    [SerializeField] float heatLossMult;
    [SerializeField] float vibrationFreq;
    public float heatOverallMult;  // multiplyer of actual player energy
    [HideInInspector] public Vector3 lastPlayerVel;
    private float vibeTimer;
    private Vector3 inputDir;

    public float energyKenetic;
    public Vector3 heatNegative;

    [Header("Components")]
    [SerializeField] Transform orientation;
    private PlayerColliderManager cm;
    private InputHandler ih;
    private Rigidbody rb;
    private PlayerStats ps;
    private PlayerGrind pg;

    [SerializeField] Slider SPD_bar;
    [SerializeField] Slider Heat_bar;

    private void Start()
    {
        cm = GetComponent<PlayerColliderManager>();
        ih = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody>();
        ps = GetComponent<PlayerStats>();
        pg = GetComponent<PlayerGrind>();
    }
    private void FixedUpdate()
    {
        energyKenetic = 0.5f * rb.mass * rb.linearVelocity.sqrMagnitude * heatOverallMult;// Ek = 0.5mv²

        if (cm.touchingWall || pg.aboveRail) // if touching wall but not new contact calc friction
            friction();
        airConvection();
        heat = Mathf.Clamp(heat, 0, 100);
    }

    private void Update()
    {
        hud();
    }

    private void friction()
    {
        Vector3 modedLastPlayerVel = lastPlayerVel + cm.colImpulse; //remove any collision acceleration
        //Debug.Log(lastPlayerVel + "&" + cm.colImpulse);
        Vector3 tempNormal;
        if (pg.aboveRail)
            tempNormal = pg.upSpline;
        else
            tempNormal = cm.wallNormal;
        float difEnergy = 0.5f * rb.mass * (Vector3.ProjectOnPlane(rb.linearVelocity, tempNormal).sqrMagnitude - Vector3.ProjectOnPlane(modedLastPlayerVel, tempNormal).sqrMagnitude) * heatOverallMult;//difference curnt and last
        //Debug.DrawRay(transform.position,(Vector3.ProjectOnPlane(rb.velocity,tempNormal) - Vector3.ProjectOnPlane(modedLastPlayerVel,tempNormal)), Color.red, 2);

        heatNegative = Vector3.zero;
        if (difEnergy < 0)
        {
            heat -= difEnergy;
            heatNegative = Vector3.ProjectOnPlane(rb.linearVelocity - modedLastPlayerVel, tempNormal);
            //Debug.DrawRay(rb.worldCenterOfMass, heatNegative / Time.fixedDeltaTime / 500, Color.red, 2);
        }
        lastPlayerVel = rb.linearVelocity;
    }

    private void airConvection()
    {
        float v = rb.linearVelocity.magnitude;
        float h = (2 + 0.1f * Mathf.Pow(v, 0.6f)) * Mathf.Pow(heat, 0.5f); //h = (2 + 0.1 * v_wind^0.6)(Tintl - Tamb)^0.5
        float rate = h * heatLossMult * heat;//Q = h * A * (Tintl - Tamb)

        heat -= rate * Time.fixedDeltaTime;
    }

    public bool subtarctHeat(float subHeat)
    {
        float newHeat = heat - subHeat;
        if (newHeat < 0)
            return false;
        heat = newHeat;
        return true;
    }

    private void hud()
    {
        // HUD
        vibeTimer += Time.deltaTime * vibrationFreq;
        if (vibeTimer > Mathf.PI * 8)
            vibeTimer -= Mathf.PI * 8;
        float vibes = Mathf.Sin(1.2f * vibeTimer)/3 + Mathf.Pow(Mathf.Sin(vibeTimer),2)/3 + Mathf.Pow(Mathf.Cos(2 * vibeTimer),2)/3; //up and down movement of bars based on time

        float display_SPD = energyKenetic + Mathf.Pow(energyKenetic, 0.7f) * vibes * 0.3f;
        float display_heat = heat + Mathf.Pow(heat, 0.7f) * vibes * 0.3f;
        SPD_bar.value = Mathf.Clamp(display_SPD , 0, 100); //The energy of the players speed blue bar
        Heat_bar.value = Mathf.Clamp(display_heat, 0, 100); //The energy of the players heat orang bar
    }
}
