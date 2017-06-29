﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class ForceDirTrackedObject : BaseSteamController
{
    public GameObject otherController;
    ForceDirTrackedObject otherTrackedObjScript;

    public GameObject menuObject;
    GameObject currMenuSubObject = null;
    MenuManager menuManager;
    public bool menuActive = false;

    public GameObject sliderLeftPnt;
    public GameObject sliderRightPnt;
    public GameObject sliderPoint;
    float sliderPointDistance = 0.0f;
    bool updateSlider = false;
    bool triggerPulled = false;


    public Ray deviceRay;

    int nodeLayerMask;
    int menuSliderMask;

    GameObject currNodeSelected = null;
    GameObject currNodeCollided = null;
    bool updateNodeSelectedPosition = false;
    bool updateNodeCollidedPosition = false;
    float nodePointDistance = 0.0f;

    GameObject currNodeInContact = null;

    public GameObject beam;
    bool castBeamAnyway = false;

    CVRSystem vrSystem;
    VRControllerState_t state;
    VRControllerState_t prevState;

    public GameObject forceDirLayoutObj;
    ForceDirLayout fDirScript;

    public int highlightGrp = -1;

    // Use this for initialization
    void Start () {
        vrSystem = OpenVR.System;

        menuManager = menuObject.GetComponent<MenuManager>();

        //menuSliderMask = 1 << LayerMask.NameToLayer("MenuSlider");
        menuSliderMask = 1 << LayerMask.NameToLayer("Menus");
        nodeLayerMask = 1 << LayerMask.NameToLayer("NodeLayer");
        beam.SetActive(false);

        otherTrackedObjScript = otherController.GetComponent<ForceDirTrackedObject>();

        fDirScript = forceDirLayoutObj.GetComponent<ForceDirLayout>();
    }

    
    new void FixedUpdate()
    {
        base.FixedUpdate();
        if (__goToParentScene)
        {
            goToParentScene("PivotScene");
        }
    }
    

    // Update is called once per frame
    void Update () { 

        // update the device ray per frame
        Quaternion rayRotation = Quaternion.AngleAxis(60.0f, transform.right);
        deviceRay.direction = rayRotation * transform.forward;

        deviceRay.origin = transform.position + deviceRay.direction * 0.07f;


        handleStateChanges();
        projectBeam();



        if (updateSlider)
        { 
            // get proposted position of the slider point in world space
            Vector3 tVec = deviceRay.GetPoint(sliderPointDistance);
            menuManager.calcSliderPosition(tVec);
        }
        else if( updateNodeSelectedPosition )
        {
            calcNodePosition();
        }

        if(updateNodeCollidedPosition)
        {
            Vector3 pt = transform.position + transform.forward * 0.05f;
            currNodeCollided.transform.position = pt;
            NodeInfo info = fDirScript.getNodeInfo(currNodeCollided.name);
            info.pos3d = pt;
        }

    }

    void projectBeam()
    {
        float beamDist = 10.0f;

        beam.SetActive(false);

        RaycastHit hitInfo;
        if (updateSlider)
        {
            beam.SetActive(true);
            beamDist = sliderPointDistance;
        }
        else if (menuManager.useNodePointers && updateNodeSelectedPosition)
        {
            beam.SetActive(true);
            beamDist = nodePointDistance;
        }
        else if (Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, beamDist, menuSliderMask))
        {
            GameObject obj = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;
            beam.SetActive(true);

            currMenuSubObject = obj;

            if (triggerPulled && obj.name.Equals("Quad_Slider_Point"))
            {
                sliderPointDistance = beamDist;
                updateSlider = true;
            }
            else updateSlider = false;

        }
        else if (menuManager.useNodePointers && Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, beamDist, nodeLayerMask))
        {
            
            currNodeSelected = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;
            beam.SetActive(true);
            if (triggerPulled)
            {
                nodePointDistance = beamDist;
                updateNodeSelectedPosition = true;
            }
        }
        else if(castBeamAnyway)
        {
            beam.SetActive(true);
            currMenuSubObject = null;
            currNodeSelected = null;
        }

        LineRenderer lineRend = beam.GetComponent<LineRenderer>();
        Vector3 end = deviceRay.GetPoint(beamDist);

        lineRend.SetPosition(0, deviceRay.origin);
        lineRend.SetPosition(1, end);
    }

    void handleStateChanges()
    {
        bool stateIsValid = vrSystem.GetControllerState((uint)index, ref state);

        if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

        if (stateIsValid && state.GetHashCode() != prevState.GetHashCode())
        {
            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) != 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) == 0)
            {
                toggleMenu();
            }


            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
            {
                // just pulled the trigger
                castBeamAnyway = true;
                triggerPulled = true;

                if( currNodeCollided != null )
                {

                    updateNodeCollidedPosition = true;
                    currNodeCollided.GetComponentInChildren<Collider>().enabled = false;
                    NodeInfo info = fDirScript.getNodeInfo(currNodeCollided.name);
                    info.positionIsStationary = true;
                }
            }
            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                // just released the trigger
                castBeamAnyway = false;
                updateSlider = false;
                updateNodeSelectedPosition = false;
                triggerPulled = false;

                if(currNodeSelected != null)
                {
                    NodeInfo info = fDirScript.getNodeInfo(currNodeSelected.name);
                    if (info != null)
                    {
                        info.positionIsStationary = false;
                        info.interState = NodeInteractionState.NONE;
                        fDirScript.numHighlighed--;
                    }

                    currNodeSelected = null;
                }

                if (currNodeCollided != null)
                {
                    updateNodeCollidedPosition = false;
                    currNodeCollided.GetComponentInChildren<Collider>().enabled = true;
                    NodeInfo info = fDirScript.getNodeInfo(currNodeCollided.name);
                    info.positionIsStationary = false;
                }
            }

            if (prevState.rAxis1.x < 1.0f && state.rAxis1.x == 1.0f)
            {
                // just pulled the trigger in all the way
                if (currNodeCollided != null)
                {
                    NodeInfo info = fDirScript.getNodeInfo(currNodeCollided.name);
                    if (info.prevInterState == NodeInteractionState.SELECTED) info.prevInterState = NodeInteractionState.NONE;
                    else info.prevInterState = NodeInteractionState.SELECTED;
                }
                else if(currMenuSubObject != null)
                {
                    if (currMenuSubObject.name.Equals("ForceBox")) menuManager.toggleForce();
                    else if (currMenuSubObject.name.Equals("ShowLinesBox")) menuManager.toggleShowLines();
                    else if (currMenuSubObject.name.Equals("NodePointerBox")) menuManager.toggleNodePointers();
                }
                    
            }


            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0)
            {
                fDirScript.grabSphereWithObject(gameObject);
            }
            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0 &&
                     (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0)
            {
                fDirScript.releaseSphereWithObject(gameObject);
            }



            prevState = state;
        }


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0 )
        {
            float h = state.rAxis0.x;
            float v = state.rAxis0.y;

            if( Mathf.Abs(h) > Mathf.Abs(v) )
            {
                fDirScript.rotateGraphHorizontal(h);
            }
            else
            {
                fDirScript.rotateGraphVertical(v);
            }
            
        }
    }

    void calcNodePosition()
    {
        Vector3 pos = deviceRay.GetPoint(nodePointDistance);
        currNodeSelected.transform.position = pos;
        NodeInfo info = fDirScript.getNodeInfo(currNodeSelected.name);
        if (info != null) {
            info.pos3d = pos;
            info.positionIsStationary = true;

            info.interState = NodeInteractionState.HIGHLIGHTED;
            fDirScript.numHighlighed++;
        }
    }


    public void toggleMenu()
    {
        if (menuActive) hideMainMenu();
        else showMainMenu();
    }

    public void showMainMenu()
    {
        menuActive = true;
        otherTrackedObjScript.menuActive = false;


        menuObject.transform.SetParent(gameObject.transform);

        menuObject.transform.localPosition = new Vector3(0.00f, 0.03f, 0.05f);
        menuObject.transform.localRotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
        menuObject.transform.localScale = new Vector3(0.25f, 0.25f, 1.0f);

        menuObject.SetActive(true);

        menuManager.updateInterface();
    }

    public void hideMainMenu()
    {
        menuActive = false;
        menuObject.SetActive(false);
    }

    
    void OnCollisionEnter(Collision col)
    {
        GameObject obj = col.gameObject;
        NodeInfo info = fDirScript.getNodeInfo(obj.name);
        if (info == null) return;
        currNodeCollided = obj;
        switch (info.interState)
        {
            case NodeInteractionState.NONE:
            case NodeInteractionState.SELECTED:
                info.prevInterState = info.interState;
                break;
        }
        info.interState = NodeInteractionState.HIGHLIGHTED;
        fDirScript.numHighlighed++;
        highlightGrp = info.group;
    }

    void OnCollisionStay(Collision col)
    {

    }

    void OnCollisionExit(Collision col)
    {
        GameObject obj = col.gameObject;
        NodeInfo info = fDirScript.getNodeInfo(obj.name);
        if (info == null) return;
        Debug.Log("Setting state to previous state for " + obj.name  + " to " + info.prevInterState);
        info.interState = info.prevInterState;
        fDirScript.numHighlighed--;
        highlightGrp = -1;



        currNodeCollided = null;
        updateNodeCollidedPosition = false;


    }
    
}
