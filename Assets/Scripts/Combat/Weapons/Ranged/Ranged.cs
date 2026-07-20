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

    private void Awake()
    {
        rh = gameObject.GetComponentInParent<RangedHandler>();
        rs = gameObject.GetComponentInChildren<RangedStateDetector>();
        Item = GetComponent<ItemInfo>();
        readyToShoot = true;
        bulletsLeft = magazineSize;
        spreadIntensity = hipSpreadIntensity;
        rh.magazineSize = magazineSize;
        rh.bulletValue = bulletValue;
        rh.baseReloadTime = baseReloadTime;
        //Set animation speed of equiping
        equipMultiplier = (1 + ps.HNDL.Value / 40) / (float)(Item.itemWeight / 40 + (Item.twoHanded ? 1.5 : 1));
        anim.SetFloat("EquipSpd", equipMultiplier);
    }

    private void Update()
    {
        active = rh.active;

        equipHandler();

        if (active && rh.equiped)
        {
            if (!isReloading)
            {
                ReloadWeapon();
                AimDownSights();
                shootWeapon();
            }
        }
    }

    private void shootWeapon()
    {
        if (ih.LMB && readyToShoot && bulletsLeft > 0)
        {
            ih.activatedLMB = true;
            readyToShoot = false;

            bulletsLeft--;
            rh.bulletsLeft = bulletsLeft;

            Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

            //Create bullet
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
            bullet.transform.up = shootingDirection;
            bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
            //Apply stats to bullet
            BulletInfo bi = bullet.GetComponent<BulletInfo>();
            bi.BaseSpeed = bulletVelocity;
            bi.Explosive = Explosive;
            bi.ExplosionRadi = ExplosionRadi;
            bi.ExplosionStr = ExplosionStr;
            bi.ExplosionDmg = ExplosionDmg;
            //destroy bullet
            StartCoroutine(DestroyBullet(bullet, bulletPrefabLifeTime));

            anim.SetTrigger("Shoot");
            recoil.recoil();
            if (allowReset)
            {
                Invoke("ResetShot", shootingDelay);
                allowReset = false;
            }
        }
    }

    public Vector3 CalculateDirectionAndSpread()
    {
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
        Vector3 direction = targetPoint - bulletSpawn.position;

        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(0, y, z);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private IEnumerator DestroyBullet(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    private void AimDownSights()
    {
        if (Input.GetKeyDown(ih._RMB))
        {
            anim.ResetTrigger("exitADS");
            anim.SetTrigger("enterADS");
            HUDManager.Instance.crosshair.SetActive(false);

            spreadIntensity = adsSpreadIntensity;
        }

        if (Input.GetKeyUp(ih._RMB))
        {
            anim.SetTrigger("exitADS");
            HUDManager.Instance.crosshair.SetActive(true);

            spreadIntensity = hipSpreadIntensity;
        }
    }

    private void ReloadWeapon()
    {
        if (ih.keyR && bulletsLeft < magazineSize)//&& cc.combo >= bulletValue
        {
            ih.activatedR = true;
            rh.bulletsLeft = bulletsLeft;
            cc.OnReload();
            anim.ResetTrigger("CancelReload");
            anim.SetTrigger("Reload");
            isReloading = true;
            reloadTime = rh.reloadTime;
            //anim.speed
            Invoke("ReloadAmmo", reloadTime);
        }
    }

    private void ReloadAmmo()
    {
        bulletsLeft = rh.bulletsLeft;
        isReloading = false;

        //anim.speed = 1;
        anim.SetTrigger("CancelReload");
    }

    private void equipHandler()
    {
        //set animation speed
        if (rh.equiping)
        {
            anim.SetBool("Equiping", true);
            Invoke("equiping", equipTime * equipMultiplier);
        }
        if (rh.unequiping)
        {
            anim.SetBool("Unequiping", true);
            Invoke("unequiping", equipTime * equipMultiplier);
        }
        if (rh.equiped)
        {
            anim.SetBool("Equiping", false);
            rh.equiping = false;
        }
        else
        {
            anim.SetBool("Unequiping", false);
            rh.unequiping = false;
        }
    }
    private void equiping()
    {
        rh.equiped = true;
    }
    private void unequiping()
    {
        rh.equiped = false;
    }
}
