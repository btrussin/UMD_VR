using UnityEngine;
using UnityEditor;
using System.Collections;

public class NodeMenuHandler : BaseMenuHandler {

    public CMRole Role;

    void Start () {
        //boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        //checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
    }
	
	void Update () {
	
	}

    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if ( Role.active )
        {
            rend.material = checkMaterial;
        }
        else
        {
            rend.material = boxMaterial;
        }
    }

    public override void handleTrigger()
    {
        switch(handlerType)
        {
            case BaseMenuHandlerType.CloseMenu:
                baseState.DestroyMenu();
                break;
            case BaseMenuHandlerType.ToggleOption:
                Role.active = !Role.active;
                UpdateMaterial();
                break;
            default:
                break;
        }
    }

}
