using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class UserDataCollectionHandler : MonoBehaviour
{
    public bool minimzed = true;
    public Material CircleMaterial;
    public Material sliderPointMaterial;

    private GameObject PopUpMenu;
    private GameObject ExpandedPopUpMenu;
    private SubmitButtonScript sbs;
    private GameObject ConfirmationPopUp;
    public FormMenuHandler.FormQuestions.Question currentQuestion;
    private MovieObject movieObject;
    private FormState formState;
    private string currentAnswerSelected;
    private List<String> currentAnswersList;
    private TextMesh QuestionText;
    private bool questionLoaded = false;

    // public GameObject NextPart;

    // Use this for initialization
    void Start ()
	{
        currentAnswersList = new List<string>();
        ExpandedPopUpMenu = GameObject.FindGameObjectWithTag("ExpandedPopUpMenu");
        ConfirmationPopUp = GameObject.FindGameObjectWithTag("ConfirmationPopUp");
        ConfirmationPopUp.SetActive(false);
        movieObject = FindObjectOfType<NodeState>().GetComponent<MovieObject>();
	    QuestionText = GameObject.FindGameObjectWithTag("CurrentQuestionText").GetComponent<TextMesh>();
        // NextPart = GameObject.FindGameObjectWithTag("NextPart");
        CircleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/circ_mat.mat");
        sliderPointMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/sliderpnt_mat.mat");

        formState = GetComponent<FormState>();
	    sbs = GameObject.FindObjectOfType<SubmitButtonScript>();

	}
    Dictionary<string, MovieObject> connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    public FormMenuHandler.FormQuestions form_questions = new FormMenuHandler.FormQuestions();


    // Update is called once per frame
    void Update () {
        
        SetQuestion();
	}


    public void GenYesNoRadioButtons()
    {  // by RK, check for Alex and Mike

        float offset_y = 0.0675f;
        float yOffsetPerLine = 0.06f;
        List<GameObject> interactableObjects = new List<GameObject>();
        int menuLayerMask = LayerMask.NameToLayer("Menus");

        for (int toggleInd = 0; toggleInd < currentQuestion.possible_answers.Count; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.tag = "RadioButton";
            quad.name = "Best Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(ExpandedPopUpMenu.transform);

            MeshRenderer rend = quad.GetComponent<MeshRenderer>();

            rend.transform.localScale = new Vector3(0.035f, 0.05f, 0.5f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition = new Vector3(-0.253f, 0.247f, -0.0019f);
            rend.transform.localPosition -= new Vector3(0, offset_y, 0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleRadio;

            GameObject optionText = new GameObject("");
            optionText.AddComponent<TextMesh>();
            if (currentQuestion.possible_answers.Count >= toggleInd)
            {
                optionText.GetComponent<TextMesh>().text = currentQuestion.possible_answers[toggleInd];
            }
            optionText.GetComponent<TextMesh>().fontSize = 50;
            optionText.GetComponent<TextMesh>().characterSize = 0.35f;
            optionText.name = "Option Text";
            optionText.transform.SetParent(quad.transform);
            optionText.transform.localRotation = Quaternion.identity;
            optionText.transform.localPosition = new Vector3(.69f, .48f, -.0015f);
            optionText.transform.localScale = new Vector3(.5f, .5f, 203); ;

            menuHandler.baseMaterial = CircleMaterial;
            menuHandler.inputInteractMaterial = sliderPointMaterial;
            menuHandler.UpdateMaterial();

            offset_y += yOffsetPerLine;
        }
        offset_y += yOffsetPerLine;
    }

    void SetQuestion()
    {
        if (form_questions.QuestionIndex <= form_questions.questions.Count - 1)
        {
            currentQuestion = form_questions.questions[form_questions.QuestionIndex];
            QuestionText.text = currentQuestion.QuestionText;
            if ((currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.RadioButtons) &&(questionLoaded==false) )
            {
               GenYesNoRadioButtons();
                questionLoaded = true;               
            }

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
        sbs.readyForSubmit = true;
        if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput || currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.RadioButtons)
        {
            ConfirmationPopUp.GetComponent<TextMesh>().text = dataSelected;
        }
        else if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.MultipleInput)
        {
            currentAnswersList.Add(dataSelected);
            ConfirmationPopUp.GetComponent<TextMesh>().text += Environment.NewLine + dataSelected;
        }
        currentAnswerSelected = dataSelected;
    }

    public void HandleUserInput()
    {
        if (currentAnswerSelected != null)
        {
            sbs.readyForSubmit = false;
            form_questions.surveyResponses.Add("QNumT:" + form_questions.QuestionIndex + " Input Value:" +
                                               currentAnswerSelected);
            /* foreach (string s in form_questions.surveyResponses)
             {
                 Debug.Log(s);
             }*/
            form_questions.QuestionIndex++;
            questionLoaded = false;
            currentAnswerSelected = null;

            if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.RadioButtons)
            {
                foreach (Transform t in GetComponentsInChildren<Transform>())
                {
                    if (t.tag == "RadioButton")
                    {
                        Destroy(t.gameObject);
                    }
                }
            }
            
            ConfirmationPopUp.SetActive(false);
            foreach (MovieObject m in GameObject.FindObjectsOfType<MovieObject>())
            {
                if (m.nodeState.isSelected)
                {
                    //m.nodeState.isSelected = false;
                    m.nodeState.toggleSelected();
                    m.nodeState.updateColor();

                    // added by Brian; the following is consistent with the controller
                    HashSet<EdgeInfo> edgeSet = m.getEdges();
                    if (m.nodeState.getIsSelected()) foreach (EdgeInfo info in edgeSet) info.select();
                    else foreach (EdgeInfo info in edgeSet) info.unselect();

                    m.connManager.ForceClearAllConnections();
                }
            }
            currentAnswersList.Clear();
            ConfirmationPopUp.GetComponent<TextMesh>().text = "";

        }
    }
}