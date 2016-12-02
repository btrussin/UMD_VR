using UnityEngine;
using UnityEditor;
using System.Collections;

public class NodeMenuHandler : MonoBehaviour {

    public enum NodeMenuHandlerType
    {
        CloseMenu,
        ToggleActor
    }

    public NodeState nodeState;
    public NodeMenuHandlerType handlerType;

    public CMRole role;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if ( role.active )
        {
            rend.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/check_mat.mat");
        }
        else
        {
            rend.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/box_mat.mat");
        }
    }

    public void handleTrigger()
    {
        switch(handlerType)
        {
            case NodeMenuHandlerType.CloseMenu:
                nodeState.destroyMenu();
                break;
            case NodeMenuHandlerType.ToggleActor:
                role.active = !role.active;
                UpdateMaterial();
                break;
            default:
                break;
        }
    }

    public void closeMenu()
    {
        nodeState.destroyMenu();
    }
}
