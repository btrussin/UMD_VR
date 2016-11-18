using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovieConnectionManager : MonoBehaviour {

    List<GameObject> connectionList = new List<GameObject>();
    bool keepConnections = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void addConnection(GameObject g)
    {
        connectionList.Add(g);
    }

    public void tryToClearAllConnections()
    {
        if (keepConnections) return;

        forceClearAllConnections();
    }

    public void forceClearAllConnections()
    {
        foreach (GameObject gObj in connectionList) { Destroy(gObj); }

        connectionList.Clear();
    }

    public void toggleKeepConnections()
    {
        keepConnections = !keepConnections;
    }

    public bool getKeepConnections()
    {
        return keepConnections;
    }

    public bool hasConnections()
    {
        return connectionList.Count > 0;
    }
}
