using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.SceneManagement;

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
    public long startTime;
    public bool startCountingTime;

    // public GameObject NextPart;

    // Use this for initialization
    void Start()
    {

        //startTime = DateTime.Now.ToFileTime();
        currentAnswersList = new List<string>();
        ExpandedPopUpMenu = GameObject.FindGameObjectWithTag("ExpandedPopUpMenu");
        ConfirmationPopUp = GameObject.FindGameObjectWithTag("ConfirmationPopUp");
        ConfirmationPopUp.SetActive(false);

        QuestionText = GameObject.FindGameObjectWithTag("CurrentQuestionText").GetComponent<TextMesh>();
        CircleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/circ_mat.mat");
        sliderPointMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/sliderpnt_mat.mat");

        formState = GetComponent<FormState>();
        sbs = GameObject.FindObjectOfType<SubmitButtonScript>();
        if (SceneManager.GetActiveScene().name == "SphereScene")
        {
            movieObject = FindObjectOfType<NodeState>().GetComponent<MovieObject>();
        }
        
        ExpandedPopUpMenu.SetActive(false);
       
        // NextPart = GameObject.FindGameObjectWithTag("NextPart");
       

    }
    Dictionary<string, MovieObject> connectionMovieObjectMap = new Dictionary<string, MovieObject>();

    public FormMenuHandler.FormQuestions form_questions = new FormMenuHandler.FormQuestions();


    // Update is called once per frame
    void Update()
    {
        if (startCountingTime && startTime == 0)
        {
            Debug.Log("Timer Start");
            startTime = DateTime.Now.ToFileTime();
            ExpandedPopUpMenu.SetActive(true);
        }
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
            if (currentQuestion.QuestionText.Length >= 55)
            {
                if (currentQuestion.QuestionText[54] == (char)32)
                {
                    QuestionText.text = currentQuestion.QuestionText.Substring(0, 55) + Environment.NewLine +
                                        currentQuestion.QuestionText.Substring(55);
                }
                else
                {
                    int index_of_blank_space = 0;
                    int index = 0;
                    foreach (char c in currentQuestion.QuestionText.Substring(55))
                    {
                        if (c == " ".ToCharArray()[0])
                        {
                            index_of_blank_space = index;
                            break;
                        }
                        index++;
                        Debug.Log(index);
                    }
                    QuestionText.text = currentQuestion.QuestionText.Substring(0, index_of_blank_space+55) + Environment.NewLine +
                    currentQuestion.QuestionText.Substring(index_of_blank_space+55);
                }
            }
            else
            {
                QuestionText.text = currentQuestion.QuestionText;
            }

            if ((currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.RadioButtons) && (questionLoaded == false))
            {
                GenYesNoRadioButtons();
                questionLoaded = true;
            }

        }
        else
        {
            
            GameObject.FindGameObjectWithTag("FormMenuParent").GetComponentInChildren<FormMenuHandler>(true).gameObject.SetActive(true);
            if (SceneManager.GetActiveScene().name == "NodeGraph")
            {
                GameObject.FindObjectOfType<ForceDirLayout>().gameObject.SetActive(false);
            }
            else if (SceneManager.GetActiveScene().name == "SphereScene")
            {
                GameObject.FindObjectOfType<SphereData>().gameObject.SetActive(false);
            }
            CollectTrash();
            gameObject.SetActive(false);
        }
        //sbs.readyForSubmit = false;
    }

    public void CollectTrash()
    {
        foreach (LineRenderer lr in GameObject.FindObjectsOfType<LineRenderer>())
        {
            lr.gameObject.SetActive(false);
        }
    }
    
    public void RefreshMovieObject(MovieObject m)
    {
        movieObject = m;
    }

    public void RemoveAnswer(string dataSelected)
    {
        TextMesh text = ConfirmationPopUp.GetComponent<TextMesh>();
        text.text = text.text.Replace(dataSelected + Environment.NewLine, "");
        currentAnswersList.Remove(dataSelected);

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
            if (currentAnswersList.Count != 0)
            {
                ConfirmationPopUp.GetComponent<TextMesh>().text += dataSelected + Environment.NewLine;
            }
        }
        currentAnswerSelected = dataSelected;
    }

    public void HandleUserInput()
    {
        if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.AnsInput ||
            currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.RadioButtons)
        {
            if (currentAnswerSelected != null)
            {
                AddToList(form_questions.QuestionIndex, currentAnswerSelected);

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
            }
        }
        else if (currentQuestion.QuestionType == FormMenuHandler.QuestionTypes.MultipleInput)
        {
            if (!currentAnswersList.Any())
            {
                return;             // careful not to change the order of this function
            }                       // because this return might break it if you do
            string test = "";
            foreach (string s in currentAnswersList)
            {
                Debug.Log(s);
                test += s;
            }
            AddToList(form_questions.QuestionIndex, test);
            
        }
        sbs.readyForSubmit = false;
        form_questions.QuestionIndex++;
        questionLoaded = false;
        currentAnswerSelected = null;


        ConfirmationPopUp.SetActive(false);
        ForceDirLayout fDirScript = GameObject.FindObjectOfType<ForceDirLayout>();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("MovieNode"))
        {
            NodeInfo info = fDirScript.getNodeInfo(g.name);
            info.interState = NodeInteractionState.NONE;
        }
        
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


    public void AddToList(int QNum, string value)
    {
        List<String> data = new List<string>();
        if (FindObjectOfType<UserDataCollectionHandler>() != null)
        {
            if (QNum == 0)
            {
                string participant_number_text = "";
                string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string path = pathDesktop + "\\mycsvfile.csv";

                int ScenesCompleted = 0;
                if (File.ReadAllText(path).Contains("PARTICIPANT"))
                {
                    using (var w = new StreamReader(path, true))
                    {

                        string pattern = @"PARTICIPANT\d";

                        string fileString;
                        fileString = w.ReadToEnd();
                        Match m = Regex.Match(fileString, pattern,RegexOptions.RightToLeft);

                        ScenesCompleted += Regex.Matches(fileString.Substring(m.Index),"SphereScene").Count;
                        ScenesCompleted += Regex.Matches(fileString.Substring(m.Index), "NodeGraph").Count;


                        string g = m.ToString();
                        int participant_number = Int32.Parse(Regex.Split(g, "PARTICIPANT")[1]);
                        participant_number++;
                        participant_number_text = "PARTICIPANT" + participant_number;
                    }
                }
                else
                {
                    participant_number_text = "PARTICIPANT1";
                    data.Add(participant_number_text);
                }
                if (ScenesCompleted >= 2)     // WARNING: THIS CODE WILL INTERPRET QUESTION SUBMISSIONS ON MORE THAN TWO SCENES AS AN ADDITIONAL PARTICIPANT.  DO NOT HAVE PARTICIPANTS ANSWER QUESTIONS IN MORE THAN TWO SCENES.
                {
                    data.Add(participant_number_text);
                }
                data.Add(SceneManager.GetActiveScene().name + SceneParams.getParamValue("ShowEdges"));
            }
            data.Add("QNumT:" + QNum + " " + "Input Value:" + value);
            SaveOutputData(data);
        }
    }
    public void SaveOutputData(List<string> selectInformation)
    {
        long endTime = DateTime.Now.ToFileTime();
        string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = pathDesktop + "\\mycsvfile.csv";


        
        using (var w = new StreamWriter(path, true))
        {   
            
            //w.WriteLine(participant_number_text);
            for (int i = 0; i < selectInformation.Count; i++)
            {               
                var first = selectInformation[i];
                string line = string.Format("{0}", first);
                w.WriteLine(line);
            }
            DateTime startDate = DateTime.FromFileTime(startTime);
            DateTime endDate = DateTime.FromFileTime(endTime);

            string lastLine = string.Format("{0},{1}", "Time Elapsed", endDate - startDate);
            w.WriteLine(lastLine);
        }
    }
}