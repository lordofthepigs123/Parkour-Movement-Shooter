using Unity.Mathematics;
using UnityEngine;

public class VisualArrow : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float strechBase;
    [SerializeField] float strechMult;
    [SerializeField] float maxStrech;
    [SerializeField] float strechShiftMod; // 1 -
    [SerializeField] float speedWidthStrech;
    [SerializeField] float cylinderPos;//2.4
    [SerializeField] float headPosMult;//2.35
    [SerializeField] float sarrowAlphaMult;
    [SerializeField] float sarrowAlphaMax;
    [SerializeField] float inputLength;
    [SerializeField] float inputWidth;
    [SerializeField] float inputAlpha;
    [SerializeField] bool disableIhBody;
    private float scurrentStrech;
    private float calcLower;

    [Header("Components")]
    [SerializeField] InputHandler ih;
    [SerializeField] PlayerColliderManager cm;
    [SerializeField] Rigidbody rb;
    [SerializeField] Camera cam;
    [SerializeField] RectTransform sarrowBody;
    [SerializeField] RectTransform sarrowHead;
    [SerializeField] RectTransform iarrowBody;
    [SerializeField] RectTransform iarrowHead;
    [SerializeField] RectTransform axis;

    private Renderer sbodyRend;
    private Renderer sheadRend;
    private Renderer ibodyRend;
    private Renderer iheadRend;

    private void Start()
    {
        calcLower = Mathf.Log(strechShiftMod, strechBase);// 0 case subtract from y so also 0

        sbodyRend = sarrowBody.GetComponent<Renderer>();
        sheadRend = sarrowHead.GetComponent<Renderer>();
        ibodyRend = iarrowBody.GetComponent<Renderer>();
        iheadRend = iarrowHead.GetComponent<Renderer>();
    }

    private void Update()
    {
        rotateScale();
    }

    private void rotateScale()
    {
        //blue speed arrow
        //apply rotation
        Quaternion camRot = Quaternion.Inverse(cam.transform.rotation);
        sarrowBody.rotation = camRot * Quaternion.FromToRotation(Vector3.forward, rb.linearVelocity);
        sarrowHead.rotation = sarrowBody.rotation;
        axis.rotation = camRot * Quaternion.AngleAxis(90, transform.right);

        //apply streches
        scurrentStrech = (Mathf.Log(rb.linearVelocity.magnitude + strechShiftMod, strechBase) - calcLower) * strechMult;
        if (scurrentStrech < 0.05f)
            scurrentStrech = 0;
        scurrentStrech = Mathf.Clamp(scurrentStrech, 0, maxStrech);
        Vector3 vecStrech = new Vector3(speedWidthStrech * (1 + scurrentStrech), speedWidthStrech * (1 + scurrentStrech), scurrentStrech);
        sarrowBody.localScale = vecStrech;
        sarrowHead.localScale = vecStrech;

        //move Arrow head
        Vector3 shiftPos = sarrowBody.rotation * (Vector3.forward * scurrentStrech) * cylinderPos;
        sarrowBody.localPosition = shiftPos;
        sarrowHead.localPosition = shiftPos * headPosMult;

        //render transparent
        float ratio = Mathf.Pow(scurrentStrech / maxStrech * sarrowAlphaMax, sarrowAlphaMult);
        Color objectColor = sbodyRend.material.color;
        Color newColor = new Color(objectColor.r, objectColor.g, objectColor.b, ratio);
        sbodyRend.material.color = newColor;
        sheadRend.material.color = newColor;

        //input arrow
        //apply rotation
        Vector3 inputs = ih.baseInputDir;
        if (cm.touchingWall)
        {
            inputs = ih.planeInputDir(cm.wallNormal,false);
        }

        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, inputs);
        iarrowBody.rotation = camRot * rot;
        iarrowHead.rotation = iarrowBody.rotation;

        //apply streches
        float inputStrech = inputLength;
        if (inputs.magnitude <= 0)
            inputStrech = 0;

        vecStrech = new Vector3(inputWidth * inputStrech, inputWidth * inputStrech, inputStrech);
        iarrowHead.localScale = vecStrech;

        //move Arrow head
        shiftPos = iarrowBody.rotation * Vector3.forward * inputStrech * cylinderPos;
        iarrowHead.localPosition = shiftPos * headPosMult;

        //render transparent
        ratio = inputStrech * inputAlpha;
        objectColor = ibodyRend.material.color;
        newColor = new Color(objectColor.r, objectColor.g, objectColor.b, ratio);
        iheadRend.material.color = newColor;

        if (!disableIhBody)
        {
            iarrowBody.localScale = vecStrech;
            iarrowBody.localPosition = shiftPos;
            ibodyRend.material.color = newColor;
            return;
        }

        iarrowBody.localScale = Vector3.zero;
    }
}
