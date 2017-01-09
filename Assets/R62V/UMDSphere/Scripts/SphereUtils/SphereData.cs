using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//TODO: Need to find a way to group the data and expand into rings.

/// <summary>
/// </summary>
public class SphereData : MonoBehaviour
{
    public enum SphereLayout
    {
        Sphere,
        ColumnX,
        ColumnY,
        ColumnZ
    }

    public static int NUM_LAYOUTS = 6; //update the layouts
    private UMD_Sphere_TrackedObject activeGrabObject;
    private bool activeMove;

    private readonly List<GameObject> activeRings = new List<GameObject>();

    private bool activeScale;

    private Color baseRingColor;

    private Vector3 centerGrpPosition;

    private CMJSONLoader cmLoader;

    private readonly int curveLOD = 100;

    private UMD_Sphere_TrackedObject grabObject1;
    private UMD_Sphere_TrackedObject grabObject2;
    private float initialDist;

    private Quaternion initialRotation;

    private Vector3 initialScale;
    private Vector3 inititalOffset;

    private readonly List<GameObject> movieConnectionList = new List<GameObject>();

    private readonly Dictionary<string, MovieObject> movieObjectMap = new Dictionary<string, MovieObject>();

    [Header("Object Path Strings", order = 1)] public string pointPrefabPath =
        "Assets/R62V/UMDSphere/Prefabs/PointPrefab.prefab"; //Set by default. May be changed in the editor.

    private int prevNumRingsActive;

    private readonly Dictionary<string, Color> ringColorMap = new Dictionary<string, Color>();

    private readonly List<GameObject> ringList = new List<GameObject>();

    [Header("Materials", order = 2)]
    public Material RingMaterial;
    public Material CurveMaterial;

    [Header("Data Object", order = 3)] public GameObject Sphere;

    private SphereLayout sphereLayout = SphereLayout.Sphere;

    // Use this for initialization
    private void Start()
    {
        cmLoader = gameObject.GetComponent<CMJSONLoader>();
        cmLoader.LoadData();
        //ringMaterial = new Material(Shader.Find("Standard"));
        RingMaterial = new Material(Shader.Find("Sprites/Default"));
        //curveMaterial = new Material(Shader.Find("Standard"));
        CurveMaterial = new Material(Shader.Find("Sprites/Default"));

        baseRingColor = new Color(0.5f, 0.5f, 0.5f);

        centerGrpPosition = new Vector3(0.0f, 0.0f, 0.0f);

        transform.Translate(new Vector3(0.0f, 1.0f, 0.0f));

        grabObject1 = null;
        grabObject2 = null;
    }

    public List<GameObject> GetRingsInCollision(Vector3 pos, float maxDist)
    {
        var list = new List<GameObject>();
        var scale = gameObject.transform.localScale.x * 0.5f;
        scale *= scale;

        foreach (var ring in ringList)
        {
            var v = pos - ring.transform.position;

            var dist = Mathf.Abs(Vector3.Dot(ring.transform.forward, v));

            if (dist < maxDist && v.sqrMagnitude < scale)
                list.Add(ring);
        }
        return list;
    }

    public void GrabSphereWithObject(GameObject obj)
    {
        var usto = obj.GetComponent<UMD_Sphere_TrackedObject>();


        if (grabObject1 == usto || grabObject2 == usto) return;

        if (grabObject1 == null)
        {
            grabObject1 = usto;
            activeGrabObject = grabObject1;
        }
        else if (grabObject2 == null)
        {
            grabObject2 = usto;
            activeGrabObject = grabObject2;
        }

        if (grabObject1 != null && grabObject2 != null)
        {
            initialScale = gameObject.transform.localScale;
            Vector3 tVec = grabObject1.CurrPosition - grabObject2.CurrPosition;
            initialDist = tVec.magnitude;
            activeScale = true;
            activeMove = false;
        }
        else
        {
            //initialRotation = Quaternion.Inverse(gameObject.transform.rotation) * activeGrabObject.currRotation;
            initialRotation = Quaternion.Inverse(activeGrabObject.CurrRotation) * gameObject.transform.rotation;
            var tmpVec = gameObject.transform.position - activeGrabObject.CurrPosition;

            inititalOffset.Set(
                Vector3.Dot(activeGrabObject.CurrUpVec, tmpVec),
                Vector3.Dot(activeGrabObject.CurrRightVec, tmpVec),
                Vector3.Dot(activeGrabObject.CurrForwardVec, tmpVec)
            );
            activeMove = true;
            activeScale = false;
        }
    }

