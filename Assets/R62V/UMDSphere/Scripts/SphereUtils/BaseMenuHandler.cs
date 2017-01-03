using UnityEngine;
using System.Collections;

public class BaseMenuHandler : MonoBehaviour {

    public Material boxMaterial;
    public Material checkMaterial;

    public BaseState baseState;

    public enum BaseMenuHandlerType
    {
        CloseMenu,
        ToggleOption
    }

    public BaseMenuHandlerType handlerType;
   
    public virtual void handleTrigger()
    {

    }

    public virtual void UpdateMaterial()
    {

    }

}
