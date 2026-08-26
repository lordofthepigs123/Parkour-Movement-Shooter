using UnityEngine;

public class FootDetectManager : MonoBehaviour
{
    [Header("FootDetectManager")]
    [SerializeField] private float _velocityScale; 
    [SerializeField] private float _distanceMod; 
    [SerializeField] [Range(0,1)] private float _distanceMult; 
    [HideInInspector] public Vector3 TrackPoint;
    [HideInInspector] public float TrackDot;
    [HideInInspector] public bool Tracking;
    [HideInInspector] public bool StateHasEnabled;

    [Header("Components")]
    public Transform Detector; 
    [SerializeField] private Rigidbody rb;
    public FootDetectorHandler Fdh; 

    private void Start()
    {
        Fdh = Detector.GetComponent<FootDetectorHandler>();
    }
    private void Update()
    {
        CalcTrackPoints(Detector, out TrackPoint, out TrackDot, out Tracking);
    }
    private void CalcTrackPoints(Transform _detector, out Vector3 _trackPoint , out float _trackDot, out bool _tracking)
    {
        float netMult = 0;
        Vector3 netPoint = Vector3.zero;

        for (int i = 0; i < Fdh.TargetStore.Count; i++)
        {
            Vector3 closePoint = Fdh.TargetStore[i];
            Vector3 displace = closePoint - _detector.position;
            float tempDot = 1 + Vector3.Dot(rb.linearVelocity, displace.normalized) / _velocityScale; // weight based on velocity alignment, scale to make small vels less impactful
            tempDot = Mathf.Clamp(tempDot, 0 , Mathf.Infinity);

            // combine all target points based on closeness weighting
            float tempMult = tempDot / (1 + Mathf.Pow(displace.magnitude * _distanceMult, _distanceMod)); // point strength : a / (1 + (bd)^c)
            netMult += tempMult;
            netPoint += closePoint * tempMult;
        }

        if (netPoint == Vector3.zero)
        {
            _tracking = false;
            _trackDot = 0;
            _trackPoint = Vector3.zero;
            return;
        }

        Vector3 avgPoint = netPoint / netMult;

        _tracking = true;
        _trackDot = Vector3.Dot(rb.linearVelocity.normalized, (avgPoint - _detector.position).normalized);
        Debug.DrawRay(_detector.position,avgPoint - _detector.position,Color.black);
        _trackPoint = avgPoint; // #
    }
}
