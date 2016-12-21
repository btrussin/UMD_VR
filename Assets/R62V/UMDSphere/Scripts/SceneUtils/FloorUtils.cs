﻿using UnityEngine;
using System.Collections;
using Valve.VR;

public class FloorUtils : MonoBehaviour {

    bool done = false;

	// Use this for initialization
	void Start () {
        CVRChaperone chap = OpenVR.Chaperone;
        float w = 0.0f;
        float l = 0.0f;
        chap.GetPlayAreaSize(ref w, ref l);
        this.transform.localScale = new Vector3(w * 0.1f, 1.0f, l * 0.1f);

        Material mat = this.gameObject.GetComponent<Renderer>().material;


        mat.mainTextureScale = new Vector2(Mathf.Round(w*5.0f), Mathf.Round(l*5.0f));
	}
	
	// Update is called once per frame
	void Update () {

    }
}
