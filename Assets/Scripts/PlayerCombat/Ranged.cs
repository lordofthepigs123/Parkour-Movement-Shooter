using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranged : MonoBehaviour
{
    [SerializeField] protected PlayerStats ps;
    [SerializeField] protected ComboCounter cc;
    [SerializeField] protected InputHandler ih;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator anim;
    [SerializeField] protected float equipTime;

    public bool active;

    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Camera cam;
    //apply to bullet
    [SerializeField] protected float bulletVelocity;
    [SerializeField] protected bool Explosive;
    [SerializeField] protected float ExplosionRadi;
    [SerializeField] protected float ExplosionStr;
    [SerializeField] protected float ExplosionDmg;

    [SerializeField] protected float bulletPrefabLifeTime;
    public bool isShooting, readyToShoot;
    [SerializeField] protected float shootingDelay;
    [SerializeField] protected LayerMask bulletLayer;
    [SerializeField] protected float baseReloadTime;
    [SerializeField] protected GameObject crossHair;
    [SerializeField] protected ProceduralRecoil recoil;
    public int magazineSize, bulletsLeft;
    public float reloadAmount;

    [SerializeField] protected float hipSpreadIntensity;
    [SerializeField] protected float adsSpreadIntensity;

    public float bulletValue; //amount of combo to replenish each bullet
    [SerializeField] protected Transform bulletSpawn;

    protected float reloadTime;
    protected float spreadIntensity;
    protected bool isReloading = false;
    protected RangedHandler rh;
    protected RangedStateDetector rs;
    protected ItemInfo Item;

    protected bool allowReset = true;
    protected RaycastHit hit;
    protected bool rayHit;
    protected float equipMultiplier;
}
