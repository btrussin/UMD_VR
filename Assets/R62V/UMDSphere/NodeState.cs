using UnityEngine;
using UnityEditor;
using System.Collections;

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
