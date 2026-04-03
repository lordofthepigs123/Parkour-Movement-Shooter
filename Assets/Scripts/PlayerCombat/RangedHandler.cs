using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedHandler : MonoBehaviour
{
    public Transform gun;
    private ItemInfo Item;
    private ProceduralRecoil pr;
    public float snappiness;
    private WeaponHandler wh;
    private float type;
    [SerializeField] PlayerStats ps;
    [SerializeField] float handType;//0 Right, 1 Left
    public bool active;
    public int magazineSize, bulletsLeft;
    public float bulletValue;
    public float baseReloadTime;
    public float reloadTime;
    public int reloadAmount;
    public bool twoHanded;
    public bool equiping;
    public bool unequiping;
    public bool equiped;

    private void Update()
    {
        Item = GetComponentInChildren<ItemInfo>();
        wh = GetComponentInParent<WeaponHandler>();

        active = (wh.activeHand == handType);

        type = Item.type;
        Item.inHand = handType;//what hand the item is in

        //recoil
        gun = GetComponentInChildren<Transform>();
        pr = GetComponentInChildren<ProceduralRecoil>();
        snappiness = pr.snappiness;

        //check for item weight and two handed
        twoHanded = Item.twoHanded;
        if (Item.itemWeight > ps.LOAD.Value + 20)
        {
            //can't equip item
        }
        else if (Item.itemWeight > ps.LOAD.Value)//
        {
            twoHanded = true;
        }
    }
}
