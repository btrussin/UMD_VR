using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class FormMenuHandler : BaseMenuHandler
{
    private bool materialStatus;

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

    void Start()
    {
        startTime = DateTime.Now.ToFileTime();
        //boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        //checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
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
