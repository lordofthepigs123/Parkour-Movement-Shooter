using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Splines.Interpolators;

public class PlayerCam : MonoBehaviour
{
    [Header("PlayerCam")]
    public float globalFov;
    [SerializeField] float sensX;
    [SerializeField] float sensY;
    [SerializeField] float fovMult;
    [SerializeField] float lockLerp;
    [SerializeField] float bufferRotArea;
    [SerializeField] AnimationCurve rollCurve;


    [Header("Components")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform camHolder;
    [SerializeField] RangedHandler rh;
    [SerializeField] Transform cam;
    [SerializeField] StateManager sm;
    [SerializeField] InputHandler ih;
    [SerializeField] Rigidbody rb;

    public float xRotation;// -180 - 180
    public float yRotation;// -180 - 180
    public bool inverse_y;
    public bool fwdLocked;
    private bool doRoll;

    private Vector3 rollAxis;
    private float rollTimer;
    private float applyRatio;

    private float mouseX;
    private float mouseY;
    private float tilt = 0;
    private bool slowRotate;
    private float barrelAngle;
    private Sequence sequence;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        DoTilt(0f);
    }

    public void DoSlowRotation(float RotationModifier)
    {
        slowRotate = true;
        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * 10f;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * 10f;

        mouseX = Mathf.Clamp(mouseX, -10 / RotationModifier, 10 / RotationModifier);
        mouseY = Mathf.Clamp(mouseY, -10 / RotationModifier, 10 / RotationModifier);
    }

    private void Update()
    {
        //set Fov
        DoFov();

        //get mouse input
        if (!slowRotate)
        {
            mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * 10f;
            mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * 10f;
        }
        else
        {
            slowRotate = false;
        }

        xRotation -= mouseY;//add input
        
        if (!inverse_y)
            yRotation += mouseX;
        else
            yRotation -= mouseX;
        //mod yrotation
        if (yRotation > 180)
            yRotation -= 360;
        else if (yRotation < -180)
            yRotation += 360;

        //Quaternion desi = Quaternion.Euler(0, yRotation, 0);
        //float angle = Quaternion.Angle(orientation.rotation, desi);
        //orientation.rotation = Quaternion.Slerp(orientation.rotation, desi, 0.8f / angle);
        //mod xrotation
        if (xRotation > 180)
            xRotation -= 360;
        else if (xRotation < -180)
            xRotation += 360;

        if (xRotation > 90 || xRotation < -90)
            inverse_y = true;
        else
            inverse_y = false;

        //try to lock x in non inversed
        if (fwdLocked)
            DoLock(lockLerp * Time.deltaTime);
    
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        //set rotation of cam
        Quaternion roll = Quaternion.identity;
        if (doRoll)
            roll = RollApply();

        Quaternion camRot = roll * Quaternion.Euler(xRotation, yRotation, 0);
        if (barrelAngle != 0)//barrel roll
        {
            camHolder.rotation = Quaternion.AngleAxis(barrelAngle, camRot * Vector3.forward) * camRot;
        }
        else
        {
            camHolder.rotation = camRot;
        }
    }

    private void DoLock(float lerp)
    {
        inverse_y = false;
        if (xRotation > 90)
            xRotation -= Mathf.Lerp(0, xRotation - 90, lerp);
        else if (xRotation < -90)
            xRotation -= Mathf.Lerp(0, xRotation + 90, lerp);
    }

    private void DoFov()//set Fov based on speed
    {
        float newFov = Mathf.Clamp(globalFov + rb.linearVelocity.magnitude * fovMult, 0, globalFov + 30);
        GetComponent<Camera>().DOFieldOfView(newFov, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
        tilt = zTilt;
    }

    public void DoRecoil(Vector3 tRotation)
    {
        tRotation = new Vector3(tRotation.x, tRotation.y, tRotation.z + tilt);
        cam.DOLocalRotate(tRotation, Time.fixedDeltaTime * rh.snappiness);
        rh.gun.DOLocalRotate(tRotation, Time.fixedDeltaTime * rh.snappiness);
    }
    /*
    public void DoBarrel(float dir, int fix, float dur)
    {
        xRotation = -xRotation + 180 * fix + ih.keyVertical * 20 * 2;
        yRotation += 180;
        barrelAngle += 180 * (int)dir;
        sequence.Kill();
        sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => barrelAngle, x => barrelAngle = x, 0, dur));//start tweening angle
    }
    */

    public void DoRoll(Vector3 dir, float dur)
    {
        doRoll = true;
        rollAxis = dir;
        rollTimer = dur;
        applyRatio = 0;
        DOTween.To(() => applyRatio, x => applyRatio = x, 1, dur).SetEase(rollCurve);
    }

    private Quaternion RollApply()
    {
        rollTimer -= Time.deltaTime;
        if (rollTimer < 0)
            doRoll = false;

        return Quaternion.AngleAxis(applyRatio * 360, rollAxis);
    }
}
