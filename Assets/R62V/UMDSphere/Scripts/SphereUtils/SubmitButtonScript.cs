using UnityEngine;
using System.Collections;

public class SubmitButtonScript : MonoBehaviour
{
    public bool readyForSubmit;
    public UserDataCollectionHandler udch;
    // Use this for initialization
    void Start () {
	    udch = GameObject.FindObjectOfType<UserDataCollectionHandler>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
