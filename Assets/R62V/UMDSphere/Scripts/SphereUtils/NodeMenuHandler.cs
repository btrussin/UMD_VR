using UnityEngine;
using UnityEditor;
using System.Collections;

public class NodeMenuHandler : BaseMenuHandler {

    public CMRole role;

    void Start () {
        //boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        //checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
    }
	
	void Update () {
	
	}

    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if ( role.active )
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
                baseState.destroyMenu();
                break;
            case BaseMenuHandlerType.ToggleOption:
                role.active = !role.active;
                UpdateMaterial();
                break;
            default:
                break;
        }
    }

}
