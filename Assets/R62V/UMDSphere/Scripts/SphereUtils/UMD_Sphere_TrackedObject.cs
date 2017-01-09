using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using System;

public class UMD_Sphere_TrackedObject : SteamVR_TrackedObject
{
    public GameObject DataObj;

    SphereData _sphereData;

    public Ray DeviceRay;
    public Vector3 CurrPosition;
    public Vector3 CurrRightVec;
    public Vector3 CurrUpVec;
    public Vector3 CurrForwardVec;
    public Quaternion CurrRotation;

    public GameObject TrackpadArrowObject;

    SphereCollider _sphereCollider;

    //List<GameObject> connectionList = new List<GameObject>();

    readonly Dictionary<string, MovieObject> _connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    CVRSystem _vrSystem;

    VRControllerState_t _state;
    VRControllerState_t _prevState;

    Quaternion _currRingBaseRotation;

    List<GameObject> _ringsInCollision;

    GameObject _beam;
    GameObject _activeBeamInterceptObj = null;

    bool _useBeam = false;
    bool _spawnedMenu = false;

    int _menusLayerMask;

    float _currRayAngle = 30.0f;

    int _prevNumRingsInCollision = 0;

    void Awake()
    {

        if (this.transform.name == "Controller (left)")
        {
            this.transform.FindChild("Model").gameObject.AddComponent<ControllerState>(); //add for left controller for now
        }
        else
        {
            _spawnedMenu = true;
        }
    }

    void Start()
    {
        _vrSystem = OpenVR.System;

        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _sphereCollider.transform.SetParent(gameObject.transform);

        _sphereData = DataObj.GetComponent<SphereData>();

        _beam = new GameObject();
        _beam.AddComponent<LineRenderer>();
        LineRenderer lineRend = _beam.GetComponent<LineRenderer>();
        lineRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRend.receiveShadows = false;
        lineRend.motionVectors = false;
        lineRend.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/BeamMaterial.mat");
        lineRend.SetWidth(0.003f, 0.003f);
        _beam.SetActive(false);

        _menusLayerMask = 1 << LayerMask.NameToLayer("Menus");

    }

