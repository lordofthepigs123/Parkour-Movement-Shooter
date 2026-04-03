using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RailDetectHandler : MonoBehaviour
{
    [Header("RailDetectHandler")]
    [SerializeField] PlayerGrind pg;
    [SerializeField] PlayerColliderManager cm;
    [SerializeField] LayerMask grindable;
    private List<Collider> ColliderList = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != Mathf.Log(grindable.value, 2))// change once swing implimented #
            return;
        
        //pg.currentRailScript = other.gameObject.GetComponent<RailScript>();
        //pg.currentRailCol = other.gameObject.GetComponent<Collider>();
        if(!ColliderList.Contains(other))
        {
            //add the object to the list
            ColliderList.Add(other);
        }
        pg.inProximity = true;

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != Mathf.Log(grindable.value, 2))// change once swing implimented #
            return;

        RailScript holdScript = pg.currentRailScript;
        Collider holdCol = pg.currentRailCol;
        float holdDis = 1000;//arbitrary large
        foreach (Collider collider in ColliderList)
        {
            RailScript tempRailScript = collider.transform.gameObject.GetComponent<RailScript>();
            Vector3 tempPoint;
            tempRailScript.CalculateTargetRailPoint(transform.position, out tempPoint);// evaluate closest location
            float tempDis = (tempPoint - transform.position).magnitude;
            if (tempDis < holdDis)
            {
                holdDis = tempDis;
                holdScript = tempRailScript;
                holdCol = collider;
            }
        }
        
        pg.getVarsRail(holdScript);
        pg.triggerNormal = (transform.position - pg.swingPoint).normalized;

        pg.currentRailScript = holdScript;
        pg.currentRailCol = holdCol;
    }

    private void OnTriggerExit(Collider other)
    {
        //if the object is in the list
        if (!ColliderList.Contains(other))
            return;

        //remove it from the list
        ColliderList.Remove(other);
        if (ColliderList.Count <= 0)
        { 
            if (!pg.aboveRail)
                cm.enableCol(true, pg.currentRailCol);//enable collisions between player and rail
            pg.currentRailScript = null;
            pg.currentRailCol = null;
            pg.inProximity = false;
            pg.aboveRail = false;
        }
    }
}
