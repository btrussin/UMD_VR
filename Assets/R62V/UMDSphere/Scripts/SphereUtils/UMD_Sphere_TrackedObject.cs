using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Valve.VR;
public class UMD_Sphere_TrackedObject : BaseSteamController
{

    private List<GameObject> ringsInCollision;
    private SphereCollider sphereCollider;
    private SphereData sphereData;

    public GameObject dataObj;
    new void Start()
    {
        base.Start();

        sphereData = dataObj.GetComponent<SphereData>();
        sphereCollider = gameObject.GetComponent<SphereCollider>();
        sphereCollider.transform.SetParent(gameObject.transform);

        otherTrackedObjScript = otherController.GetComponent<UMD_Sphere_TrackedObject>();

        setSliderLocalPosition(sphereData.BundlingStrength);
    }

    new void FixedUpdate()
    {
        base.FixedUpdate();
        if (__goToParentScene)
        {
            NodeDetailsManager.removeAllDetails();
            goToParentScene("PivotScene");
        }
    }

   new void Update()
   {
        base.Update();
       //if (sphereData == null)
       //{
       //     sphereData = dataObj.GetComponent<SphereData>();  // this conditional was to fix a null reference on line 47 that only happens sometimes.  TODO: figure out why
       // }
        ringsInCollision = sphereData.getRingsInCollision(currPosition + (currForwardVec - currUpVec) * (0.03f + sphereCollider.radius), sphereCollider.radius * 2.0f);
        if (ringsInCollision.Count > 0)
        {
            sphereData.addActiveRings(ringsInCollision);
            if (prevNumRingsInCollision == 0) ShowTrackpadArrows();
        }
        else if (prevNumRingsInCollision > 0 && (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0)
        {
            HideTrackpadArrows();
        }

        prevNumRingsInCollision = ringsInCollision.Count;

        if (useBeam) ProjectBeam();

        if (updateSlider)
        {
            CalcSliderPosition();
            sphereData.updateAllConnections();
        }
    }

    void ShowTrackpadArrows()
    {
        trackpadArrowObject.SetActive(true);
    }

    void HideTrackpadArrows()
    {
        trackpadArrowObject.SetActive(false);
    }


    new void LateUpdate()
    {
        base.LateUpdate();
    }

    //TODO - Call the MenuManager for both Sphere and ForceDirTrackedObj, instead of this one
    void CalcSliderPosition()
    {
        // get proposted position of the slider point in world space
        Vector3 tVec = deviceRay.GetPoint(sliderPointDistance);

        // project that point onto the world positions of the slider ends
        Vector3 v1 = sliderRightPnt.transform.position - sliderLeftPnt.transform.position;
        Vector3 v2 = tVec - sliderLeftPnt.transform.position;

        // 'd' is the vector-projection amount of v2 onto v1
        float d = Vector3.Dot(v1, v2) / Vector3.Dot(v1, v1);

        // 'd' is also the correct linear combination of the left and right slider edges
        // left * d + right * ( 1 - d )
        setSliderLocalPosition(d);
    }

    void setSliderLocalPosition(float dist)
    {
        // clamp dist to 0.0 and 1.0
        // float tDist = Mathf.Min(1.0f, Mathf.Max(0.0f, dist));
        float tDist = Mathf.Clamp(dist, 0.0f, 1.0f);
        Vector3 tVec = (sliderRightPnt.transform.localPosition - sliderLeftPnt.transform.localPosition) * tDist;
        sliderPoint.transform.localPosition = sliderLeftPnt.transform.localPosition + tVec;

        sphereData.BundlingStrength = tDist;
    }


    void ProjectBeam()
    {
        float beamDist = 10.0f;

        RaycastHit hitInfo;

        if (Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, 30.0f, menusLayerMask))
        {
            beam.SetActive(true);
            activeBeamInterceptObj = hitInfo.collider.gameObject;
            beamDist = hitInfo.distance;

            if (activeBeamInterceptObj != activeNodeMenu)
            {
                ReduceSizeOfActiveMenu();
                activeNodeMenu = activeBeamInterceptObj;
                IncreaseSizeOfActiveMenu();
            }

            if (activeBeamInterceptObj.name.Contains("Actor:") && activeBeamInterceptObj != activeActorText)
            {
                if (activeActorText != null) activeActorText.transform.localScale = actorTextNormalScale;
                activeActorText = activeBeamInterceptObj;
                activeActorText.transform.localScale = actorTextLargeScale;
            }
        }
        else
        {
            ReduceSizeOfActiveMenu();

            activeNodeMenu = null;
            activeBeamInterceptObj = null;
        }

        LineRenderer lineRend = beam.GetComponent<LineRenderer>();
        Vector3 end = deviceRay.GetPoint(beamDist);

        lineRend.SetPosition(0, deviceRay.origin);
        lineRend.SetPosition(1, end);
    }

