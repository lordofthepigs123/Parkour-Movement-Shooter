using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    [SerializeField] protected PlayerStats ps;
    [SerializeField] protected PlayerColliderManager cm;
    [SerializeField] protected ComboCounter cc;
    [SerializeField] protected InputHandler ih;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator anim;
    [SerializeField] protected float equipTime;

    public bool active;

    [SerializeField] protected PlayerStateMachine sm;
    [SerializeField] protected Wedge wedge;
    [SerializeField] protected WallRunning wr;
    [SerializeField] protected PlayerCam cam;
    public bool isBlocking;
    [SerializeField] protected float meleeBoostRange;
    [SerializeField] protected float meleeboostForce;
    [SerializeField] protected float parryBoostRange;
    [SerializeField] protected float parryboostForce;
    [SerializeField] protected LayerMask attackLayer;
    [SerializeField] protected LayerMask parryLayer;
    [SerializeField] protected GameObject ReflectObj;
    [SerializeField] protected Deflect de;
    [SerializeField] protected float baseHealTime;

    protected float healTime;
    protected bool charging;
    protected float timeSinceAttack;
    protected int currentAttack = 0;
    protected RaycastHit rayHit;
    protected MeleeHandler mh;
    protected MeleeStateDetector ms;
    protected ItemInfo Item;

    protected bool inBlock;
    protected bool healing;
    protected float timeSinceBlock;
    protected int currentParry = 0;
    protected RaycastHit pRayHit;
    protected float equipMultiplier;
}
