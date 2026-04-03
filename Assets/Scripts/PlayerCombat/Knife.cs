using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : Melee
{

    private void Awake()
    {
        mh = gameObject.GetComponentInParent<MeleeHandler>();
        ms = gameObject.GetComponentInChildren<MeleeStateDetector>();
        Item = GetComponent<ItemInfo>();
        mh.baseHealTime = baseHealTime;
        //Set animation speed of equiping
        equipMultiplier = (1 + ps.HNDL.Value / 40) / (float)(Item.itemWeight / 40 + (Item.twoHanded ? 1.5 : 1));
        anim.SetFloat("EquipSpd", equipMultiplier);
    }

    private void Update()
    {
        active = mh.active;
        timeSinceAttack += Time.deltaTime;
        timeSinceBlock += Time.deltaTime;

        equipHandler();

        if (active && mh.equiped)
        {
            if (!inBlock && !healing)
                Attack();
            if (!charging && !healing)
                Block();
            if (!charging && !inBlock)
                Heal();
        }
    }

    private void Attack()
    {
        if (ih.heldLMB && ih.holdTimeLMB > 0.3f)
        {
            anim.SetBool("Charge", true);
            charging = true;
        }

        if (Input.GetKeyUp(ih._LMB) && (timeSinceAttack > 0.4f || charging == true))
        {
            ms.isAttacking = true;
            if (charging)
            {
                anim.SetBool("Charge", false);
                anim.SetTrigger("Thrust");
                timeSinceAttack = 0;
                charging = false;

                if (!cm.grounded)
                {
                    wedge.raycasting = true;
                }
            }
            else
            {
                currentAttack += 1;

                if (currentAttack > 2)
                    currentAttack = 1;

                //reset
                if (timeSinceAttack > 0.9f)
                    currentAttack = 1;

                //call attack
                anim.SetTrigger("Hit" + currentAttack);

                //reset timer
                timeSinceAttack = 0;

                if (!cm.grounded)
                {
                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out rayHit, meleeBoostRange, attackLayer))
                    {
                        cc.onMeleeBoost();
                        Vector3 OriginPos = new Vector3(rb.position.x, rb.position.y + 2f, rb.position.z);
                        Vector3 direction = -(rayHit.point - OriginPos).normalized;

                        meleeboostForce = meleeboostForce / ps.weight.Value * 100;
                        rb.AddForce(direction * meleeboostForce, ForceMode.Impulse);

                        if (sm.wallRunning)
                        {
                            wr.exitingWall = true;
                            wr.exitWallTimer = wr.exitWallTime;
                        }
                    }
                }
            }
        }
        
    }
    private void Block()
    {
        if (ih.heldRMB && ih.holdTimeRMB > 0.3f)
        {
            anim.SetBool("inBlock", true);
            inBlock = true;
            de.inBlock = true;
        }

        if (ih.RMB)
        {
            ih.activatedRMB = true;
            Reflecting();
        }

        if (Input.GetKeyUp(ih._RMB) && (timeSinceBlock > 0.9f || inBlock == true))
        {
            isBlocking = true;
            if (inBlock)
            {
                anim.SetBool("inBlock", false);
                timeSinceBlock = 0;
                inBlock = false;
                de.inBlock = false;
            }
            else
            {
                currentParry += 1;

                if (currentParry > 2)
                    currentParry = 1;

                //reset
                if (timeSinceBlock > 1.3f)
                    currentParry = 1;

                //call parry
                anim.SetTrigger("Parry" + currentParry);

                //reset timer
                timeSinceBlock = 0;
            }
        }

        if (!cm.grounded && ms.pParry)
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out pRayHit, parryBoostRange, parryLayer))
            {
                cc.onParryBoost();
                Vector3 OriginPos = new Vector3(rb.position.x, rb.position.y + 2f, rb.position.z);
                Vector3 direction = -(pRayHit.point - OriginPos).normalized;

                float newParryForce = parryboostForce / ps.weight.Value * 100;
                rb.AddForce(direction * newParryForce, ForceMode.Impulse);
                ms.pParry = false;

                if (sm.wallRunning)
                {
                    wr.exitingWall = true;
                    wr.exitWallTimer = wr.exitWallTime;
                }
            }
        }
    }
    private void Reflecting()
    {
        //activate projectile reflecting hitbox
        //detect when projectile is in hitbox
        //activate reflect combo
    }

    private void Heal()
    {
        if (ih.keyR && ps.currentHealth < ps.maxHP.Value && cc.combo > 0)
        {
            ih.activatedR = true;
            cc.OnHeal();
            ps.CalcHeal();
            anim.SetTrigger("Heal");
            healing = true;
            healTime = mh.healTime;
            //anim speed
            Invoke("regainHealth", healTime);
        }
    }

    private void regainHealth()
    {
        ps.Heal();
        healing = false;
        anim.SetTrigger("CancelHeal");
    }

    //used in animation event to reset
    public void ResetAttack()
    {
        isBlocking = false;
        ms.isAttacking = false;
    }

    private void equipHandler()
    {
        //set animation speed
        if (mh.equiping)
        {
            anim.SetBool("Equiping", true);
            Invoke("equiping", equipTime * equipMultiplier);
        }
        if (mh.unequiping)
        {
            anim.SetBool("Unequiping", true);
            Invoke("unequiping", equipTime * equipMultiplier);
        }
        if (mh.equiped)
        {
            anim.SetBool("Equiping", false);
            mh.equiping = false;
        }
        else
        {
            anim.SetBool("Unequiping", false);
            mh.unequiping = false;
        }
    }
    private void equiping()
    {
        mh.equiped = true;
    }
    private void unequiping()
    {
        mh.equiped = false;
    }
}

