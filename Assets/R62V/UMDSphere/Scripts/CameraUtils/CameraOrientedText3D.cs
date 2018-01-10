using UnityEngine;
using System.Collections;

public class CameraOrientedText3D : MonoBehaviour {

    //public Camera mainCamera;

	void Start () {
    }
	
	void Update () {
        //Vector3 v = gameObject.transform.position - Camera.main.transform.position;
        //v.Normalize();
        //gameObject.transform.forward = v;

        gameObject.transform.forward = Camera.main.transform.forward;
    }
}
