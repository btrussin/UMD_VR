using UnityEngine;
using System.Collections;
using UnityEditor;

//And class which needs menu will extend from this class.
//Reason for needing more than one class is that a menu for the node
//isn't the same as a menu that is on startup.
public class BaseState : MonoBehaviour {

    public Material ptMatOrig;
    public Material ptMatSelected;
    public Material ptMatCollision;

    public Material boxMaterial;
    public Material checkMaterial;
    public Material closeMaterial;

    protected bool isSelected;
    protected GameObject menu;

    protected int collisionCount = 0;
    protected MovieConnectionManager connManager = null;
    protected Renderer nodeRend = null;
    protected TextMesh tMesh = null;
    protected bool valuesNotSet = true;


    void Start()
    {
        ptMatOrig = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/PointMaterial.mat");
        ptMatSelected = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/PointMaterialRed.mat");
        ptMatCollision = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/PointMaterialYellow.mat");

        boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
        closeMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/close_mat.mat");
    }

    public void destroyMenu()
    {
        GameObject.Destroy(menu);
        menu = null;
    }

    //TODO: May need to make this virtual
    public void toggleSelected()
    {
        isSelected = !isSelected;

        if (isSelected) bringUpMenu();

        else if (menu != null)
        {
            destroyMenu();
        }
    }

    public void setSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool getIsSelected()
    {
        return isSelected;
    }

    public static GameObject addText(GameObject obj, string text, TextAlignment alignment, TextAnchor anchor, Vector3 offset)
    {
        GameObject textObj = new GameObject();
        textObj.transform.SetParent(obj.transform);
        textObj.AddComponent<MeshRenderer>();
        textObj.AddComponent<TextMesh>();
        TextMesh ringText = textObj.GetComponent<TextMesh>();
        ringText.anchor = anchor;
        ringText.alignment = alignment;
        ringText.text = text;
        ringText.characterSize = 0.03f;
        ringText.fontSize = 100;

        float scale = 0.03f;
        textObj.transform.localScale = new Vector3(scale, scale, scale);
        textObj.transform.localPosition = offset;

        return textObj;
    }

    public int getConnectionCount()
    {
        return collisionCount;
    }

    public void addCollision()
    {
        collisionCount++;
    }

    public void removeCollision()
    {
        collisionCount--;
        if (collisionCount < 0) collisionCount = 0;

        if (collisionCount == 0)
        {
            if (!isSelected) connManager.forceClearAllConnections();
        }
    }

    public virtual void updateColor()
    {
 
    }

    public virtual void bringUpMenu()
    {
        //Overriden in Derived Classes
    }

}
