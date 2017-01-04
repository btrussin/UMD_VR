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

        if (rend.material.name.StartsWith("check_mat"))
        {
            if (ringLayoutState == RingLayoutState.Publisher)
            {
                FindObjectOfType<SphereData>().CreateRingsForPublisher();
            }
            else if (ringLayoutState == RingLayoutState.Studio)
            {
                FindObjectOfType<SphereData>().CreateRingsForStudio();
            }
            else if (ringLayoutState == RingLayoutState.Year)
            {
                FindObjectOfType<SphereData>().CreateRingsForYear();
            }
            else if (ringLayoutState == RingLayoutState.Comic)
            {
                FindObjectOfType<SphereData>().CreateRingsForComic();
            }
            else if (ringLayoutState == RingLayoutState.Distributor)
            {
                FindObjectOfType<SphereData>().CreateRingsForDistributor();
            }
            else if (ringLayoutState == RingLayoutState.Grouping)
            {
                FindObjectOfType<SphereData>().CreateRingsForGrouping();
            }
        }

        rend.material = (rend.material.name.StartsWith("check_mat")) ? boxMaterial : checkMaterial; //TODO: How can I check the material a better way?

        ClearOtherCategories();

    }

    private void ClearOtherCategories()
    {

    }

    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if (ringLayoutState == RingLayoutState.Publisher)
        {
            rend.material = checkMaterial;
            FindObjectOfType<SphereData>().CreateRingsForPublisher(); //TODO: How to handle when there is a possibility of multiple spheres?
        } else
        {
            rend.material = boxMaterial;
        }
    }

}
