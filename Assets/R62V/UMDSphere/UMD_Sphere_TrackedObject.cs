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

    Material ptMatOrig;
    Material ptMatAlt;

    List<GameObject> connectionList = new List<GameObject>();

    Dictionary<string, List<GameObject>> connectionMap = new Dictionary<string, List<GameObject>>();

    CVRSystem vrSystem;

    VRControllerState_t state;
    VRControllerState_t prevState;

    bool touchingDataSphere = false;

    // Use this for initialization
    void Start()
    {
        vrSystem = OpenVR.System;

        sphereCollider = gameObject.GetComponent<SphereCollider>();
        sphereCollider.transform.SetParent(gameObject.transform);

        ptMatOrig = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/PointMaterial.mat");
        ptMatAlt = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/PointMaterialAlt.mat");

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

            prevState = state;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        GameObject obj = col.gameObject;
        if (obj.name.Contains("MovieNode:"))
        {
            
            Renderer rend = obj.GetComponent<Renderer>();
            rend.material = ptMatAlt;

            MovieObject mo = obj.GetComponent<MovieObject>();

            string key = MovieDBUtils.getMovieDataKey(mo.cmData);

            List<GameObject> objList;

            if (!connectionMap.TryGetValue(key, out objList))
            {
                objList = sphereData.connectMoviesByActors(mo.cmData);
                connectionMap.Add(key, objList);
            }
        }
    }

    void OnCollisionStay(Collision col)
    {
       
    }

    void OnCollisionExit(Collision col)
    {
        GameObject obj = col.gameObject;
        if (obj.name.Contains("MovieNode:"))
        {
            Renderer rend = obj.GetComponent<Renderer>();
            rend.material = ptMatOrig;

            MovieObject mo = obj.GetComponent<MovieObject>();
            string key = MovieDBUtils.getMovieDataKey(mo.cmData);

            List<GameObject> objList;

            if (connectionMap.TryGetValue(key, out objList))
            {
                foreach (GameObject gObj in objList) Destroy(gObj);
                objList.Clear();
                connectionMap.Remove(key);
            }
        }
    }
}
