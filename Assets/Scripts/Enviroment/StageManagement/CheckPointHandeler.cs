using UnityEngine;

public class CheckPointHandeler : MonoBehaviour
{
    public CheckPoint[] triggers;
    [SerializeField] Transform player;
    public LayerMask playerLayer;
    [SerializeField] int currentNum;
    

    private void Start()
    {
        System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
    }
    private void Update()
    {
        //check triggers
        for (int i = 0; i < triggers.Length; i++)
        {
            if (triggers[i].activated && (currentNum + 1) == i)
            {
                currentNum++;
            }
        }
    }

    //Death Plane
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Mathf.Log(playerLayer.value, 2))
        {
            player.position = triggers[currentNum].resetPos;
        }
    }
    
}
