using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    [Header("TriggerDetector")]
    public LayerMask TriggerLayer;
    [HideInInspector] public bool Triggering;
    [HideInInspector] public List<Collider> StayCols = new List<Collider>();
    protected int numCols = 0;

    //Detect tigger with Wall
    private void OnTriggerEnter(Collider other)
    {
        if ((TriggerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (numCols < 0)
                numCols = 0;
            numCols++;
            StayCols.Add(other);

            Triggering = true;
            EnterVars(other);
        }
    }

    protected virtual void EnterVars(Collider other){}

    private void FixedUpdate()
    {
        foreach (Collider other in StayCols)
        {
            StayPer(other);
        }
    }
    protected virtual void StayPer(Collider other){}

    //End trigger
    private void OnTriggerExit(Collider other)
    {
        if (Triggering && (TriggerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            numCols--;
            StayCols.Remove(other);
            ExitVars(other);

            if (numCols <= 0)
            {
                Triggering = false;
                StayCols.Clear();
                ResetVars();
            }
        }
    }

    protected virtual void ExitVars(Collider other){}
    protected virtual void ResetVars(){}
}
