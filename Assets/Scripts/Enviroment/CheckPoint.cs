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
        if (other.gameObject.layer == Mathf.Log(cph.playerLayer.value, 2))
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
