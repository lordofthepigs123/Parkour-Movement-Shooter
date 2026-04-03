using UnityEngine;

public class Attacher : MonoBehaviour
{
    [Header("Attacher")]
    protected SpringJoint joint;

    protected Rigidbody rb;
    protected InputHandler ih;
    protected float spdMult_ang;

    protected void createJoint(GameObject referenceFrom, Vector3 swingPoint, float swingMaxDis, float swingMinDis, float massScale)
    {
        joint = referenceFrom.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        joint.maxDistance = swingMaxDis;
        joint.minDistance = swingMinDis;

        joint.spring = 0; // k value
        joint.damper = 0; // loss of energy
        joint.massScale = massScale; // 
    }

    protected void removeJoint()
    {
        Destroy(joint);
    }
}
