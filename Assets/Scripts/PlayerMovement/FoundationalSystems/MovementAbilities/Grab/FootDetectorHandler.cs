using UnityEngine;

public class FootDetectorHandler : MonoBehaviour
{
    private Quaternion ogRot;
    [Header("Components")]
    [SerializeField] private Transform cam;

    private void Start()
    {
        ogRot = transform.rotation;
    }

    private void Update()
    {
        transform.rotation = cam.rotation * ogRot;
    }
}
