using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EdgeInfo : MonoBehaviour {

    enum colorTypeIdx
    {
        HIGHLIGHT = 0,
        SELECT = 1,
        NONE = 2,
        DIM = 3
    };

    HashSet<string> actorNames = new HashSet<string>();
    public bool updateEdgePositionsThisFrame = false;
    public bool updateEdgeColorThisFrame = false;
    public int numControlPoints = 100;
    public float bundlingStrength = 0.5f;
    MovieObject fromMovieObject;
    MovieObject toMovieObject;
    float edgeWidth = 0.001f;
    bool isInnerRingEdge = true;

    Transform parentTransform;

    LineRenderer lineRend;

    public float _highlightAmt = 0.95f;
    public float _selectedAmt = 0.75f;
    public float _noneAmt = 0.5f;
    public float _dimAmt = 0.35f;

    public float highlightAmt
    {
        set
        {
            _highlightAmt = value;
            setColors(colorTypeIdx.HIGHLIGHT);
            updateEdgeColorThisFrame = true;
        }
        get
        {
            return _highlightAmt;
        }
    }

    public float selectedAmt
    {
        set
        {
            _selectedAmt = value;
            setColors(colorTypeIdx.SELECT);
            updateEdgeColorThisFrame = true;
        }
        get
        {
            return _selectedAmt;
        }
    }

    public float noneAmt
    {
        set
        {
            _noneAmt = value;
            setColors(colorTypeIdx.NONE);
            updateEdgeColorThisFrame = true;
        }
        get
        {
            return _noneAmt;
        }
    }

    public float dimAmt
    {
        set
        {
            _dimAmt = value;
            setColors(colorTypeIdx.DIM);
            updateEdgeColorThisFrame = true;  
        }
        get
        {
            return _dimAmt;
        }
    }

    //public float 

    bool selected = false;
    bool highlighted = false;
    bool dimmed = false;
    bool alwaysOn = false;

    int numNodesSelected = 0;
    int numNodesHighlighted = 0;

    Color startColor = Color.white;
    Color endColor = Color.white;
    Color[] startColors = new Color[4];
    Color[] endColors = { Color.white, Color.white, Color.white, Color.white };

    bool useHSL = true;

    void setColors()
    {
        setColors(colorTypeIdx.HIGHLIGHT);
        setColors(colorTypeIdx.SELECT);
        setColors(colorTypeIdx.NONE);
        setColors(colorTypeIdx.DIM);
    }

    void setColors(colorTypeIdx idx)
    {
        float amt = 0.0f;
        switch(idx)
        {
            case colorTypeIdx.HIGHLIGHT:
                amt = _highlightAmt;
                break;
            case colorTypeIdx.SELECT:
                amt = _selectedAmt;
                break;
            case colorTypeIdx.NONE:
                amt = _noneAmt;
                break;
            case colorTypeIdx.DIM:
                amt = _dimAmt;
                break;
        }

        if( useHSL )
        {
            ColorHSL tmpHsl;
            tmpHsl = new ColorHSL(startColor);
            tmpHsl.l = amt;
            startColors[(int)idx] = tmpHsl.getRGBColor();

            tmpHsl = new ColorHSL(endColor);
            tmpHsl.l = amt;
            endColors[(int)idx] = tmpHsl.getRGBColor();
        }
        else
        {
            startColors[(int)idx] = startColor * amt;
            endColors[(int)idx] = endColor * amt;
        }


    }

    public bool IsOn
    {
        get
        {
            return numNodesSelected > 0 || numNodesHighlighted > 0;
        }
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(updateEdgePositionsThisFrame)
        {
            updateEdgePositions();
            updateEdgePositionsThisFrame = false;
        }

        if( updateEdgeColorThisFrame )
        {
            updateEdgeWidthAndColors();
            updateEdgeColorThisFrame = false;
        }
	}

    public void addActor(string actor)
    {
        if (!actorNames.Contains(actor)) actorNames.Add(actor);

        edgeWidth = 0.001f * Mathf.Max(Mathf.Min(actorNames.Count - 1, 5), 1.0f);
    }

    public void updateEdgePositions()
    {
        Vector3[] basePts = new Vector3[4];

        basePts[0] = fromMovieObject.point.transform.position;
        basePts[3] = toMovieObject.point.transform.position;


        /* old way
        basePts[1] = (fromMovieObject.ring.transform.position - basePts[0]) * 0.5f + basePts[0];
        basePts[2] = (toMovieObject.ring.transform.position - basePts[3]) * 0.5f + basePts[3];

        Vector3[] pts = MovieDBUtils.getBezierPoints(basePts, numControlPoints, bundlingStrength);
        */

        /* new way */
        basePts[1] = (fromMovieObject.ring.transform.position - basePts[0]) * bundlingStrength + basePts[0];
        basePts[2] = (toMovieObject.ring.transform.position - basePts[3]) * bundlingStrength + basePts[3];

        Vector3[] pts = MovieDBUtils.getBezierPoints(basePts, numControlPoints, -1.0f);
        /* end new way */

        for ( int i = 0; i < pts.Length; i++ )
        {
            pts[i] = lineRend.transform.InverseTransformPoint(pts[i]);
        }

        lineRend.SetPositions(pts);


    }

    public void updateEdgeWidthAndColors()
    {
        /*
        Color fromColor = fromMovieObject.color;
        Color toColor = toMovieObject.color;

        if( highlighted )
        {
            fromColor *= _highlightAmt;
            toColor *= _highlightAmt;
        }
        else if( selected )
        {
            fromColor *= _selectedAmt;
            toColor *= _selectedAmt;
        }
        else if (dimmed)
        {
            fromColor *= _dimAmt;
            toColor *= _dimAmt;
        }
        else
        {
            fromColor *= _noneAmt;
            toColor *= _noneAmt;
        }

        //fColHSL.s *= edgeWidth * 100.0f;
        //tColHSL.s = fColHSL.s;
        */

        lineRend.SetWidth(edgeWidth, edgeWidth);
        //lineRend.SetColors(fColHSL.getRGBColor(), tColHSL.getRGBColor());
        //lineRend.SetColors(fromColor, toColor);

        if (highlighted)
        {
            lineRend.SetColors(startColors[(int)colorTypeIdx.HIGHLIGHT], endColors[(int)colorTypeIdx.HIGHLIGHT]);
        }
        else if (selected)
        {
            lineRend.SetColors(startColors[(int)colorTypeIdx.SELECT], endColors[(int)colorTypeIdx.SELECT]);
        }
        else if (dimmed)
        {
            lineRend.SetColors(startColors[(int)colorTypeIdx.DIM], endColors[(int)colorTypeIdx.DIM]);
        }
        else
        {
            lineRend.SetColors(startColors[(int)colorTypeIdx.NONE], endColors[(int)colorTypeIdx.NONE]);
        }
    }

    public void setValues(Transform transform, MovieObject moFrom, MovieObject moTo, int numCtrlPoints)
    {
        parentTransform = transform;
        fromMovieObject = moFrom;
        toMovieObject = moTo;
        numControlPoints = numCtrlPoints;

        startColor = fromMovieObject.color;
        endColor = toMovieObject.color;
        setColors();

        isInnerRingEdge = fromMovieObject.ring.name.Equals(toMovieObject.ring.name);
    }

    public bool getIsInnerRingEdge()
    {
        return isInnerRingEdge;
    }

    public void setup(CMData from, CMData to, Material material)
    {

        for( int i = 0; i < from.roles.Length; i++ )
        {
            for (int j = 0; j < to.roles.Length; j++)
            {
                if( from.roles[i].actor.Equals(to.roles[j].actor) )
                {
                    addActor(from.roles[i].actor);
                    j += to.roles.Length;
                }
            }
        }
        
        ColorHSL fColHSL = new ColorHSL(fromMovieObject.color);
        ColorHSL tColHSL = new ColorHSL(toMovieObject.color);
        fColHSL.s *= edgeWidth * 100.0f;
        tColHSL.s = fColHSL.s;

        gameObject.name = "Conn: " + from.movie + " - " + to.movie;

        gameObject.AddComponent<LineRenderer>();
        lineRend = gameObject.GetComponent<LineRenderer>();
        lineRend.SetWidth(edgeWidth, edgeWidth);
        lineRend.SetColors(fColHSL.getRGBColor(), tColHSL.getRGBColor());
        lineRend.SetVertexCount(numControlPoints);
        lineRend.material = material;
        lineRend.material.shader = Shader.Find("Custom/Custom_AlphaBlend");
        lineRend.material.color = new Color(0.3f, 0.3f, 0.3f);

        
        if (fromMovieObject.ring.name.Equals(toMovieObject.ring.name))
        {
            gameObject.transform.SetParent(fromMovieObject.ring.transform);
        }
        
        lineRend.useWorldSpace = false;

    }

    public void hightlight()
    {
        numNodesHighlighted++;
        if (numNodesHighlighted > 2) numNodesHighlighted = 2;
        highlighted = true;
        updateEdgeColorThisFrame = true;
    }

    public void unhightlight()
    {
        numNodesHighlighted--;
        if (numNodesHighlighted <= 0)
        {
            numNodesHighlighted = 0;
            highlighted = false;
            updateEdgeColorThisFrame = true;
        }
    }

    public void select()
    {
        numNodesSelected++;
        if (numNodesSelected > 2) numNodesSelected = 2;
        selected = true;
        updateEdgeColorThisFrame = true;
    }

    public void unselect()
    {
        numNodesSelected--;
        if( numNodesSelected <= 0 )
        {
            numNodesSelected = 0;
            selected = false;
            updateEdgeColorThisFrame = true;
        }
    }

    public void dim()
    {
        dimmed = true;
        updateEdgeColorThisFrame = true;
    }

    public void undim()
    {
        dimmed = false;
        updateEdgeColorThisFrame = true;
    }

    public void toggleSelected()
    {
        if (selected) unselect();
        else select();
    }

    public void setAlwaysOn(bool on)
    {
        alwaysOn = on;
        updateEdgeColorThisFrame = true;
    }
}
