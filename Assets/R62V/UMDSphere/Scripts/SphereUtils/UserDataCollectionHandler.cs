using UnityEngine;
using System.Collections;

public class UserDataCollectionHandler : MonoBehaviour
{
    public bool minimzed = true;
    private GameObject PopUpMenu;
    private GameObject ExpandedPopUpMenu;
   
	// Use this for initialization
	void Start ()
	{
	    PopUpMenu = GameObject.FindGameObjectWithTag("PopUpMenu");
	    ExpandedPopUpMenu = GameObject.FindGameObjectWithTag("ExpandedPopUpMenu");
	}
	
	// Update is called once per frame
	void Update () {
        
	    if (minimzed)
	    {
	        PopUpMenu.SetActive(true);
            ExpandedPopUpMenu.SetActive(false);
	    }
	    else
	    {
            PopUpMenu.SetActive(false);
            ExpandedPopUpMenu.SetActive(true);
        }
        
	}

}
