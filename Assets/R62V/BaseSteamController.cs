using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using UnityEngine.SceneManagement;

public class BaseSteamController : SteamVR_TrackedObject
{
    protected int __prevStateHash = 0;
    protected VRControllerState_t __currState;
    protected CVRSystem __vrSystem;

    protected int __frameCount = 0;

    protected int __parentSceneFrameCount = 200;

    protected bool __goToParentScene = false;

    public bool justReleasedTrigger = true;
    public bool collidedWithNode;
    public GameObject otherController;
    protected BaseSteamController otherTrackedObjScript;

    public GameObject menuObject;
    public bool menuActive = false;
    public static bool animationLayout = true;

    public Ray deviceRay;
    public Vector3 currPosition;
    public Vector3 currRightVec;
    public Vector3 currUpVec;
    public Vector3 currForwardVec;
    public Quaternion currRotation;

    public GameObject trackpadArrowObject;

    //List<GameObject> connectionList = new List<GameObject>();

    public Dictionary<string, MovieObject> connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    protected VRControllerState_t state;
    protected VRControllerState_t prevState;

    Quaternion currRingBaseRotation;

    protected GameObject beam;
    protected GameObject activeBeamInterceptObj = null;
    protected GameObject activeNodeMenu = null;
    protected GameObject activeActorText = null;
    protected Vector3 actorTextNormalScale = Vector3.one * 0.1f;
    //Vector3 actorTextLargeScale = Vector3.one * 0.15f;
    protected Vector3 actorTextLargeScale = Vector3.one * 0.1f;

    private GameObject submitButton;
    private SubmitButtonScript sbs;
    public float amount_scrolled = 0;


    protected bool useBeam = false;

    protected int menusLayerMask;

    protected float currRayAngle = 60.0f;

    //bool trackpadArrowsAreActive = false;
    protected int prevNumRingsInCollision = 0;

    public GameObject sliderLeftPnt;
    public GameObject sliderRightPnt;
    public GameObject sliderPoint;

    protected float sliderPointDistance = 0.0f;
    protected bool updateSlider = false;

    private bool isCollidingWithRing;
    public bool padJustPressedDown;

    protected UserDataCollectionHandler udch;

    // reference to instance of FormQuestions class
    private FormMenuHandler.FormQuestions form_questions;
    // reference to form menu script
    public FormMenuHandler fmh_script;

    // Use this for initialization
    protected void Start()
    {
        __vrSystem = OpenVR.System;
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

        udch = FindObjectOfType<UserDataCollectionHandler>();
        fmh_script = GameObject.FindGameObjectWithTag("FormMenuParent").GetComponentInChildren<FormMenuHandler>(true);
        //form_questions = fmh_script.form_questions;
        submitButton = GameObject.FindGameObjectWithTag("SubmitButton");
        // this is null because there are two TrackedObject scripts
        if (submitButton != null)
        {
            sbs = submitButton.GetComponent<SubmitButtonScript>();
        }
    }

    protected void Update()
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

