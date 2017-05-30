using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

//Fill out a Form in VR which rates the survey
public class FormState : BaseState {

    protected override void Start()
    {
        base.Start();

        bringUpMenu();
    }

    public override void bringUpMenu()
    {
        int checkBoxesLength = 3;
        int bestOptionLength = 3;
        List<GameObject> interactableObjects = new List<GameObject>();

        int menuLayerMask = LayerMask.NameToLayer("Menus");

        menu = new GameObject();
        menu.name = "Evaluation Form";
        menu.transform.SetParent(gameObject.transform);
        menu.transform.localRotation = Quaternion.identity;
        menu.transform.localPosition = Vector3.zero;
        menu.transform.parent.gameObject.layer = menuLayerMask;

        List<GameObject> textObjects = new List<GameObject>();

        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        float yOffsetPerLine = 0.02f;

        Vector3 offset = Vector3.zero;

        textObjects.Add(AddText(menu, "Select all of the following that apply: ", roleAlign, roleAnchor, offset, 100));
        offset.y -= yOffsetPerLine;

        offset.x = 0.04f;
        GameObject newText = AddText(menu, "A", roleAlign, roleAnchor, offset, 100);
        textObjects.Add(newText);
        interactableObjects.Add(newText);
        offset.y -= yOffsetPerLine;

        newText = AddText(menu, "B", roleAlign, roleAnchor, offset, 100);
        textObjects.Add(newText);
        interactableObjects.Add(newText);
        offset.y -= yOffsetPerLine;

        newText = AddText(menu, "C", roleAlign, roleAnchor, offset, 100);
        textObjects.Add(newText);
        interactableObjects.Add(newText);
        offset.y -= yOffsetPerLine;

        offset.x = 0;
        textObjects.Add(AddText(menu, "Select the best option: ", roleAlign, roleAnchor, offset, 100));
        offset.y -= yOffsetPerLine;

        offset.x = 0.04f;
        newText = AddText(menu, "A", roleAlign, roleAnchor, offset, 100);
        textObjects.Add(newText);
        interactableObjects.Add(newText);
        offset.y -= yOffsetPerLine;

        newText = AddText(menu, "B", roleAlign, roleAnchor, offset, 100);
        textObjects.Add(newText);
        interactableObjects.Add(newText);
        offset.y -= yOffsetPerLine;

        newText = AddText(menu, "C", roleAlign, roleAnchor, offset, 100);
        textObjects.Add(newText);
        interactableObjects.Add(newText);
        offset.y -= yOffsetPerLine;

        offset.x = 0;
        offset.y -= yOffsetPerLine;
        textObjects.Add(AddText(menu, "Rate the following on a scale of 1-10", roleAlign, roleAnchor, offset, 100));
        offset.y -= yOffsetPerLine;

        /*--------------Slider here for rating experience from 1 to 10------------*/
        GameObject sliderContainer = new GameObject("Slider_Container");
        sliderContainer.transform.SetParent(menu.transform);
        sliderContainer.transform.localPosition = Vector3.zero;
        sliderContainer.transform.localRotation = Quaternion.identity;

        GameObject slider = new GameObject("Quad_Slider");
        GameObject sliderPoint = new GameObject("Quad_Slider_Point");
        GameObject leftMostPoint = new GameObject("Quad_Slider_left");
        GameObject rightMostPoint = new GameObject("Quad_Slider_right");
        GameObject leftText = new GameObject("Text-Rating_1");
        GameObject rightText = new GameObject("Text-Rating_10");

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
        leftText.AddComponent<MeshRenderer>();
        rightText.AddComponent<TextMesh>();
        rightText.AddComponent<MeshRenderer>();

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
        slider.transform.localPosition -= new Vector3(0,0.1f,0.01f);
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
        /*--------------------------------------------------------------------------*/

        float minX = float.MaxValue;
        float minY = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (GameObject obj in textObjects)
        {
            MeshRenderer rend = obj.GetComponent<MeshRenderer>();
            Vector3 min = rend.bounds.min;
            Vector3 max = rend.bounds.max;

            if (min.x < minX) minX = min.x;
            if (max.x > maxX) maxX = max.x;

            if (min.y < minY) minY = min.y;
            if (max.y > maxY) maxY = max.y;
        }

        float xDim = (maxX - minX) - 0.05f;
        float yDim = (maxY - minY);

        Vector3 basePos = new Vector3(-xDim, yDim, 0.0f);

        foreach (GameObject obj in textObjects)
        {
            obj.transform.localPosition += basePos;
            obj.transform.localRotation = Quaternion.identity;
        }

        /*-----------------Submit button (GUI is close symbol for now)-----------------*/
        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.name = "Submit";
        quad1.layer = menuLayerMask;
        quad1.transform.SetParent(menu.transform);

        quad1.transform.localPosition = Vector3.zero;

        MeshRenderer qrend = quad1.GetComponent<MeshRenderer>();
        qrend.material = closeMaterial;
        quad1.transform.localScale = new Vector3(0.02f, 0.02f, 1.0f);
        quad1.transform.localPosition -= new Vector3(-0.0918f, -0.1f, 0.02f);
        quad1.transform.localRotation = Quaternion.identity;

        quad1.AddComponent<FormMenuHandler>();
        quad1.GetComponent<FormMenuHandler>().baseState = this;
        quad1.GetComponent<FormMenuHandler>().handlerType = FormMenuHandler.FormMenuHandlerType.SubmitForm;
        

        offset = Vector3.zero;
        offset.x = -0.055f;
        offset.y = 0.0675f;

        /*----------Checkboxes here for multi-selection-------------------*/
        for (int toggleInd = 0; toggleInd < checkBoxesLength; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Multi Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(interactableObjects[toggleInd].transform);
            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            rend.material = checkMaterial;
            rend.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition -= new Vector3(0.61f, 0.15f, 0.2f);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = this;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleCheckbox;
            menuHandler.baseMaterial = boxMaterial;
            menuHandler.inputInteractMaterial = checkMaterial;

            menuHandler.UpdateMaterial();

            offset.y -= yOffsetPerLine;
        }

        offset.y -= yOffsetPerLine;

        /*----------Radio here for single-selection-------------------*/
        for (int toggleInd = 0; toggleInd < bestOptionLength; toggleInd++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Best Option";
            quad.layer = menuLayerMask;
            quad.transform.SetParent(interactableObjects[toggleInd + 3].transform);
            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            rend.material = circleMaterial;
            rend.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);
            rend.transform.localRotation = Quaternion.identity;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localPosition -= new Vector3(0.61f, 0.15f, 0.2f);

            quad.AddComponent<FormMenuHandler>();
            FormMenuHandler menuHandler = quad.GetComponent<FormMenuHandler>();
            menuHandler.baseState = this;
            menuHandler.handlerType = FormMenuHandler.FormMenuHandlerType.ToggleRadio;
            menuHandler.baseMaterial = circleMaterial;
            menuHandler.inputInteractMaterial = sliderPointMaterial;

            menuHandler.UpdateMaterial();

            offset.y -= yOffsetPerLine;
        }


    }

}