    public void ReleaseSphereWithObject(GameObject obj)
    {
        var usto = obj.GetComponent<UMD_Sphere_TrackedObject>();

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
            if (grabObject1 == null) activeMove = false;
        }
    }

    private void UpdateMove()
    {
        if (activeMove)
        {
            gameObject.transform.rotation = activeGrabObject.CurrRotation * initialRotation;

            gameObject.transform.position = activeGrabObject.DeviceRay.origin +
                                            inititalOffset.x * activeGrabObject.CurrUpVec +
                                            inititalOffset.y * activeGrabObject.CurrRightVec +
                                            inititalOffset.z * activeGrabObject.CurrForwardVec;
        }
    }

    private void UpdateScale()
    {
        if (activeScale)
        {
            Vector3 tVec = grabObject1.CurrPosition - grabObject2.CurrPosition;

            var scale = tVec.magnitude / initialDist;

            gameObject.transform.localScale = initialScale * scale;
        }
    }

    public void ToggleMainLayout()
    {
        switch (sphereLayout)
        {
            case SphereLayout.Sphere:
                sphereLayout = SphereLayout.ColumnX;
                break;
            case SphereLayout.ColumnX:
                sphereLayout = SphereLayout.ColumnY;
                break;
            case SphereLayout.ColumnY:
                sphereLayout = SphereLayout.ColumnZ;
                break;
            case SphereLayout.ColumnZ:
                sphereLayout = SphereLayout.Sphere;
                break;
        }

        SetRingLayout(ringList, centerGrpPosition, sphereLayout);
    }

    private void CreateRings(string[] vals, List<CMData>[] lists)
    {
        var palette = MovieDBUtils.getColorPalette();
        palette = MovieDBUtils.randomizeColorPalette(palette);

        Random.InitState(97);

        SortBySize(vals, lists, 0, vals.Length - 1);
        OrderIntoFourGroups(vals, lists);

        for (var i = 0; i < vals.Length; i++)
        {
            var ring = getRing(vals[i], lists[i], palette[i % palette.Length], i);
            ringList.Add(ring);

            ring.transform.SetParent(transform);
        }

        SetRingLayout(ringList, centerGrpPosition, sphereLayout);
    }

    private void SetRingLayout(List<GameObject> list, Vector3 centerGrpPosition, SphereLayout layout)
    {
        float numRings = list.Count;

        if (layout == SphereLayout.Sphere)
        {
            Quaternion rotation;

            var i = 0.0f;
            foreach (var ring in list)
            {
                rotation = Quaternion.Euler(new Vector3(0.0f, 180.0f * i / numRings, 0.0f));
                ring.transform.localRotation = rotation;
                ring.transform.localPosition = centerGrpPosition;
                i += 1.0f;
                ring.GetComponent<RingState>().UpdateColor();
            }
        }
        else
        {
            var minDist = 0.05f;
            var maxDist = (numRings - 1.0f) * minDist;
            var width = 1.0f;

            if (maxDist > width)
                width = maxDist;

            var intOffset = -width / 2.0f;
            var inc = width / (numRings - 1.0f);


            var offsetInc = new Vector3(0.0f, 0.0f, 0.0f);
            var currOffset = new Vector3(0.0f, 0.0f, 0.0f);

            var rotVals = new Vector3(0.0f, 0.0f, 0.0f);
            switch (layout)
            {
                case SphereLayout.ColumnX:
                    rotVals = new Vector3(0.0f, 90.0f, 0.0f);
                    offsetInc = new Vector3(inc, 0.0f, 0.0f);
                    currOffset = new Vector3(intOffset, 0.0f, 0.0f);
                    break;
                case SphereLayout.ColumnY:
                    rotVals = new Vector3(90.0f, 0.0f, 0.0f);
                    offsetInc = new Vector3(0.0f, inc, 0.0f);
                    currOffset = new Vector3(0.0f, intOffset, 0.0f);
                    break;
                case SphereLayout.ColumnZ:
                    offsetInc = new Vector3(0.0f, 0.0f, inc);
                    currOffset = new Vector3(0.0f, 0.0f, intOffset);
                    break;
            }

            var rotation = Quaternion.Euler(rotVals);

            foreach (var ring in list)
            {
                ring.transform.localRotation = rotation;
                ring.transform.localPosition = centerGrpPosition + currOffset;
                currOffset += offsetInc;

                ring.GetComponent<RingState>().UpdateColor();
            }
        }

        UpdateAllKeptConnections();
    }

    public void UpdateAllKeptConnections()
    {
        MovieConnectionManager connMan;
        NodeState ns;
        foreach (var m in movieObjectMap.Values)
        {
            connMan = m.connManager;
            ns = m.nodeState;
            if (ns.getIsSelected() && connMan.HasConnections())
            {
                connMan.ForceClearAllConnections();
                ConnectMoviesByActors(m.cmData);
            }
        }
    }

    private GameObject getRing(string name, List<CMData> list, Color baseColor, int idx = 0)
    {
        var numSegments = 60;

        var quat = Quaternion.Euler(new Vector3(0.0f, 0.0f, 360.0f / numSegments));

        var ring = new GameObject();
        ring.name = "Ring: " + name;

        ringColorMap.Add(ring.name, baseColor);

        var innerRotationObj = new GameObject();
        innerRotationObj.name = "innerRotataion";
        innerRotationObj.transform.SetParent(ring.transform);

        innerRotationObj.AddComponent<MeshCollider>();

        var ringLines = new GameObject();
        ringLines.name = "RingLines";

        ringLines.transform.SetParent(innerRotationObj.transform);

        //MovieDBUtils.addMeshFilter(ring, ringMaterial);


        ringLines.AddComponent<LineRenderer>();
        var rend = ringLines.GetComponent<LineRenderer>();
        rend.SetWidth(0.005f, 0.005f);
        rend.SetVertexCount(numSegments + 1);
        rend.material = RingMaterial;
        //rend.material.color = baseColor * baseRingColor;
        rend.useWorldSpace = false;

        var arr = new Vector3[numSegments + 1];
        var baseVec = Vector3.right * 0.5f;

        for (var i = 0; i <= numSegments; i++)
        {
            arr[i] = new Vector3(baseVec.x, baseVec.y, baseVec.z);
            baseVec = quat * baseVec;
        }

        rend.transform.SetParent(innerRotationObj.transform);
        rend.SetPositions(arr);


        var ringLabel = new GameObject();
        ringLabel.name = "RingLabel";
        ringLabel.transform.SetParent(innerRotationObj.transform);
        ringLabel.AddComponent<MeshRenderer>();
        ringLabel.AddComponent<TextMesh>();
        ringLabel.AddComponent<CameraOrientedText3D>();
        var ringText = ringLabel.GetComponent<TextMesh>();
        ringText.anchor = TextAnchor.UpperCenter;
        ringText.alignment = TextAlignment.Center;
        ringText.text = name;
        //ringText.color = baseColor;
        ringText.characterSize = 0.1f;
        ringText.fontSize = 100;
        ringText.offsetZ = -2.0f;


        var scale = 0.03f;
        ringLabel.transform.localScale = new Vector3(scale, scale, scale);

        var labelOffset = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        switch (idx % 3)
        {
            case 1:
                labelOffset = Quaternion.Euler(new Vector3(0.0f, 0.0f, -12.0f));
                break;
            case 2:
                labelOffset = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
                break;
            default:
                break;
        }

        ringLabel.transform.position = labelOffset * (Vector3.right * 0.5f);


        // add movies
        var count = 0.0f;
        var factor = 360.0f / list.Count;
        var itemScale = 0.01f;

        var randQuat = Quaternion.Euler(new Vector3(0.0f, 0.0f, Random.value * 60.0f));

        var ptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pointPrefabPath);

        string movieKey;
        foreach (var data in list)
        {
            movieKey = MovieDBUtils.getMovieDataKey(data);

            var itemOffset = randQuat * Quaternion.Euler(new Vector3(0.0f, 0.0f, factor * count));

            var movieNodeObj = new GameObject();
            movieNodeObj.name = "Movie: " + movieKey;
            movieNodeObj.transform.SetParent(innerRotationObj.transform);
            movieNodeObj.AddComponent<MovieObject>();

            var itemLabel = new GameObject();
            itemLabel.name = "MovieLabel";
            itemLabel.transform.SetParent(movieNodeObj.transform);
            itemLabel.AddComponent<MeshRenderer>();
            itemLabel.AddComponent<TextMesh>();
            itemLabel.AddComponent<CameraOrientedText3D>();
            var itemText = itemLabel.GetComponent<TextMesh>();
            itemText.anchor = TextAnchor.UpperCenter;
            itemText.alignment = TextAlignment.Center;
            itemText.fontStyle = FontStyle.Bold;
            itemText.text = data.movie;
            itemText.color = baseColor * baseRingColor;
            itemLabel.transform.localScale = new Vector3(itemScale, itemScale, itemScale);
            itemLabel.transform.localPosition = itemOffset * new Vector3(0.05f, 0.0f, 0.0f);
            itemText.characterSize = 0.1f;
            itemText.fontSize = 100;
            itemText.offsetZ = -1.0f;


            var point = Instantiate(ptPrefab);
            point.name = "MovieNode";
            point.transform.SetParent(movieNodeObj.transform);
            point.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);


            movieNodeObj.transform.position = itemOffset * (Vector3.right * 0.5f);


            point.AddComponent<MovieConnectionManager>();

            var connMan = point.GetComponent<MovieConnectionManager>();

            var mo = movieNodeObj.GetComponent<MovieObject>();
            mo.ring = ring;
            mo.cmData = data;
            mo.label = itemLabel;
            mo.point = point;
            mo.color = baseColor;
            mo.connManager = connMan;

            if (movieObjectMap.ContainsKey(movieKey))
                Debug.Log(movieKey + " aready exists");
            else movieObjectMap.Add(movieKey, mo);


            movieNodeObj.AddComponent<NodeState>();
            var ns = movieNodeObj.GetComponent<NodeState>();

            mo.nodeState = ns;

            count += 1.0f;
        }


        ring.AddComponent<RingState>();
        var ringState = ring.GetComponent<RingState>();
        ringState.SetRingColor(baseColor);

        ringState.UpdateColor();

        return ring;
    }


    private void FixedUpdate()
    {
        //updateScale();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateScale();
        UpdateMove();

        if (activeRings.Count > 0)
        {
            prevNumRingsActive = activeRings.Count;
            HighlightActiveRings();
        }
        else if (prevNumRingsActive > 0)
        {
            UnhighlightAllRings();
            prevNumRingsActive = 0;
        }
    }

    public void CreateRingsForYear()
    {
        this.name = "DataObject: Year";

        ClearRings();

        var years = cmLoader.getAllYears();
        var vals = new string[years.Length];
        var lists = new List<CMData>[years.Length];
        for (var i = 0; i < years.Length; i++)
        {
            vals[i] = "" + years[i];
            lists[i] = cmLoader.getCMDataForYear(years[i]);
        }
        CreateRings(vals, lists);
    }

    public void CreateRingsForPublisher()
    {
        this.name = "DataObject: Publisher";

        ClearRings();

        var vals = cmLoader.getAllPublishers();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForPublisher(vals[i]);
        CreateRings(vals, lists);
    }

    public void CreateRingsForGrouping()
    {
        this.name = "DataObject: Grouping";

        ClearRings();

        var vals = cmLoader.getAllGroupings();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForGrouping(vals[i]);
        CreateRings(vals, lists);
    }

    public void CreateRingsForComic()
    {
        this.name = "DataObject: Comic";

        ClearRings();

        var vals = cmLoader.getAllComics();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForComic(vals[i]);
        CreateRings(vals, lists);
    }

    public void CreateRingsForDistributor()
    {
        this.name = "DataObject: Distributor";

        ClearRings();

        var vals = cmLoader.getAllDistributors();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForDistributor(vals[i]);
        CreateRings(vals, lists);
    }


    public void CreateRingsForStudio()
    {
        this.name = "DataObject: Studio";

        ClearRings();

        var vals = cmLoader.getAllStudios();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = cmLoader.getCMDataForStudio(vals[i]);
        CreateRings(vals, lists);
    }

    public void ClearRings()
    {
        MovieConnectionManager connMan;

        foreach (var m in movieObjectMap.Values)
        {
            connMan = m.connManager;
            connMan.ForceClearAllConnections();
        }

        for (var ring = 0; ring < ringList.Count; ring++)
            Destroy(ringList[ring]);

        ringList.Clear();
        ringColorMap.Clear();
        movieObjectMap.Clear();
        activeRings.Clear();
        movieConnectionList.Clear();
    }

    private void OrderIntoFourGroups(string[] vals, List<CMData>[] lists)
    {
        var sLinsts = new List<string>[4];
        var lLinsts = new List<List<CMData>>[4];
        for (var i = 0; i < 4; i++)
        {
            sLinsts[i] = new List<string>();
            lLinsts[i] = new List<List<CMData>>();
        }

        for (var i = 0; i < vals.Length; i += 4)
        for (var j = 0; j < 4 && j + i < vals.Length; j++)
        {
            sLinsts[j].Add(vals[i + j]);
            lLinsts[j].Add(lists[i + j]);
        }

        var sIIdx = 0;
        var lIIdx = 0;
        var tIdx = 0;
        for (var i = 0; i < 4; i++)
        {
            foreach (var s in sLinsts[tIdx % 4])
                vals[sIIdx++] = s;

            foreach (var l in lLinsts[tIdx % 4])
                lists[lIIdx++] = l;

            tIdx += 3;
        }
    }

    // sort lowest [idx = 0] to highest [idx = list.Count - 1]
    private void SortBySize(string[] vals, List<CMData>[] lists, int beginIdx, int endIdx)
    {
        var idxDist = endIdx - beginIdx;
        if (idxDist < 1)
            return;
        if (idxDist == 1)
        {
            if (lists[beginIdx].Count > lists[endIdx].Count) swapElements(vals, lists, beginIdx, endIdx);
            return;
        }

        var midIdx = (beginIdx + endIdx) / 2;
        var countVal = lists[midIdx].Count;

        swapElements(vals, lists, midIdx, endIdx);


        var s = beginIdx;
        var e = endIdx - 1;

        while (s < e)
            if (lists[s].Count > countVal)
            {
                swapElements(vals, lists, s, e);
                e--;
            }
            else
            {
                s++;
            }

        var divider = midIdx;

        for (var i = beginIdx; i < endIdx; i++)
            if (lists[i].Count > countVal)
            {
                divider = i;
                swapElements(vals, lists, divider, endIdx);
                break;
            }

        SortBySize(vals, lists, beginIdx, divider - 1);
        SortBySize(vals, lists, divider + 1, endIdx);
    }

    private void swapElements(string[] vals, List<CMData>[] lists, int i, int j)
    {
        var t = vals[i];
        var tList = lists[i];

        vals[i] = vals[j];
        lists[i] = lists[j];

        vals[j] = t;
        lists[j] = tList;
    }

    public void trackMovieConnection(GameObject gObj)
    {
        movieConnectionList.Add(gObj);
    }

    public void trackMovieConnection(GameObject[] gObjs)
    {
        for (var i = 0; i < gObjs.Length; i++) trackMovieConnection(gObjs[i]);
    }

    public void trackMovieConnection(List<GameObject> list)
    {
        foreach (var gObj in list) trackMovieConnection(gObj);
    }

    public void ConnectMoviesAndTrack(CMData from, CMData to, Color c)
    {
        trackMovieConnection(ConnectMovies(from, to, c));
    }

    public GameObject ConnectMovies(CMData from, CMData to, Color c)
    {
        MovieObject moFrom;
        MovieObject moTo;

        movieObjectMap.TryGetValue(MovieDBUtils.getMovieDataKey(from), out moFrom);
        movieObjectMap.TryGetValue(MovieDBUtils.getMovieDataKey(to), out moTo);

        var basePts = new Vector3[4];

        basePts[0] = moFrom.point.transform.position;
        basePts[3] = moTo.point.transform.position;

        basePts[1] = (moFrom.ring.transform.position - basePts[0]) * 0.5f + basePts[0];
        basePts[2] = (moTo.ring.transform.position - basePts[3]) * 0.5f + basePts[3];

        var pts = MovieDBUtils.getBezierPoints(basePts, curveLOD);

        var connCurve = new GameObject();
        connCurve.name = "Conn: " + from.movie + " - " + to.movie;

        connCurve.transform.SetParent(gameObject.transform);

        connCurve.AddComponent<LineRenderer>();
        var rend = connCurve.GetComponent<LineRenderer>();
        rend.SetWidth(0.005f, 0.005f);
        rend.SetColors(moFrom.color, moTo.color);
        rend.SetVertexCount(curveLOD);
        rend.material = CurveMaterial;
        rend.material.color = new Color(0.3f, 0.3f, 0.3f);
        rend.useWorldSpace = false;

        rend.SetPositions(pts);

        moFrom.connManager.AddConnection(connCurve, moFrom, moTo);

        return connCurve;
    }

    public void ConnectMoviesByActors(CMData cmData, bool track = true)
    {
        var mainKey = MovieDBUtils.getMovieDataKey(cmData);

        string tKey;
        MovieObject fMo;
        MovieObject tMo;

        movieObjectMap.TryGetValue(mainKey, out fMo);

        var trackList = new List<GameObject>();

        for (var i = 0; i < fMo.cmData.roles.Length; i++)
        {
            if (!fMo.cmData.roles[i].active) continue;
            var list = cmLoader.getCMDataForActor(fMo.cmData.roles[i].actor);

            foreach (var data in list)
            {
                tKey = MovieDBUtils.getMovieDataKey(data);
                if (mainKey.Equals(tKey)) continue;

                movieObjectMap.TryGetValue(tKey, out tMo);
                trackList.Add(ConnectMovies(fMo.cmData, tMo.cmData, Color.red));
            }
        }
    }

    public void ClearAllConnections()
    {
        foreach (var gObj in movieConnectionList) Destroy(gObj);
        movieConnectionList.Clear();
    }

    public void AddActiveRings(List<GameObject> list)
    {
        activeRings.AddRange(list);
    }

    private void UnhighlightAllRings()
    {
        RingState rs;
        foreach (var ring in ringList)
        {
            rs = ring.GetComponent<RingState>();
            rs.UpdateColor();
        }
    }

    private void DimAllRings()
    {
        RingState rs;
        foreach (var ring in ringList)
        {
            rs = ring.GetComponent<RingState>();
            rs.SetDimmed();
            rs.UpdateColor();
        }
    }

    private void HighlightActiveRings()
    {
        DimAllRings();

        RingState rs;
        foreach (var ring in activeRings)
        {
            rs = ring.GetComponent<RingState>();
            rs.SetHighlighted();
            rs.UpdateColor();
        }

        activeRings.Clear();
    }
}