using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerStats ps;
    [SerializeField] Transform orientation;
    [SerializeField] Transform cam;
    public Vector3 baseInputDir;
    public Vector3 lastInputDir;
    public float acuTime;
    public float buffSPACE;
    private float BufferCounterSPACE;
    public KeyCode _SPACE = KeyCode.Space;
    public bool ikeySPACE;
    public bool rkeySPACE;
    public bool heldSPACE;
    public float holdTimeSPACE;
    public bool activatedSPACE;
    public bool forceUpSpace;
    [SerializeField] SPbarHandeler bs;
    [SerializeField] float cutOffAcu; // between 0 - 1
    public float acuSPACE;

    public float keyHorizontal;
    public float keyVertical;

    [SerializeField] float buffQ;
    private float BufferCounterQ;
    public KeyCode _Q = KeyCode.Q;
    public bool keyQ;
    public bool heldQ;
    public float holdTimeQ;
    public bool activatedQ;

    [SerializeField] float buffF;
    private float BufferCounterF;
    public KeyCode _F = KeyCode.F;
    public bool keyF;
    public bool heldF;
    public float holdTimeF;
    public bool activatedF;

    [SerializeField] float buffE;
    private float BufferCounterE;
    public KeyCode _E = KeyCode.E;
    public bool keyE;
    public bool heldE;
    public float holdTimeE;
    public bool activatedE;

    [SerializeField] float buffR;
    private float BufferCounterR;
    public KeyCode _R = KeyCode.R;
    public bool keyR;
    public bool heldR;
    public float holdTimeR;
    public bool activatedR;

    [SerializeField] float buffX;
    private float BufferCounterX;
    public KeyCode _X = KeyCode.X;
    public bool keyX;
    public bool heldX;
    public float holdTimeX;
    public bool activatedX;


    [SerializeField] float buffSHIFT;
    private float BufferCounterSHIFT;
    private float BufferVerticalSHIFT;
    private float BufferHorizontalSHIFT;
    public KeyCode _SHIFT = KeyCode.LeftShift;
    public bool keySHIFT;
    public bool heldSHIFT;
    public float holdTimeSHIFT;
    public bool activatedSHIFT;

    [SerializeField] float buffLMB;
    private float BufferCounterLMB;
    public KeyCode _LMB = KeyCode.Mouse0;
    public bool LMB;
    public bool heldLMB;
    public float holdTimeLMB;
    public bool activatedLMB;

    [SerializeField] float buffRMB;
    private float BufferCounterRMB;
    public KeyCode _RMB = KeyCode.Mouse1;
    public bool RMB;
    public bool heldRMB;
    public float holdTimeRMB;
    public bool activatedRMB;

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        //SPACE
        if (Input.GetKeyDown(_SPACE))
        {
            holdTimeSPACE = 0;
            BufferCounterSPACE = buffSPACE;
            ikeySPACE = true;
            rkeySPACE = false;
            activatedSPACE = false;

            bs.Held = true;
        }
        else if (BufferCounterSPACE > 0 && !activatedSPACE)
        {
            BufferCounterSPACE -= Time.deltaTime;
        }
        else
        {
            ikeySPACE = false;
        }

        if ((Input.GetKeyUp(_SPACE) || forceUpSpace) && heldSPACE)
        {
            //Off instant
            forceUpSpace = false;

            if (activatedSPACE)
            {
                //On real
                rkeySPACE = true;

                float _acu = holdTimeSPACE / acuTime;//between 0 - inf
                if (_acu > 1)
                    _acu = 2 - _acu;
                _acu = (_acu - cutOffAcu) / (1 - cutOffAcu); // below cutoff becomes negative
                acuSPACE = Mathf.Clamp(_acu, -1, 1);
            }

            bs.Held = false;
        }

        if (Input.GetKey(_SPACE))
        {
            holdTimeSPACE += Time.deltaTime;
            heldSPACE = true;
        }
        else
        {
            heldSPACE = false;
        }

        //SPACE Bar UI 
        bs.Value = holdTimeSPACE / (acuTime * 2);
        //horizontal
        keyHorizontal = Input.GetAxisRaw("Horizontal");
        //vertical
        keyVertical = Input.GetAxisRaw("Vertical");
        //Universal Input direction
        baseInputDir = (orientation.forward * keyVertical + orientation.right * keyHorizontal).normalized;
        if (baseInputDir != Vector3.zero)
            lastInputDir = baseInputDir;
        //Q
        if (Input.GetKeyDown(_Q))
        {
            holdTimeQ = 0;
            BufferCounterQ = buffQ;
            keyQ = true;
            activatedQ = false;
        }
        else if (BufferCounterQ > 0 && !activatedQ)
        {
            BufferCounterQ -= Time.deltaTime;
        }
        else
        {
            keyQ = false;
        }

        if (Input.GetKey(_Q))
        {
            holdTimeQ += Time.deltaTime;
            heldQ = true;
        }
        else
        {
            heldQ = false;
        }

        //F
        if (Input.GetKeyDown(_F))
        {
            holdTimeF = 0;
            BufferCounterF = buffF;
            keyF = true;
            activatedF = false;
        }
        else if (BufferCounterF > 0 && !activatedF)
        {
            BufferCounterF -= Time.deltaTime;
        }
        else
        {
            keyF = false;
        }

        if (Input.GetKey(_F))
        {
            holdTimeF += Time.deltaTime;
            heldF = true;
        }
        else
        {
            heldF = false;
        }

        //E
        if (Input.GetKeyDown(_E))
        {
            holdTimeE = 0;
            BufferCounterE = buffE;
            keyE = true;
            activatedE = false;
        }
        else if (BufferCounterE > 0 && !activatedE)
        {
            BufferCounterE -= Time.deltaTime;
        }
        else
        {
            keyE = false;
        }

        if (Input.GetKey(_E))
        {
            holdTimeE += Time.deltaTime;
            heldE = true;
        }
        else
        {
            heldE = false;
        }

        //R
        if (Input.GetKeyDown(_R))
        {
            holdTimeR = 0;
            BufferCounterR = buffR;
            keyR = true;
            activatedR = false;
        }
        else if (BufferCounterR > 0 && !activatedR)
        {
            BufferCounterR -= Time.deltaTime;
        }
        else
        {
            keyR = false;
        }

        if (Input.GetKey(_R))
        {
            holdTimeR += Time.deltaTime;
            heldR = true;
        }
        else
        {
            heldR = false;
        }
        //X
        if (Input.GetKeyDown(_X))
        {
            holdTimeX = 0;
            BufferCounterX = buffX;
            keyX = true;
            activatedX = false;
        }
        else if (BufferCounterX > 0 && !activatedX)
        {
            BufferCounterX -= Time.deltaTime;
        }
        else
        {
            keyX = false;
        }

        if (Input.GetKey(_X))
        {
            holdTimeX += Time.deltaTime;
            heldX = true;
        }
        else
        {
            heldX = false;
        }
        //SHIFT
        if (Input.GetKeyDown(_SHIFT))
        {
            holdTimeSHIFT = 0;
            keySHIFT = true;
            activatedSHIFT = false;
            if (BufferCounterSHIFT <= 0)//else use previous buffer
            {
                BufferVerticalSHIFT = keyVertical;
                BufferHorizontalSHIFT = keyHorizontal;
            }
            BufferCounterSHIFT = buffSHIFT;
        }
        else if (BufferCounterSHIFT > 0 && !activatedSHIFT)
        {
            //decrease counter
            BufferCounterSHIFT -= Time.deltaTime;
            //update buffer in real-time
            if (Mathf.Abs(BufferVerticalSHIFT) < Mathf.Abs(keyVertical))
                BufferVerticalSHIFT = keyVertical;
            if (Mathf.Abs(BufferHorizontalSHIFT) < Mathf.Abs(keyHorizontal))
                BufferHorizontalSHIFT = keyHorizontal;
        }
        else
        {
            //reset buffers
            BufferVerticalSHIFT = 0;
            BufferHorizontalSHIFT = 0;
            keySHIFT = false;
        }

        if (Input.GetKey(_SHIFT))
        {
            holdTimeSHIFT += Time.deltaTime;
            heldSHIFT = true;
        }
        else
        {
            heldSHIFT = false;
        }
        //LMB
        if (Input.GetKeyDown(_LMB))
        {
            holdTimeLMB = 0;
            BufferCounterLMB = buffLMB;
            LMB = true;
            activatedLMB = false;
        }
        else if (BufferCounterLMB > 0 && !activatedLMB)
        {
            BufferCounterLMB -= Time.deltaTime;
        }
        else
        {
            LMB = false;
        }

        if (Input.GetKey(_LMB))
        {
            holdTimeLMB += Time.deltaTime;
            heldLMB = true;
        }
        else
        {
            heldLMB = false;
        }
        //RMB
        if (Input.GetKeyDown(_RMB))
        {
            holdTimeRMB = 0;
            BufferCounterRMB = buffRMB;
            RMB = true;
            activatedRMB = false;
        }
        else if (BufferCounterRMB > 0 && !activatedRMB)
        {
            BufferCounterRMB -= Time.deltaTime;
        }
        else
        {
            RMB = false;
        }

        if (Input.GetKey(_RMB))
        {
            holdTimeRMB += Time.deltaTime;
            heldRMB = true;
        }
        else
        {
            heldRMB = false;
        }
    }
    
    public Vector3 planeInputDir(Vector3 normal, bool last) // possibly allow lastInputDir#
    {
        Quaternion fromTo = Quaternion.FromToRotation(Vector3.up, normal);
        if (last)
            return fromTo * lastInputDir;
        return fromTo * baseInputDir;
    }
}