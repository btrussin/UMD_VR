using UnityEngine;
using System.Collections;

public class CameraOrientedText3D : MonoBehaviour {

    //public Camera mainCamera;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.forward = Camera.main.transform.forward;

        //TextMesh tMesh = gameObject.GetComponent<TextMesh>();
        //if (tMesh) {

            //tMesh.offsetZ = 1.0f;

       // }

    }
}
