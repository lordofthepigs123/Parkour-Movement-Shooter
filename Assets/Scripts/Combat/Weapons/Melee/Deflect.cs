using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deflect : MonoBehaviour
{
    [SerializeField] PlayerStats ps;
    [SerializeField] Camera cam;
    [SerializeField] LayerMask bulletLayer;

    private RaycastHit hit;
    private bool rayHit;
    private bool deflectBreak = false;
    private float repairTime;
    public bool inBlock;

    private void Update()
    {
        if (deflectBreak)
        {
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            GetComponent<Collider>().enabled = inBlock;
            GetComponent<MeshRenderer>().enabled = inBlock;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        BulletInfo bi = collision.gameObject.GetComponent<BulletInfo>();
        if (!bi.Reflectable)
        {
            repairTime = bi.Weight - ps.DEF.Value;
            DeflectBroken();
            return;
        }

        Vector3 position = collision.transform.position;
        float bulletVelocity;

        if (ps.BLNC.Value >= bi.Weight)
        {
            bulletVelocity = bi.BaseSpeed;
        }
        else if (ps.BLNC.Value > bi.Weight / 2)
        {
            bulletVelocity = bi.BaseSpeed * bi.Weight / (2 * bi.Weight - ps.BLNC.Value);
        }
        else
        {
            repairTime = (bi.Weight - ps.DEF.Value) / 20;
            DeflectBroken();
            return;
        }
        
        rayHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100, bulletLayer);

        Vector3 targetPoint;
        if (rayHit)
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = cam.transform.position + cam.transform.forward * 100;
        }
        Vector3 direction = (targetPoint - position).normalized;
        GameObject reflectedBullet = Instantiate(bi.ReflectObject, position + direction * 0.4f, Quaternion.identity);
        BulletInfo _bi = reflectedBullet.GetComponent<BulletInfo>();
        //Transfer stats of bullet
        _bi.Reflectable = bi.Reflectable;
        _bi.Gravity = bi.Gravity;
        _bi.BaseSpeed = bi.BaseSpeed;
        _bi.Weight = bi.Weight;
        reflectedBullet.GetComponent<Rigidbody>().AddForce(direction * bulletVelocity, ForceMode.Impulse);
        Destroy(collision.gameObject);
        Destroy(reflectedBullet, 4);
    }

    private void DeflectBroken()
    {
        deflectBreak = true;
        repairTime = Mathf.Clamp(repairTime, 0.5f, 10);
        Invoke("DeflectFix", repairTime);
    }
    private void DeflectFix()
    {
        deflectBreak = false;
    }
}
