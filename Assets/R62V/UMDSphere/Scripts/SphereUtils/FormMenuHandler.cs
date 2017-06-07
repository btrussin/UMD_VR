using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;

public class FormMenuHandler : BaseMenuHandler
{
    
    private bool materialStatus;

    public Material CheckMaterial;
    public Material BoxMaterial;
    public Material CircleMaterial;
    public Material sliderPointMaterial;
    public Material sliderBarMaterial;

    private FormMenuHandler submitButton;
    private List<String> answers = new List<string>();
    private TextMesh current_question_text;
    private FormState formState;
    private FormQuestions.Question currentQuestion;

    public enum FormMenuHandlerType
    {
        ToggleCheckbox,
        ToggleRadio,
        SubmitForm,
        SubmitQuestionAnswer
    }
    public enum QuestionTypes
    {
        RadioButtons,
        CheckBoxes,
        Slider,
        AnsInput
    }

    public new FormMenuHandlerType handlerType;

    private static List<string> allActiveGOs = new List<string>();

    private long startTime;
    // class for inputing form questions in editor
    [System.Serializable]
    public class FormQuestions
    {
        [System.Serializable]
        public class Question
        {
            public string QuestionText;
            public QuestionTypes QuestionType;
            public int NumberOfAnswers;
            public List<String> possible_answers = new List<String>();

            


        }
        public int QuestionIndex;
        public  List<Question> questions = new List<Question>();
        //public String[] answers;

    }
    public FormQuestions form_questions = new FormQuestions();

    void Start()
    {
        BoxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        CheckMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
        CircleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/circ_mat.mat");
        sliderPointMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/sliderpnt_mat.mat");
        sliderBarMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/sliderbar_mat.mat");

        formState = GetComponent<FormState>();

        submitButton = GameObject.FindGameObjectWithTag("SubmitButton").GetComponent<FormMenuHandler>();
        submitButton.baseState = formState;
        submitButton.handlerType = FormMenuHandler.FormMenuHandlerType.SubmitQuestionAnswer;
        

        foreach (TextMesh text in gameObject.GetComponentsInChildren<TextMesh>())
        {
            if (text.tag == "CurrentQuestionText")
            {
                current_question_text = text;
            }
        }

        startTime = DateTime.Now.ToFileTime();
        SetQuestion();
    }

