using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class PivotSceneController : SteamVR_TrackedObject
{

    public SceneAsset sphereScene;
    public SceneAsset nodeLinkScene;
    private AssetBundle loadedAssets;


    private VRControllerState_t prevState;
    private VRControllerState_t currState;
    private CVRSystem vrSystem;

    Ray deviceRay;

    public GameObject hitObj;


    // Use this for initialization
    void Start () {
        vrSystem = OpenVR.System;

    }
	
	// Update is called once per frame
	void Update () {
        Quaternion rayRotation = Quaternion.AngleAxis(60.0f, transform.right);

        deviceRay.origin = transform.position;
        deviceRay.direction = rayRotation * transform.forward;
        RaycastHit hitInfo;
        GameObject selectedObject = null;

        if (Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, 60.0f))
        {
            hitObj.SetActive(true);
            hitObj.transform.position = deviceRay.GetPoint(hitInfo.distance);
            selectedObject = hitInfo.collider.gameObject;
        }
        else hitObj.SetActive(false);

        

        bool stateIsValid = vrSystem.GetControllerState((uint)index, ref currState);

        if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

        if (stateIsValid && currState.GetHashCode() != prevState.GetHashCode())
        {
            if ((currState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                if (prevState.rAxis1.x < 1.0f && currState.rAxis1.x == 1.0f)
                {
                    // just pulled the trigger in all the way

                    if (selectedObject == null) return;

                    if( selectedObject.name.Equals("Sphere_no"))
                    {
                        SceneParams.setParamValue("ShowEdges", "false");
                        SceneManager.LoadScene(sphereScene.name, LoadSceneMode.Single);
                    }
                    else if (selectedObject.name.Equals("Sphere_yes"))
                    {
                        SceneParams.setParamValue("ShowEdges", "true");
                        SceneManager.LoadScene(sphereScene.name, LoadSceneMode.Single);
                    }
                    else if (selectedObject.name.Equals("NodeLink_no"))
                    {
                        SceneParams.setParamValue("ShowEdges", "false");
                        SceneManager.LoadScene(nodeLinkScene.name, LoadSceneMode.Single);
                    }
                    else if (selectedObject.name.Equals("NodeLink_yes"))
                    {
                        SceneParams.setParamValue("ShowEdges", "true");
                        SceneManager.LoadScene(nodeLinkScene.name, LoadSceneMode.Single);
                    }


                }
            }

            prevState = currState;
        }

    }

    public void SwitchScenes(string scene)
    {
        SceneParams.setParamValue("ShowEdges", "false");
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}

public static class SceneParams
{
    private static Dictionary<string, string> paramMap = new Dictionary<string, string>();

    public static void setParamValue(string key, string val)
    {
        if (paramMap.ContainsKey(key)) paramMap[key] = val;
        else paramMap.Add(key, val);
    }

    public static string getParamValue(string key)
    {
        if (paramMap.ContainsKey(key)) return paramMap[key];
        else return "";
    }
}
