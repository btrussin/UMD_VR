﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

public class NodeState : MonoBehaviour {

    int collisionCount = 0;

    public bool isSelected;

    Renderer nodeRend = null;
    MovieConnectionManager connManager = null;

    bool valuesNotSet = true;

    public Material ptMatOrig;
    public Material ptMatSelected;
    public Material ptMatCollision;

    public Material boxMaterial;
    public Material checkMaterial;
    public Material closeMaterial;

    GameObject nodeMenu;

    static int maxCharSize = 30;
    static int menuLayerMask = 0;

    //static List<GameObject> menus = new List<GameObject>();

    void Start () {
        isSelected = false;
        menuLayerMask = LayerMask.NameToLayer("Menus");
    }

	void Update () {
	}

    public void updateColor()
    {
        if (valuesNotSet)
        {
            Transform subTransform = gameObject.transform;

            GameObject node = subTransform.FindChild("MovieNode").gameObject;

            nodeRend = node.GetComponent<Renderer>();
            connManager = node.GetComponent<MovieConnectionManager>();

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
            if( !isSelected ) connManager.ForceClearAllConnections();
        }
    }

    public void toggleSelected()
    {
        isSelected = !isSelected;

        
        if (isSelected) bringUpMenu();
        else if (nodeMenu != null)
        {
            destroyMenu();  
        }
        
    }

    public void destroyMenu()
    {
        CMData data = gameObject.GetComponent<MovieObject>().cmData;
        string mKey = MovieDBUtils.getMovieDataKey(data);
        NodeDetailsManager.removeDetails(mKey);

        GameObject.Destroy(nodeMenu);
        nodeMenu = null;
    }

    public void bringUpMenu()
    {
        // clear out all other menus that may be present
        //foreach (GameObject obj in menus) GameObject.Destroy(obj);
        //menus.Clear();


        GameObject ptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/R62V/UMDSphere/Prefabs/MenuPlane.prefab");
        GameObject plane = (GameObject)Instantiate(ptPrefab);
        plane.layer = menuLayerMask;


        CMData data = gameObject.GetComponent<MovieObject>().cmData;

        string mKey = MovieDBUtils.getMovieDataKey(data);

        nodeMenu = new GameObject();
        nodeMenu.name = "Menu: " + mKey;
        nodeMenu.tag = "NodeMenu";

        //nodeMenu.AddComponent<CameraOrientedText3D>();
        nodeMenu.AddComponent<NodeMenuUtils>();
        NodeMenuUtils menuUtils = nodeMenu.GetComponent<NodeMenuUtils>();
        menuUtils.movieObject = gameObject.GetComponent<MovieObject>();
        FindObjectOfType<UserDataCollectionHandler>().RefreshMovieObject(menuUtils.movieObject);

        List<GameObject> textObjects = new List<GameObject>();

       
        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        float normalTextSize = 0.05f;
        float yOffsetPerLine = normalTextSize;

        Vector3 offset = new Vector3(0.3f, -0.3f, 0.0f);
        offset.y += -0.02f;

        //offset.x = 0.0f;

        if (mKey.Length > maxCharSize)
        {
            maxCharSize = mKey.Length;
        }

        if( mKey.Length > 15 )
        {
            int numLines;
            string tKey = MovieDBUtils.getWordWrappedString(mKey, 12, out numLines);

            textObjects.Add(addText(nodeMenu, tKey, roleAlign, roleAnchor, offset, 0.08f, false));

            offset.y -= 0.08f * numLines;
        }
        else
        {
            textObjects.Add(addText(nodeMenu, mKey, roleAlign, roleAnchor, offset, 0.08f, false));
            offset.y -= 0.08f;
        }

        
        //offset.x += 0.04f;

        textObjects.Add(addText(nodeMenu, "Distributor: " + data.distributor, roleAlign, roleAnchor, offset, normalTextSize, false));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(nodeMenu, "Comic: " + data.comic, roleAlign, roleAnchor, offset, normalTextSize, false));
        offset.y -= yOffsetPerLine;
        offset.y -= yOffsetPerLine;

        //offset.x = 0.01f;
        textObjects.Add(addText(nodeMenu, "Actors (Roles)", roleAlign, roleAnchor, offset, normalTextSize, false));
        offset.y -= yOffsetPerLine/4.0f;
        textObjects.Add(addText(nodeMenu, "______________", roleAlign, roleAnchor, offset, normalTextSize, false));
        offset.y -= yOffsetPerLine;

        float firstBoxY = offset.y;
        //offset.x = 0.04f;
        for ( int i = 0; i < data.roles.Length; i++ )
        {
            GameObject gObj = addText(nodeMenu, data.roles[i].actor + " (" + data.roles[i].name + ")", roleAlign, roleAnchor, offset, normalTextSize, true);
            //gObj.transform.localScale = Vector3.one * 0.1f;
            textObjects.Add(gObj);
            offset.y -= yOffsetPerLine;
        }



