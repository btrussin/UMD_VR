using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeDetailsManager : MonoBehaviour {

    static Dictionary<string, GameObject> detailsMap = new Dictionary<string, GameObject>();
    private static float radius = 2.0f;
    private static float circumference = radius * Mathf.PI * 2f;

    public static void addDetails(string name, GameObject obj)
    {
        detailsMap.Add(name, obj);
        adjustSpacing();
    }

    public static void removeDetails(string name)
    {
        detailsMap.Remove(name);
        adjustSpacing();
    }

    static void adjustSpacing()
    {
        float yOffset = 1.5f;
        float maxHeight = 0f;
        float currCircum = 0f;
        foreach(KeyValuePair<string, GameObject> kv in detailsMap)
        {
            GameObject currObj = kv.Value;

            if( currCircum >= circumference )
            {
                currCircum = 0f;
                yOffset -= maxHeight;
                maxHeight = 0f;
            }

            if(currObj.transform.localScale.y > maxHeight)
            {
                maxHeight = currObj.transform.localScale.y;
            }

            float angle = 360f * currCircum / circumference;

            Vector3 vec = new Vector3(1f, 0f, 0f);
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            vec = rotation * vec;

            currObj.transform.forward = vec;
            vec *= radius;
            vec.y = yOffset;
            currObj.transform.position = vec;


            currCircum += currObj.transform.localScale.x;

        }
    }


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
