﻿using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class UMD_Sphere_TrackedObject : SteamVR_TrackedObject
{
    public GameObject dataObj;

    public GameObject otherController;
    UMD_Sphere_TrackedObject otherTrackedObjScript;

    public GameObject menuObject;
    public bool menuActive = false;
    public static bool animationLayout = true;

    SphereData sphereData;

    public Ray deviceRay;
    public Vector3 currPosition;
    public Vector3 currRightVec;
    public Vector3 currUpVec;
    public Vector3 currForwardVec;
    public Quaternion currRotation;

    public GameObject trackpadArrowObject;

    SphereCollider sphereCollider;

    //List<GameObject> connectionList = new List<GameObject>();

    Dictionary<string, MovieObject> connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    CVRSystem vrSystem;

    VRControllerState_t state;
    VRControllerState_t prevState;

    Quaternion currRingBaseRotation;

    List<GameObject> ringsInCollision;

    GameObject beam;
    GameObject activeBeamInterceptObj = null;

    bool useBeam = false;

    int menusLayerMask;

    float currRayAngle = 60.0f;
    bool adjustRayAngle = false;

    //bool trackpadArrowsAreActive = false;
    int prevNumRingsInCollision = 0;

    public GameObject sliderLeftPnt;
    public GameObject sliderRightPnt;
    public GameObject sliderPoint;

    float sliderPointDistance = 0.0f;
    bool updateSlider = false;

    private bool isCollidingWithRing;
    public bool padJustPressedDown;

    // returns true on touchpad click up
    public bool padClicked()
    {
        if (padJustPressedDown && (state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) == 0)
        {
            padJustPressedDown = false;
            return true;
        }
        return false;
    }
    // reference to instance of FormQuestions class
    private FormMenuHandler.FormQuestions form_questions;
    // reference to form menu script
    public FormMenuHandler fmh_script;

    void Start()
    {
        vrSystem = OpenVR.System;

        sphereCollider = gameObject.GetComponent<SphereCollider>();
        sphereCollider.transform.SetParent(gameObject.transform);

        sphereData = dataObj.GetComponent<SphereData>();

        beam = new GameObject();
        beam.AddComponent<LineRenderer>();
        LineRenderer lineRend = beam.GetComponent<LineRenderer>();
        lineRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRend.receiveShadows = false;
        lineRend.motionVectors = false;
        lineRend.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/BeamMaterial.mat");
        lineRend.SetWidth(0.003f, 0.003f);
        beam.SetActive(false);

        menusLayerMask = 1 << LayerMask.NameToLayer("Menus");
        //menuObject.SetActive(false);

        otherTrackedObjScript = otherController.GetComponent<UMD_Sphere_TrackedObject>();

        sphereData.setMainLayout(SphereData.SphereLayout.Sphere);
        sphereData.SetMainRingCategory(SphereData.MainRingCategory.Year);

        setSliderLocalPosition(sphereData.bundlingStrength);

        //this was getting some other reference to another instance of fmh_script
        //fmh_script = GameObject.FindObjectOfType<FormMenuHandler>();
        form_questions = fmh_script.form_questions;
    }

    void Update()
    {
        currPosition = transform.position;
        currRightVec = transform.right;
        currUpVec = transform.up;
        currForwardVec = transform.forward;
        currRotation = transform.rotation;
        deviceRay.origin = currPosition;

        Quaternion rayRotation = Quaternion.AngleAxis(currRayAngle, currRightVec);

        deviceRay.direction = rayRotation * currForwardVec;

        sphereCollider.center = new Vector3(0.0f, 0.0f, 0.03f);

        handleStateChanges();

        ringsInCollision = sphereData.getRingsInCollision(currPosition + (currForwardVec - currUpVec) * (0.03f + sphereCollider.radius) , sphereCollider.radius*2.0f);
        if (ringsInCollision.Count > 0)
        {
            sphereData.addActiveRings(ringsInCollision);
            if (prevNumRingsInCollision == 0) showTrackpadArrows();
        }
        else if (prevNumRingsInCollision > 0 && (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
        {
            hideTrackpadArrows();
        }

        prevNumRingsInCollision = ringsInCollision.Count;

        if (useBeam) projectBeam();

        if( updateSlider )
        {
            calcSliderPosition();
            sphereData.updateAllKeptConnections();
        }
    }

    void calcSliderPosition()
    {
        // get proposted position of the slider point in world space
        Vector3 tVec = deviceRay.GetPoint(sliderPointDistance);

        // project that point onto the world positions of the slider ends
        Vector3 v1 = sliderRightPnt.transform.position - sliderLeftPnt.transform.position;
        Vector3 v2 = tVec - sliderLeftPnt.transform.position;

        // 'd' is the vector-projection amount of v2 onto v1
        float d = Vector3.Dot(v1, v2)/ Vector3.Dot(v1, v1);

        // 'd' is also the correct linear combination of the left and right slider edges
        // left * d + right * ( 1 - d )
        setSliderLocalPosition(d);
    }

    void setSliderLocalPosition(float dist)
    {
        // clamp dist to 0.0 and 1.0
        // float tDist = Mathf.Min(1.0f, Mathf.Max(0.0f, dist));
        float tDist = Mathf.Clamp(dist, 0.0f, 1.0f);
        Vector3 tVec = (sliderRightPnt.transform.localPosition - sliderLeftPnt.transform.localPosition)* tDist;
        sliderPoint.transform.localPosition = sliderLeftPnt.transform.localPosition + tVec;

        sphereData.bundlingStrength = tDist;
    }

    void projectBeam()
    {
        float beamDist = 10.0f;

        RaycastHit hitInfo;

        if (Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, 30.0f, menusLayerMask))
        {
            activeBeamInterceptObj = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;
        }
        else
        {
            activeBeamInterceptObj = null;
        }

        LineRenderer lineRend = beam.GetComponent<LineRenderer>();
        Vector3 end = deviceRay.GetPoint(beamDist);

        lineRend.SetPosition(0, deviceRay.origin);
        lineRend.SetPosition(1, end);
    }

    void triggerActiverBeamObject()
    {
        if( activeBeamInterceptObj != null )
        { 
            NodeMenuHandler menuHandler = activeBeamInterceptObj.GetComponent<NodeMenuHandler>();
          
            if (menuHandler != null )
            {

                menuHandler.handleTrigger();

                //MovieObject mo = activeBeamInterceptObj.transform.parent.transform.parent.gameObject.GetComponent<MovieObject>();
                MovieObject mo = activeBeamInterceptObj.transform.parent.GetComponent<NodeMenuUtils>().movieObject;
                sphereData.connectMoviesByActors(mo.cmData);
                sphereData.updateAllKeptConnections();
            }

            FormMenuHandler formMenuHandler = activeBeamInterceptObj.GetComponent<FormMenuHandler>();

            if (formMenuHandler != null)
            {
               
                formMenuHandler.handleTrigger();

                if (activeBeamInterceptObj != null)
                {
                    if (activeBeamInterceptObj.name.CompareTo("Quad_Slider_Point") == 0)
                    {
                        //TODO
                        //formMenuHandler.UpdateSlider(true);
                    }
                }
            }

            if (activeBeamInterceptObj.name.Contains("Text"))
            {
                activeBeamInterceptObj.transform.GetComponentInChildren<FormMenuHandler>().handleTrigger();
            }

            else
            {
                
                MainMenuUtils mainMenu = this.GetComponent<MainMenuUtils>();
                if( mainMenu != null )
                {
                    if (mainMenu.sphereData == null)
                    {
                        mainMenu.sphereData = sphereData;
                    }

                    if (activeBeamInterceptObj.name.Contains("Box-Lay"))
                    {
                        SphereData.SphereLayout destLayout = SphereData.SphereLayout.Sphere;

                        if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Animation") == 0)
                            animationLayout = !animationLayout;

                        else
                        {
                            if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Sphere") == 0)
                                destLayout = SphereData.SphereLayout.Sphere;
                            else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl_X") == 0)
                                destLayout = SphereData.SphereLayout.Column_X;
                            else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl_Y") == 0)
                                destLayout = SphereData.SphereLayout.Column_Y;
                            else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl_Z") == 0)
                                destLayout = SphereData.SphereLayout.Column_Z;

                            sphereData.setMainLayout(destLayout);
                            mainMenu.updateLayout();
                        }
                    }
                    else if (activeBeamInterceptObj.name.Contains("Box-Cat"))
                    {
                        SphereData.MainRingCategory destCategory = SphereData.MainRingCategory.Publisher;

                        if (activeBeamInterceptObj.name.CompareTo("Box-Cat_Dist") == 0) destCategory = SphereData.MainRingCategory.Distributor;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Cat_Grp") == 0) destCategory = SphereData.MainRingCategory.Grouping;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Cat_Comic") == 0) destCategory = SphereData.MainRingCategory.Comic;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Cat_Pub") == 0) destCategory = SphereData.MainRingCategory.Publisher;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Cat_Studio") == 0) destCategory = SphereData.MainRingCategory.Studio;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Cat_Year") == 0) destCategory = SphereData.MainRingCategory.Year;
                        
                        sphereData.SetMainRingCategory(destCategory);
                        mainMenu.updateLayout();
                    }
                    else if(activeBeamInterceptObj.name.CompareTo("Quad_Slider_Point") == 0 )
                    {
                        updateSlider = true;

                        Vector3 distVec = sliderPoint.transform.position - gameObject.transform.position;
                        sliderPointDistance = distVec.magnitude;
                    }

                    mainMenu.updateOneStates(animationLayout);
                }
            }

            activeBeamInterceptObj = null;
        }
    }

    void handleStateChanges()
    {
        // enter survey question answer
        if (padClicked() && !isCollidingWithRing)
        {
            Debug.Log(fmh_script.form_questions.QuestionIndex);
            fmh_script.form_questions.QuestionIndex++;
            fmh_script.SetQuestion();

        }
        bool stateIsValid = vrSystem.GetControllerState((uint)index, ref state);


        if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

        if (stateIsValid && state.GetHashCode() != prevState.GetHashCode())
        {

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) != 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) == 0)
            {
                //sphereData.toggleMainLayout();

                toggleMenu();
            }

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0 )
            {
                sphereData.grabSphereWithObject(gameObject);
            }
            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0 )
            {
                sphereData.releaseSphereWithObject(gameObject);
            }

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
               (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
            {
                // activate beam
                beam.SetActive(true);
                useBeam = true;

                if( adjustRayAngle ) showTrackpadArrows();
            }

            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
               (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                // deactivate beam
                beam.SetActive(false);
                useBeam = false;
                activeBeamInterceptObj = null;
                if (ringsInCollision.Count == 0) hideTrackpadArrows();

                updateSlider = false;
            }

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
                prevState.rAxis1.x < 1.0f && state.rAxis1.x == 1.0f )
            {

                triggerActiverBeamObject();

                // toggle connections with all movies
                foreach (MovieObject m in connectionMovieObjectMap.Values)
                {
                    m.nodeState.toggleSelected();
                    m.nodeState.updateColor();
                }

            }


            prevState = state;
        }

        if ((SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            GameObject slider = GameObject.FindGameObjectWithTag("Slider");
            Transform sliderPoint = null;
            Transform sliderLeftLimit = null;
            Transform sliderRightLimit = null;
            if (slider != null)
            {
                //slider.transform.Translate(state.rAxis0.x/100,0,0);
                foreach (Transform t in slider.GetComponentsInChildren<Transform>())
                {
                    if ((t.tag == "SliderPoint"))
                    {
                        sliderPoint = t;
                    }
                    else if (t.tag == "SliderLeftLimit")
                    {
                        
                        sliderLeftLimit = t;
                        Debug.Log("left " + sliderLeftLimit.position);
                    }
                    else if (t.tag == "SliderRightLimit")
                    {
                        sliderRightLimit = t;
                        
                        Debug.Log("right " + sliderRightLimit.position);
                    }
                }
               

                // if it is right of the leftmost point 
                if (!(sliderPoint.position.x >= sliderLeftLimit.position.x) && (!(sliderPoint.position.x <= sliderRightLimit.position.x)))
                {
                    Debug.Log("second");
                    sliderPoint.Translate(0, -state.rAxis0.x / 100, 0);
                }

                else if ((state.rAxis0.x <= 0) && (!(sliderPoint.position.x >= sliderLeftLimit.position.x)) )
                {
                        sliderPoint.Translate(0, -state.rAxis0.x / 100, 0);
                    
                }

                else if((state.rAxis0.x >= 0) && (!(sliderPoint.position.x <= sliderRightLimit.position.x)))
                {
                    sliderPoint.Translate(0, -state.rAxis0.x / 100, 0);
                }

            }
        }

        /*
                        Debug.Log((sliderPoint.position.x) >= (sliderRightLimit.position.x));

                        if (!((sliderPoint.position.x) <= (sliderRightLimit.position.x)))
                        {
                            Debug.Log("second"+-state.rAxis0.x);
                            if ((-state.rAxis0.x < 0) && (-state.rAxis0.x < sliderLeftLimit.))
                            {
                                //move left
                                sliderPoint.Translate(0, -state.rAxis0.x / 100, 0);
                            }
                        }

                        if (!((sliderPoint.position.x) >= (sliderLeftLimit.position.x)))
                        {
                            if (-state.rAxis0.x == 0)
                            {
                                // move right
                                sliderPoint.Translate(0, -state.rAxis0.x / 100, 0);
                            }
                        }

            */

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            padJustPressedDown = true;
            // reset the collision check to false before checking again
            isCollidingWithRing = false;

            Quaternion addRotation = Quaternion.Euler(0.0f, 0.0f, state.rAxis0.y);
            Quaternion origRot;
            GameObject innerRot;
            foreach (GameObject g in ringsInCollision)
            {
                innerRot = g.transform.GetChild(0).gameObject;
                origRot = innerRot.transform.localRotation;

                innerRot.transform.localRotation = origRot * addRotation;

                // check if colliding with ring
                if (innerRot != null)
                {
                    isCollidingWithRing = true;
                }
            }
            
            UpdateConnections();

            sphereData.updateAllKeptConnections();

            // update the beam ray direction
            if (adjustRayAngle && ringsInCollision.Count == 0 && (state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 )
            {

                if (state.rAxis0.y > 0.0f) currRayAngle -= 1.0f;
                else if (state.rAxis0.y < 0.0f) currRayAngle += 1.0f;

                // keep within the bounds of 0 and 90 degrees
                if (currRayAngle > 90.0f) currRayAngle = 90.0f;
                else if (currRayAngle < 0.0f) currRayAngle = 0.0f;

            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        GameObject obj = col.gameObject;
        if (obj.name.Contains("MovieNode"))
        {
            MovieObject mo = obj.transform.parent.gameObject.GetComponent<MovieObject>();

            NodeState ns = mo.nodeState;
            ns.addCollision();
            ns.updateColor();

            string key = MovieDBUtils.getMovieDataKey(mo.cmData);

            if ( !ns.getIsSelected() )
            {
                sphereData.connectMoviesByActors(mo.cmData);
            }

            connectionMovieObjectMap.Add(key, mo);
        }
        
    }

    void OnCollisionStay(Collision col)
    {
       
    }

    void OnCollisionExit(Collision col)
    {
        GameObject obj = col.gameObject;
        if (obj.name.Contains("MovieNode"))
        {
            MovieObject mo = obj.transform.parent.gameObject.GetComponent<MovieObject>();
            string key = MovieDBUtils.getMovieDataKey(mo.cmData);

            mo.nodeState.removeCollision();
            mo.nodeState.updateColor();

            if( !mo.nodeState.getIsSelected() )
            {
                mo.connManager.ForceClearAllConnections();
            }

            connectionMovieObjectMap.Remove(key);
        }
        
    }

    void UpdateConnections()
    {
      
        Dictionary<string, MovieObject>.KeyCollection keys = connectionMovieObjectMap.Keys;

        if (keys.Count < 1) return;
        MovieObject mo;
        foreach ( string key in keys )
        {
            if( connectionMovieObjectMap.TryGetValue(key, out mo) )
            {
                mo.connManager.ForceClearAllConnections();
                sphereData.connectMoviesByActors(mo.cmData);
            }
        }
    }

    void showTrackpadArrows()
    {
        trackpadArrowObject.SetActive(true);
    }

    void hideTrackpadArrows()
    {
        trackpadArrowObject.SetActive(false);
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

        menuObject.transform.localPosition = new Vector3(0.0f, 0.02f, 0.0f);
        menuObject.transform.localRotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
        menuObject.transform.localScale = new Vector3(0.25f, 0.25f, 1.0f);

        menuObject.SetActive(true);
    }

    public void hideMainMenu()
    {
        menuActive = false;
        menuObject.SetActive(false);
    }

    
}
