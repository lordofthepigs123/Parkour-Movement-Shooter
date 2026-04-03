using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProceduralRecoil : MonoBehaviour
{
    [SerializeField] PlayerStats ps;
    [SerializeField] ComboCounter cc;
    [SerializeField] PlayerColliderManager cm;
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform cam;
    [SerializeField] PlayerCam pc;
    [SerializeField] float recoilX;
    [SerializeField] float recoilY;
    [SerializeField] float recoilZ;
    [SerializeField] float kickBackZ;
    [SerializeField] float boostForce;

    public float snappiness, returnAmount;

    private Vector3 currentRotation, targetRotation, targetPosition, initialGunPosition;

    private void Start()
    {
        initialGunPosition = transform.localPosition;
    }

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, Time.deltaTime * returnAmount);
        pc.DoRecoil(targetRotation);

        cam.localRotation = Quaternion.Euler(currentRotation);

        back(); // kick back
    }

    public void recoil()
    {
        targetPosition -= new Vector3(0, 0, kickBackZ);
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        recoilBoost();
    }

    private void back()
    {
        targetPosition = Vector3.Lerp(targetPosition, initialGunPosition, Time.deltaTime * returnAmount);
        transform.DOLocalMove(targetPosition, Time.fixedDeltaTime * (snappiness*100/ps.weight.Value));
    }

    private void recoilBoost()
    {
        if (!cm.grounded)
        {
            float newBoostForce = boostForce / ps.weight.Value * 100;
            rb.AddForce(-cam.forward * newBoostForce, ForceMode.Impulse);
            cc.onRecoilBoost();
        }
    }
}
