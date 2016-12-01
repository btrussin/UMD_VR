using UnityEngine;
using System.Collections;

public class CameraOrientedText3D : MonoBehaviour {

    //public Camera mainCamera;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 v = gameObject.transform.position - Camera.main.transform.position;
        v.Normalize();
        gameObject.transform.forward = v;
    }
}
