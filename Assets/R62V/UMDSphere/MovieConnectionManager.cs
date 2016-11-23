using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovieConnectionManager : MonoBehaviour {

    List<GameObject> connectionList = new List<GameObject>();
    Dictionary<string, RingState> activeRingMap = new Dictionary<string, RingState>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void addConnection(GameObject g, MovieObject from, MovieObject to )
    {
        connectionList.Add(g);

        string key = MovieDBUtils.getMovieDataKey(from.cmData);
        RingState rs;

        if (!activeRingMap.ContainsKey(key))
        {
            rs = from.ring.GetComponent<RingState>();
            rs.addConnection();
            activeRingMap.Add(key, rs);
        }

        key = MovieDBUtils.getMovieDataKey(to.cmData);

        if (!activeRingMap.ContainsKey(key))
        {
            rs = to.ring.GetComponent<RingState>();
            rs.addConnection();
            activeRingMap.Add(key, rs);
        }
    }
   
    public void forceClearAllConnections()
    {
        foreach (GameObject gObj in connectionList) { Destroy(gObj); }

        foreach (RingState rs in activeRingMap.Values) rs.removeConnection();

        connectionList.Clear();
        activeRingMap.Clear();
    }

    public bool hasConnections()
    {
        return connectionList.Count > 0;
    }
}
