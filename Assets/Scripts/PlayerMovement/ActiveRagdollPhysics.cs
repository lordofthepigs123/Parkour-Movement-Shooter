using UnityEngine;

public class ActiveRagdollPhysics : MonoBehaviour
{
    [Header("ActiveRagdollPhysics")]

    [Header("Components")]
    private PlayerColliderManager cm;
    private MainRagdollHandeler rh;

    private void Start()
    {
        cm = GetComponent<PlayerColliderManager>();
        rh = GetComponent<MainRagdollHandeler>();
    }
}
