using System;
using UnityEngine;
using System.Collections;

public class BaseMenuHandler : MonoBehaviour {

    public Material baseMaterial;
    public Material inputInteractMaterial;

    public BaseState baseState;

    public enum BaseMenuHandlerType
    {
        CloseMenu,
        ToggleOption
    }

    [NonSerialized]
    public BaseMenuHandlerType handlerType;
   
    public virtual void handleTrigger()
    {

    }

    public virtual void UpdateMaterial()
    {

    }

}
