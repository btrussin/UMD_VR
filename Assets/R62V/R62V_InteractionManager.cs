using UnityEngine;
using System.Collections;

public class R62V_InteractionManager : MonoBehaviour {

    public Ray deviceRay;
    public Vector3 currPosition;
    public Vector3 currRightVec;
    public Vector3 currUpVec;
    public Vector3 currForwardVec;
    public Quaternion currRotation;

    public enum InterfaceActionState
    {
        SelectObject,
        PlaceObject,
        MovingObject
    }

    public enum InterfaceSelectState
    {
        Universe,
        Ball
    }


    public GameObject ptLine;
    public GameObject modeLabel;

    public GameObject labelLine;

    GameObject currSelectedUniverse = null;
    GameObject currSelectedBall = null;

    UniverseGroupManager uniGrpManager;
    public GameObject universeGroup;

    float baseDist;
    Quaternion baseQuat;
    Vector3 baseOffset;

    float prevModeSwapTime = 0.0f;

    InterfaceActionState currActionState = InterfaceActionState.SelectObject;
    InterfaceSelectState currInterfaceState = InterfaceSelectState.Universe;

    public GameObject placementCylinder;

    // Use this for initialization
    void Start()
    {
        if (!ptLine) ptLine = new GameObject();

        uniGrpManager = universeGroup.GetComponent<UniverseGroupManager>();

        displayRayBeam(false, 0.0f);

        updateLabelText();

        setPlacementCylinder();

    }

    public void updateLabels()
    {
        // update the labels
        float scaleValue = 0.025f;
        Quaternion qX = Quaternion.AngleAxis(90.0f, Vector3.right);
        Vector3 offset = new Vector3(0.05f, 0.005f, 0.0f);
        Vector3 scale = new Vector3(scaleValue, scaleValue, scaleValue);

        Vector3 trans1 = currForwardVec * offset.z + currUpVec * offset.y + currRightVec * offset.x;



        modeLabel.transform.rotation = currRotation * qX;
        modeLabel.transform.position = currPosition + trans1;
        modeLabel.transform.localScale = scale;

        Vector3 pt0 = getViveMenuButtonPosition();
        Vector3 pt1 = pt0 + currRightVec * 0.05f;
       

        LineRenderer lineRend = labelLine.GetComponent<LineRenderer>();
        lineRend.SetPosition(0, pt0);
        lineRend.SetPosition(1, pt1);

    }

    // Update is called once per frame
    void Update()
    {
        updateLabels();

        if( currActionState == InterfaceActionState.PlaceObject )
        {
            setPlacementCylinder();
        }
        else if( currActionState == InterfaceActionState.MovingObject )
        {
            updateSelectedObject();
        }

    }

    public void unselectBallAndUniverse()
    {
        displayRayBeam(false, 0.0f);

        currSelectedBall = null;
        currSelectedUniverse = null;

        baseDist = 5.0f;

        uniGrpManager.releaseBall();

        currActionState = InterfaceActionState.SelectObject;
        updateLabelText();

        setPlacementCylinder();
    }

    public void adjustBaseDistance(float d)
    {
        float yDist = d;
        baseDist += yDist * 0.05f;

        if (baseDist > 5.0f) baseDist = 5.0f;
        else if (baseDist < 0.1f) baseDist = 0.1f;
    }

    public void updateSelectedObject()
    {
        GameObject tmpObj;
        if (currActionState != InterfaceActionState.MovingObject) return;
        if (currSelectedUniverse != null) tmpObj = currSelectedUniverse;
        else if (currSelectedBall != null) tmpObj = currSelectedBall;
        else return;

        tmpObj.transform.rotation = currRotation * baseQuat;
       
        tmpObj.transform.position = deviceRay.GetPoint(baseDist) + 
            baseOffset.x * tmpObj.transform.up +
            baseOffset.y * tmpObj.transform.right +
            baseOffset.z * tmpObj.transform.forward;

        displayRayBeam(true, baseDist);
    }

    public void tryToSelectObject()
    {
        if( currActionState == InterfaceActionState.PlaceObject )
        {
            placeObject();
            return;
        }

        if ((currSelectedUniverse != null || currSelectedBall != null) && currActionState == InterfaceActionState.MovingObject)
        {
            unselectBallAndUniverse();
            currActionState = InterfaceActionState.SelectObject;
            return;
        }
        if( currActionState == InterfaceActionState.SelectObject )
        {
            Vector3 intPt = Vector3.zero;
            Transform currTrans = null;
            if (currInterfaceState == InterfaceSelectState.Universe)
            {
                currSelectedUniverse = uniGrpManager.getUniverse(deviceRay, out intPt);
                if (currSelectedUniverse != null)
                {
                    currTrans = currSelectedUniverse.transform;
                }
            }
            else if (currInterfaceState == InterfaceSelectState.Ball)
            {
                currSelectedBall = uniGrpManager.getBall(deviceRay, out intPt);

                if (currSelectedBall != null)
                {
                    currTrans = currSelectedBall.transform;
                }
            }


            if(currTrans != null )
            {
                baseQuat = Quaternion.Inverse(currRotation) * currTrans.rotation;

                Vector3 tmpVec = currTrans.position - intPt;

                baseOffset.Set(
                    Vector3.Dot(currTrans.up, tmpVec),
                    Vector3.Dot(currTrans.right, tmpVec),
                    Vector3.Dot(currTrans.forward, tmpVec)
                    );

                Vector3 d = intPt - deviceRay.origin;
                baseDist = d.magnitude;

                currActionState = InterfaceActionState.MovingObject;
                updateLabelText();
            }

        }

        
    }

