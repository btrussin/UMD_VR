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
        menuObject.SetActive(false);

        otherTrackedObjScript = otherController.GetComponent<UMD_Sphere_TrackedObject>();
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
            if(menuHandler != null )
            {

                menuHandler.handleTrigger();

                //MovieObject mo = activeBeamInterceptObj.transform.parent.transform.parent.gameObject.GetComponent<MovieObject>();
                MovieObject mo = activeBeamInterceptObj.transform.parent.GetComponent<NodeMenuUtils>().movieObject;
                sphereData.connectMoviesByActors(mo.cmData);
                sphereData.updateAllKeptConnections();
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

                        if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Sphere") == 0) destLayout = SphereData.SphereLayout.Sphere;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl_X") == 0) destLayout = SphereData.SphereLayout.Column_X;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl_Y") == 0) destLayout = SphereData.SphereLayout.Column_Y;
                        else if (activeBeamInterceptObj.name.CompareTo("Box-Lay_Cyl_Z") == 0) destLayout = SphereData.SphereLayout.Column_Z;

                        sphereData.setMainLayout(destLayout);
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
                }
            }

            activeBeamInterceptObj = null;
        }
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
                // activate bean
                beam.SetActive(true);
                useBeam = true;

                if( adjustRayAngle ) showTrackpadArrows();
            }

            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
               (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                // deactivate bean
                beam.SetActive(false);
                useBeam = false;
                activeBeamInterceptObj = null;
                if (ringsInCollision.Count == 0) hideTrackpadArrows();
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


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            Quaternion addRotation = Quaternion.Euler(0.0f, 0.0f, state.rAxis0.y);
            Quaternion origRot;
            GameObject innerRot;
            foreach (GameObject g in ringsInCollision)
            {
                innerRot = g.transform.GetChild(0).gameObject;
                origRot = innerRot.transform.localRotation;

                innerRot.transform.localRotation = origRot * addRotation;
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
        if (menuActive ) hideMainMenu();
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
