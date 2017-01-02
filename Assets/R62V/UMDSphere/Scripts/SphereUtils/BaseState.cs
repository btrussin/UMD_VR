using UnityEngine;
using System.Collections;

//And class which needs menu will extend from this class.
//Reason for needing more than one class is that a menu for the node
//isn't the same as a menu that is on startup.
public class BaseState : MonoBehaviour {

    protected bool isSelected;
    protected GameObject nodeMenu;

    public void destroyMenu()
    {
        GameObject.Destroy(nodeMenu);
        nodeMenu = null;
    }

    //TODO: May need to make this 
    public void toggleSelected()
    {
        isSelected = !isSelected;

        if (isSelected) bringUpMenu();

        else if (nodeMenu != null)
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

    public virtual void bringUpMenu()
    {
        //Overriden in Derived Classes
    }

}
