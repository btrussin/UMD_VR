using System;
using UnityEngine;
using System.Collections;

public class UserDataCollectionHandler : MonoBehaviour
{
    public bool minimzed = true;
    private GameObject PopUpMenu;
    private GameObject ExpandedPopUpMenu;
    private GameObject ConfirmationPopUp;
    private FormMenuHandler.FormQuestions.Question currentQuestion;
    public int QuestionIndex;
    private MovieObject movieObject;
    // public GameObject NextPart;

    // Use this for initialization
    void Start ()
	{
	    PopUpMenu = GameObject.FindGameObjectWithTag("PopUpMenu");
	    ExpandedPopUpMenu = GameObject.FindGameObjectWithTag("ExpandedPopUpMenu");
        ConfirmationPopUp = GameObject.FindGameObjectWithTag("ConfirmationPopUp");
        ConfirmationPopUp.SetActive(false);
        movieObject = FindObjectOfType<NodeState>().GetComponent<MovieObject>();

        // NextPart = GameObject.FindGameObjectWithTag("NextPart");
    }


    public FormMenuHandler.FormQuestions form_questions = new FormMenuHandler.FormQuestions();


    // Update is called once per frame
    void Update () {
        
	    if (minimzed)
	    {
	        PopUpMenu.SetActive(true);
            ExpandedPopUpMenu.SetActive(false);
           // NextPart.SetActive(true);
	    }
	    else
	    {
            PopUpMenu.SetActive(false);
            ExpandedPopUpMenu.SetActive(true);
        }
	}

    public void RefreshMovieObject(MovieObject m)
    {
        movieObject = m;
    }
    public void PromptUserInput(string dataSelected)
    {
        ConfirmationPopUp.SetActive(true);
        currentQuestion = form_questions.questions[QuestionIndex];
        if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput)
        {
            Debug.Log(ConfirmationPopUp.GetComponentInChildren<TextMesh>());
            ConfirmationPopUp.GetComponentInChildren<TextMesh>().text = "You selected " + dataSelected + ". " +
                                                              Environment.NewLine +
                                                              "Click the trackpad to submit your answer.";
        }
        
    }
    void HandleUserInput(string dataSelected)
    {
        form_questions.surveyResponses.Add(dataSelected);
    }

}
