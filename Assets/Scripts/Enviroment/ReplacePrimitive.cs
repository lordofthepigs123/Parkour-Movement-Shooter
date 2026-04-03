using UnityEngine;
using System.Collections.Generic;

public class ReplacePrimitive : MonoBehaviour
{
    private void Start()
    {
        Replace();
    }

    private void Replace()
    {
        Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        List<Collider> primitiveColliders = new List<Collider>();

        foreach (Collider collider in allColliders)
        {
            if (collider is BoxCollider || collider is SphereCollider) //  || collider is CapsuleCollider
            {
                primitiveColliders.Add(collider);
            }
        }

        int replacedNum = 0;

        foreach (Collider collider in primitiveColliders)
        {
            GameObject obj = collider.gameObject;

            //check if has meshfilter
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                DestroyImmediate(collider); // remove

                MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                if (!GetComponentsInParent<Rigidbody>().Equals(null))
                    meshCollider.convex = true;
                meshCollider.sharedMesh = meshFilter.sharedMesh;

                replacedNum ++;
            }
        }

        Debug.Log("Replaced Colliders : " + replacedNum);
    }
}
