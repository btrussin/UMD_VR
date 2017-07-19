using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Debug = UnityEngine.Debug;

public class FormMenuHandler : BaseMenuHandler
{

    public bool materialStatus;
    public bool readyForSubmit;

    public Material CheckMaterial;
    public Material BoxMaterial;
    public Material CircleMaterial;
    public Material sliderPointMaterial;
    public Material sliderBarMaterial;
    public float amountScrolled = 0;

    private FormMenuHandler submitButton;
    private List<String> answers = new List<string>();
    private TextMesh current_question_text;
    private FormState formState;
    private FormQuestions.Question currentQuestion;
    private FormMenuHandler FormMenu;
    private GameObject selectText;
    private SubmitButtonScript sbs;
    public int currentSliderValue;
    public enum FormMenuHandlerType
    {
        ToggleCheckbox,
        ToggleRadio,
        SubmitForm,
        SubmitQuestionAnswer,
        NotClickable
    }
    public enum QuestionTypes
    {
        RadioButtons,
        CheckBoxes,
        Slider,
        AnsInput,
        MultipleInput
    }

    public new FormMenuHandlerType handlerType;

    private static List<string> allActiveGOs = new List<string>();

    public long startTime;
    // class for inputing form questions in editor
    [System.Serializable]
    public class FormQuestions
    {
        [System.Serializable]
        public class Question
        {
            public string QuestionText;
            public QuestionTypes QuestionType;
            //public int NumberOfAnswers;
            public List<String> possible_answers = new List<String>();




        }
        public int QuestionIndex;
        public List<Question> questions = new List<Question>();
        public List<String> surveyResponses = new List<String>();
        //public String[] answers;

    }


    public FormQuestions form_questions = new FormQuestions();

    void Start()
    {
        
        if (tag == "FormMenu")
        {
            sbs = GameObject.FindObjectOfType<SubmitButtonScript>();
            selectText = GameObject.FindGameObjectWithTag("SelectText");
            FormMenu = GameObject.FindGameObjectWithTag("FormMenu").GetComponent<FormMenuHandler>();
            BoxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
            CheckMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
            CircleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/circ_mat.mat");
            sliderPointMaterial =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/sliderpnt_mat.mat");
            sliderBarMaterial =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/sliderbar_mat.mat");
            
            formState = GetComponent<FormState>();
        }
        try
        {
            submitButton = GameObject.FindGameObjectWithTag("SubmitButton").GetComponent<FormMenuHandler>();
            submitButton.baseState = formState;
            submitButton.handlerType = FormMenuHandler.FormMenuHandlerType.SubmitQuestionAnswer;
        }
        catch (NullReferenceException)
        {

        }

        foreach (TextMesh text in gameObject.GetComponentsInChildren<TextMesh>())
        {
            if (text.tag == "CurrentQuestionText")
            {
                FormMenu.current_question_text = text;
            }
        }

        startTime = DateTime.Now.ToFileTime();
        if (tag == "FormMenu")
        {
            SetQuestion();
        }
        /*
        if (tag == "FormMenu")
        {
            IEnumerator coroutine;
            coroutine = WaitAndTurnOff(1f, gameObject);
            StartCoroutine(coroutine);
        }*/

    }
    /*
    private IEnumerator WaitAndTurnOff(float waitTime,GameObject g)
    {
        while (true)
        {
            
            yield return new WaitForSeconds(waitTime);
            g.SetActive(false);
        }
    }*/

