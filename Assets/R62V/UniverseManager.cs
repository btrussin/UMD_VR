using UnityEngine;
using UnityEditor;
using System.Collections;

public class UniverseManager : MonoBehaviour
{

    // Use this for initialization


    void Start()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/R62V/MainBall.prefab");
        // from -0.5 to 0.5

        Vector3 max = new Vector3(0, 0, 0);
        Vector3 min = new Vector3(0, 0, 0);
        BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();


        float xDim = 1.0f;
        float yDim = 1.0f;

        int numBallsX = 10;
        int numBallsY = 10;

        float xInc = xDim / (numBallsX - 1);
        float yInc = yDim / (numBallsY - 1);

        float xStart = -xDim * 0.5f;
        float yStart = -yDim * 0.5f;
        float xCoord, yCoord;
        float zCoord = -0.0f;

        xCoord = xStart;

        int count = 0;

        MeshCollider ballCollider = null;

        int ballLayerMask = LayerMask.NameToLayer("BallGraph");

        for (int x = 0; x < numBallsX; x++, xCoord += xInc)
        {
            yCoord = yStart;
            for (int y = 0; y < numBallsY; y++, yCoord += yInc)
            {
                GameObject ball = (GameObject)Instantiate(prefab);

                ball.name = this.gameObject.name + ":" + x + "|" + y;

                Vector3 v = new Vector3(xCoord, yCoord, zCoord);
                ball.transform.position = this.gameObject.transform.position + v;
                ball.transform.parent = this.gameObject.transform;
                count++;

                ball.layer = ballLayerMask;


                ballCollider = ball.GetComponent<MeshCollider>();

                if (ballCollider != null)
                {

                    //Vector3 val = cubeCollider.bounds.extents + cubeCollider.bounds.center;
                    Vector3 val = ballCollider.bounds.extents;
                    max.x = Mathf.Max(max.x, val.x);
                    max.y = Mathf.Max(max.y, val.y);
                    max.z = Mathf.Max(max.z, val.z);

                    min.x = Mathf.Min(min.x, val.x);
                    min.y = Mathf.Min(min.y, val.y);
                    min.z = Mathf.Min(min.z, val.z);
                }

            }
        }

        if(ballCollider != null ) collider.size = new Vector3(xDim, yDim, ballCollider.bounds.extents.z );
        else collider.size = new Vector3(xDim, yDim, max.z-min.z);

        collider.center = new Vector3(0.0f, 0.0f, zCoord);

       

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            translate(0, 0, 0.2f);
        }
    }

    void translate(Vector3 vec)
    {
        this.gameObject.transform.position += vec;
    }

    void translate(float x, float y, float z)
    {
        translate(new Vector3(x, y, z));
    }
}
