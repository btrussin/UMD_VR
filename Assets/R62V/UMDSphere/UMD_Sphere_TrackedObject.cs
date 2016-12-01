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

    // Use this for initialization
    void Start()
    {
        vrSystem = OpenVR.System;

        sphereCollider = gameObject.GetComponent<SphereCollider>();
        sphereCollider.transform.SetParent(gameObject.transform);

        sphereData = dataObj.GetComponent<SphereData>();
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
        deviceRay.direction = currForwardVec;

        sphereCollider.center = new Vector3(0.0f, 0.0f, 0.03f);

        handleStateChanges();

        ringsInCollision = sphereData.getRingsInCollision(currPosition + (currForwardVec - currUpVec) * (0.03f + sphereCollider.radius) , sphereCollider.radius*2.0f);
        if (ringsInCollision.Count > 0) sphereData.addActiveRings(ringsInCollision);
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
                prevState.rAxis1.x < 1.0f && state.rAxis1.x == 1.0f )
            {
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
}
