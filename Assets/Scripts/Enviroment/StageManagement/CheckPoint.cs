using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Collider trigger;
    public bool activated;
    public Vector3 resetPos;
    [SerializeField] CheckPointHandeler cph;

    private void Start()
    {
        trigger = GetComponent<Collider>();
        resetPos = trigger.bounds.center;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((cph.playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            activated = true;
            Invoke("resetActive", 0.1f);
        }
    }

    private void resetActive()
    {
        activated = false;
    }

}
