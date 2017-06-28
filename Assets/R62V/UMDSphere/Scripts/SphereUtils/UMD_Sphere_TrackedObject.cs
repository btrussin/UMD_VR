using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float amount_scrolled = 0;

    SphereCollider sphereCollider;

    //List<GameObject> connectionList = new List<GameObject>();

    public Dictionary<string, MovieObject> connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    CVRSystem vrSystem;

    VRControllerState_t state;
    VRControllerState_t prevState;

    Quaternion currRingBaseRotation;

    List<GameObject> ringsInCollision;

    GameObject beam;
    GameObject activeBeamInterceptObj = null;
    GameObject activeNodeMenu = null;
    GameObject activeActorText = null;
    Vector3 actorTextNormalScale = Vector3.one * 0.1f;
    Vector3 actorTextLargeScale = Vector3.one * 0.15f;

    private GameObject submitButton;
    private SubmitButtonScript sbs;
    bool useBeam = false;

    int menusLayerMask;

    float currRayAngle = 60.0f;

    //bool trackpadArrowsAreActive = false;
    int prevNumRingsInCollision = 0;

    public GameObject sliderLeftPnt;
    public GameObject sliderRightPnt;
    public GameObject sliderPoint;

    float sliderPointDistance = 0.0f;
    bool updateSlider = false;

    private bool isCollidingWithRing;
    public bool padJustPressedDown;

    private UserDataCollectionHandler udch;
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
        udch = FindObjectOfType<UserDataCollectionHandler>();
        /*
        sphereData.setMainLayout(SphereData.SphereLayout.Sphere);
        sphereData.SetMainRingCategory(SphereData.MainRingCategory.Year);
        */

        setSliderLocalPosition(sphereData.BundlingStrength);


        //this was getting some other reference to another instance of fmh_script
        //fmh_script = GameObject.FindObjectOfType<FormMenuHandler>();
        form_questions = fmh_script.form_questions;
        submitButton = GameObject.FindGameObjectWithTag("SubmitButton");
        sbs = submitButton.GetComponent<SubmitButtonScript>();
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

        //sphereCollider.center = new Vector3(0.0f, 0.0f, 0.03f);

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
            sphereData.updateAllConnections();
        }
    }

    void LateUpdate()
    {
        if (submitButton != null)
        {
            submitButton.SetActive(sbs.readyForSubmit);
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

        sphereData.BundlingStrength = tDist;
    }

    void projectBeam()
    {
        float beamDist = 10.0f;

        RaycastHit hitInfo;

        if (Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, 30.0f, menusLayerMask))
        {
          
            activeBeamInterceptObj = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;

            if (activeBeamInterceptObj != activeNodeMenu)
            {
                reduceSizeOfActiveMenu();
                activeNodeMenu = activeBeamInterceptObj;
                increaseSizeOfActiveMenu();
            }

            if( activeBeamInterceptObj.name.Contains("Actor:") && activeBeamInterceptObj != activeActorText)
            {
                if(activeActorText != null ) activeActorText.transform.localScale = actorTextNormalScale;
                activeActorText = activeBeamInterceptObj;
                activeActorText.transform.localScale = actorTextLargeScale;
            }
        }
        else
        {
            reduceSizeOfActiveMenu();

            activeNodeMenu = null;
            activeBeamInterceptObj = null;
        }

        LineRenderer lineRend = beam.GetComponent<LineRenderer>();
        Vector3 end = deviceRay.GetPoint(beamDist);

        lineRend.SetPosition(0, deviceRay.origin);
        lineRend.SetPosition(1, end);
    }

    void increaseSizeOfActiveMenu()
    {
        if (activeNodeMenu != null)
        {
            NodeMenuUtils menuUtils = getFirstNodeMenuUtilsOfParents(activeNodeMenu);
            if (menuUtils != null)
            {
                menuUtils.makeLarge();
            }
        }
    }

    void reduceSizeOfActiveMenu()
    {
        if (activeNodeMenu != null)
        {
            NodeMenuUtils menuUtils = getFirstNodeMenuUtilsOfParents(activeNodeMenu);
            if (menuUtils != null)
            {
                menuUtils.makeSmall();
            }
        }

        if(activeActorText != null)
        {
            activeActorText.transform.localScale = actorTextNormalScale;
            activeActorText = null;
        }
    }


    NodeMenuUtils getFirstNodeMenuUtilsOfParents(GameObject obj)
    {

        GameObject tObj = obj;
        NodeMenuUtils menuUtils = null;

        while (tObj != null)
        {
            menuUtils = tObj.GetComponent<NodeMenuUtils>();
            if (menuUtils != null)
            {
                return menuUtils;
            }

            if (tObj.transform.parent != null) tObj = tObj.transform.parent.gameObject;
            else tObj = null;
        }

        return null;
    }

    void triggerActiverBeamObject()
    {
        if( activeBeamInterceptObj != null )
        {
            if (activeBeamInterceptObj.tag == "CloseButton")
            {
                activeBeamInterceptObj.transform.parent.gameObject.SetActive(false); // All purpose blind close button code: sets direct parent of button inactive
            }
            else if (activeBeamInterceptObj.tag == "RadioButton")
            {
                udch.PromptUserInput(activeBeamInterceptObj.GetComponentInChildren<TextMesh>().text);
            }
            else if (activeBeamInterceptObj.tag == "SubmitButton")
            {
                sbs = activeBeamInterceptObj.GetComponent<SubmitButtonScript>();
                submitButton = activeBeamInterceptObj;
                if (udch.gameObject.activeSelf)
                {
                    udch.HandleUserInput();
                }
                else
                {
                    fmh_script.SubmitQuestionAnswer();
                }
            }
            NodeMenuHandler menuHandler = activeBeamInterceptObj.GetComponent<NodeMenuHandler>();
          
            if (menuHandler != null )
            {

                menuHandler.handleTrigger();

                //MovieObject mo = activeBeamInterceptObj.transform.parent.transform.parent.gameObject.GetComponent<MovieObject>();
                MovieObject mo = activeBeamInterceptObj.transform.parent.GetComponent<NodeMenuUtils>().movieObject;
                sphereData.connectMoviesByActors(mo.cmData);
                sphereData.updateAllKeptConnections(ringsInCollision);
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
                            else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl") == 0)
                                destLayout = SphereData.SphereLayout.Column_X;
                           

                            sphereData.setMainLayout(destLayout);
                            mainMenu.updateLayout();
                        }
                    }
                    else if(activeBeamInterceptObj.name.CompareTo("Box-Show_Conn") == 0)
                    {
                        sphereData.toggleEdgesAlwaysOn();
                        mainMenu.updateLayout();
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

    public void handleSlider()
    {
        if (fmh_script.amountScrolled < 70 && state.rAxis0.x >= 0)
        {
            fmh_script.amountScrolled += state.rAxis0.x;
        }

        else if (fmh_script.amountScrolled >= 0 && state.rAxis0.x < 0)
        {
            fmh_script.amountScrolled += state.rAxis0.x;
        }
        else if (fmh_script.amountScrolled <= 70 && fmh_script.amountScrolled >= 0)
        {
            fmh_script.amountScrolled += state.rAxis0.x;
        }

        GameObject formMenu = GameObject.FindGameObjectWithTag("FormMenu");
        float radioButtonOffset = 70;
        if (formMenu != null)
        {
            foreach (Transform t in formMenu.GetComponentsInChildren<Transform>())
            {
                if (t.tag == "Slider")
                {
                    radioButtonOffset -= 10f;

                    FormMenuHandler fmh = t.GetComponent<FormMenuHandler>();
                    if (radioButtonOffset < fmh_script.amountScrolled && radioButtonOffset > fmh_script.amountScrolled - 10)
                    {
                        t.gameObject.GetComponent<FormMenuHandler>().materialStatus = true;
                        sbs.readyForSubmit = true;
                    }
                    else if ((radioButtonOffset == 0 && fmh_script.amountScrolled < 10) || (fmh_script.amountScrolled > 70 && radioButtonOffset == 60))
                    {
                        t.gameObject.GetComponent<FormMenuHandler>().materialStatus = true;
                        sbs.readyForSubmit = true;
                    }
                    else
                    {
                        t.gameObject.GetComponent<FormMenuHandler>().materialStatus = false;
                    }
                    fmh.UpdateMaterial();

                }               
            }
        }
        radioButtonOffset = 70;
    }
    void handleStateChanges()
    {
        
        // enter survey question answer
        if (padClicked() && !isCollidingWithRing)
        {
            //udch.HandleUserInput();

        }
        bool stateIsValid = vrSystem.GetControllerState((uint)index, ref state);


        //if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

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
            }

            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
               (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                // deactivate beam
                if (activeBeamInterceptObj != null)
                {
                    if (activeBeamInterceptObj.tag == "ExpandedPopUpMenu" || activeBeamInterceptObj.tag == "PopUpMenu")
                    {
                        udch.minimzed = !udch.minimzed;
                    }
                }
                if( activeNodeMenu != null )
                {
                    reduceSizeOfActiveMenu();
                    activeNodeMenu = null;
                }

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

                    if (udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput || udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.MultipleInput)
                    {
                        udch.PromptUserInput(m.name);
                    }
                    m.nodeState.toggleSelected();
                    m.nodeState.updateColor();

                    HashSet<EdgeInfo> edgeSet = m.getEdges();
                    if (m.nodeState.getIsSelected()) foreach (EdgeInfo info in edgeSet) info.select();
                    else foreach (EdgeInfo info in edgeSet) info.unselect();
                }

            }


            prevState = state;
        }      
                if ((state.ulButtonTouched) == 4294967296) // if the touchpad is touched 
                {           
                  handleSlider();
                }


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            padJustPressedDown = true;
            if(ringsInCollision.Count < 1)
            {
                Vector2 vec;
                if( Mathf.Abs(state.rAxis0.x) > Mathf.Abs(state.rAxis0.y ) )
                {
                    vec = new Vector2(state.rAxis0.x, 0f);
                    sphereData.rotateGraph(vec);
                }
                else
                {
                    vec = new Vector2(0f, state.rAxis0.y);
                    sphereData.rotateGraph(vec);
                }
                
            }
            else
            {
                Quaternion addRotation = Quaternion.Euler(0.0f, 0.0f, state.rAxis0.y);
                Quaternion origRot;
                foreach (GameObject g in ringsInCollision)
                {
                    origRot = g.transform.localRotation;
                    g.transform.localRotation = origRot * addRotation;
                }

                UpdateConnections();

                sphereData.updateAllKeptConnections(ringsInCollision);
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

            HashSet<EdgeInfo> edgeSet = mo.getEdges();
            foreach (EdgeInfo info in edgeSet)
            {
                info.hightlight();
            }

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

            HashSet<EdgeInfo> edgeSet = mo.getEdges();
            foreach(EdgeInfo info in edgeSet)
            {
                info.unhightlight();
            }

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
