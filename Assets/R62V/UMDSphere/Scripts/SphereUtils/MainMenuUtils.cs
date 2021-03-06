﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public class MainMenuUtils : MonoBehaviour {

    Material boxMaterial;
    Material checkMaterial;

    public GameObject sphereLayoutBox;
    public GameObject cylinderLayoutBox;
    public GameObject showEdgesBox;
    public GameObject animationLayoutBox;

    public GameObject distCategoryBox;
    public GameObject grpCategoryBox;
    public GameObject comicCategoryBox;
    public GameObject pubCategoryBox;
    public GameObject studioCategoryBox;
    public GameObject yearCategoryBox;

    MeshRenderer sphereBoxRenderer;
    MeshRenderer cylBoxRenderer;
    MeshRenderer showEdgesBoxRenderer;
    MeshRenderer animationBoxRenderer;

    MeshRenderer distBoxRenderer;
    MeshRenderer grpBoxRenderer;
    MeshRenderer comicBoxRenderer;
    MeshRenderer pubBoxRenderer;
    MeshRenderer studioBoxRenderer;
    MeshRenderer yearBoxRenderer;


    public SphereData sphereData = null;

    // Use this for initialization
    void Start () {
        boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");

        sphereBoxRenderer = sphereLayoutBox.GetComponent<MeshRenderer>();
        cylBoxRenderer = cylinderLayoutBox.GetComponent<MeshRenderer>();
        showEdgesBoxRenderer = showEdgesBox.GetComponent<MeshRenderer>();
        animationBoxRenderer = animationLayoutBox.GetComponent<MeshRenderer>();

        distBoxRenderer = distCategoryBox.GetComponent<MeshRenderer>();
        grpBoxRenderer = grpCategoryBox.GetComponent<MeshRenderer>();
        comicBoxRenderer = comicCategoryBox.GetComponent<MeshRenderer>();
        pubBoxRenderer = pubCategoryBox.GetComponent<MeshRenderer>();
        studioBoxRenderer = studioCategoryBox.GetComponent<MeshRenderer>();
        yearBoxRenderer = yearCategoryBox.GetComponent<MeshRenderer>();


        updateLayout();

    }

    public void updateLayout()
    {
        sphereBoxRenderer.material = boxMaterial;
        cylBoxRenderer.material = boxMaterial;

        showEdgesBoxRenderer.material = boxMaterial;


        distBoxRenderer.material = boxMaterial;
        grpBoxRenderer.material = boxMaterial;
        comicBoxRenderer.material = boxMaterial;
        pubBoxRenderer.material = boxMaterial;
        studioBoxRenderer.material = boxMaterial;
        yearBoxRenderer.material = boxMaterial;

        switch (sphereData.getCurrentLayout())
        {
            case SphereData.SphereLayout.Sphere:
                sphereBoxRenderer.material = checkMaterial;
                break;
            default:
                cylBoxRenderer.material = checkMaterial;
                break;
        }

        if( sphereData.edgesAlwaysOn )
        {
            showEdgesBoxRenderer.material = checkMaterial;
        }


        switch (sphereData.getMainRingCategory())
        {
            case SphereData.MainRingCategory.Distributor:
                distBoxRenderer.material = checkMaterial;
                break;
            case SphereData.MainRingCategory.Grouping:
                grpBoxRenderer.material = checkMaterial;
                break;
            case SphereData.MainRingCategory.Comic:
                comicBoxRenderer.material = checkMaterial;
                break;
            case SphereData.MainRingCategory.Publisher:
                pubBoxRenderer.material = checkMaterial;
                break;
            case SphereData.MainRingCategory.Studio:
                studioBoxRenderer.material = checkMaterial;
                break;
            case SphereData.MainRingCategory.Year:
                yearBoxRenderer.material = checkMaterial;
                break;
            default:
                break;
        }
    }

    public void updateOneStates(bool status)
    {
        animationBoxRenderer.material = (status) ? checkMaterial : boxMaterial;
    }

}
