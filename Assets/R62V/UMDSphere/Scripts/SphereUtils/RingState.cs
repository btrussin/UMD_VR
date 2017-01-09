using UnityEngine;
using System.Collections;

public class RingState : MonoBehaviour {

    /*public enum RingColorState
    {
        NONE,
        SELECTED,
        HIGHLIGHTED,
        DIMMED
    }

    RingColorState currColorState = RingColorState.NONE;*/

    Color _ringColor;

    float highlightAmt = 0.95f;
    float selectedAmt = 0.75f;
    float noneAmt = 0.5f;
    float dimAmt = 0.35f;

    int _connectionCount = 0;

    LineRenderer _lRend = null;
    TextMesh _tMesh = null;

    bool _valuesNotSet = true;

    bool _doHighlight = false;
    bool _doDim = false;

    void Start () {


    }
	
	void Update () {
	
	}

    
    public void UpdateColor()
    {
        if( _valuesNotSet )
        {
            Transform subTransform = gameObject.transform.GetChild(0);

            GameObject lines = subTransform.FindChild("RingLines").gameObject;
            GameObject label = subTransform.FindChild("RingLabel").gameObject;

            _lRend = lines.GetComponent<LineRenderer>();
            _tMesh = label.GetComponent<TextMesh>();

            _valuesNotSet = false;
        }

        if( _doHighlight )
        {
            _lRend.material.color = _ringColor * highlightAmt;
            _doHighlight = false;
        }
        else if (_connectionCount > 0)
        {
            _lRend.material.color = _ringColor * selectedAmt;

        }
        else if (_doDim)
        {
            _lRend.material.color = _ringColor * dimAmt;
            _doDim = false;
        }
        else
        {
            _lRend.material.color = _ringColor * noneAmt;
        }

       
        _tMesh.color = _ringColor;

    }

    public void AddConnection()
    {
        _connectionCount++;
    }

    public void RemoveConnection()
    {
        _connectionCount--;
        if (_connectionCount < 0) _connectionCount = 0;
    }

    public bool IsSelected()
    {
        return false;
    }

    /*public void setColorState(RingColorState state)
    {
        currColorState = state;
    }*/

    public void SetRingColor(Color c)
    {
        _ringColor = new Color(c.r, c.g, c.b) ;
    }

    public void SetHighlighted()
    {
        _doHighlight = true;
    }

    public void SetDimmed()
    {
        _doDim = true;
    }

}