    protected override void TriggerActiverBeamObject()
    {
        if (activeBeamInterceptObj != null)
        {
            base.TriggerActiverBeamObject();    
            NodeMenuHandler menuHandler = activeBeamInterceptObj.GetComponent<NodeMenuHandler>();

            if (menuHandler != null)
            {

                menuHandler.handleTrigger();

                //MovieObject mo = activeBeamInterceptObj.transform.parent.transform.parent.gameObject.GetComponent<MovieObject>();
                MovieObject mo = activeBeamInterceptObj.transform.parent.GetComponent<NodeMenuUtils>().movieObject;
                sphereData.connectMoviesByActors(mo.cmData);
                sphereData.updateAllKeptConnections(ringsInCollision);
            }

            else
            {

                MainMenuUtils mainMenu = this.GetComponent<MainMenuUtils>();
                if (mainMenu != null)
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
                    else if (activeBeamInterceptObj.name.CompareTo("Box-Show_Conn") == 0)
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
                    else if (activeBeamInterceptObj.name.CompareTo("Quad_Slider_Point") == 0)
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

    //Put specific state calls in this function
    protected override void ApplyStateChanges() 
    {
        base.ApplyStateChanges();


        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0 &&
            (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0)
        {
            sphereData.grabSphereWithObject(gameObject);
        }

        else if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) == 0 &&
            (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Grip) != 0)
        {
            sphereData.releaseSphereWithObject(gameObject);
        }

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) == 0 &&
               (prevState.ulButtonPressed & SteamVR_Controller.ButtonMask.Trigger) != 0)
        {
            if (activeNodeMenu != null)
            {
                ReduceSizeOfActiveMenu();
                activeNodeMenu = null;
            }

            beam.SetActive(false);
            useBeam = false;
            activeBeamInterceptObj = null;
            if (ringsInCollision.Count == 0) HideTrackpadArrows();

            updateSlider = false;
        }

        if ((state.ulButtonPressed & SteamVR_Controller.ButtonMask.Touchpad) != 0)
        {
            padJustPressedDown = true;
            if (ringsInCollision.Count < 1)
            {
                Vector2 vec;
                if (Mathf.Abs(state.rAxis0.x) > Mathf.Abs(state.rAxis0.y))
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

        prevState = state;
    }

    void OnCollisionEnter(Collision col)
    {
        collidedWithNode = true;
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

            if (!ns.getIsSelected())
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
        collidedWithNode = false;
        GameObject obj = col.gameObject;
        if (obj.name.Contains("MovieNode"))
        {
            MovieObject mo = obj.transform.parent.gameObject.GetComponent<MovieObject>();
            string key = MovieDBUtils.getMovieDataKey(mo.cmData);

            mo.nodeState.removeCollision();
            mo.nodeState.updateColor();

            HashSet<EdgeInfo> edgeSet = mo.getEdges();
            foreach (EdgeInfo info in edgeSet)
            {
                info.unhightlight();
            }

            if (!mo.nodeState.getIsSelected())
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
        foreach (string key in keys)
        {
            if (connectionMovieObjectMap.TryGetValue(key, out mo))
            {
                mo.connManager.ForceClearAllConnections();
                sphereData.connectMoviesByActors(mo.cmData);
            }
        }
    }

}