    public void swapMode()
    {


        if( currActionState == InterfaceActionState.SelectObject )
        {
            if (currInterfaceState == InterfaceSelectState.Universe)
            {
                if (currSelectedUniverse != null) currSelectedUniverse = null;
                currInterfaceState = InterfaceSelectState.Ball;
            }
            else if (currInterfaceState == InterfaceSelectState.Ball)
            {
                if (currSelectedBall != null) currSelectedBall = null;
                currInterfaceState = InterfaceSelectState.Universe;
            }
        }
        else if (currActionState == InterfaceActionState.MovingObject)
        {
            currActionState = InterfaceActionState.PlaceObject;
            displayRayBeam(false,0.0f);
        }
        else if (currActionState == InterfaceActionState.PlaceObject)
        {
            currActionState = InterfaceActionState.MovingObject;

            setPlacementCylinder();
        }


        updateLabelText();
    }

    public void updateLabelText()
    {
        string text = "Change Mode\n\nCurrent Mode: ";

        if (currActionState == InterfaceActionState.SelectObject) text += "Select ";
        else if (currActionState == InterfaceActionState.MovingObject) text += "Move ";
        else if (currActionState == InterfaceActionState.PlaceObject) text += "Place ";

        if (currInterfaceState == InterfaceSelectState.Universe) text += "Universe";
        else if (currInterfaceState == InterfaceSelectState.Ball) text += "Ball";

        TextMesh tm = modeLabel.GetComponent<TextMesh>();
        if (tm != null) tm.text = text;
    }

    public void setPlacementCylinder()
    {
        RaycastHit hitInfo;
        float beamLength = 5.0f;

        if (currActionState == InterfaceActionState.PlaceObject && Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, 30.0f))
        {
            placementCylinder.SetActive(true);

            placementCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            placementCylinder.transform.position = hitInfo.point + hitInfo.normal*0.025f;
            beamLength = hitInfo.distance - 0.05f;
        }
        else
        {
            placementCylinder.SetActive(false);
        }
        displayRayBeam(true, beamLength);
       
    }

    public void placeObject()
    {
        RaycastHit hitInfo;

        if (currActionState == InterfaceActionState.PlaceObject && Physics.Raycast(deviceRay.origin, deviceRay.direction, out hitInfo, 30.0f))
        {
            placementCylinder.SetActive(false);

            GameObject currObj = null;
            if (currInterfaceState == InterfaceSelectState.Ball && currSelectedBall != null)
            {
                currObj = currSelectedBall;
            }
            else if (currInterfaceState == InterfaceSelectState.Universe && currSelectedUniverse != null)
            {
                currObj = currSelectedUniverse;
            }

            if(currObj != null)
            {
                currObj.transform.rotation = Quaternion.FromToRotation(Vector3.back, hitInfo.normal);
                currObj.transform.position = hitInfo.point + hitInfo.normal * 0.025f;
            }

            unselectBallAndUniverse();
        }
       
    }

    public void displayRayBeam(bool display, float dist)
    {
        if (ptLine)
        {


            LineRenderer lineRend = ptLine.GetComponent<LineRenderer>();

            if (lineRend == null)
            {
                ptLine.AddComponent<LineRenderer>();
                lineRend = ptLine.GetComponent<LineRenderer>();
            }

            if (!display)
            {
               
                ptLine.SetActive(false);
                lineRend.enabled = false;
                return;
            }

            ptLine.SetActive(true);
            lineRend.enabled = true;

            Ray ray = deviceRay;



            Vector3 pt2;

              
            //if (currSelectedBall != null || currSelectedUniverse != null) pt2 = ray.GetPoint(baseDist);
            //else pt2 = ray.GetPoint(dist);

            pt2 = ray.GetPoint(dist);

            lineRend.SetPosition(0, ray.origin);
            lineRend.SetPosition(1, pt2);

        }
    }

    Vector3 getViveMenuButtonPosition()
    {
        Vector3 vec = currPosition + currForwardVec * -0.02f + currUpVec * 0.008f;
        return vec;
    }

    Vector3 getViveSettingsButtonPosition()
    {
        Vector3 vec = currPosition + currForwardVec * -0.09f + currUpVec * 0.001f;
        return vec;
    }

    Vector3 getViveTrackPadMiddlePosition()
    {
        Vector3 vec = currPosition + currForwardVec * -0.05f;
        return vec;
    }

    Vector3 getViveTriggerPosition()
    {
        Vector3 vec = currPosition + currForwardVec * -0.05f + currUpVec * -0.038f;
        return vec;
    }

    Vector3 getViveLeftGripPosition()
    {
        Vector3 vec = currPosition + currForwardVec * -0.09f + currUpVec * -0.015f + currRightVec * -0.018f;
        return vec;
    }

    Vector3 getViveRightGripPosition()
    {
        Vector3 vec = currPosition + currForwardVec * -0.09f + currUpVec * -0.015f + currRightVec* 0.018f;
        return vec;
    }
}
