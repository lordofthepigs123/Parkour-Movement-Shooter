using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public float activeHand = 0;//0 Right, 1 Left
    private RangedHandler rh;
    private MeleeHandler mh;
    private InputHandler ih;
    private PlayerStats ps;
    [SerializeField] Transform player;

    private void Start()
    {
        rh = GetComponentInChildren<RangedHandler>();
        mh = GetComponentInChildren<MeleeHandler>();
        ih = player.GetComponent<InputHandler>();
        ps = player.GetComponent<PlayerStats>();
    }

    private void Update()
    {
        stateHandler();

        if (ih.keyQ && !ih.activatedQ)
        {
            ih.activatedQ = true;
            //set active hand
            if (activeHand == 0)
            {
                activeHand = 1;
            }
            else
            {
                activeHand = 0;
            }
        }
    }
    private void stateHandler()
    {
        if (activeHand == 0)
        {
            if (!((!rh.equiped || mh.twoHanded && mh.equiped) && !rh.equiping && !mh.unequiping))
                return;
            if (!mh.equiped || (!rh.twoHanded && !mh.twoHanded))
            {
                rh.equiping = true;
                return;
            }
            mh.unequiping = true;
            if (!rh.twoHanded)
            {
                rh.equiping = true;
            }
            return;
        }
        else
        {
            if (!((!mh.equiped || rh.twoHanded && rh.equiped) && !mh.equiping && !rh.unequiping))
                return;
            if (!rh.equiped || (!rh.twoHanded && !mh.twoHanded))
            {
                mh.equiping = true;
                return;
            }
            rh.unequiping = true;
            if (!mh.twoHanded)
            {
                mh.equiping = true;
            }
            return;
        }
    }
}
