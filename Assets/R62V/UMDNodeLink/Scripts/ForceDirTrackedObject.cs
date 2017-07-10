using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class ForceDirTrackedObject : BaseSteamController
{
    GameObject currMenuSubObject = null;
    MenuManager menuManager;

    bool triggerPulled = false;

    int nodeLayerMask;
    int menuSliderMask;

    GameObject currNodeSelected = null;
    GameObject currNodeCollided = null;
    bool updateNodeSelectedPosition = false;
    bool updateNodeCollidedPosition = false;
    float nodePointDistance = 0.0f;

    GameObject currNodeInContact = null;
    bool castBeamAnyway = false;

    public GameObject forceDirLayoutObj;
    ForceDirLayout fDirScript;

    public int highlightGrp = -1;

    // Use this for initialization
    new void Start ()
    {
        base.Start();
        menuManager = menuObject.GetComponent<MenuManager>();

        //menuSliderMask = 1 << LayerMask.NameToLayer("MenuSlider");
        menuSliderMask = 1 << LayerMask.NameToLayer("Menus");
        nodeLayerMask = 1 << LayerMask.NameToLayer("NodeLayer");

        otherTrackedObjScript = otherController.GetComponent<ForceDirTrackedObject>();

        fDirScript = forceDirLayoutObj.GetComponent<ForceDirLayout>();
        beam.SetActive(false);
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
    new void Update ()
    {
        //ShowMainMenu();
        base.Update();
        deviceRay.origin = transform.position + deviceRay.direction * 0.07f;

        projectBeam();
        ApplyStateChanges();
        if (updateSlider)
        { 
            // get proposted position of the slider point in world space
            Vector3 tVec = deviceRay.GetPoint(sliderPointDistance);
            menuManager.calcSliderPosition(tVec);
        }
        else if( updateNodeSelectedPosition )
        {
            CalcNodePosition();
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
            activeBeamInterceptObj = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;
            beam.SetActive(true);

            currMenuSubObject = activeBeamInterceptObj;
            if (triggerPulled)
            {
                if (activeBeamInterceptObj.name.Equals("Quad_Slider_Point"))
                {
                    sliderPointDistance = beamDist;
                    updateSlider = true;
                }
                
                else updateSlider = false;

            }
            else beam.SetActive(false);
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

    protected override void ApplyStateChanges()
    {
        base.ApplyStateChanges();

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
            (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
        {
            // just pulled the trigger
            castBeamAnyway = true;
            triggerPulled = true;

            // THIS CODE IS VERY BROKEN
            // TODO: FIX IT
            if( currNodeCollided != null )
            {
                updateNodeCollidedPosition = true;
                currNodeCollided.GetComponentInChildren<Collider>().enabled = false;
                NodeInfo info = fDirScript.getNodeInfo(currNodeCollided.name);
                udch.startCountingTime = true;
                info.positionIsStationary = true;

                if (udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput ||
                    udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.MultipleInput)
                {
                    udch.PromptUserInput(currNodeCollided.name);
                }

                foreach (GameObject g in GameObject.FindGameObjectsWithTag("MovieNode"))
                {
                    if (udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput ||
                        udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.MultipleInput)
                    {
                        NodeInfo Ninfo = fDirScript.getNodeInfo(g.name);
                        if (Ninfo.interState != NodeInteractionState.SELECTED && g != currNodeCollided)
                        {
                            
                            udch.RemoveAnswer(g.name);
                        }
                    }
                }
                
            }
        }


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
            prevState.rAxis1.x < 1.0f && state.rAxis1.x == 1.0f)
        {
            
            //TriggerActiverBeamObject();
        }


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
            (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
        {
            beam.SetActive(false);
            // just released the trigger
            castBeamAnyway = false;
            updateSlider = false;
            updateNodeSelectedPosition = false;
            triggerPulled = false;
            if (currNodeSelected != null)
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



        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            float h = state.rAxis0.x;
            float v = state.rAxis0.y;

            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                fDirScript.rotateGraphHorizontal(h);
            }
            else
            {
                fDirScript.rotateGraphVertical(v);
            }

        }

        prevState = state;
    }

    void CalcNodePosition()
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

    new void ShowMainMenu()
    {
        base.ShowMainMenu();

        menuManager.updateInterface();
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
        info.interState = info.prevInterState;
        fDirScript.numHighlighed--;
        highlightGrp = -1;



        currNodeCollided = null;
        updateNodeCollidedPosition = false;


    }
    
}
