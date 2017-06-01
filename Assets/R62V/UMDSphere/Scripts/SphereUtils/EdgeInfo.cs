using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EdgeInfo : MonoBehaviour {

    HashSet<string> actorNames = new HashSet<string>();
    public bool updateEdgePositionsThisFrame = false;
    public bool updateEdgeColorThisFrame = false;
    public int numControlPoints = 100;
    public float bundlingStrength = 0.5f;
    MovieObject fromMovieObject;
    MovieObject toMovieObject;
    float edgeWidth = 0.001f;
    bool isInnerRingEdge = false;

    Transform parentTransform;

    LineRenderer lineRend;

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

        basePts[1] = (fromMovieObject.ring.transform.position - basePts[0]) * 0.5f + basePts[0];
        basePts[2] = (toMovieObject.ring.transform.position - basePts[3]) * 0.5f + basePts[3];

        Vector3[] pts = MovieDBUtils.getBezierPoints(basePts, numControlPoints, bundlingStrength);

        for( int i = 0; i < pts.Length; i++ )
        {
            pts[i] = lineRend.transform.InverseTransformPoint(pts[i]);
        }

        lineRend.SetPositions(pts);


    }

    public void updateEdgeWidthAndColors()
    {
        ColorHSL fColHSL = new ColorHSL(fromMovieObject.color);
        ColorHSL tColHSL = new ColorHSL(toMovieObject.color);
        fColHSL.s *= edgeWidth * 100.0f;
        tColHSL.s = fColHSL.s;

        lineRend.SetWidth(edgeWidth, edgeWidth);
        lineRend.SetColors(fColHSL.getRGBColor(), tColHSL.getRGBColor());
    }

    public void setValues(Transform transform, MovieObject moFrom, MovieObject moTo, int numCtrlPoints)
    {
        parentTransform = transform;
        fromMovieObject = moFrom;
        toMovieObject = moTo;
        numControlPoints = numCtrlPoints;

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
}
