using UnityEngine;
using UnityEditor;
using System.Collections;

public class NodeMenuHandler : MonoBehaviour
{

    public enum NodeMenuHandlerType
    {
        CloseMenu,
        ToggleActor
    }

    public NodeState nodeState;
    public NodeMenuHandlerType handlerType;

    public Material boxMaterial;
    public Material checkMaterial;

    public CMRole role;

    void Start()
    {
        //boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        //checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
    }

    void Update()
    {

    }

    public void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if (role.active)
        {
            rend.material = checkMaterial;
        }
        else
        {
            rend.material = boxMaterial;
        }
    }

    public void handleTrigger()
    {
        switch (handlerType)
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