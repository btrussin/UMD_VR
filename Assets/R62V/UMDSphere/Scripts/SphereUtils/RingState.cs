﻿using UnityEngine;
using System.Collections;

public class RingState : MonoBehaviour
{

    /*public enum RingColorState
    {
        NONE,
        SELECTED,
        HIGHLIGHTED,
        DIMMED
    }

    RingColorState currColorState = RingColorState.NONE;*/

    Color ringColor;

    public float highlightAmt = 0.95f;
    public float selectedAmt = 0.75f;
    public float noneAmt = 0.5f;
    public float dimAmt = 0.35f;

    int connectionCount = 0;


    LineRenderer lRend = null;
    TextMesh tMesh = null;

    bool valuesNotSet = true;

    bool doHighlight = false;
    bool doDim = false;

    public void UpdateColor()
    {
        if (valuesNotSet)
        {
            Transform subTransform = gameObject.transform.GetChild(0);

            GameObject lines = subTransform.FindChild("RingLines").gameObject;
            GameObject label = subTransform.FindChild("RingLabel").gameObject;

            lRend = lines.GetComponent<LineRenderer>();
            tMesh = label.GetComponent<TextMesh>();

            valuesNotSet = false;
        }

        if (doHighlight)
        {
            //lRend.material.color = ringColor * highlightAmt;
            lRend.SetColors(ringColor * highlightAmt, ringColor * highlightAmt);
            doHighlight = false;
        }
        else if (connectionCount > 0)
        {
            //lRend.material.color = ringColor * selectedAmt;
            lRend.SetColors(ringColor * selectedAmt, ringColor * selectedAmt);

        }
        else if (doDim)
        {
            //lRend.material.color = ringColor * dimAmt;
            lRend.SetColors(ringColor * dimAmt, ringColor * dimAmt);
            doDim = false;
        }
        else
        {
            //lRend.material.color = ringColor * noneAmt;
            lRend.SetColors(ringColor * noneAmt, ringColor * noneAmt);
        }

        tMesh.color = ringColor;
    }

    public void AddConnection()
    {
        connectionCount++;
    }

    public void RemoveConnection()
    {
        connectionCount--;
        if (connectionCount < 0) connectionCount = 0;
    }

    public bool IsSelected()
    {
        return false;
    }

    /*public void SetColorState(RingColorState state)
    {
        currColorState = state;
    }*/

    public void SetRingColor(Color c)
    {
        ringColor = new Color(c.r, c.g, c.b);
    }

    public void SetHighlighted()
    {
        doHighlight = true;
    }

    public void SetDimmed()
    {
        doDim = true;
    }
}