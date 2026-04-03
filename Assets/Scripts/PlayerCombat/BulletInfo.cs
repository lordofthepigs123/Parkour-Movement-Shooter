using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInfo : MonoBehaviour
{
    public bool Reflectable;
    public bool Gravity;
    public float BaseSpeed;
    public float Weight;
    public bool Explosive;
    public float ExplosionRadi;
    public float ExplosionStr;
    public float ExplosionDmg;
    public GameObject ReflectObject;
    private Rigidbody rb;

    private void Update()
    {
        gameObject.GetComponent<Rigidbody>().useGravity = Gravity;
        rb = GetComponent<Rigidbody>();
        rb.mass = Weight / 100;
    }
}
