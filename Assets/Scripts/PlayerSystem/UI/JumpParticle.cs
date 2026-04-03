using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class JumpParticle : MonoBehaviour
{
    [HideInInspector] public float lifeTime;
    [HideInInspector] public float travelDis;
    [SerializeField] float travelDisBase;
    [SerializeField] float growSize;
    [SerializeField] AnimationCurve alphaCurve;
    private RectTransform trans;
    private Image image;
    private void Start()
    {
        trans = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        Invoke(nameof(particleStart), 0.01f);
    }

    private void particleStart()
    {
        //do particle change
        trans.DOScale(trans.localScale * growSize, lifeTime);
        trans.DOMove(transform.position + Vector3.up * travelDisBase * travelDis,lifeTime);
        image.DOFade(1,lifeTime).SetEase(alphaCurve);
    }
}
