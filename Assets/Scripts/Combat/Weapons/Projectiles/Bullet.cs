using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private BulletInfo bi;

    private void Start()
    {
        bi = gameObject.GetComponentInParent<BulletInfo>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (bi.Explosive)
        {
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, bi.ExplosionRadi);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(bi.ExplosionStr, explosionPos, bi.ExplosionRadi, 0.0F);
                }
            }
        }
        Destroy(gameObject);
    }
}