    void Update()
    {
        CurrPosition = transform.position;
        CurrRightVec = transform.right;
        CurrUpVec = transform.up;
        CurrForwardVec = transform.forward;
        CurrRotation = transform.rotation;
        DeviceRay.origin = CurrPosition;

        Quaternion rayRotation = Quaternion.AngleAxis(_currRayAngle, CurrRightVec);

        DeviceRay.direction = rayRotation * CurrForwardVec;

        _sphereCollider.center = new Vector3(0.0f, 0.0f, 0.03f);

        if (!_spawnedMenu && GameObject.Find("Controller (left)") != null)
        {
            SetupControllerMenu();
        }

        HandleStateChanges();

        _ringsInCollision = _sphereData.GetRingsInCollision(CurrPosition + (CurrForwardVec - CurrUpVec) * (0.03f + _sphereCollider.radius) , _sphereCollider.radius*2.0f);
        if (_ringsInCollision.Count > 0)
        {
            //TODO Spawn a Red Animation Circle In The Center of the Controller
            SpawnRedCircle();
            _useBeam = false;
            _sphereData.AddActiveRings(_ringsInCollision);
            if (_prevNumRingsInCollision == 0) showTrackpadArrows();
        }
        else if (_prevNumRingsInCollision > 0 && (_prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
        {
            hideTrackpadArrows();
        }

        _prevNumRingsInCollision = _ringsInCollision.Count;

        if (_useBeam) ProjectBeam();
        else
        {
            _beam.SetActive(false);
            _activeBeamInterceptObj = null;
        }
    }

    private void SpawnRedCircle()
    {
        //TODO To be used to tell users when one is hovering over a datapoint
    }

    void SetupControllerMenu()
    {
        this.transform.FindChild("Model").gameObject.GetComponent<ControllerState>().toggleSelected();

        _spawnedMenu = true;
    }

    void ProjectBeam()
    {
        float beamDist = 10.0f;

        RaycastHit hitInfo;

        if (Physics.Raycast(DeviceRay.origin, DeviceRay.direction, out hitInfo, 30.0f, _menusLayerMask))
        {
            _activeBeamInterceptObj = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;
        }
        else
        {
            _activeBeamInterceptObj = null;
        }

        LineRenderer lineRend = _beam.GetComponent<LineRenderer>();
        Vector3 end = DeviceRay.GetPoint(beamDist);

        lineRend.SetPosition(0, DeviceRay.origin);
        lineRend.SetPosition(1, end);
    }

    void TriggerActiverBeamObject()
    {
        if( _activeBeamInterceptObj != null )
        {
            NodeMenuHandler menuHandler = _activeBeamInterceptObj.GetComponent<NodeMenuHandler>();
            if(menuHandler != null )
            {

                menuHandler.handleTrigger();

                MovieObject mo = _activeBeamInterceptObj.transform.parent.transform.parent.gameObject.GetComponent<MovieObject>();
                _sphereData.ConnectMoviesByActors(mo.cmData);
                _sphereData.UpdateAllKeptConnections();
            }

            ControllerMenuHandler controllerMenuHandler = _activeBeamInterceptObj.GetComponent<ControllerMenuHandler>();
            if (controllerMenuHandler != null)
            {
                controllerMenuHandler.handleTrigger();
            }

            _activeBeamInterceptObj = null;
        }
    }

    void HandleStateChanges()
    {
        bool stateIsValid = _vrSystem.GetControllerState((uint)index, ref _state);

        if (!stateIsValid) Debug.Log("Invalid State for Idx: " + index);

        if (stateIsValid && _state.GetHashCode() != _prevState.GetHashCode())
        {

            if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) != 0 &&
                (_prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.ApplicationMenu) == 0)
            {
                _sphereData.ToggleMainLayout();
            }

            if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0 &&
                (_prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0 )
            {
                _sphereData.GrabSphereWithObject(gameObject);
            }
            else if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0 &&
                (_prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0 )
            {
                _sphereData.ReleaseSphereWithObject(gameObject);
            }

            if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
               (_prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
            {
                // activate bean
                _beam.SetActive(true);
                _useBeam = true;

                showTrackpadArrows();
            }

            else if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
               (_prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
            {
                // deactivate bean
                _beam.SetActive(false);
                _useBeam = false;
                _activeBeamInterceptObj = null;
                if (_ringsInCollision.Count == 0)
                {
                    hideTrackpadArrows();
                }
                if (this.transform.FindChild("Model").GetComponent<ControllerState>() != null &&
                    !this.transform.FindChild("Model").GetComponent<ControllerState>().getIsSelected())
                {
                    this.transform.FindChild("Model").GetComponent<ControllerState>().toggleSelected();
                }
            }

            if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 &&
                _prevState.rAxis1.x < 1.0f && _state.rAxis1.x == 1.0f )
            {

                TriggerActiverBeamObject();

                // toggle connections with all movies
                foreach (MovieObject m in _connectionMovieObjectMap.Values)
                {
                    m.nodeState.toggleSelected();
                    m.nodeState.updateColor();

                    if (this.transform.FindChild("Model").GetComponent<ControllerState>() != null &&
                        this.transform.FindChild("Model").GetComponent<ControllerState>().getIsSelected())
                    {
                        this.transform.FindChild("Model").GetComponent<ControllerState>().toggleSelected();
                    }
                }

            } 

            _prevState = _state;
        }


        if ((_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            Quaternion addRotation = Quaternion.Euler(0.0f, 0.0f, _state.rAxis0.y);
            Quaternion origRot;
            GameObject innerRot;
            foreach (GameObject g in _ringsInCollision)
            {
                innerRot = g.transform.GetChild(0).gameObject;
                origRot = innerRot.transform.localRotation;

                innerRot.transform.localRotation = origRot * addRotation;
            }

            UpdateConnections();

            _sphereData.UpdateAllKeptConnections();

            // update the beam ray direction
            if (_ringsInCollision.Count == 0 && (_state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0 )
            {
                if (_state.rAxis0.y > 0.0f) _currRayAngle -= 1.0f;
                else if (_state.rAxis0.y < 0.0f) _currRayAngle += 1.0f;

                // keep within the bounds of 0 and 90 degrees
                if (_currRayAngle > 90.0f) _currRayAngle = 90.0f;
                else if (_currRayAngle < 0.0f) _currRayAngle = 0.0f;

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
                _sphereData.ConnectMoviesByActors(mo.cmData);
            }

            _connectionMovieObjectMap.Add(key, mo);
        } else
        {

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

            _connectionMovieObjectMap.Remove(key);
        }
    }

    void UpdateConnections()
    {
        Dictionary<string, MovieObject>.KeyCollection keys = _connectionMovieObjectMap.Keys;

        if (keys.Count < 1) return;
        MovieObject mo;
        foreach ( string key in keys )
        {
            if( _connectionMovieObjectMap.TryGetValue(key, out mo) )
            {
                mo.connManager.ForceClearAllConnections();
                _sphereData.ConnectMoviesByActors(mo.cmData);
            }
        }
    }

    void showTrackpadArrows()
    {
        TrackpadArrowObject.SetActive(true);
    }

    void hideTrackpadArrows()
    {
        TrackpadArrowObject.SetActive(false);
    }

    
}
