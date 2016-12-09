using UnityEngine;
using System.Collections;

public class RingState : MonoBehaviour {

    public enum RingColorState
    {
        NONE,
        SELECTED,
        HIGHLIGHTED,
        DIMMED
    }

    RingColorState currColorState = RingColorState.NONE;

    Color ringColor;

    float highlightAmt = 0.95f;
    float selectedAmt = 0.75f;
    float noneAmt = 0.5f;
    float dimAmt = 0.35f;

    int connectionCount = 0;


    LineRenderer lRend = null;
    TextMesh tMesh = null;

    bool valuesNotSet = true;

    bool doHighlight = false;
    bool doDim = false;

    void Start () {


    }
	
	void Update () {
	
	}

    
    public void updateColor()
    {
        if( valuesNotSet )
        {
            Transform subTransform = gameObject.transform.GetChild(0);

            GameObject lines = subTransform.FindChild("RingLines").gameObject;
            GameObject label = subTransform.FindChild("RingLabel").gameObject;

            lRend = lines.GetComponent<LineRenderer>();
            tMesh = label.GetComponent<TextMesh>();

            valuesNotSet = false;
        }

        if( doHighlight )
        {
            lRend.material.color = ringColor * highlightAmt;
            doHighlight = false;
        }
        else if (connectionCount > 0)
        {
            lRend.material.color = ringColor * selectedAmt;

        }
        else if (doDim)
        {
            lRend.material.color = ringColor * dimAmt;
            doDim = false;
        }
        else
        {
            lRend.material.color = ringColor * noneAmt;
        }

       
        tMesh.color = ringColor;

    }

    public void addConnection()
    {
        connectionCount++;
    }

    public void removeConnection()
    {
        connectionCount--;
        if (connectionCount < 0) connectionCount = 0;
    }

    public bool isSelected()
    {
        return false;
    }

    public void setColorState(RingColorState state)
    {
        currColorState = state;
    }

    public void setRingColor(Color c)
    {
        ringColor = new Color(c.r, c.g, c.b) ;
    }

    public void setHighlighted()
    {
        doHighlight = true;
    }

    public void setDimmed()
    {
        doDim = true;
    }
}
