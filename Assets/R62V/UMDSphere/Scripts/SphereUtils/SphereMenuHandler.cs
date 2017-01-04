using UnityEngine;
using System.Collections;

public class SphereMenuHandler : BaseMenuHandler {

    public enum RingLayoutState
    {
        Distributor,
        Grouping,
        Comic,
        Publisher,
        Studio,
        Year
    }

    public RingLayoutState ringLayoutState = RingLayoutState.Publisher;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void handleTrigger()
    {
        switch (handlerType)
        {
            case BaseMenuHandlerType.CloseMenu:
                baseState.destroyMenu();
                Debug.Log("Destroy function call");
                break;
            /*case BaseMenuHandlerType.ToggleOption:
                type.active = !type.active;
                UpdateMaterial();
                break;*/
            default:
                break;
        }
    }

    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if (ringLayoutState == RingLayoutState.Publisher)
        {
            rend.material = checkMaterial;
        }
        else
        {
            rend.material = boxMaterial;
        }
    }

}
