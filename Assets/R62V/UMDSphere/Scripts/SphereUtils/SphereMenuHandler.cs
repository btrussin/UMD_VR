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
            case BaseMenuHandlerType.ToggleOption:
                Debug.Log("AGH");
                //type.active = !type.active;
                SetNewMaterialCallback();
                break;
            default:
                break;
        }
    }

    private void SetNewMaterialCallback()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
        Debug.Log(rend.material);

        if (ringLayoutState == RingLayoutState.Publisher)
        {
            rend.material = (rend.material == checkMaterial) ? boxMaterial : checkMaterial;
        } else if (ringLayoutState == RingLayoutState.Studio)
        {
            rend.material = (rend.material == checkMaterial) ? boxMaterial : checkMaterial;
        }
        else if (ringLayoutState == RingLayoutState.Year)
        {
            rend.material = (rend.material == checkMaterial) ? boxMaterial : checkMaterial;
        }
        else if (ringLayoutState == RingLayoutState.Comic)
        {
            rend.material = (rend.material == checkMaterial) ? boxMaterial : checkMaterial;
        }
        else if (ringLayoutState == RingLayoutState.Distributor)
        {
            rend.material = (rend.material == checkMaterial) ? boxMaterial : checkMaterial;
        }
        else if (ringLayoutState == RingLayoutState.Grouping)
        {
            rend.material = (rend.material == checkMaterial) ? boxMaterial : checkMaterial;
        }

    }

    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if (ringLayoutState == RingLayoutState.Publisher)
        {
            rend.material = checkMaterial;
        } else
        {
            rend.material = boxMaterial;
        }
    }

}