    public void SetQuestion()    // generates the text and the radio buttons or checkboxes or slider
    {
        if (form_questions.QuestionIndex < form_questions.questions.Count)
        {
            clearSelection();
            currentQuestion = FormMenu.form_questions.questions[form_questions.QuestionIndex];
            if (current_question_text == null)
            {
                Debug.Log("current_question_text is null");
                return;
            }

            current_question_text.text = currentQuestion.QuestionText;

            if (current_question_text.text.Length > 60)
            {
                if (FormMenu.current_question_text.text[60].ToString() == " " ||
                    FormMenu.current_question_text.text[59].ToString() == " ")
                {
                    current_question_text.text = current_question_text.text.Substring(0, 60) + Environment.NewLine + current_question_text.text.Substring(60);
                }
                else
                {
                    for (int i = 0; i < FormMenu.current_question_text.text.Substring(60).Length; i++)
                    {
                        if (FormMenu.current_question_text.text.Substring(60)[i] == ' ')
                        {

                            current_question_text.text = current_question_text.text.Substring(0, 60 + i) + Environment.NewLine + current_question_text.text.Substring(60 + i);
                            break;
                        }
                    }
                }
            }


            if (currentQuestion.QuestionType == QuestionTypes.CheckBoxes)
            {
                GenCheckBox();
            }
            if (currentQuestion.QuestionType == QuestionTypes.RadioButtons)
            {
                GenRadioButton();
            }
            if (currentQuestion.QuestionType == QuestionTypes.Slider)
            {
                GenNewSlider();
            }
            if (currentQuestion.QuestionType == QuestionTypes.AnsInput)
            {
                AnsInput("");
            }
        }
        else
        {

        }
        sbs.readyForSubmit = false;
        amountScrolled = 35;
    }

