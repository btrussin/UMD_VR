//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using UnityEngine;
using Valve.VR;

public class R62V_SteamVR_TrackedObject : MonoBehaviour
{
    public Ray deviceRay;

    public enum EIndex
    {
        None = -1,
        Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
        Device1,
        Device2,
        Device3,
        Device4,
        Device5,
        Device6,
        Device7,
        Device8,
        Device9,
        Device10,
        Device11,
        Device12,
        Device13,
        Device14,
        Device15
    }

    public EIndex index;
    public Transform origin; // if not set, relative to parent
    public bool isValid = false;
    protected SteamVR_Utils.RigidTransform currPose;

    protected Vector3 currPosition;
    protected Vector3 currRightVec;
    protected Vector3 currUpVec;
    protected Vector3 currForwardVec;
    protected Quaternion currRotation;


    VRControllerState_t state;
    VRControllerState_t prevState;

    CVRSystem vrSystem;

    R62V_InteractionManager interactionManager;



    void Start()
    {

        vrSystem = OpenVR.System;

        interactionManager = this.gameObject.GetComponent<R62V_InteractionManager>();
    }


    // Update is called once per frame
    void Update()
    {

        interactionManager.deviceRay = deviceRay;
        interactionManager.currPosition = currPosition;
        interactionManager.currRotation = currRotation;
        interactionManager.currRightVec = currRightVec;
        interactionManager.currUpVec = currUpVec;
        interactionManager.currForwardVec = currForwardVec;

        bool stateIsValid = vrSystem.GetControllerState((uint)index, ref state);

        if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

        if (stateIsValid && state.GetHashCode() != prevState.GetHashCode())
        {

            if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) != 0 &&
                (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) == 0)
            {
                interactionManager.swapMode();
            }
            if (prevState.rAxis1.x < 1.0f && state.rAxis1.x >= 1.0f)
            {
                interactionManager.tryToSelectObject();
            }
            else if (prevState.rAxis1.x > 0.0f && state.rAxis1.x <= 0.0f)
            {
                interactionManager.displayRayBeam(false, 0.0f);
            }


            prevState = state;
        }

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            interactionManager.adjustBaseDistance(state.rAxis0.y);
        }

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
        {
            interactionManager.displayRayBeam(true, 5.0f);
        }

    }



    public Ray getDeviceRay()
    {
        return deviceRay;
    }

    protected void OnNewPoses(params object[] args)
    {
        if (index == EIndex.None)
            return;

        var i = (int)index;

        isValid = false;
        var poses = (Valve.VR.TrackedDevicePose_t[])args[0];
        if (poses.Length <= i)
            return;

        if (!poses[i].bDeviceIsConnected)
            return;

        if (!poses[i].bPoseIsValid)
            return;

        isValid = true;

        currPose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

        if (origin != null)
        {
            currPose = new SteamVR_Utils.RigidTransform(origin) * currPose;
            currPose.pos.x *= origin.localScale.x;
            currPose.pos.y *= origin.localScale.y;
            currPose.pos.z *= origin.localScale.z;
            transform.position = currPose.pos;
            transform.rotation = currPose.rot;
        }
        else
        {
            transform.localPosition = currPose.pos;
            transform.localRotation = currPose.rot;
        }

        currPosition = currPose.pos;
        currRotation = currPose.rot;
        currForwardVec = currRotation * Vector3.forward;
        currRightVec = currRotation * Vector3.right;
        currUpVec = currRotation * Vector3.up;

        deviceRay = new Ray(currPosition, currForwardVec);

        
    }

    void OnEnable()
    {
        var render = SteamVR_Render.instance;
        if (render == null)
        {
            enabled = false;
            return;
        }

        //SteamVR_Utils.Event.Listen("new_poses", OnNewPoses);
    }

    void OnDisable()
    {
        //SteamVR_Utils.Event.Remove("new_poses", OnNewPoses);
        isValid = false;
    }

    public void SetDeviceIndex(int index)
    {
        if (System.Enum.IsDefined(typeof(EIndex), index))
            this.index = (EIndex)index;
    }
}
