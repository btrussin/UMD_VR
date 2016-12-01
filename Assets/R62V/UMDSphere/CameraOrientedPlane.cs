using UnityEngine;
using System.Collections;

public class CameraOrientedPlane : MonoBehaviour
{

    //public Camera mainCamera;

    // Use this for initialization
    void Start()
    {
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();

        mf.mesh.Clear();

        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(0.0f, 0.0f, 0.0f);
        verts[1] = new Vector3(0.0f, -1.0f, 0.0f);
        verts[2] = new Vector3(1.0f, -1.0f, 0.0f);
        verts[3] = new Vector3(1.0f, 0.0f, 0.0f);

        Vector3[] norms = new Vector3[4];
        for( int i = 0; i < 4; i++ ) norms[i] = new Vector3(0.0f, 0.0f, -1.0f);

        int[] tris = new int[6];
        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;
        tris[3] = 0;
        tris[4] = 2;
        tris[5] = 3;

        mf.mesh.vertices = verts;
        mf.mesh.normals = norms;
        mf.mesh.triangles = tris;


    }

    // Update is called once per frame
    void Update()
    {
        

    }
}