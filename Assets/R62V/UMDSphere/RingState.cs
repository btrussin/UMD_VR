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

    float nonHighlightDimPercent = 60.0f;
    float nonSelectedDimPercent = 30.0f;

    float nonHighlightAmt;
    float nonSelectedAmt;

    LineRenderer lRend = null;
    TextMesh tMesh = null;

    // Use this for initialization
    void Start () {

        nonHighlightAmt = 1.0f - nonHighlightDimPercent * 0.01f;
        nonSelectedAmt = 1.0f - nonSelectedDimPercent * 0.01f;

        Transform subTransform = gameObject.transform.GetChild(0);

        GameObject lines = subTransform.FindChild("RingLines").gameObject;
        GameObject label = subTransform.FindChild("RingLabel").gameObject;

        lRend = lines.GetComponent<LineRenderer>();
        tMesh = label.GetComponent<TextMesh>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void updateColor()
    {
        lRend.material.color = ringColor;

        tMesh.color = ringColor;

    }

    public void setColorState(RingColorState state)
    {
        currColorState = state;
    }

    public void setRingColor(Color c)
    {
        ringColor = new Color(c.r, c.g, c.b) ;
    }

    public void setSelected(bool selected)
    {
        if (selected) lRend.material.color = ringColor;
        else lRend.material.color = ringColor * nonSelectedAmt;
    }

    public void setHighlighted(bool highlighted)
    {
        if (highlighted) lRend.material.color = ringColor;
        else lRend.material.color = ringColor * nonHighlightAmt;
    }
}
