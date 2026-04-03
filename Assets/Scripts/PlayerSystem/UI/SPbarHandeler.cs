using UnityEngine;
using UnityEngine.UI;

public class SPbarHandeler : MonoBehaviour
{
    private bool _enabled;
    [SerializeField] float maxAcu;
    [SerializeField] Slider SP_bar;
    [SerializeField] GameObject SP_obj;
    [SerializeField] GameObject effect_obj;
    public bool Held;
    public float Value;
    public float baseTime;
    private float timer;
    private Vector3 basePos;
    private void Start()
    {
        var renderers = SP_obj.GetComponentsInChildren<Image>();
        foreach (Image r in renderers)
        {
            r.enabled = false;
        }
        var erenderers = effect_obj.GetComponents<Image>();
        foreach (Image r in erenderers)
        {
            r.enabled = true;
            basePos = r.rectTransform.localPosition;
        }
    }

    private void Update()
    {
        Mathf.Clamp(Value, 0, 1);
        if (!_enabled && timer < baseTime)
            effects();
        else
            SP_bar.value = 100 * Value;

        if (!Held && _enabled)
        {
            var renderers = SP_obj.GetComponentsInChildren<Image>();
            foreach (Image image in renderers)
            {
                image.enabled = false;
            }
            var erenderers = effect_obj.GetComponents<Image>();
            foreach (Image r in erenderers)
            {
                r.enabled = true;
                effects();
            }
            timer = 0;
            _enabled = false;
            return;
        }

        if (Held && !_enabled)
        {
            var renderers = SP_obj.GetComponentsInChildren<Image>();
            foreach (Image image in renderers)
            {
                image.enabled = true;
            }
            var erenderers = effect_obj.GetComponents<Image>();
            foreach (Image r in erenderers)
            {
                r.enabled = false;
            }
            _enabled = true;
        }
    }

    private void effects()
    {
        timer += Time.deltaTime;

        var erenderers = effect_obj.GetComponentsInChildren<Image>();
        foreach (Image r in erenderers)
        {
            float tempValue = Value - 0.5f;
            r.rectTransform.localScale = Vector3.up * (1 + timer / baseTime * maxAcu * Mathf.Pow(1 - 2 * Mathf.Abs(tempValue),2)) + Vector3.right;
            r.canvasRenderer.SetAlpha(1 - timer/baseTime - Mathf.Abs(tempValue));
            r.rectTransform.localPosition = basePos + Vector3.right * tempValue * 40;
        }
    }
}
