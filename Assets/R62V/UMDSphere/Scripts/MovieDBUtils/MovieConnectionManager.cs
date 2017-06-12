using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovieConnectionManager : MonoBehaviour {

    readonly Dictionary<string, RingState> _activeRingMap = new Dictionary<string, RingState>();

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

   

    public void AddConnectionDEL(GameObject g, MovieObject from, MovieObject to )
    {
        

        string key = MovieDBUtils.getMovieDataKey(from.cmData);
        RingState rs;

        if (!_activeRingMap.ContainsKey(key))
        {
            rs = from.ring.GetComponent<RingState>();
            rs.AddConnection();
            _activeRingMap.Add(key, rs);
        }

        key = MovieDBUtils.getMovieDataKey(to.cmData);

        if (!_activeRingMap.ContainsKey(key))
        {
            rs = to.ring.GetComponent<RingState>();
            rs.AddConnection();
            _activeRingMap.Add(key, rs);
        }
    }
   
    public void ForceClearAllConnections()
    {
       
        foreach (RingState rs in _activeRingMap.Values) rs.RemoveConnection();

        _activeRingMap.Clear();
    }

}
