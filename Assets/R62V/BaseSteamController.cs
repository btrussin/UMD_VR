﻿using System;
using UnityEngine;
using Valve.VR;
using UnityEngine.SceneManagement;

public class BaseSteamController : SteamVR_TrackedObject
{
    private int __prevStateHash = 0;
    private VRControllerState_t __currState;
    private CVRSystem __vrSystem;

    private int __frameCount = 0;

    protected int __parentSceneFrameCount = 200;

    protected bool __goToParentScene = false;

    // Use this for initialization
    void Start () {
        __vrSystem = OpenVR.System;
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    protected void FixedUpdate()
    {
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
}