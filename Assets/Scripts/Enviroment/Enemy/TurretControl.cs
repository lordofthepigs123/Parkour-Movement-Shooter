using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TurretControl : MonoBehaviour
{
    [SerializeField] float bulletVelocity;
    [SerializeField] float fireDelay;
    [SerializeField] float howClose;
    [SerializeField] Transform head;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] GameObject bullet;
    [SerializeField] LayerMask objectLayer;
    private Rigidbody player;
    private Vector3  playerPos;
    private float distance;
    private float nextFire;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }

    private void Update()
    {
        playerPos = player.worldCenterOfMass;
        distance = Vector3.Distance(playerPos, transform.position);
        Debug.DrawRay(bulletSpawn.position, (playerPos - bulletSpawn.position).normalized);
        if(distance <= howClose && !Physics.Raycast(bulletSpawn.position, (playerPos - bulletSpawn.position).normalized, distance, objectLayer))//add raycast
        {
            Quaternion desi = Quaternion.Euler((playerPos - transform.position).normalized);
            float angle = Quaternion.Angle(head.rotation, desi);
            head.DOLookAt(playerPos, angle/100); // add constraints
            if(Time.time >= nextFire)
            {
                nextFire = Time.time + fireDelay;
                shoot();
            }
        }
    }

    private void shoot()
    {
        GameObject newBullet = Instantiate(bullet, bulletSpawn.position, head.rotation);
        newBullet.GetComponent<Rigidbody>().AddForce(head.forward * bulletVelocity, ForceMode.Impulse);
        newBullet.GetComponent<BulletInfo>().BaseSpeed = bulletVelocity;
        Destroy(newBullet, 4);
    }
}