    public void SetQuestion()    // generates the text and the radio buttons or checkboxes or slider
    {
        
        if (form_questions.QuestionIndex < form_questions.questions.Count)
        {
            clearSelection();
            currentQuestion = form_questions.questions[form_questions.QuestionIndex];

            current_question_text.text = currentQuestion.QuestionText;

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
                GenSlider();
            }
            if (currentQuestion.QuestionType == QuestionTypes.AnsInput)
            {
                AnsInput("");
            }
        }
        
    }

    public void GenCheckBox()
    { 
        float offset_y = 0.0675f;
        float yOffsetPerLine = 0.12f;
         
        List<GameObject> interactableObjects = new List<GameObject>();
        int menuLayerMask = LayerMask.NameToLayer("Menus");

        for (int toggleInd = 0; toggleInd < currentQuestion.NumberOfAnswers; toggleInd++)
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
            rend.transform.localPosition = new Vector3(-0.47f,0.316f,0.887f);
            rend.transform.localPosition -= new Vector3(0,offset_y,0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleCheckbox;
            
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

        for (int toggleInd = 0; toggleInd < currentQuestion.NumberOfAnswers; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.tag = "RadioButton";
            quad.name = "Best Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(transform);

            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            //rend.material = CircleMaterial;
            rend.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition = new Vector3(-0.49f, 0.31f, 0.88f);
            rend.transform.localPosition -= new Vector3(0, offset_y, 0);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = formState;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleRadio;

            menuHandler.baseMaterial = CircleMaterial;
            menuHandler.inputInteractMaterial = sliderPointMaterial;
            menuHandler.UpdateMaterial();

            offset_y += yOffsetPerLine;
        }

        offset_y += yOffsetPerLine;
    }

    public void SubmitQuestionAnswer()
    {
       
    }

    public void AnsInput(String answer)
    {

    }

    public void GenSlider()
    {
        GameObject sliderContainer = new GameObject("Slider_Container");
        sliderContainer.transform.SetParent(transform);
        sliderContainer.transform.localPosition = new Vector3(-0.02f, 1.26f, 0.94f);
        sliderContainer.transform.localRotation = Quaternion.identity;
        sliderContainer.transform.localScale = new Vector3(7.9f, 9.4f, 4.02f);
        sliderContainer.tag = "Slider";

        GameObject slider = new GameObject("Quad_Slider");
        GameObject sliderPoint = new GameObject("Quad_Slider_Point");
        //
        GameObject leftMostPoint = new GameObject("Quad_Slider_left");
        GameObject rightMostPoint = new GameObject("Quad_Slider_right");
        GameObject leftText = new GameObject("Text-Rating_1");
        GameObject rightText = new GameObject("Text-Rating_10");
        List<GameObject> textObjects = new List<GameObject>();


        slider.AddComponent<MeshFilter>();
        slider.AddComponent<MeshRenderer>();
        sliderPoint.AddComponent<MeshFilter>();
        sliderPoint.AddComponent<MeshRenderer>();
        sliderPoint.AddComponent<MeshCollider>();
        sliderPoint.AddComponent<Rigidbody>();
        sliderPoint.GetComponent<Rigidbody>().isKinematic = true;
        leftMostPoint.AddComponent<MeshFilter>();
        leftMostPoint.AddComponent<MeshRenderer>();
        rightMostPoint.AddComponent<MeshFilter>();
        rightMostPoint.AddComponent<MeshRenderer>();
        leftText.AddComponent<TextMesh>();
        rightText.AddComponent<TextMesh>();

        slider.GetComponent<Renderer>().material = sliderBarMaterial;
        sliderPoint.GetComponent<Renderer>().material = sliderPointMaterial;
        leftMostPoint.GetComponent<Renderer>().material = sliderPointMaterial;
        rightMostPoint.GetComponent<Renderer>().material = sliderPointMaterial;
        leftText.GetComponent<TextMesh>().text = "1";
        leftText.GetComponent<TextMesh>().fontSize = 16;
        rightText.GetComponent<TextMesh>().text = "10";
        rightText.GetComponent<TextMesh>().fontSize = 16;

        // Create a quad game object
        GameObject quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        // Assign the mesh from that quad to your gameobject's mesh    
        slider.GetComponent<MeshFilter>().mesh = quadGO.GetComponent<MeshFilter>().mesh;
        sliderPoint.GetComponent<MeshFilter>().mesh = quadGO.GetComponent<MeshFilter>().mesh;
        leftMostPoint.GetComponent<MeshFilter>().mesh = quadGO.GetComponent<MeshFilter>().mesh;
        rightMostPoint.GetComponent<MeshFilter>().mesh = quadGO.GetComponent<MeshFilter>().mesh;
        GameObject.Destroy(quadGO);

        slider.transform.SetParent(sliderContainer.transform);
        sliderPoint.transform.SetParent(sliderContainer.transform);
        leftMostPoint.transform.SetParent(sliderContainer.transform);
        rightMostPoint.transform.SetParent(sliderContainer.transform);
        leftText.transform.SetParent(sliderContainer.transform);
        rightText.transform.SetParent(sliderContainer.transform);

        slider.transform.localPosition = Vector3.zero;
        slider.transform.localPosition -= new Vector3(0, 0.1f, 0.01f);
        slider.transform.localRotation = Quaternion.identity;
        slider.transform.Rotate(Vector3.forward, 90);
        slider.transform.localScale = new Vector3(0.0009747184f, 0.06f, 0.7797724f);
        sliderPoint.transform.localPosition = Vector3.zero;
        sliderPoint.transform.localPosition -= new Vector3(0, 0.1f, 0.01f);
        sliderPoint.transform.localRotation = Quaternion.identity;
        sliderPoint.transform.Rotate(Vector3.forward, 90);
        sliderPoint.transform.localScale = new Vector3(0.007797747f, 0.007797728f, 0.7797745f);
        leftMostPoint.transform.localPosition = Vector3.zero;
        leftMostPoint.transform.localPosition -= new Vector3(0.03f, 0.1f, 0.01f);
        leftMostPoint.transform.localRotation = Quaternion.identity;
        leftMostPoint.transform.Rotate(Vector3.forward, 90);
        leftMostPoint.transform.localScale = new Vector3(0.001949439f, 0.001949435f, 0.7797739f);
        rightMostPoint.transform.localPosition = Vector3.zero;
        rightMostPoint.transform.localPosition -= new Vector3(-0.03f, 0.1f, 0.01f);
        rightMostPoint.transform.localRotation = Quaternion.identity;
        rightMostPoint.transform.Rotate(Vector3.forward, 90);
        rightMostPoint.transform.localScale = leftMostPoint.transform.localScale;
        leftText.transform.localPosition = leftMostPoint.transform.localPosition - new Vector3(0.004f, -0.0017f, 0);
        rightText.transform.localPosition = rightMostPoint.transform.localPosition + new Vector3(0.004f, 0.0017f, 0);
        leftText.transform.localRotation = Quaternion.identity;
        rightText.transform.localRotation = Quaternion.identity;
        leftText.transform.localScale = leftMostPoint.transform.localScale;
        rightText.transform.localScale = leftMostPoint.transform.localScale;
        sliderPoint.tag = "SliderPoint";
        leftMostPoint.tag = "SliderLeftLimit";
        rightMostPoint.tag = "SliderRightLimit";

    }

    public void clearSelection()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>() )
        {
            if (t.tag == ("RadioButton") || (t.tag ==  "CheckBox") || (t.tag == "Slider"))
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
        Debug.Log(materialStatus);
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
            Debug.Log(baseMaterial);
           
            rend.material = baseMaterial;

            if (allActiveGOs.Count > 0)
            {
                allActiveGOs.Remove(transform.parent.gameObject.GetComponent<TextMesh>().text);
            }
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
             
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("RadioButton") )
                {
                    //Debug.Log("before" + g.GetComponent<FormMenuHandler>().materialStatus);
                    g.GetComponent<FormMenuHandler>().materialStatus = false;
                    g.GetComponent<FormMenuHandler>().UpdateMaterial();
                    //Debug.Log(g.GetComponent<FormMenuHandler>().materialStatus);
                }
                materialStatus = !materialStatus;
                UpdateMaterial();
                break;
            case FormMenuHandlerType.SubmitForm:
                FindObjectOfType<UMD_Sphere_TrackedObject>().hideMainMenu();
                CSVEntries.SaveOutputData(allActiveGOs, startTime);
                allActiveGOs.Clear();
                baseState.DestroyMenu();
         
                break;
            case FormMenuHandlerType.SubmitQuestionAnswer:
         
                break;
            default:
                break;
        }
    }
    /*
    // converts String[] arrays to List<String>
    public static List<String> ArrayToList(String[] array)
    {
        List<String> newList = new List<String>();
        foreach (String i in array)
        {
            newList.Add(i);
        }
        return newList;
    }

    // sends the answers to the survey questions to the SaveOutputData function
    public void SubmitForm()
    {
        SaveOutputData(ArrayToList(form_questions.answers),startTime);
    }

    */




    public static void SaveOutputData(List<string> selectInformation, long startTime)
    {
        long endTime = DateTime.Now.ToFileTime();

        string path = "Form_Results/CSVFormData_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".csv";
        Debug.Log(selectInformation.Count);

        using (var w = new StreamWriter(path))
        {
            for (int i = 0; i < selectInformation.Count; i++)
            {
                var first = selectInformation[i];
                string line = string.Format("{0},{1}", "Checked Option", first); //using string interpolation
                w.WriteLine(line);
                w.Flush();
            }

            DateTime startDate = DateTime.FromFileTime(startTime);
            DateTime endDate = DateTime.FromFileTime(endTime);

            string startToEnd = string.Format("{0},{1}", startDate, endDate);
            w.WriteLine(startToEnd);
            w.Flush();

            string lastLine = string.Format("{0},{1}", "Time Elapsed", endDate - startDate);
            w.WriteLine(lastLine);
            w.Flush();
        }
    }
}
