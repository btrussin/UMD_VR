using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class NodeDetailsManager : MonoBehaviour {

    static Dictionary<string, GameObject> detailsMap = new Dictionary<string, GameObject>();
    private static float radius = 2.0f;
    private static float circumference = radius * Mathf.PI * 2f;

    static bool circleLayout = false;

    public static void addDetails(string name, GameObject obj)
    {
        detailsMap.Add(name, obj);
        if (circleLayout) adjustSpacingInCircle();
        else adjustSpacingInGrid();
    }

    public static void removeDetails(string name)
    {
        detailsMap.Remove(name);
        if (circleLayout) adjustSpacingInCircle();
        else adjustSpacingInGrid();
    }

    

    static void adjustSpacingInGrid()
    {

        float w, l, minX, maxX, minZ, maxZ;

        w = 0.0f;
        l = 0.0f;
        OpenVR.Chaperone.GetPlayAreaSize(ref l, ref w);

        float boudaryOffset = 0.0f;

        maxX = l * 0.5f + boudaryOffset;
        minX = -maxX;

        maxZ = w * 0.5f + boudaryOffset;
        minZ = -maxZ;

        float lowestY = 0.0f;
        float maxHeight = 0f;
        int currVecIdx = 0;

        float wallOffset = 0.0f;

        Vector3 heightVector = new Vector3(0f, 2.5f, 0f);
        Vector3 offsetVector = Vector3.zero;

        Vector3[] startVectors = new Vector3[3];
        startVectors[0] = new Vector3(maxX, 0f, minZ - wallOffset);
        startVectors[1] = new Vector3(minX - wallOffset, 0f, minZ);
        startVectors[2] = new Vector3(minX, 0f, maxZ + wallOffset);

        Vector3[] dirVectors = new Vector3[3];
        dirVectors[0] = new Vector3(-1f, 0f, 0f);
        dirVectors[1] = new Vector3(0f, 0f, 1f);
        dirVectors[2] = new Vector3(1f, 0f, 0f);

        Vector3[] forwardVectors = new Vector3[3];
        forwardVectors[0] = new Vector3(0f, 0f, -1f);
        forwardVectors[1] = new Vector3(-1f, 0f, 0f);
        forwardVectors[2] = new Vector3(0f, 0f, 1f);

        bool newLine = false;

        int numPopulated = 0;
        int numRows = 0;

        offsetVector = startVectors[0];

        foreach (KeyValuePair<string, GameObject> kv in detailsMap)
        {
            GameObject currObj = kv.Value;
            NodeMenuUtils menuUtils = currObj.GetComponent<NodeMenuUtils>();

            switch(currVecIdx)
            {
                case 0:
                    if (offsetVector.x < minX) newLine = true;
                    break;
                case 1:
                    if (offsetVector.z > maxZ) newLine = true;
                    break;
                case 2:
                    if (offsetVector.x > maxX) newLine = true;
                    break;
            }

            if( newLine )
            {
                numRows++;
                heightVector.y -= maxHeight * 0.6f;
                maxHeight = 0f;
                newLine = false;

                if( heightVector.y < lowestY || numRows > 2)
                {
                    currVecIdx++;
                    if (currVecIdx > 2)
                    {
                        Debug.Log("Maximum menus reached[" + numPopulated + "]");
                        return;
                    }
                    heightVector = new Vector3(0f, 2.5f, 0f);
                }

                offsetVector = startVectors[currVecIdx];
            }

            
            if (menuUtils.yDimension > maxHeight)
            {
                maxHeight = menuUtils.yDimension;
            }

            currObj.transform.position = offsetVector + heightVector;
            currObj.transform.forward = forwardVectors[currVecIdx];

            offsetVector += menuUtils.xDimension * dirVectors[currVecIdx];

            menuUtils.largePosition = currObj.transform.position;
            menuUtils.smallPosition = currObj.transform.position + forwardVectors[currVecIdx] * 0.15f;


            menuUtils.makeSmall();


            numPopulated++;
        }


    }

    static void adjustSpacingInCircle()
    {
        float yOffset = 1.5f;
        float maxHeight = 0f;
        float currCircum = 0f;
        foreach (KeyValuePair<string, GameObject> kv in detailsMap)
        {
            GameObject currObj = kv.Value;
            NodeMenuUtils menuUtils = currObj.GetComponent<NodeMenuUtils>();

            if ((currCircum + menuUtils.xDimension) >= circumference)
            {
                currCircum = 0f;
                yOffset -= maxHeight;
                maxHeight = 0f;
            }

            if (menuUtils.yDimension > maxHeight)
            {
                maxHeight = menuUtils.yDimension;
            }

            float angle = 360f * currCircum / circumference;

            Vector3 vec = new Vector3(1f, 0f, 0f);
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            vec = rotation * vec;

            currObj.transform.forward = vec;
            vec *= radius;
            //vec.y = yOffset - menuUtils.yDimension * 0.5f;
            vec.y = yOffset;

            currObj.transform.position = vec;


            currCircum += menuUtils.xDimension;

            menuUtils.makeLarge();
            menuUtils.makeSmall();

        }
    }


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