    public void GenCheckBox()
    {
        float offset_y = 0.0675f;
        float yOffsetPerLine = 0.12f;

        List<GameObject> interactableObjects = new List<GameObject>();
        int menuLayerMask = LayerMask.NameToLayer("Menus");

        for (int toggleInd = 0; toggleInd < currentQuestion.possible_answers.Count; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.tag = "CheckBox";
            quad.name = "Multi Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(transform);



            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            rend.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition = new Vector3(-0.44f, 0.31f, 0.887f);
            rend.transform.localPosition -= new Vector3(0, offset_y, 0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleCheckbox;

            GameObject optionText = new GameObject("");
            optionText.AddComponent<TextMesh>();
            if (FormMenu.currentQuestion.possible_answers.Count >= toggleInd)
            {
                optionText.GetComponent<TextMesh>().text = FormMenu.currentQuestion.possible_answers[toggleInd];
            }
            optionText.GetComponent<TextMesh>().fontSize = 16;
            optionText.name = "Option Text";
            optionText.transform.SetParent(quad.transform);
            optionText.transform.localRotation = Quaternion.identity;
            optionText.transform.localPosition = new Vector3(.69f, .48f, -.0015f);
            optionText.transform.localScale = new Vector3(.5f, .5f, 203); ;


            menuHandler.baseMaterial = BoxMaterial;
            menuHandler.inputInteractMaterial = CheckMaterial;
            menuHandler.UpdateMaterial();

            offset_y += yOffsetPerLine;
        }

        offset_y += yOffsetPerLine;
    }


    public void GenRadioButton()
    {  // by RK, check for Alex and Mike

        float offset_y = 0.0675f;
        float yOffsetPerLine = 0.12f;
        List<GameObject> interactableObjects = new List<GameObject>();
        int menuLayerMask = LayerMask.NameToLayer("Menus");

        for (int toggleInd = 0; toggleInd < currentQuestion.possible_answers.Count; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.tag = "RadioButton";
            quad.name = "Best Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(transform);

            MeshRenderer rend = quad.GetComponent<MeshRenderer>();

            rend.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition = new Vector3(-0.49f, 0.31f, 0.88f);
            rend.transform.localPosition -= new Vector3(0, offset_y, 0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleRadio;

            GameObject optionText = new GameObject("");
            optionText.AddComponent<TextMesh>();
            if (FormMenu.currentQuestion.possible_answers.Count >= toggleInd)
            {
                optionText.GetComponent<TextMesh>().text = FormMenu.currentQuestion.possible_answers[toggleInd];
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


    public void SubmitQuestionAnswer()
    {
        // submits the response from the particular question into the list.   
        if (FormMenu.currentQuestion.QuestionType == QuestionTypes.CheckBoxes)
        {
            int numberofCheckBoxSelected = 0;
            foreach (FormMenuHandler fmh in FormMenu.GetComponentsInChildren<FormMenuHandler>())
            {
                if ((fmh.tag == "CheckBox") && fmh.materialStatus)
                {
                    numberofCheckBoxSelected++;
                }
            }
            AddToList(FormMenu.form_questions.QuestionIndex + 1, numberofCheckBoxSelected.ToString());

        }
        else if (FormMenu.currentQuestion.QuestionType == QuestionTypes.RadioButtons)
        {
            foreach (FormMenuHandler fmh in FormMenu.GetComponentsInChildren<FormMenuHandler>())
            {
                if ((fmh.tag == "RadioButton") && fmh.materialStatus)
                {
                    Debug.Log(fmh.transform.GetComponentInChildren<TextMesh>().text);
                }
            }
        }
        else if (FormMenu.currentQuestion.QuestionType == QuestionTypes.Slider)
        {
            AddToList(FormMenu.form_questions.QuestionIndex + 1, currentSliderValue.ToString());
        }

        FormMenu.form_questions.QuestionIndex++;
        FormMenu.SetQuestion();

    }

    public void AnsInput(String answer)
    {

    }

    public void GenNewSlider()
    {
        float offset_x = 0.0675f;
        float yOffsetPerLine = 0.12f;
        List<GameObject> interactableObjects = new List<GameObject>();
        int menuLayerMask = LayerMask.NameToLayer("Menus");

        for (int toggleInd = 0; toggleInd < currentQuestion.possible_answers.Count; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.tag = "RadioButton";
            quad.name = "Best Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(transform);

            MeshRenderer rend = quad.GetComponent<MeshRenderer>();

            rend.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition = new Vector3(-0.3f, 0.31f, 0.88f);
            rend.transform.localPosition += new Vector3(offset_x, 0, 0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleRadio;

            GameObject optionText = new GameObject("");
            optionText.AddComponent<TextMesh>();
            if (FormMenu.currentQuestion.possible_answers.Count >= toggleInd)
            {
                optionText.GetComponent<TextMesh>().text = FormMenu.currentQuestion.possible_answers[toggleInd];
            }
            optionText.GetComponent<TextMesh>().fontSize = 35;
            optionText.GetComponent<TextMesh>().characterSize = 0.35f;
            optionText.name = "Option Text";
            optionText.transform.SetParent(quad.transform);
            optionText.transform.localRotation = Quaternion.identity;
            optionText.transform.localPosition = new Vector3(-1.69f, -0.71f, -.0003f);
            optionText.transform.localScale = new Vector3(.5f, .5f, 203);

            menuHandler.baseMaterial = CircleMaterial;
            menuHandler.inputInteractMaterial = sliderPointMaterial;
            menuHandler.UpdateMaterial();

            offset_x += yOffsetPerLine;
        }
        offset_x += yOffsetPerLine;
    }
    public void GenOldSlider()
    {   // by RK, check for Alex and Mike, as requested this is the radio button slider.

        float offset_y = -0.8f;
        float yOffsetPerLine = 0.12f;
        int number = 1;
        List<GameObject> interactableObjects = new List<GameObject>();
        int menuLayerMask = LayerMask.NameToLayer("Menus");

        for (int toggleInd = 0; toggleInd < currentQuestion.possible_answers.Count; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.tag = "Slider";
            quad.name = "Best Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(transform);

            MeshRenderer rend = quad.GetComponent<MeshRenderer>();

            rend.transform.localScale = new Vector3(0.15f, 0.15f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition = new Vector3(-0.16f, 0.24f, 0.88f);
            rend.transform.localPosition -= new Vector3(offset_y, 0, 0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.NotClickable;

            GameObject optionText = new GameObject("");
            optionText.AddComponent<TextMesh>();
            if (FormMenu.currentQuestion.possible_answers.Count >= toggleInd)
            {
                optionText.GetComponent<TextMesh>().text = FormMenu.currentQuestion.possible_answers[toggleInd];
            }

            optionText.GetComponent<TextMesh>().fontSize = 35;
            optionText.GetComponent<TextMesh>().characterSize = 0.35f;
            optionText.name = "Option Text";
            optionText.transform.SetParent(quad.transform);
            optionText.transform.localRotation = Quaternion.identity;
            optionText.transform.localPosition = new Vector3(-1.69f, -0.71f, -.0003f);
            optionText.transform.localScale = new Vector3(.5f, .5f, 203); ;

            menuHandler.baseMaterial = CircleMaterial;
            menuHandler.inputInteractMaterial = sliderPointMaterial;
            if (number == 4)
            {
                menuHandler.materialStatus = true;
            }
            menuHandler.UpdateMaterial();


            offset_y += yOffsetPerLine + 0.05f;
            number += 1;
        }
        offset_y += yOffsetPerLine;
    }


    public void clearSelection()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.tag == ("RadioButton") || (t.tag == "CheckBox") || (t.tag == "Slider"))
            {
                Destroy(t.gameObject);
            }
        }
    }

    void Update()
    {

    }
    public override void UpdateMaterial()
    {
        MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();

        if (materialStatus)
        {
            rend.material = inputInteractMaterial;
            try
            {
                allActiveGOs.Add(transform.parent.gameObject.GetComponent<TextMesh>().text);
            }
            catch (MissingComponentException)
            {
            }

        }
        else
        {
            rend.material = baseMaterial;

            if (allActiveGOs.Count > 0)
            {
                allActiveGOs.Remove(transform.parent.gameObject.GetComponent<TextMesh>().text);
            }
        }
    }

    public void AddToList(int QNum, string value)
    {
        QNum = FormMenu.form_questions.QuestionIndex;

        List<String> data = new List<string>();
        data.Add("QNumS:" + QNum + " " + "Input Value:" + value);
        Debug.Log("saved survey");
        SaveOutputData(data);

        if (QNum == form_questions.questions.Count - 2)
        {
            saveFile(data);
        }
    }



    public override void handleTrigger()
    {

        switch (handlerType)
        {
            case FormMenuHandlerType.ToggleCheckbox:
                materialStatus = !materialStatus;
                UpdateMaterial();
                break;
            case FormMenuHandlerType.ToggleRadio:
                // sets all the radiobuttons to false
                {

                    foreach (GameObject g in GameObject.FindGameObjectsWithTag("RadioButton"))
                    {
                        g.GetComponent<FormMenuHandler>().materialStatus = false;
                        g.GetComponent<FormMenuHandler>().UpdateMaterial();
                    }
                    // sets selected radio button to true and updates materials
                    materialStatus = !materialStatus;
                    UpdateMaterial();
                }
                break;
            case FormMenuHandlerType.SubmitForm:
       
                FindObjectOfType<UMD_Sphere_TrackedObject>().HideMainMenu();
                CSVEntries.SaveOutputData(allActiveGOs, startTime);
                allActiveGOs.Clear();
                baseState.DestroyMenu();

                break;
            case FormMenuHandlerType.SubmitQuestionAnswer:
                SubmitQuestionAnswer();
                break;
            default:
                break;
        }
    }


    public void saveFile(List<string> selectInformation)
    {
        long endTime = DateTime.Now.ToFileTime();
        string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = pathDesktop + "\\mycsvfile.csv";

        using (var w = new StreamWriter(path, true))
        {
            for (int i = 0; i < selectInformation.Count; i++)
            {
                var first = selectInformation[i];
                string line = string.Format("{0}", first); //using string interpolation
                w.WriteLine(line);

            }

            DateTime startDate = DateTime.FromFileTime(startTime);
            DateTime endDate = DateTime.FromFileTime(endTime);

            string lastLine = string.Format("{0},{1}", "Time Elapsed", endDate - startDate);
            w.WriteLine(lastLine);

            string startToEnd = string.Format("{0},{1}", startDate, endDate);
            w.WriteLine(startToEnd);
        }

    }


    public void SaveOutputData(List<string> selectInformation)
    {
        long endTime = DateTime.Now.ToFileTime();
        string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = pathDesktop + "\\mycsvfile.csv";

        using (var w = new StreamWriter(path, true))
        {
            for (int i = 0; i < selectInformation.Count; i++)
            {
                var first = selectInformation[i];
                string line = string.Format("{0}", first); //using string interpolation
                w.WriteLine(line);
            }

            DateTime startDate = DateTime.FromFileTime(startTime);
            DateTime endDate = DateTime.FromFileTime(endTime);

            string lastLine = string.Format("{0},{1}", "Time Elapsed", endDate - startDate);
            w.WriteLine(lastLine);
        }
    }
}