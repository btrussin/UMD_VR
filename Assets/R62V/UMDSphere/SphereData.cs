using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SphereData : MonoBehaviour {

    public enum SphereLayout
    {
        Sphere,
        Column_X,
        Column_Y,
        Column_Z
    }

    CMJSONLoader cmLoader;
    public GameObject sphere;

    public Material ringMaterial;
    public Material curveMaterial;

    List<GameObject> ringList = new List<GameObject>();

    Dictionary<string, MovieObject> movieObjectMap = new Dictionary<string, MovieObject>();

    Dictionary<string, Color> ringColorMap = new Dictionary<string, Color>();

    List<GameObject> movieConnectionList = new List<GameObject>();

    Color baseRingColor;

    int curveLOD = 100;

    SphereLayout sphereLayout = SphereLayout.Sphere;

    Vector3 centerGrpPosition;

    UMD_Sphere_TrackedObject grabObject1;
    UMD_Sphere_TrackedObject grabObject2;
    UMD_Sphere_TrackedObject activeGrabObject;

    Vector3 initialScale;
    float initialDist;

    Quaternion initialRotation;
    Vector3 inititalOffset;

    bool activeScale = false;
    bool activeMove = false;
 
    int prevNumRingsActive = 0;

    List<GameObject> activeRings = new List<GameObject>();

    // Use this for initialization
    void Start () {
        cmLoader = this.gameObject.GetComponent<CMJSONLoader>();
        cmLoader.LoadData();
        //ringMaterial = new Material(Shader.Find("Sprites/Default"));
        //ringMaterial = new Material(Shader.Find("Standard"));
        ringMaterial = new Material(Shader.Find("Sprites/Default"));
        //curveMaterial = new Material(Shader.Find("Standard"));
        curveMaterial = new Material(Shader.Find("Sprites/Default"));

        baseRingColor = new Color(0.5f, 0.5f, 0.5f);

        centerGrpPosition = new Vector3(0.0f, 0.0f, 0.0f);

        this.transform.Translate(new Vector3(0.0f, 1.0f, 0.0f));

        grabObject1 = null;
        grabObject2 = null;


        //CreateRingsForDistributor();
        //CreateRingsForGrouping();
        CreateRingsForComic();
        //CreateRingsForPublisher();
        //CreateRingsForStudio();
        //CreateRingsForYear();

    }

    public List<GameObject> getRingsInCollision(Vector3 pos, float maxDist)
    {
        List<GameObject> list = new List<GameObject>();
        float scale = gameObject.transform.localScale.x * 0.5f;
        scale *= scale;

        foreach (GameObject ring in ringList)
        {

            Vector3 v = pos - ring.transform.position;
            
            float dist = Mathf.Abs(Vector3.Dot(ring.transform.forward, v));

            if (dist < maxDist && v.sqrMagnitude < scale)
            {
                list.Add(ring);
            }
            
        }
        return list;
    }

    public void grabSphereWithObject(GameObject obj)
    {
        UMD_Sphere_TrackedObject usto = obj.GetComponent<UMD_Sphere_TrackedObject>();

        
        if (grabObject1 == usto || grabObject2 == usto) return;

        if (grabObject1 == null )
        {
            grabObject1 = usto;
            activeGrabObject = grabObject1;
        }
        else if (grabObject2 == null )
        {
            grabObject2 = usto;
            activeGrabObject = grabObject2;
        }

        if( grabObject1 != null && grabObject2 != null )
        {
            initialScale = gameObject.transform.localScale;
            Vector3 tVec = grabObject1.currPosition - grabObject2.currPosition;
            initialDist = tVec.magnitude;
            activeScale = true;
            activeMove = false;
        }
        else
        {
            //initialRotation = Quaternion.Inverse(gameObject.transform.rotation) * activeGrabObject.currRotation;
            initialRotation = Quaternion.Inverse(activeGrabObject.currRotation) * gameObject.transform.rotation;
            Vector3 tmpVec = gameObject.transform.position - activeGrabObject.currPosition;

            inititalOffset.Set(
                Vector3.Dot(activeGrabObject.currUpVec, tmpVec),
                Vector3.Dot(activeGrabObject.currRightVec, tmpVec),
                Vector3.Dot(activeGrabObject.currForwardVec, tmpVec)
                );
            activeMove = true;
            activeScale = false;
        }
        
    }

    public void releaseSphereWithObject(GameObject obj)
    {
        UMD_Sphere_TrackedObject usto = obj.GetComponent<UMD_Sphere_TrackedObject>();

        if (usto == null) return;

        if (grabObject1 == usto)
        {
            grabObject1 = null;
            activeScale = false;

            if (grabObject2 == null) activeMove = false;
        }
        
        else if (grabObject2 == usto)
        {
            grabObject2 = null;
            activeScale = false;
            if( grabObject1 == null ) activeMove = false;
            
        }

        
        
    }

    void updateMove()
    {
        if( activeMove)
        {
            gameObject.transform.rotation = activeGrabObject.currRotation * initialRotation;

            gameObject.transform.position = activeGrabObject.deviceRay.origin +
                inititalOffset.x * activeGrabObject.currUpVec +
                inititalOffset.y * activeGrabObject.currRightVec +
                inititalOffset.z * activeGrabObject.currForwardVec;
        }
    }

    void updateScale()
    {
        if (activeScale)
        {
            Vector3 tVec = grabObject1.currPosition - grabObject2.currPosition;
           
            float scale = tVec.magnitude / initialDist;

            gameObject.transform.localScale = initialScale * scale;

        }
    }

    public void toggleMainLayout()
    {

        switch(sphereLayout)
        {
            case SphereLayout.Sphere:
                sphereLayout = SphereLayout.Column_X;
                break;
            case SphereLayout.Column_X:
                sphereLayout = SphereLayout.Column_Y;
                break;
            case SphereLayout.Column_Y:
                sphereLayout = SphereLayout.Column_Z;
                break;
            case SphereLayout.Column_Z:
                sphereLayout = SphereLayout.Sphere;
                break;
        }

        setRingLayout(ringList, centerGrpPosition, sphereLayout);
    }

    void CreateRings(string[] vals, List<CMData>[] lists)
    {
        Color[] palette = MovieDBUtils.getColorPalette();
        palette = MovieDBUtils.randomizeColorPalette(palette);

        Random.InitState(97);

        SortBySize(vals, lists, 0, vals.Length - 1);
        OrderIntoFourGroups(vals, lists);
  
        for (int i = 0; i < vals.Length; i++)
        {
            GameObject ring = getRing(vals[i], lists[i], palette[i% palette.Length], i);
            ringList.Add(ring);

            ring.transform.SetParent(this.transform);
        }

        setRingLayout(ringList, centerGrpPosition, sphereLayout);

    }

    void setRingLayout(List<GameObject> list, Vector3 centerGrpPosition, SphereLayout layout)
    {
        float numRings = (float)list.Count;

        if (layout == SphereLayout.Sphere )
        {
            Quaternion rotation;
           
            float i = 0.0f;
            foreach (GameObject ring in list)
            {
                rotation = Quaternion.Euler(new Vector3(0.0f, 180.0f * i / numRings, 0.0f));
                ring.transform.localRotation = rotation;
                ring.transform.localPosition = centerGrpPosition;
                i += 1.0f;
            }

        }
        else
        {

            float minDist = 0.05f;
            float maxDist = (numRings - 1.0f) * minDist;
            float width = 1.0f;

            if (maxDist > width)
            {
                width = maxDist;
            }

            float intOffset = -width / 2.0f;
            float inc = width / (numRings - 1.0f);



            Vector3 offsetInc = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 currOffset = new Vector3(0.0f, 0.0f, 0.0f);

            Vector3 rotVals = new Vector3(0.0f, 0.0f, 0.0f);
            switch (layout)
            {
                case SphereLayout.Column_X:
                    rotVals = new Vector3(0.0f, 90.0f, 0.0f);
                    offsetInc = new Vector3(inc, 0.0f, 0.0f);
                    currOffset = new Vector3(intOffset, 0.0f, 0.0f);
                    break;
                case SphereLayout.Column_Y:
                    rotVals = new Vector3(90.0f, 0.0f, 0.0f);
                    offsetInc = new Vector3(0.0f, inc, 0.0f);
                    currOffset = new Vector3(0.0f, intOffset, 0.0f);
                    break;
                case SphereLayout.Column_Z:
                    offsetInc = new Vector3(0.0f, 0.0f, inc);
                    currOffset = new Vector3(0.0f, 0.0f, intOffset);
                    break;
            }

            Quaternion rotation = Quaternion.Euler(rotVals);

            foreach (GameObject ring in list)
            {
                ring.transform.localRotation = rotation;
                ring.transform.localPosition = (centerGrpPosition + currOffset);
                currOffset += offsetInc;

            }
            
        }

        updateAllKeptConnections();
    }

    public void updateAllKeptConnections()
    {
        MovieConnectionManager connMan;
        foreach ( MovieObject m in movieObjectMap.Values )
        {
            connMan = m.gameObject.GetComponent<MovieConnectionManager>();
            if( connMan.getKeepConnections() && connMan.hasConnections() )
            {
                connMan.forceClearAllConnections();
                connectMoviesByActors(m.cmData);
            }
        }
    }

    GameObject getRing(string name, List<CMData> list, Color baseColor, int idx = 0)
    {
        int numSegments = 60;

        Quaternion quat = Quaternion.Euler(new Vector3(0.0f, 0.0f, 360.0f/numSegments));

        GameObject ring = new GameObject();
        ring.name = "Ring: " + name;

        ringColorMap.Add(ring.name, baseColor);

        GameObject innerRotationObj = new GameObject();
        innerRotationObj.name = "innerRotataion";
        innerRotationObj.transform.SetParent(ring.transform);

        innerRotationObj.AddComponent<MeshCollider>();

        GameObject ringLines = new GameObject();
        ringLines.name = "RingLines: " + name;

        ringLines.transform.SetParent(innerRotationObj.transform);

        //MovieDBUtils.addMeshFilter(ring, ringMaterial);


        ringLines.AddComponent<LineRenderer>();
        LineRenderer rend = ringLines.GetComponent<LineRenderer>();
        rend.SetWidth(0.005f, 0.005f);
        rend.SetVertexCount(numSegments+1);
        rend.material = ringMaterial;
        rend.material.color = baseColor * baseRingColor;
        rend.useWorldSpace = false;

        Vector3[] arr = new Vector3[numSegments + 1];
        Vector3 baseVec = Vector3.right * 0.5f;

        for ( int i = 0; i <= numSegments; i++ )
        {
            arr[i] = new Vector3(baseVec.x, baseVec.y, baseVec.z);
            baseVec = quat * baseVec;
        }

        rend.transform.SetParent(innerRotationObj.transform);
        rend.SetPositions(arr);
        

        GameObject ringLabel = new GameObject();
        ringLabel.name = "RingLabel: " + name;
        ringLabel.transform.SetParent(innerRotationObj.transform);
        ringLabel.AddComponent<MeshRenderer>();
        ringLabel.AddComponent<TextMesh>();
        ringLabel.AddComponent<CameraOrientedText3D>();
        TextMesh ringText = ringLabel.GetComponent<TextMesh>();
        ringText.anchor = TextAnchor.UpperCenter;
        ringText.alignment = TextAlignment.Center;
        ringText.text = name;
        ringText.color = baseColor;
        ringText.characterSize = 0.1f;
        ringText.fontSize = 100;
        ringText.offsetZ = -2.0f;


        float scale = 0.03f;
        ringLabel.transform.localScale = new Vector3(scale, scale, scale);

        Quaternion labelOffset = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        switch (idx%3)
        {
            case 1: labelOffset = Quaternion.Euler(new Vector3(0.0f, 0.0f, -12.0f)); break;
            case 2: labelOffset = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f)); break;
            default: break;
        }

        ringLabel.transform.position = labelOffset * (Vector3.right * 0.5f);


        // add movies
        float count = 0.0f;
        float factor = 360.0f / (float)list.Count;
        float itemScale = 0.01f;

        Quaternion randQuat = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.value*60.0f));

        GameObject ptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/R62V/UMDSphere/PointPrefab.prefab");

        string movieKey;
        foreach (CMData data in list)
        {
            movieKey = MovieDBUtils.getMovieDataKey(data);
            GameObject itemLabel = new GameObject();
            itemLabel.name = "MovieLabel: " + movieKey;
            itemLabel.transform.SetParent(innerRotationObj.transform);
            itemLabel.AddComponent<MeshRenderer>();
            itemLabel.AddComponent<TextMesh>();
            itemLabel.AddComponent<CameraOrientedText3D>();
            TextMesh itemText = itemLabel.GetComponent<TextMesh>();
            itemText.anchor = TextAnchor.UpperCenter;
            itemText.alignment = TextAlignment.Center;
            itemText.fontStyle = FontStyle.Bold;
            itemText.text = data.movie;
            itemText.color = baseColor * baseRingColor;
            itemLabel.transform.localScale = new Vector3(itemScale, itemScale, itemScale);
            itemText.characterSize = 0.1f;
            itemText.fontSize = 100;
            itemText.offsetZ = -1.0f;

            Quaternion itemOffset = randQuat * Quaternion.Euler(new Vector3(0.0f, 0.0f, factor * count));
            itemLabel.transform.position = itemOffset * (Vector3.right * 0.5f);


            GameObject point = (GameObject)Instantiate(ptPrefab);
            point.name = "MovieNode: " + movieKey;
            point.transform.SetParent(innerRotationObj.transform);
            point.transform.position = itemOffset * (Vector3.right * 0.5f);

            point.AddComponent<MovieObject>();
            point.AddComponent<MovieConnectionManager>();

            MovieObject mo = point.GetComponent<MovieObject>();
            mo.ring = ring;
            mo.cmData = data;
            mo.label = itemLabel;
            mo.point = point;
            mo.color = baseColor;

            if (movieObjectMap.ContainsKey(movieKey))
            {
                Debug.Log(movieKey + " aready exists");
            }
            else movieObjectMap.Add(movieKey, mo);


            
            

            count += 1.0f;
        }



        return ring;
    }

   

    void FixedUpdate()
    {
        //updateScale();
    }
	
	// Update is called once per frame
	void Update () {
        updateScale();
        updateMove();

        if( activeRings.Count > 0 )
        {
            prevNumRingsActive = activeRings.Count;
            highlightActiveRings();
        }
        else if ( prevNumRingsActive > 0)
        {
            unhighlightAllRings();
            prevNumRingsActive = 0;
        }


    }















    void CreateRingsForYear()
    {
        int[] years = cmLoader.getAllYears();
        string[] vals = new string[years.Length];
        List<CMData>[] lists = new List<CMData>[years.Length];
        for (int i = 0; i < years.Length; i++)
        {
            vals[i] = "" + years[i];
            lists[i] = cmLoader.getCMDataForYear(years[i]);
        }
        CreateRings(vals, lists);
    }

    void CreateRingsForPublisher()
    {
        string[] vals = cmLoader.getAllPublishers();
        List<CMData>[] lists = new List<CMData>[vals.Length];
        for (int i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForPublisher(vals[i]);
        CreateRings(vals, lists);
    }

    void CreateRingsForGrouping()
    {
        string[] vals = cmLoader.getAllGroupings();
        List<CMData>[] lists = new List<CMData>[vals.Length];
        for (int i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForGrouping(vals[i]);
        CreateRings(vals, lists);
    }

    void CreateRingsForComic()
    {
        string[] vals = cmLoader.getAllComics();
        List<CMData>[] lists = new List<CMData>[vals.Length];
        for (int i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForComic(vals[i]);
        CreateRings(vals, lists);
    }

    void CreateRingsForDistributor()
    {
        string[] vals = cmLoader.getAllDistributors();
        List<CMData>[] lists = new List<CMData>[vals.Length];
        for (int i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForDistributor(vals[i]);
        CreateRings(vals, lists);
    }


    void CreateRingsForStudio()
    {
        string[] vals = cmLoader.getAllStudios();
        List<CMData>[] lists = new List<CMData>[vals.Length];
        for (int i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForStudio(vals[i]);
        CreateRings(vals, lists);
    }

    void OrderIntoFourGroups(string[] vals, List<CMData>[] lists)
    {
        List<string>[] sLinsts = new List<string>[4];
        List<List<CMData>>[] lLinsts = new List<List<CMData>>[4];
        for (int i = 0; i < 4; i++)
        {
            sLinsts[i] = new List<string>();
            lLinsts[i] = new List<List<CMData>>();
        }

        for( int i = 0; i < vals.Length; i+= 4)
        {
            for (int j = 0; j < 4 && j+i < vals.Length; j++)
            {
                sLinsts[j].Add(vals[i + j]);
                lLinsts[j].Add(lists[i + j]);
            }
        }

        int sIIdx = 0;
        int lIIdx = 0;
        int tIdx = 0;
        for (int i = 0; i < 4; i++)
        {
            foreach (string s in sLinsts[tIdx % 4])
            {
                vals[sIIdx++] = s;
            }

            foreach (List<CMData> l in lLinsts[tIdx % 4])
            {
                lists[lIIdx++] = l;
            }

            tIdx += 3;
        }

    }

    // sort lowest [idx = 0] to highest [idx = list.Count - 1]
    void SortBySize(string[] vals, List<CMData>[] lists, int beginIdx, int endIdx)
    {
        int idxDist = endIdx - beginIdx;
        if (idxDist < 1) return;
        else if (idxDist == 1)
        {
            if (lists[beginIdx].Count > lists[endIdx].Count) swapElements(vals, lists, beginIdx, endIdx);
            return;
        }

        int midIdx = (beginIdx + endIdx) / 2;
        int countVal = lists[midIdx].Count;

        swapElements(vals, lists, midIdx, endIdx);


        int s = beginIdx;
        int e = endIdx-1;
        
        while ( s < e )
        {
            if( lists[s].Count > countVal )
            {
                swapElements(vals, lists, s, e);
                e--;
            }
            else s++;
        }

        int divider = midIdx;

        for (int i = beginIdx; i < endIdx; i++)
        {
            if(lists[i].Count > countVal)
            {
                divider = i;
                swapElements(vals, lists, divider, endIdx);
                break;
            }
        }

        SortBySize(vals, lists, beginIdx, divider-1);
        SortBySize(vals, lists, divider + 1, endIdx);
    }

    void swapElements(string[] vals, List<CMData>[] lists, int i, int j)
    {
        string t = vals[i];
        List<CMData> tList = lists[i];

        vals[i] = vals[j];
        lists[i] = lists[j];

        vals[j] = t;
        lists[j] = tList;
        return;
    }

    public void trackMovieConnection(GameObject gObj)
    {
        movieConnectionList.Add(gObj);
    }

    public void trackMovieConnection(GameObject[] gObjs)
    {
        for (int i = 0; i < gObjs.Length; i++) trackMovieConnection(gObjs[i]);
    }

    public void trackMovieConnection(List<GameObject> list)
    {
        foreach(GameObject gObj in list ) trackMovieConnection(gObj);
    }

    public void connectMoviesAndTrack(CMData from, CMData to, Color c)
    {
        trackMovieConnection(connectMovies(from, to, c));
    }

    public GameObject connectMovies(CMData from, CMData to, Color c)
    {
        MovieObject moFrom;
        MovieObject moTo;

        movieObjectMap.TryGetValue(MovieDBUtils.getMovieDataKey(from), out moFrom);
        movieObjectMap.TryGetValue(MovieDBUtils.getMovieDataKey(to), out moTo);

        Vector3[] basePts = new Vector3[4];

        basePts[0] = moFrom.point.transform.position;
        basePts[3] = moTo.point.transform.position;

        basePts[1] = (moFrom.ring.transform.position - basePts[0]) * 0.5f + basePts[0];
        basePts[2] = (moTo.ring.transform.position - basePts[3]) * 0.5f + basePts[3];

        Vector3[] pts = MovieDBUtils.getBezierPoints(basePts, curveLOD);

        GameObject connCurve = new GameObject();
        connCurve.name = "Conn: " + from.movie + " - " + to.movie;

        connCurve.transform.SetParent(gameObject.transform);

        connCurve.AddComponent<LineRenderer>();
        LineRenderer rend = connCurve.GetComponent<LineRenderer>();
        rend.SetWidth(0.005f, 0.005f);
        rend.SetColors(moFrom.color, moTo.color);
        rend.SetVertexCount(curveLOD);
        rend.material = curveMaterial;
        rend.material.color = new Color(0.3f, 0.3f, 0.3f);
        rend.useWorldSpace = false;

        rend.SetPositions(pts);

        MovieConnectionManager connMan = moFrom.gameObject.GetComponent<MovieConnectionManager>();
        connMan.addConnection(connCurve);

        return connCurve;
    }

    public void connectMoviesByActors(CMData cmData, bool track = true)
    {
        string mainKey = MovieDBUtils.getMovieDataKey(cmData);

        string tKey;
        MovieObject fMo;
        MovieObject tMo;

        movieObjectMap.TryGetValue(mainKey, out fMo);

        List<GameObject> trackList = new List<GameObject>();

        for (int i = 0; i < fMo.cmData.roles.Length; i++)
        {
            List<CMData> list = cmLoader.getCMDataForActor(fMo.cmData.roles[i].actor);

            foreach (CMData data in list)
            {
                tKey = MovieDBUtils.getMovieDataKey(data);
                if (mainKey.Equals(tKey)) continue;

                movieObjectMap.TryGetValue(tKey, out tMo);
                trackList.Add(connectMovies(fMo.cmData, tMo.cmData, Color.red));
            }
        }
    }

    public void clearAllConnections()
    {
        foreach (GameObject gObj in movieConnectionList) Destroy(gObj);
        movieConnectionList.Clear();
    }

    public void addActiveRings(List<GameObject> list)
    {
        activeRings.AddRange(list);
    }

    void unhighlightAllRings()
    {
        LineRenderer rnd = null;
        Color color;
        GameObject innerRot;
        foreach (GameObject ring in ringList)
        {

            innerRot = ring.transform.GetChild(0).gameObject;

            for (int i = 0; i < innerRot.transform.childCount; i++)
            {
                rnd = innerRot.transform.GetChild(i).GetComponent<LineRenderer>();
                if (rnd)
                {
                    if (ringColorMap.TryGetValue(ring.name, out color))
                    {
                        rnd.material.color = color * baseRingColor;
                        break;
                    }
                }

            }
        }
    }

    void dimAllRings()
    {
        LineRenderer rnd = null;
        Color color;

        float perc = 0.3f;

        Color dimColor = baseRingColor * (1.0f - perc);
        GameObject innerRot;
        foreach (GameObject ring in ringList)
        {
            innerRot = ring.transform.GetChild(0).gameObject;
            for (int i = 0; i < innerRot.transform.childCount; i++)
            {
                rnd = innerRot.transform.GetChild(i).GetComponent<LineRenderer>();
                if (rnd)
                {
                    if (ringColorMap.TryGetValue(ring.name, out color))
                    {
                        rnd.material.color = color * dimColor;
                        break;
                    }
                }

            }
        }
    }

    void highlightActiveRings()
    {
        dimAllRings();

        LineRenderer rnd = null;
        Color color;
        GameObject innerRot;
        foreach (GameObject ring in activeRings)
        {
            innerRot = ring.transform.GetChild(0).gameObject;
            for (int i = 0; i < innerRot.transform.childCount; i++)
            {
                rnd = innerRot.transform.GetChild(i).GetComponent<LineRenderer>();
                if (rnd)
                {
                    if (ringColorMap.TryGetValue(ring.name, out color))
                    {
                        rnd.material.color = color * baseRingColor;
                        break;
                    }
                }
            }
        }

        activeRings.Clear();
    }

}


