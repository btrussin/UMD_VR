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

    Material boxMaterial;
    Material checkMaterial;
    Material closeMaterial;

    GameObject nodeMenu;

    static List<GameObject> menus = new List<GameObject>();
	static uint currentLevel;

	float animationTime = 1.5f;
	const int MAX_LEVEL = 2;

    void Start () {
        isSelected = false;

        ptMatOrig = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/PointMaterial.mat");
        ptMatSelected = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/PointMaterialRed.mat");
        ptMatCollision = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/PointMaterialYellow.mat");

        boxMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/box_mat.mat");
        checkMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/check_mat.mat");
        closeMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/R62V/UMDSphere/Materials/close_mat.mat");
    }

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

		if (isSelected && currentLevel == MAX_LEVEL) bringUpMenu();
        else if (nodeMenu != null)
        {
            destroyMenu();  
        }
    }

    public void destroyMenu()
    {
        GameObject.Destroy(nodeMenu);
        nodeMenu = null;
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

        nodeMenu.AddComponent<CameraOrientedText3D>();

        List<GameObject> textObjects = new List<GameObject>();

       
        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        float yOffsetPerLine = 0.02f;

        Vector3 offset = Vector3.zero;
        offset.y = -0.02f;
        offset.x = 0.04f;

        textObjects.Add(addText(nodeMenu, "Movie: " + mKey, roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(nodeMenu, "Distributor: " + data.distributor, roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(nodeMenu, "Comic: " + data.comic, roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        offset.y -= yOffsetPerLine;

        offset.x = 0.01f;
        textObjects.Add(addText(nodeMenu, "Actors (Roles)", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine/4.0f;
        textObjects.Add(addText(nodeMenu, "______________", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;

        float firstBoxY = offset.y;
        offset.x = 0.04f;
        for ( int i = 0; i < data.roles.Length; i++ )
        {
            textObjects.Add(addText(nodeMenu, data.roles[i].actor + " (" + data.roles[i].name + ")", roleAlign, roleAnchor, offset));
            offset.y -= yOffsetPerLine;
        }



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


        int menuLayerMask = LayerMask.NameToLayer("Menus");

        GameObject ptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/R62V/UMDSphere/Prefabs/MenuPlane.prefab");
        GameObject plane = (GameObject)Instantiate(ptPrefab);

        float xDim = (maxX - minX) + 0.1f;
        float yDim = (maxY - minY) + 0.04f;


        plane.transform.localScale = new Vector3(xDim, yDim, 1.0f);
        plane.transform.localPosition = new Vector3(xDim*0.5f, yDim * -0.5f, 0.0f);

        plane.transform.SetParent(nodeMenu.transform);


        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.name = "Close: " + mKey;
        quad1.layer = menuLayerMask;
        quad1.transform.SetParent(nodeMenu.transform);
        MeshRenderer qrend = quad1.GetComponent<MeshRenderer>();
        qrend.material = closeMaterial;
        quad1.transform.localScale = new Vector3(0.04f, 0.04f, 1.0f);
        quad1.transform.localPosition = new Vector3(xDim-0.02f, -0.02f, 0.0f);

        quad1.AddComponent<NodeMenuHandler>();
        quad1.GetComponent<NodeMenuHandler>().nodeState = this;
        quad1.GetComponent<NodeMenuHandler>().handlerType = NodeMenuHandler.NodeMenuHandlerType.CloseMenu;

        offset = Vector3.zero;
        offset.y = firstBoxY - 0.005f;
        offset.x = 0.02f;

        for (int i = 0; i < data.roles.Length; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Toggle: " + data.roles[i].actor;
            quad.layer = menuLayerMask;
            quad.transform.SetParent(nodeMenu.transform);
            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            rend.material = checkMaterial;
            rend.transform.localScale = new Vector3(0.02f, 0.02f, 1.0f);
            rend.transform.localPosition = offset;

            quad.AddComponent<NodeMenuHandler>();
            NodeMenuHandler menuHandler = quad.GetComponent<NodeMenuHandler>();
            menuHandler.nodeState = this;
            menuHandler.handlerType = NodeMenuHandler.NodeMenuHandlerType.ToggleActor;
            menuHandler.role = data.roles[i];
            menuHandler.boxMaterial = boxMaterial;
            menuHandler.checkMaterial = checkMaterial;

            menuHandler.UpdateMaterial();

            offset.y -= yOffsetPerLine;
        }



        Vector3 ringCenter = gameObject.transform.parent.transform.position;
        Vector3 nodePosition = gameObject.transform.position;
        Vector3 dir = nodePosition - ringCenter;
        dir.Normalize();
        nodeMenu.transform.position = nodePosition + dir * 0.2f;

        nodeMenu.AddComponent<CameraOrientedText3D>();

        menus.Add(nodeMenu);


        


    }

    static GameObject addText(GameObject obj, string text, TextAlignment alignment, TextAnchor anchor, Vector3 offset)
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

    public void setSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool getIsSelected()
    {
        return isSelected;
    }

	/*--------Mike - Level Expansion------*/
	public void expand() {
		summonRing ();

		currentLevel++;
	}

	public void contract() {
		destroyRing ();

		currentLevel--;
	}

	private void summonRing() {	
		StartCoroutine (ExpandRingAnimation(animationTime));
	}

	private IEnumerator ExpandRingAnimation(float secondsToComplete) {

	}

	private void destroyRing() {	
		StartCoroutine (DiminishRing(animationTime));
	}

	private IEnumerator DiminishRing(float secondsToComplete) {

	}

}
