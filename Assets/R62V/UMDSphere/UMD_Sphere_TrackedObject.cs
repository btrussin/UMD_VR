using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class UMD_Sphere_TrackedObject : SteamVR_TrackedObject
{
    public GameObject dataObj;

    SphereData sphereData;

    public Ray deviceRay;
    public Vector3 currPosition;
    public Vector3 currRightVec;
    public Vector3 currUpVec;
    public Vector3 currForwardVec;
    public Quaternion currRotation;

    SphereCollider sphereCollider;

    //List<GameObject> connectionList = new List<GameObject>();

    Dictionary<string, MovieObject> connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    CVRSystem vrSystem;

    VRControllerState_t state;
    VRControllerState_t prevState;

    bool touchingDataSphere = false;

    GameObject currRingInCollision = null;
    Quaternion currRingBaseRotation;

    List<GameObject> ringsInCollision;

    GameObject beam;
    GameObject activeBeamInterceptObj = null;

    bool useBeam = false;

    int menusLayerMask;

    float currRayAngle = 30.0f;

    // Use this for initialization
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
        lineRend.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/BeamMaterial.mat");
        lineRend.SetWidth(0.003f, 0.003f);
        beam.SetActive(false);

        menusLayerMask = 1 << LayerMask.NameToLayer("Menus");
    }

    // Update is called once per frame
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
        if (ringsInCollision.Count > 0) sphereData.addActiveRings(ringsInCollision);

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

                MovieObject mo = activeBeamInterceptObj.transform.parent.transform.parent.gameObject.GetComponent<MovieObject>();
                sphereData.connectMoviesByActors(mo.cmData);
                sphereData.updateAllKeptConnections();
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
                sphereData.toggleMainLayout();
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

                showTrackpadArrows();
            }

            else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
               (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                // deactivate bean
                beam.SetActive(false);
                useBeam = false;
                activeBeamInterceptObj = null;
                hideTrackpadArrows();
            }

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
                prevState.rAxis1.x < 1.0f && state.rAxis1.x == 1.0f )
            {

                triggerActiverBeamObject();

                // toggle connections with all movied
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

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 )
            {

                if (state.rAxis0.y > 0.0f) currRayAngle += 1.0f;
                else if (state.rAxis0.y < 0.0f) currRayAngle -= 1.0f;

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
        else if (obj.name.Contains("Ring:"))
        {
            currRingInCollision = obj;
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
                mo.connManager.forceClearAllConnections();
            }

            connectionMovieObjectMap.Remove(key);
        }
        else if (obj.name.Contains("Ring:"))
        {
            currRingInCollision = null;

            Debug.Log("Released a ring");
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
                mo.connManager.forceClearAllConnections();
                sphereData.connectMoviesByActors(mo.cmData);
            }
        }
    }

    void showTrackpadArrows()
    {
        int numChildren = gameObject.transform.childCount;
        GameObject obj;
        for( int i = 0; i < numChildren; i++ )
        {
            obj = gameObject.transform.GetChild(i).gameObject;
            if ( obj.name.CompareTo("trackpadArrows") == 0 )
            {
                obj.SetActive(true);
                break;
            }
        }
    }

    void hideTrackpadArrows()
    {
        int numChildren = gameObject.transform.childCount;
        GameObject obj;
        for (int i = 0; i < numChildren; i++)
        {
            obj = gameObject.transform.GetChild(i).gameObject;
            if (obj.name.CompareTo("trackpadArrows") == 0)
            {
                obj.SetActive(false);
                break;
            }
        }
    }
}
