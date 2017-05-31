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
    private List<String> answers = new List<string>();
    private TextMesh current_question_text;
    public enum FormMenuHandlerType
    {
        ToggleCheckbox,
        ToggleRadio,
        SubmitForm
    }

    [NonSerialized]
    public new FormMenuHandlerType handlerType;

    private static List<string> allActiveGOs = new List<string>();

    private long startTime;
    // class for inputing form questions in editor
    [System.Serializable]
    public class FormQuestions
    {
        public int QuestionIndex;
        public  List<String> questions = new List<string>();
        public String[] answers;
    }
    public FormQuestions form_questions = new FormQuestions();

    void Start()
    {
        form_questions.answers = new string[form_questions.questions.Count];
        
        startTime = DateTime.Now.ToFileTime();
        //boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        //checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
        SetQuestion();
    }

    public void SetQuestion()
    {
        foreach (TextMesh text in gameObject.GetComponentsInChildren<TextMesh>())
        {
            if (text.tag == "CurrentQuestion")
            {
                current_question_text = text;
            }
        }
        if (form_questions.QuestionIndex < form_questions.questions.Count)
        {
            current_question_text.text = form_questions.questions[form_questions.QuestionIndex];
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
            allActiveGOs.Add(transform.parent.gameObject.GetComponent<TextMesh>().text);
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

    public override void handleTrigger()
    {
        switch (handlerType)
        {
            case FormMenuHandlerType.ToggleCheckbox:
                materialStatus = !materialStatus;
                UpdateMaterial();
                break;
            case FormMenuHandlerType.ToggleRadio:
                //TODO
                break;
            case FormMenuHandlerType.SubmitForm:
                FindObjectOfType<UMD_Sphere_TrackedObject>().hideMainMenu();
                CSVEntries.SaveOutputData(allActiveGOs, startTime);
                allActiveGOs.Clear();
                baseState.DestroyMenu();
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
