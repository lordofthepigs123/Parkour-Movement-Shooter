using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public CharacterStat maxHP;//
    public float currentHealth;
    public CharacterStat weight;//
    public CharacterStat Scombo;//
    public CharacterStat MDMG;
    public CharacterStat RDMG;
    public CharacterStat DEF;
    public CharacterStat VIT;
    public CharacterStat SPD;// - 300
    public CharacterStat BLNC;//
    public CharacterStat LINKP;
    public CharacterStat EFI;
    public CharacterStat CRT;
    public CharacterStat LOAD;
    public CharacterStat PRY;
    public CharacterStat HNDL;// 0 - 100
    public CharacterStat ASPD;

    private ComboCounter cc;

    [SerializeField] Rigidbody rb;

    public void Start()
    {
        cc = GetComponent<ComboCounter>();
        currentHealth = maxHP.Value;
    }

    public void TakeDM(float damage)
    {
        damage -= DEF.Value;
        damage = Mathf.Clamp(damage, 0, float.MaxValue);
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            die();
        }
    }

    /*
    public void Reload()
    {
        if (cc.multiplier < rh.bulletValue * (rh.magazineSize - rh.bulletsLeft))
        {
            reloadAmount = (int)(cc.multiplier / rh.bulletValue);
            rh.reloadTime = rh.baseReloadTime;
        }
        else if (cc.multiplier < rh.bulletValue * (rh.magazineSize - rh.bulletsLeft) * 2)
        {
            reloadAmount = rh.magazineSize - rh.bulletsLeft;
            rh.reloadTime = rh.baseReloadTime / (1 + (cc.multiplier - rh.bulletValue * (rh.magazineSize - rh.bulletsLeft)) / 3);
        }
        else
        {
            reloadAmount = rh.magazineSize - rh.bulletsLeft;
            rh.reloadTime = 0;
            spillOver = (int)(cc.multiplier - rh.bulletValue * (rh.magazineSize - rh.bulletsLeft) * 2);
        }
        reloadAmount = (int) Mathf.Clamp(reloadAmount, 0, float.MaxValue);
        spillOver = Mathf.Clamp(spillOver, 0, int.MaxValue);

        if (rh.magazineSize - rh.bulletsLeft < reloadAmount)
        {
            rh.bulletsLeft = rh.magazineSize;
        }
        else
        {
            rh.bulletsLeft += reloadAmount;
        }

        cc.combo += spillOver;
    }
    */

    public virtual void die()
    {
        //add death
    }

    public void Update()
    {
        rb.mass = weight.Value / 100;
        Mathf.Clamp(SPD.Value, 30, float.MaxValue);
        Mathf.Clamp(BLNC.Value, 20, float.MaxValue);
    }
}
