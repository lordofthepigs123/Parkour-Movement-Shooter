using UnityEngine;
using UnityEngine.UI;

public class HeatDashIndicator : MonoBehaviour
{
    [SerializeField] HeatDash hd;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }
    private void Update()
    {
        image.enabled = hd.canHeatDash;
    }
}