        HandleStateChanges();
        handleSlider(); // new slider code
    }

     void HandleStateChanges()
    {
        bool stateIsValid = __vrSystem.GetControllerState((uint) index, ref state);

        //if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

        if (stateIsValid && state.GetHashCode() != prevState.GetHashCode())
        {
           ApplyStateChanges();
        }
    }

    //Put stuff that both classes need in this base function 
    protected virtual void ApplyStateChanges()
    {

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) != 0 &&
        (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) == 0)
        {
         
            //sphereData.toggleMainLayout();
            ToggleMenu();
        }


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
            (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
        {
            // activate beam
            if (!collidedWithNode)
            {
                beam.SetActive(true);
                useBeam = true;
            }
        }

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
        prevState.rAxis1.x < 1.0f && state.rAxis1.x == 1.0f)
        {
           
            TriggerActiverBeamObject();

            // toggle connections with all movies

            //udch.startCountingTime = true;   // UNCOMMENT THIS LINE IF YOU WANT THE TIME TO START UPON FIRST NODE CLICK
            
            foreach (MovieObject m in connectionMovieObjectMap.Values)
            {
                m.nodeState.toggleSelected();
                m.nodeState.updateColor();
                
                if (udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput || udch.currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.MultipleInput)
                {
                    
                    if (m.nodeState.isSelected)
                    {
                        udch.PromptUserInput(m.name);
                    }
                    else
                    {
                        udch.RemoveAnswer(m.name);
                    }
                }


                HashSet<EdgeInfo> edgeSet = m.getEdges();
                if (m.nodeState.getIsSelected()) foreach (EdgeInfo info in edgeSet) info.select();
                else foreach (EdgeInfo info in edgeSet) info.unselect();
            }

        }


        if ((state.ulButtonTouched) == 4294967296) // if the touchpad is touched 
        {
            /*       OLD SLIDER CODE
            
            if (fmh_script != null)
            {
                handleSlider();
            }*/
        }
    }

    protected void LateUpdate()
    {
        if (submitButton != null)
        {
            if (!udch.gameObject.activeSelf)
            {
                GameObject FormMenu = GameObject.FindGameObjectWithTag("FormMenu");
                sbs = FormMenu.GetComponentInChildren<SubmitButtonScript>(true);

                submitButton = sbs.gameObject;
            }
            submitButton.SetActive(sbs.readyForSubmit);
        }
    }

    protected void FixedUpdate()
    {
        // USEFUL HOTKEYS
        if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            udch.startCountingTime = true;
        }
        /*
        if (Input.GetKeyUp(KeyCode.KeypadDivide))
        {                                                       // developer hotkey
            udch.form_questions.QuestionIndex = 5;
        }
        */

        // END USEFUL HOTKEYS
        // Menu Code Start
        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            SceneParams.setParamValue("ShowEdges", "false");
            SceneManager.LoadScene("SphereScene", LoadSceneMode.Single);
            return;
        }
        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            SceneParams.setParamValue("ShowEdges", "false");
            SceneManager.LoadScene("NodeGraph", LoadSceneMode.Single);
            return;
        }
        if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            SceneParams.setParamValue("ShowEdges", "true");
            SceneManager.LoadScene("SphereScene", LoadSceneMode.Single);
            return;
        }
        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            SceneParams.setParamValue("ShowEdges", "true");
            SceneManager.LoadScene("NodeGraph", LoadSceneMode.Single);
            return;
        }
        // Menu Code End
        if (__vrSystem == null) __vrSystem = OpenVR.System;

        if (__vrSystem.GetControllerState((uint)index, ref __currState))
        {
            if ((__currState.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) != 0) __frameCount++;
            else __frameCount = 0;
        }

        if (__frameCount > __parentSceneFrameCount)
        {
            __goToParentScene = true;
        }
    }

    protected void goToParentScene(string sceneName)
    {
        //Debug.Log("Go to Parent Scene: " + sceneName + " [" + __parentSceneFrameCount + "]");

        __goToParentScene = false;
        __frameCount = 0;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        //Debug.Log("Go to Parent Scene: " + sceneName + " [" + __frameCount + "]");
    }


    /***************************************** MENU *****************************************************/
    protected void IncreaseSizeOfActiveMenu()
    {
        if (activeNodeMenu != null)
        {
            NodeMenuUtils menuUtils = GetFirstNodeMenuUtilsOfParents(activeNodeMenu);
            if (menuUtils != null)
            {
                menuUtils.makeLarge();
            }
        }
    }


    NodeMenuUtils GetFirstNodeMenuUtilsOfParents(GameObject obj)
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

    protected void ReduceSizeOfActiveMenu()
    {
        if (activeNodeMenu != null)
        {
            NodeMenuUtils menuUtils = GetFirstNodeMenuUtilsOfParents(activeNodeMenu);
            if (menuUtils != null)
            {
                menuUtils.makeSmall();
            }
        }

        if (activeActorText != null)
        {
            activeActorText.transform.localScale = actorTextNormalScale;
            activeActorText = null;
        }
    }

    void ToggleMenu()
    {
        if (menuActive) HideMainMenu();
        else ShowMainMenu();
    }

    protected void ShowMainMenu()
    {
        Debug.Log("on");
        menuActive = true;
        otherTrackedObjScript.menuActive = false;

        
        menuObject.transform.SetParent(gameObject.transform);

        menuObject.transform.localPosition = new Vector3(0.0f, 0.02f, 0.0f);
        menuObject.transform.localRotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
        menuObject.transform.localScale = new Vector3(0.25f, 0.25f, 1.0f);
        
        menuObject.SetActive(true);
        Debug.Log(menuObject);
    }

    public void HideMainMenu()
    {
        Debug.Log("off");
        menuActive = false;
        menuObject.SetActive(false);
    }


    /***************************************** BEAM OBJECT *********************************************/
    protected virtual void TriggerActiverBeamObject()
    {
        if (activeBeamInterceptObj != null)
        {

            FormMenuHandler formMenuHandler = activeBeamInterceptObj.GetComponent<FormMenuHandler>();
            if (formMenuHandler != null)
            {
                formMenuHandler.handleTrigger();  
            }

            if (activeBeamInterceptObj.tag == "CloseButton")
            {
                activeBeamInterceptObj.transform.parent.gameObject.SetActive(false);
             
                // All purpose blind close button code: sets direct parent of button inactive
            }

            else if (activeBeamInterceptObj.tag == "RadioButton")
            {
                udch.PromptUserInput(activeBeamInterceptObj.GetComponentInChildren<TextMesh>().text);
                FormMenuHandler fmh = activeBeamInterceptObj.GetComponentInChildren<FormMenuHandler>();
                fmh.UpdateMaterial();
                
            }
            else if (activeBeamInterceptObj.tag == "SubmitButton")
            {
                if (justReleasedTrigger)
                {
                    //sbs = activeBeamInterceptObj.GetComponent<SubmitButtonScript>();
                    submitButton = activeBeamInterceptObj;
                    if (udch.gameObject.activeSelf)
                    {
                        udch.HandleUserInput();
                    }
                    else
                    {
                        fmh_script.SubmitQuestionAnswer();
                    }


                    if (activeBeamInterceptObj.name.Contains("Text"))
                    {

                        activeBeamInterceptObj.transform.GetComponentInChildren<FormMenuHandler>().handleTrigger();
                    }
                }
                
                if (SceneManager.GetActiveScene().name == "NodeGraph")
                {
                    justReleasedTrigger = false;
                }
            }
        }
    }


    protected void handleSlider()
    {
        //            THIS FUNCTION HAS BEEN KNOWN TO THROW NULL REFERENCE ON SBS
        //            IN THE EVENT THAT THIS OCCURS, PUT NULL CHECKS ON THE LINES IT THROWS ERRORS ON
        if (fmh_script != null)
        {
            if (fmh_script.gameObject.activeSelf)
            {
                //if (sbs.readyForSubmit == false)     // probably irrelevant
                bool found_active = false;
                foreach (FormMenuHandler fmh in fmh_script.GetComponentsInChildren<FormMenuHandler>())
                {
                    if ((fmh.tag == "RadioButton") && fmh.materialStatus)
                    {
                        sbs.readyForSubmit = true;
                        fmh.UpdateMaterial();
                        found_active = true;
                        break;
                    }
                }
                if (!found_active)
                {
                    sbs.readyForSubmit = false;
                }
            }
        }


        //OLD SLIDER CODE
                /* 
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
                                fmh_script.currentSliderValue = Mathf.RoundToInt((radioButtonOffset + 10) / 10);

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
            */
            }
    
}
