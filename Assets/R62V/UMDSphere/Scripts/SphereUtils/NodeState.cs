using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class NodeState : BaseState
{

    static List<GameObject> menus = new List<GameObject>();

    uint _currentLevel;
    float animationTime = 1.5f;
	const int MAX_LEVEL = 2;

    Transform _referenceLine;
    Vector3 startLoc;
    Vector3 endLoc;

	void Update () {
	
	}

    public override void updateColor()
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

    public override void bringUpMenu()
    {
        // clear out all other menus that may be present
        foreach (GameObject obj in menus) GameObject.Destroy(obj);

        menus.Clear();

        CMData data = gameObject.GetComponent<MovieObject>().cmData;

        string mKey = MovieDBUtils.getMovieDataKey(data);

        menu = new GameObject();
        menu.name = "Menu: " + mKey;
        menu.transform.SetParent(gameObject.transform);

        menu.AddComponent<CameraOrientedText3D>();

        List<GameObject> textObjects = new List<GameObject>();

       
        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        float yOffsetPerLine = 0.02f;

        Vector3 offset = Vector3.zero;
        offset.y = -0.02f;
        offset.x = 0.04f;

        textObjects.Add(addText(menu, "Movie: " + mKey, roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Distributor: " + data.distributor, roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Comic: " + data.comic, roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        offset.y -= yOffsetPerLine;

        offset.x = 0.01f;
        textObjects.Add(addText(menu, "Actors (Roles)", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine/4.0f;
        textObjects.Add(addText(menu, "______________", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;

        float firstBoxY = offset.y;
        offset.x = 0.04f;
        for ( int i = 0; i < data.roles.Length; i++ )
        {
            textObjects.Add(addText(menu, data.roles[i].actor + " (" + data.roles[i].name + ")", roleAlign, roleAnchor, offset));
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

        plane.transform.SetParent(menu.transform);

        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.name = "Close: " + mKey;
        quad1.layer = menuLayerMask;
        quad1.transform.SetParent(menu.transform);
        MeshRenderer qrend = quad1.GetComponent<MeshRenderer>();
        qrend.material = closeMaterial;
        quad1.transform.localScale = new Vector3(0.04f, 0.04f, 1.0f);
        quad1.transform.localPosition = new Vector3(xDim-0.02f, -0.02f, 0.0f);

        quad1.AddComponent<NodeMenuHandler>();
        quad1.GetComponent<NodeMenuHandler>().baseState = this;
        quad1.GetComponent<NodeMenuHandler>().handlerType = BaseMenuHandler.BaseMenuHandlerType.CloseMenu;

        offset = Vector3.zero;
        offset.y = firstBoxY - 0.005f;
        offset.x = 0.02f;

        for (int i = 0; i < data.roles.Length; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Toggle: " + data.roles[i].actor;
            quad.layer = menuLayerMask;
            quad.transform.SetParent(menu.transform);
            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            rend.material = checkMaterial;
            rend.transform.localScale = new Vector3(0.02f, 0.02f, 1.0f);
            rend.transform.localPosition = offset;

            quad.AddComponent<NodeMenuHandler>();
            NodeMenuHandler menuHandler = quad.GetComponent<NodeMenuHandler>();
            menuHandler.baseState = this;
            menuHandler.handlerType = BaseMenuHandler.BaseMenuHandlerType.ToggleOption;
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
        menu.transform.position = nodePosition + dir * 0.2f;

        menu.AddComponent<CameraOrientedText3D>();

        menus.Add(menu);

    }

	/*--------Mike - Level Expansion In Progress------*/
	public void Expand() {
		SummonRing ();

		_currentLevel++;
        Debug.Log("Current Level: " + _currentLevel);
	}

	public void Contract() {
		DestroyRing ();

		_currentLevel--;
	}

	private void SummonRing() {	
        //TODO: May have to add boolean to not spam coroutines to be called
		 StartCoroutine (ExpandRingAnimation(animationTime));
	}

	private IEnumerator ExpandRingAnimation(float secondsToComplete) {
        float t = 0f;

        _referenceLine = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        _referenceLine.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        _referenceLine.GetComponent<LineRenderer>().material.color = MovieDBUtils.getColorPalette()[1];
        _referenceLine.parent = transform;

        while (t < 1.0f)
        {
            transform.localPosition = Vector3.Lerp(startLoc, endLoc, t);
            transform.localScale = Vector3.Lerp(Vector3.one * .015f, Vector3.one * .5f, t / 1f);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

    }

	private void DestroyRing() {
        //TODO: May have to add boolean to not spam coroutines to be called
        StartCoroutine(DiminishRingAnimation(animationTime));
	}

	private IEnumerator DiminishRingAnimation(float secondsToComplete) {
        float t = 0f;

        while (t < 1.0f)
        {
            transform.localPosition = Vector3.Lerp(endLoc, startLoc, t);
            transform.localScale = Vector3.Lerp(Vector3.one * .015f, Vector3.one * .5f, t / 1f);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (_referenceLine)
            Destroy(_referenceLine.gameObject);
        _referenceLine = null;
    }

}
