using UnityEngine;
using System.Collections;

public class RingState : MonoBehaviour {

    public enum RingColorState
    {
        NONE,
        SELECTED,
        IN_CONTACT
    }

    RingColorState currColorState = RingColorState.NONE;

    Color ringColor;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void setColorState(RingColorState state)
    {
        currColorState = state;
    }

    public void setRingColor(Color c)
    {
        ringColor = c;
    }
}
