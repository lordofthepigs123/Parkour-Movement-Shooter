using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHandler : MonoBehaviour
{
    private ItemInfo Item;
    private WeaponHandler wh;
    private float type;
    [SerializeField] PlayerStats ps;
    [SerializeField] float handType;//0 Right, 1 Left
    public bool active;
    public float baseHealTime;
    public float healTime;
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
