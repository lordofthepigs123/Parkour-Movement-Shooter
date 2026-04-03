using System.Collections;
using UnityEngine;
using DG.Tweening;

public class AngleJumpParticleManager : MonoBehaviour
{
    [Header("Jump Particle")]
    [SerializeField] float baseMultMod;
    [SerializeField] float maxBaseMult;
    [SerializeField] GameObject jumpParticle;
    [SerializeField] float particleLifeTime;
    [SerializeField] float minTravelDis;
    [SerializeField] float travelDisMult;
    [SerializeField] float minScale;
    [SerializeField] float scaleMult;


    public void createParticle(float baseMult)
    {
        if (baseMult > 1)
            baseMult = Mathf.Pow(baseMult, 1 / baseMultMod);
        baseMult = Mathf.Clamp(baseMult, 0, maxBaseMult);
        //Create
        GameObject particle = Instantiate(jumpParticle, transform.position, Quaternion.identity, transform);// attach particle to this object
        particle.transform.localScale = Vector3.one * (minScale + baseMult * scaleMult);
        JumpParticle jp = particle.GetComponent<JumpParticle>();
        jp.lifeTime = particleLifeTime;
        jp.travelDis = minTravelDis + baseMult * travelDisMult;
         
        //destroy particle
        StartCoroutine(destroyParticle(particle, particleLifeTime));
    }

    private IEnumerator destroyParticle(GameObject particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        DOTween.Kill(particle);
        Destroy(particle);
    }
}
