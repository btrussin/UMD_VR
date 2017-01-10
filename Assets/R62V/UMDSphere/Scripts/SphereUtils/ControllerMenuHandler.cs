﻿using UnityEngine;
using System.Collections;

public class ControllerMenuHandler : BaseMenuHandler {

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
                break;
            case BaseMenuHandlerType.ToggleOption:
                SetNewMaterialCallback();
                break;
            default:
                break;
        }
    }

    private void SetNewMaterialCallback()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if (rend.material.name.StartsWith("box_mat"))
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

            rend.material = checkMaterial;
        } else {
            rend.material = boxMaterial;
            FindObjectOfType<SphereData>().ClearRings();
        }

        ClearOtherCategories();
    }

    private void ClearOtherCategories()
    {
        for (int layerInd = 0; layerInd < SphereData.NUM_LAYOUTS; layerInd++)
        {
            if ((RingLayoutState) layerInd != ringLayoutState)
            {
                GameObject.Find("Toggle Option: " + ((RingLayoutState)layerInd).ToString()).GetComponent<Renderer>().material = boxMaterial; 
            }
        }
    }

    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        rend.material = boxMaterial;

        string sphereName = FindObjectOfType<SphereData>().gameObject.name;

        if (sphereName != "DataObject" && sphereName == "DataObject: " + ringLayoutState.ToString())
        {
                    GameObject.Find("Toggle Option: " + ringLayoutState.ToString())
                        .GetComponent<Renderer>()
                        .material = checkMaterial;
        }
    }

}