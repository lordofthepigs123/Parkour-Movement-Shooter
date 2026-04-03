using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrientation : MonoBehaviour
{
    [SerializeField] Transform playerT;
    private void Update()
    {
        this.transform.position = playerT.transform.position + playerT.transform.up * playerT.localScale.y; //*0.5f
    }
}
