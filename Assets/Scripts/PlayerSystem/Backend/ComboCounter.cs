using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboCounter : MonoBehaviour
{
    public int combo;
    public float comboTimer;
    public float maxComboTimer;
    [SerializeField] TextMeshProUGUI comboDisplay;
    [SerializeField] Slider _comboTimer;
    private PlayerStats ps;

    public float reload;
    public float heal;
    public int multiplier;

    private int lastCombo;
    private bool slide;
    private bool wallJump;
    private bool meleeBoost;
    private bool wedgeBoost;
    private bool rocketJump;
    private bool wallDash;
    private bool parry;
    private bool reflect;
    private bool crit;
    private bool recoilBoost;
    private bool parryBoost;
    private bool reflectBoost;
    private bool kill;

    private void Start()
    {
        combo = 0;
        lastCombo = combo;
        ps = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (combo != lastCombo)
        {
            timerControls();
            SetMaxTime();
            lastCombo = combo;
            comboTimer = maxComboTimer;
            //ps.SPD.RemoveAllModifiersFromSource(this);
            //ps.SPD.AddModifier(new StatModifier(combo * 4, StatModType.Flat, this));
            //ps.BLNC.RemoveAllModifiersFromSource(this);
            //ps.BLNC.AddModifier(new StatModifier(combo, StatModType.Flat, this));
        }

        if (comboTimer > 0 && combo > 0)
        {
            comboTimer -= Time.deltaTime;
            _comboTimer.value = comboTimer;
        }
        else
        {
            combo = 0;
        }

        comboDisplay.text = combo.ToString();
    }

    private void timerControls()
    {
        if (combo < 5)
        {
            maxComboTimer = 10;
        }
        else if (combo < 10)
        {
            maxComboTimer = 8;
        }
        else if (combo < 15)
        {
            maxComboTimer = 6;
        }
        else if (combo < 20)
        {
            maxComboTimer = 4.5f;
        }
        else if (combo < 25)
        {
            maxComboTimer = 3;
        }
        else if (combo < 30)
        {
            maxComboTimer = 2.6f;
        }
        else if (combo < 35)
        {
            maxComboTimer = 2.3f;
        }
        else if (combo < 40)
        {
            maxComboTimer = 2.15f;
        }
        else if (combo < 45)
        {
            maxComboTimer = 2;
        }
        else if (combo < 50)
        {
            maxComboTimer = 1.9f;
        }
        else if (combo < 55)
        {
            maxComboTimer = 1.8f;
        }
        else if (combo < 60)
        {
            maxComboTimer = 1.75f;
        }
        else if (combo < 65)
        {
            maxComboTimer = 1.7f;
        }
        else if (combo < 70)
        {
            maxComboTimer = 1.65f;
        }
        else if (combo < 75)
        {
            maxComboTimer = 1.6f;
        }
        else if (combo < 80)
        {
            maxComboTimer = 1.55f;
        }
        else if (combo < 85)
        {
            maxComboTimer = 1.52f;
        }
        else if (combo < 90)
        {
            maxComboTimer = 1.49f;
        }
        else if (combo < 95)
        {
            maxComboTimer = 1.46f;
        }
        else if (combo < 100)
        {
            maxComboTimer = 1.43f;
        }
        else
        {
            maxComboTimer = 1.4f;
        }

    }

    public void SetMaxTime()
    {
        _comboTimer.maxValue = maxComboTimer;
        _comboTimer.value = maxComboTimer;
    }

    public void OnHeal()
    {
        multiplier = combo;

        combo = (int)ps.Scombo.Value;
    }

    public void OnReload()
    {
        multiplier = combo;

        combo = (int)ps.Scombo.Value;
    }

    public void onSlide()
    {
        if (!slide)
        {
            combo++;
            slide = true;
            Invoke("resetSlide", 3);
        }
    }

    private void resetSlide()
    {
        slide = false;
    }

    public void onWallJump()
    {
        if (!wallJump)
        {
            combo++;
            wallJump = true;
            Invoke("resetWallJump", 2);
        }
    }

    public void resetWallJump()
    {
        wallJump = false;
    }

    public void onMeleeBoost()
    {
        if (!meleeBoost)
        {
            combo++;
            meleeBoost = true;
            Invoke("resetMeleeBoost", 2);
        }
    }

    public void resetMeleeBoost()
    {
        meleeBoost = false;
    }

    public void onWedgeBoost()
    {
        if (!wedgeBoost)
        {
            combo++;
            wedgeBoost = true;
            Invoke("resetWedgeBoost", 2);
        }
    }

    public void resetWedgeBoost()
    {
        wedgeBoost = false;
    }

    public void onRocketJump()//
    {
        if (!rocketJump)
        {
            combo++;
            rocketJump = true;
            Invoke("resetRocketJump", 1.5f);
        }
    }

    public void resetRocketJump()//
    {
        rocketJump = false;
    }

    public void onWallDash()
    {
        if (!wallDash)
        {
            combo++;
            wallDash = true;
            Invoke("resetWallDash", 1f);
        }
    }

    public void resetWallDash()
    {
        wallDash = false;
    }

    public void onParry()//
    {
        if (!parry)
        {
            combo++;
            parry = true;
            Invoke("resetParry", 0.5f);
        }
    }

    public void resetParry()//
    {
        parry = false;
    }

    public void onReflect()//
    {
        if (!reflect)
        {
            combo++;
            reflect = true;
            Invoke("resetReflect", 3);
        }
    }

    public void resetReflect()//
    {
        reflect = false;
    }

    public void onCrit()//
    {
        if (crit!)
        {
            combo++;
            crit = true;
            Invoke("resetCrit", 1.5f);
        }
    }

    public void resetCrit()//
    {
        crit = false;
    }

    public void onRecoilBoost()
    {
        if (!recoilBoost)
        {
            combo++;
            recoilBoost = true;
            Invoke("resetRecoilBoost", 4);
        }
    }

    public void resetRecoilBoost()
    {
        recoilBoost = false;
    }

    public void onParryBoost()
    {
        if (!parryBoost)
        {
            combo++;
            parryBoost = true;
            Invoke("resetParryBoost", 0.5f);
        }
    }

    public void resetParryBoost()
    {
        parryBoost = false;
    }

    public void onReflectBoost()//
    {
        if (!reflectBoost)
        {
            combo++;
            reflectBoost = true;
            Invoke("resetReflectBoost", 4);
        }
    }

    public void resetReflectBoost()//
    {
        reflectBoost = false;
    }

    public void onKill()//
    {
        if (!kill)
        {
            combo++;
            kill = true;
            Invoke("resetKill", 1);
        }
    }

    public void resetKill()//
    {
        kill = false;
    }
}