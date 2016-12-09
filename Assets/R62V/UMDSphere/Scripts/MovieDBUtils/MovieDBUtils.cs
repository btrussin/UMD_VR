﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovieDBUtils : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static Color[] randomizeColorPalette(Color[] orig, int seed = 44356)
    {
        Random.InitState(seed);
        int size = orig.Length;
        Color[] copyPalette = new Color[size];
        Color[] diffPalette = new Color[size];

        for (int i = 0; i < size; i++) copyPalette[i] = orig[i];

        for ( int i = 0; i < size; i++ )
        {
            // compute random index
            float r = Random.value;
            int idx = (int)( r * (size-i-1));
            diffPalette[i] = copyPalette[idx];

            // overwrite the color just placed with the end value
            copyPalette[idx] = copyPalette[size - i - 1];
        }

        return diffPalette;

    }

    public static Color[] getColorPalette()
    {
        Color[] palette = new Color[24];

        int i = 0;
        palette[i++] = new Color32(102, 255, 255, 255);
        palette[i++] = new Color32(153, 255, 102, 255);
        palette[i++] = new Color32( 81, 194,   1, 255);
        palette[i++] = new Color32( 60, 208, 112, 255);
        palette[i++] = new Color32(133, 187, 101, 255);
        palette[i++] = new Color32(126, 145,  86, 255);
        palette[i++] = new Color32( 33, 171, 205, 255);
        palette[i++] = new Color32(  0, 134, 167, 255);
        palette[i++] = new Color32(153, 204, 255, 255);
        palette[i++] = new Color32(255,   0, 255, 255);
        palette[i++] = new Color32(188, 108, 172, 255);
        palette[i++] = new Color32(153, 102, 204, 255);
        palette[i++] = new Color32(153, 153, 255, 255);
        palette[i++] = new Color32(153, 102, 255, 255);
        palette[i++] = new Color32(255,   0, 153, 255);
        palette[i++] = new Color32(255, 151,   0, 255);
        palette[i++] = new Color32(255, 204, 153, 255);
        palette[i++] = new Color32(191, 106,  31, 255);
        palette[i++] = new Color32(255, 128,   0, 255);
        palette[i++] = new Color32(185, 150, 133, 255);
        palette[i++] = new Color32(255, 255, 153, 255);
        palette[i++] = new Color32(167, 139,   0, 255);
        palette[i++] = new Color32(226, 182,  49, 255);
        palette[i++] = new Color32(255, 255,  51, 255);


        return palette;
    }

    public static string getMovieDataKey(CMData data)
    {
        return data.movie + "(" + data.year + ")";
    }

    public static Vector3[] getBezierPoints(Vector3[] basePts, int size)
    {
        float h = 1.0f / (float)(size - 1);
        float h_2 = h * h;

        //          A0                   B0              C0       D0
        // t^3(-p0+3p1-3p2+p3) + t^2(3p0-6p1+3p2) + t(-3p0+3p1) + p0
        Vector3 A0 = basePts[0] * -1.0f + basePts[1] * 3.0f + basePts[2] * -3.0f + basePts[3];
        Vector3 B0 = basePts[0] * 3.0f + basePts[1] * -6.0f + basePts[2] * 3.0f;
        Vector3 C0 = basePts[0] * -3.0f + basePts[1] * 3.0f;

        //      A1            B1               C1
        // t^2(3A0h) + t(3A0h^2+2B0h) + (A0h^3+B0h^2+C0h)
        Vector3 A1 = A0 * 3.0f * h;
        Vector3 B1 = A0 * 3.0f * h_2 + B0 * 2.0f * h;
        Vector3 C1 = A0 * h * h_2 + B0 * h_2 + C0 * h;

        //    A2          B2
        // t(2A1h) + (A1h^2+B1h)
        Vector3 A2 = A1 * 2.0f * h;
        Vector3 B2 = A1 * h_2 + B1 * h;

        //  A3
        // (A2h)
        Vector3 A3 = A2 * h;


        // D1 = C1
        Vector3 D1 = C1;

        // D2 = B2
        Vector3 D2 = B2;

        // D3 = A3
        Vector3 D3 = A3;

        Vector3[] pts = new Vector3[size];
        pts[0] = basePts[0];


        for (int i = 1; i < size; i++)
        {
            pts[i] = pts[i - 1] + D1;
            D1 += D2;
            D2 += D3;
        }

        return pts;
    }



    public static void addMeshFilter(GameObject g, Material mat)
    {

        int numSegments = 60;
        Quaternion quat = Quaternion.Euler(new Vector3(0.0f, 0.0f, 360.0f / numSegments));


        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();


        Vector3 baseVec;

        Vector3 vOffset = new Vector3(0.0f, 0.0f, 0.0025f);

        Vector3 v1;
        Vector3 v2;
        Vector3 n1;

        for (int j = 0; j < 2; j++)
        {
            baseVec = Vector3.right * 0.5f;
            for (int i = 0; i <= numSegments; i++)
            {
                v1 = baseVec - vOffset;
                v2 = v1 + vOffset;
                n1 = baseVec;
                n1.Normalize();
                verts.Add(v1);
                verts.Add(v2);
                if (j == 1) n1 = n1 * -1.0f;
                normals.Add(n1);
                normals.Add(n1);
                baseVec = quat * baseVec;
            }
        }

        int[] tris = new int[numSegments * 12];

        int idx = 0;
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < numSegments; i++)
            {
                int baseIdx = i * 2;
                if (j == 1)
                {
                    baseIdx += (numSegments + 1) * 2;
                    tris[idx++] = baseIdx;
                    tris[idx++] = baseIdx + 1;
                    tris[idx++] = baseIdx + 3;

                    tris[idx++] = baseIdx;
                    tris[idx++] = baseIdx + 3;
                    tris[idx++] = baseIdx + 2;
                }
                else
                {
                    tris[idx++] = baseIdx;
                    tris[idx++] = baseIdx + 3;
                    tris[idx++] = baseIdx + 1;

                    tris[idx++] = baseIdx;
                    tris[idx++] = baseIdx + 2;
                    tris[idx++] = baseIdx + 3;
                }


            }
        }

        g.AddComponent<MeshFilter>();
        g.AddComponent<MeshRenderer>();


        MeshRenderer rend = g.GetComponent<MeshRenderer>();
        rend.material = mat;

        Mesh m = g.GetComponent<MeshFilter>().mesh;
        m.SetVertices(verts);
        m.SetNormals(normals);
        m.SetTriangles(tris, 0);



    }
}