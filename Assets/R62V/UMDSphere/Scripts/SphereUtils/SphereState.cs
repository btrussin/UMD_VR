using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//TODO: Use for SphereData in customizing the type of data on the ring
public class SphereState : MonoBehaviour {

    //TODO: Need to Derive from some variation of SphereState

    Material boxMaterial;
    Material checkMaterial;
    Material closeMaterial;

    public void bringUpMenu()
    {
        GameObject nodeMenu = new GameObject();
        nodeMenu.name = "Create spheres by:";
        nodeMenu.transform.SetParent(gameObject.transform);

        nodeMenu.AddComponent<CameraOrientedText3D>();

        List<GameObject> textObjects = new List<GameObject>();


        TextAlignment roleAlign = TextAlignment.Left;

        TextAnchor roleAnchor = TextAnchor.UpperLeft;

        float yOffsetPerLine = 0.02f;

        Vector3 offset = Vector3.zero;
        offset.y = -0.02f;
        offset.x = 0.04f;

        textObjects.Add(NodeState.addText(nodeMenu, "Distributor", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(NodeState.addText(nodeMenu, "Grouping", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(NodeState.addText(nodeMenu, "Comic", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;

        offset.x = 0.01f;
        textObjects.Add(NodeState.addText(nodeMenu, "Publisher", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine / 4.0f;
        textObjects.Add(NodeState.addText(nodeMenu, "Studio", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;
        textObjects.Add(NodeState.addText(nodeMenu, "Year", roleAlign, roleAnchor, offset));
        offset.y -= yOffsetPerLine;

        float firstBoxY = offset.y;

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

        plane.transform.SetParent(nodeMenu.transform);


        GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.name = "Close";
        quad1.layer = menuLayerMask;
        quad1.transform.SetParent(nodeMenu.transform);
        MeshRenderer qrend = quad1.GetComponent<MeshRenderer>();
        qrend.material = closeMaterial;
        quad1.transform.localScale = new Vector3(0.04f, 0.04f, 1.0f);
        quad1.transform.localPosition = new Vector3(xDim - 0.02f, -0.02f, 0.0f);

        quad1.AddComponent<NodeMenuHandler>();
        //quad1.GetComponent<NodeMenuHandler>().nodeState = this;
        quad1.GetComponent<NodeMenuHandler>().handlerType = NodeMenuHandler.NodeMenuHandlerType.CloseMenu;

        offset = Vector3.zero;
        offset.y = firstBoxY - 0.005f;
        offset.x = 0.02f;

        /*for (int i = 0; i < data.roles.Length; i++)
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
        */


        Vector3 ringCenter = gameObject.transform.parent.transform.position;
        Vector3 nodePosition = gameObject.transform.position;
        Vector3 dir = nodePosition - ringCenter;
        dir.Normalize();
        nodeMenu.transform.position = nodePosition + dir * 0.2f;

        nodeMenu.AddComponent<CameraOrientedText3D>();


    }

}
