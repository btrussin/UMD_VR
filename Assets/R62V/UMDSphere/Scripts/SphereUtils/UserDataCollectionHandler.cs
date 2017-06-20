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
    private MovieObject movieObject;
    private string currentAnswerSelected;
    private TextMesh QuestionText;
    // public GameObject NextPart;

    // Use this for initialization
    void Start ()
	{
	    PopUpMenu = GameObject.FindGameObjectWithTag("PopUpMenu");
	    ExpandedPopUpMenu = GameObject.FindGameObjectWithTag("ExpandedPopUpMenu");
        ConfirmationPopUp = GameObject.FindGameObjectWithTag("ConfirmationPopUp");
        ConfirmationPopUp.SetActive(false);
        movieObject = FindObjectOfType<NodeState>().GetComponent<MovieObject>();
	    QuestionText = GameObject.FindGameObjectWithTag("CurrentQuestionText").GetComponent<TextMesh>();
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
        SetQuestion();
	}

    void SetQuestion()
    {
        if (form_questions.QuestionIndex <= form_questions.questions.Count - 1)
        {
            currentQuestion = form_questions.questions[form_questions.QuestionIndex];
            QuestionText.text = currentQuestion.QuestionText;
        }
        else
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<FormMenuHandler>(true).gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
    public void RefreshMovieObject(MovieObject m)
    {
        movieObject = m;
    }
    public void PromptUserInput(string dataSelected)
    {
        ConfirmationPopUp.SetActive(true);
        
        if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput)
        {
            ConfirmationPopUp.GetComponent<TextMesh>().text = "You selected " + dataSelected + ". " +
                                                              Environment.NewLine +
                                                              "Click the trackpad to submit your answer.";
        }
        currentAnswerSelected = dataSelected;
    }

    public void HandleUserInput()
    {
        if (currentAnswerSelected != null)
        {
            form_questions.surveyResponses.Add("QNumT:"+form_questions.QuestionIndex+" Input Value:"+currentAnswerSelected);
            foreach (string s in form_questions.surveyResponses)
            {
                Debug.Log(s);
            }
            form_questions.QuestionIndex++;
            currentAnswerSelected = null;
        }
    }

}
