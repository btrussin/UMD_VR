﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovieDBUtils {

 
    public static string getMovieDataKey(CMData data)
    {
        return data.movie + " (" + data.year + ")";
    }

    public static string getWordWrappedString(string origStr, int linePartLength, out int numLines)
    {
        char[] splitParams = { ' ' };
        string[] origParts = origStr.Split(splitParams);

        List<string> resultLines = new List<string>();
        string tName = origParts[0];

        for (int i = 1; i < origParts.Length; i++)
        {
            if (tName.Length >= linePartLength)
            {
                resultLines.Add(tName);
                tName = origParts[i];
            }
            else
            {
                tName += " " + origParts[i];
            }
        }

        if (tName.Length > 0) resultLines.Add(tName);

        string result = "";

        foreach (string s in resultLines)
        {
            if (result.Length == 0) result = s;
            else result += "\n" + s;
        }

        numLines = resultLines.Count;
        return result;
    }

    // use forward-differencing to calculate bezier points
    public static Vector3[] getBezierPoints(Vector3[] basePts, int size, float bundlingStrength)
    {
        //TODO Could possibly use bundling with the control points A0, B0, C0, D0 and then with future control points

        //P0' = BS * P0 + (1 - BS) * (P0 + 0/(N - 1) * (P(N-1) - P0))
        //P1' = BS * P1 + (1 - BS) * (P0 + 1/(N - 1) * (P(N-1) - P0))

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

        if( bundlingStrength < 0f || bundlingStrength >= 1f )
        {
            return pts;
        }

        //Straightening a spline curve. Got this information from research paper.
        int lastIndexedControlPoint = size - 1;

        // adding constant terms for optimization


        // defn: B = bundlingStrength
        // defn: Bc = 1-B (B-compliment)
        // defn: L = lastIndexedControlPoint
        // defn: pi = pts[i]
        // defn: p0 = pts[0]
        // defn: pN = pts[lastIndexedControlPoint]
        // pi = B*pi + Bc*[p0 + i/L*(pN-p0)]
        //    = B*pi + Bc*p0 + i*Bc/L*(pN-p0)

        // defn: cnstVec1 = Bc*p0
        // defn: cnstVec2 = Bc/L*(pN-p0)

        // final: pi = B*pi + cnstVec1 + i*cnstVec2

        Vector3 cnstVec1 = (1 - bundlingStrength) * pts[0];
        Vector3 cnstVec2 = (pts[lastIndexedControlPoint] - pts[0]) * (1 - bundlingStrength) / lastIndexedControlPoint;

        // however, since cnstVec2 just increments by integers 0, 1, 2, ..., size-1; we can start with a zero-vector 
        // and just add cnstVec2 to it every iteration
        Vector3 incrementedVec = Vector3.zero;


        for (int i = 0; i < size; i++)
        {
            //pts[i] = bundlingStrength * pts[i] +
            //             (1 - bundlingStrength) * (pts[0] + (float)i / lastIndexedControlPoint
            //                                       * (pts[lastIndexedControlPoint] - pts[0]));

            pts[i] = bundlingStrength * pts[i] + cnstVec1 + incrementedVec;

            incrementedVec += cnstVec2;
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
