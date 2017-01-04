using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//TODO: Use withing a Player Class in customizing the type of data on the ring
public class ControllerState : BaseState
{
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
        else {
            nodeRend.material = ptMatOrig;
        }

    }

    public override void bringUpMenu()
    {
        menu = new GameObject();
        menu.name = "Sphere Type Menu";
        menu.transform.SetParent(gameObject.transform);

        menu.AddComponent<CameraOrientedText3D>();

        List<GameObject> textObjects = new List<GameObject>();


        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        float yOffsetPerLine = 0.02f;

        Vector3 offset = Vector3.zero;
        offset.y = -0.02f;
        offset.x = 0.04f;

        textObjects.Add(addText(menu, "Distributor", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Grouping", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Comic", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Publisher", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Studio", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(addText(menu, "Year", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;

        float firstBoxY = -0.02f;

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
        plane.transform.localPosition = new Vector3(xDim * 0.5f, yDim * -0.5f, 0.0f);

        plane.transform.SetParent(menu.transform);


        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad1.GetComponent<MeshCollider>());
        quad1.AddComponent<BoxCollider>();

        quad1.name = "Close";
        quad1.layer = menuLayerMask;
        quad1.transform.SetParent(menu.transform);
        MeshRenderer qrend = quad1.GetComponent<MeshRenderer>();
        qrend.material = closeMaterial;
        quad1.transform.localScale = new Vector3(0.04f, 0.04f, 1.0f);
        quad1.transform.localPosition = new Vector3(xDim - 0.02f, -0.02f, 0.0f);

        quad1.AddComponent<ControllerMenuHandler>();
        quad1.GetComponent<ControllerMenuHandler>().baseState = this;
        quad1.GetComponent<ControllerMenuHandler>().handlerType = BaseMenuHandler.BaseMenuHandlerType.CloseMenu;

        offset = Vector3.zero;
        offset.y = firstBoxY - 0.005f;
        offset.x = 0.02f;

        for (int layoutInd = 0; layoutInd < SphereData.NUM_LAYOUTS; layoutInd++)
        {
            ControllerMenuHandler.RingLayoutState ringlayoutState = (ControllerMenuHandler.RingLayoutState) layoutInd;

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<MeshCollider>());
            quad.AddComponent<BoxCollider>();

            quad.name = "Toggle Option: " + ringlayoutState;
            quad.layer = menuLayerMask;
            quad.transform.SetParent(menu.transform);
            MeshRenderer rend = quad.GetComponent<MeshRenderer>();
            rend.material = checkMaterial;
            rend.transform.localScale = new Vector3(0.02f, 0.02f, 1.0f);
            rend.transform.localPosition = offset;

            quad.AddComponent<ControllerMenuHandler>();
            ControllerMenuHandler menuHandler = quad.GetComponent<ControllerMenuHandler>();
            menuHandler.baseState = this;
            menuHandler.handlerType = NodeMenuHandler.BaseMenuHandlerType.ToggleOption;
            menuHandler.ringLayoutState = ringlayoutState;
            menuHandler.boxMaterial = boxMaterial;
            menuHandler.checkMaterial = checkMaterial;

            menuHandler.UpdateMaterial();

            offset.y -= yOffsetPerLine;
        }

        Vector3 controllerPosition = transform.position;
        Vector3 userPosition = GameObject.Find("Camera (eye)").transform.position;
        Vector3 dir = controllerPosition - userPosition;
        dir.Normalize();
        menu.transform.position = userPosition + dir;

        menu.AddComponent<CameraOrientedText3D>();
    }

}
