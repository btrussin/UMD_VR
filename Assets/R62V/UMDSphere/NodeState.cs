using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class NodeState : MonoBehaviour {

    int collisionCount = 0;

    bool isSelected;

    Renderer nodeRend = null;
    TextMesh tMesh = null;
    MovieConnectionManager connManager = null;

    bool valuesNotSet = true;

    Material ptMatOrig;
    Material ptMatSelected;
    Material ptMatCollision;

    GameObject nodeMenu;

    static List<GameObject> menus = new List<GameObject>();

    // Use this for initialization
    void Start () {
        isSelected = false;

        ptMatOrig = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/PointMaterial.mat");
        ptMatSelected = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/PointMaterialSelected.mat");
        ptMatCollision = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/PointMaterialCollision.mat");
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void updateColor()
    {
        if (valuesNotSet)
        {
            Transform subTransform = gameObject.transform;

            GameObject node = subTransform.FindChild("MovieNode").gameObject;
            GameObject label = subTransform.FindChild("MovieLabel").gameObject;

            nodeRend = node.GetComponent<Renderer>();
            connManager = node.GetComponent<MovieConnectionManager>();
            tMesh = label.GetComponent<TextMesh>();

            valuesNotSet = false;
        }

        if (isSelected)
        {
            nodeRend.material = ptMatSelected;
        }
        else if (collisionCount > 0)
        {
            nodeRend.material = ptMatCollision;
        }
        else
        {
            nodeRend.material = ptMatOrig;
        }

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

        if( collisionCount == 0 )
        {
            if( !isSelected ) connManager.forceClearAllConnections();
        }
    }

    public void toggleSelected()
    {
        isSelected = !isSelected;

        if (isSelected) bringUpMenu();
        else if (nodeMenu != null)
        {
            GameObject.Destroy(nodeMenu);
            nodeMenu = null;   
        }
    }

    public void bringUpMenu()
    {
        // clear out all other menus that may be present
        foreach (GameObject obj in menus) GameObject.Destroy(obj);

        menus.Clear();


        CMData data = gameObject.GetComponent<MovieObject>().cmData;

        string mKey = MovieDBUtils.getMovieDataKey(data);

        nodeMenu = new GameObject();
        nodeMenu.name = "Menu: " + mKey;
        nodeMenu.transform.SetParent(gameObject.transform);

        List<GameObject> textObjects = new List<GameObject>();

       
        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        Vector3 offset = Vector3.zero;

        int txtIdx = 3;

        textObjects.Add(addText(nodeMenu, "Actors (Roles)", txtIdx++, roleAlign, roleAnchor, offset));
        textObjects.Add(addText(nodeMenu, "______________", txtIdx++, roleAlign, roleAnchor, offset));
        for ( int i = 0; i < data.roles.Length; i++ )
        {
            textObjects.Add(addText(nodeMenu, data.roles[i].actor + " (" + data.roles[i].name + ")", txtIdx++, roleAlign, roleAnchor, offset));
        }

 
        float maxMidX = float.MinValue;

        foreach (GameObject obj in textObjects)
        {
            MeshRenderer rend = obj.GetComponent<MeshRenderer>();
            float tx = rend.bounds.center.x;
            if (tx > maxMidX) maxMidX = tx;
        }

        offset.x = 0.02f;

        textObjects.Add(addText(nodeMenu, "Movie: " + mKey, 0, roleAlign, roleAnchor, offset));
        textObjects.Add(addText(nodeMenu, "Distributor: " + data.distributor, 1, roleAlign, roleAnchor, offset));
        textObjects.Add(addText(nodeMenu, "Comic: " + data.comic, 2, roleAlign, roleAnchor, offset));


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


     

        GameObject ptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/R62V/UMDSphere/MenuPlane.prefab");
        GameObject plane = (GameObject)Instantiate(ptPrefab);

        float xDim = (maxX - minX);
        float yDim = (maxY - minY);




        plane.transform.localScale = new Vector3(xDim + 0.1f, yDim + 0.04f, 1.0f);
        plane.transform.localPosition = new Vector3(-0.02f, 0.02f, 0.0f);

        plane.transform.SetParent(nodeMenu.transform);






        Vector3 ringCenter = gameObject.transform.parent.transform.position;
        Vector3 nodePosition = gameObject.transform.position;
        Vector3 dir = nodePosition - ringCenter;
        dir.Normalize();
        nodeMenu.transform.position = nodePosition + dir * 0.2f;

        nodeMenu.AddComponent<CameraOrientedText3D>();

        menus.Add(nodeMenu);


        


    }

    static GameObject addText(GameObject obj, string text, int lineNum, TextAlignment alignment, TextAnchor anchor, Vector3 offset)
    {
        GameObject textObj = new GameObject();
        textObj.transform.SetParent(obj.transform);
        textObj.AddComponent<MeshRenderer>();
        textObj.AddComponent<TextMesh>();
        textObj.AddComponent<CameraOrientedText3D>();
        TextMesh ringText = textObj.GetComponent<TextMesh>();
        ringText.anchor = anchor;
        ringText.alignment = alignment;
        ringText.text = text;
        ringText.characterSize = 0.03f;
        ringText.fontSize = 100;
        //ringText.offsetZ = -2.0f;

        float scale = 0.03f;
        textObj.transform.localScale = new Vector3(scale, scale, scale);
        textObj.transform.localPosition = Vector3.down * (lineNum * 0.03f) + offset;

        textObj.AddComponent<CameraOrientedText3D>();
        return textObj;
    }

    public void setSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool getIsSelected()
    {
        return isSelected;
    }
}
