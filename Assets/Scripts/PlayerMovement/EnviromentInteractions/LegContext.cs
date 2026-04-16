using UnityEngine;
using UnityEngine.UIElements;

public class LegContext : MonoBehaviour
{
    private EnviromentInteractionContext.EBodySide _side;
    public LegContext(EnviromentInteractionContext context, EnviromentInteractionContext.EBodySide side)
    {
        Context = context;
        _side = side;
    }
    //read only
    public EnviromentInteractionContext.EBodySide Side => _side;
    //Set-able 
    public EnviromentInteractionContext Context {get; private set;}
    public RaycastHit StepHit;
    
}
