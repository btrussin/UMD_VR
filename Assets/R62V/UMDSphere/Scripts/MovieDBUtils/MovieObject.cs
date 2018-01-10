using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovieObject : MonoBehaviour
{

    public CMData cmData;
    public GameObject ring;
    public GameObject label;
    public GameObject point;
    public Color color;
    public NodeState nodeState;
    public MovieConnectionManager connManager;
    public int currentLevel;
    public HashSet<EdgeInfo> edgeSet = new HashSet<EdgeInfo>();

    public void addEdge(EdgeInfo info)
    {
        if( !edgeSet.Contains(info) )
        {
            edgeSet.Add(info);
        }
    }

    public HashSet<EdgeInfo> getEdges()
    {
        return edgeSet;
    }
}