        float minX = float.MaxValue;
        float minY = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;

        bool passedFirstElement = false;

        foreach (GameObject obj in textObjects)
        {
            MeshRenderer rend = obj.GetComponent<MeshRenderer>();
            Vector3 min = rend.bounds.min;
            Vector3 max = rend.bounds.max;

            if (min.x < minX) minX = min.x;
            if (max.x > maxX) maxX = max.x;

            if (min.y < minY) minY = min.y;
            if (max.y > maxY) maxY = max.y;

            if (passedFirstElement) obj.transform.localScale = Vector3.one * 0.1f;

            passedFirstElement = true;
        }


        float xDim = (maxX - minX) + 0.1f;
        float yDim = (maxY - minY) + 0.04f;

        //Vector3 basePos = new Vector3(-xDim * 0.5f, yDim * 0.5f, 0.0f);
        Vector3 basePos = new Vector3(-xDim * 0.5f, 0.0f, 0f);

        foreach (GameObject obj in textObjects)
        {
            obj.transform.localPosition += basePos;
        }

        //plane.transform.localScale = new Vector3(xDim, yDim, 1.0f);
        //plane.transform.localPosition = basePos + new Vector3(xDim * 0.5f, yDim * -0.5f, 0.005f);

        plane.transform.localPosition = basePos + new Vector3(xDim, -yDim, 0.005f);


        //nodeMenu.transform.localScale = new Vector3(xDim, yDim, yDim);
        plane.transform.SetParent(nodeMenu.transform);      
        
        
        /*
        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.name = "Close: " + mKey;
        quad1.layer = menuLayerMask;
        quad1.transform.SetParent(nodeMenu.transform);
        MeshRenderer qrend = quad1.GetComponent<MeshRenderer>();
        qrend.material = closeMaterial;
        quad1.transform.localScale = new Vector3(0.04f, 0.04f, 1.0f);
        quad1.transform.localPosition = basePos + new Vector3(xDim -0.02f, -0.02f, 0.0f);

        quad1.AddComponent<NodeMenuHandler>();
        quad1.GetComponent<NodeMenuHandler>().nodeState = this;
        quad1.GetComponent<NodeMenuHandler>().handlerType = NodeMenuHandler.NodeMenuHandlerType.CloseMenu;
        */


        /*
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
            rend.transform.localPosition = basePos + offset;

            quad.AddComponent<NodeMenuHandler>();
            NodeMenuHandler menuHandler = quad.GetComponent<NodeMenuHandler>();
            menuHandler.nodeState = this;
            menuHandler.handlerType = NodeMenuHandler.NodeMenuHandlerType.ToggleActor;
            menuHandler.role = data.roles[i];

            menuHandler.boxMaterial = boxMaterial;
            menuHandler.checkMaterial = checkMaterial;

            quad.SetActive(false);

            menuHandler.UpdateMaterial();

            offset.y -= yOffsetPerLine;
        }
        */

        /*
        Vector3 ringCenter = gameObject.transform.parent.transform.position;
        Vector3 nodePosition = gameObject.transform.position;
        Vector3 dir = nodePosition - ringCenter;
        dir.Normalize();
        nodeMenu.transform.position = nodePosition + dir * 0.2f;
        */

        //nodeMenu.AddComponent<CameraOrientedText3D>();
        //menus.Add(nodeMenu);

        // nodeMenu.transform.localScale = new Vector3(xDim, yDim, yDim);

        // set the x-y dimensions
        menuUtils.xDimension = xDim;
        menuUtils.yDimension = yDim;
        nodeMenu.layer = menuLayerMask;
        

        NodeDetailsManager.addDetails(mKey, nodeMenu);


    }

    static GameObject addText(GameObject obj, string text, TextAlignment alignment, TextAnchor anchor, Vector3 offset, float charSize = 0.05f, bool isActor = true)
    {
        GameObject textObj = new GameObject();
        textObj.transform.SetParent(obj.transform);
        textObj.AddComponent<MeshRenderer>();
        textObj.AddComponent<TextMesh>();
        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        textMesh.anchor = anchor;
        textMesh.alignment = alignment;
        textMesh.text = text;
        textMesh.characterSize = charSize;
        textMesh.fontSize = 100;

        if(isActor)
        {
            textObj.name = "Actor: " + text;
            textObj.AddComponent<BoxCollider>();
            textObj.layer = menuLayerMask;
        }
        else
        {
            textObj.name = "Text: " + text;
        }

        

        float scale = charSize;
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
}
