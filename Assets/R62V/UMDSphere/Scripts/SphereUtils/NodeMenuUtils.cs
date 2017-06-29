using UnityEngine;
using System.Collections;

public class NodeMenuUtils : MonoBehaviour {

    public MovieObject movieObject;
    public float xDimension;
    public float yDimension;

    public Vector3 largePosition;
    public Vector3 smallPosition;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void makeLarge()
    {
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.localPosition = largePosition;
    }

    public void makeSmall()
    {
        gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        gameObject.transform.localPosition = smallPosition;
    }
}
