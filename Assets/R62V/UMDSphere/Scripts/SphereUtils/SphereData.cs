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
    private UMD_Sphere_TrackedObject _activeGrabObject;
    private bool _activeMove;

    private readonly List<GameObject> _activeRings = new List<GameObject>();

    private bool _activeScale;

    private Color _baseRingColor;

    private Vector3 _centerGrpPosition;

    private CMJSONLoader _cmLoader;

    private readonly int curveLOD = 100;

    private UMD_Sphere_TrackedObject _grabObject1;
    private UMD_Sphere_TrackedObject _grabObject2;
    private float _initialDist;

    private Quaternion _initialRotation;

    private Vector3 _initialScale;
    private Vector3 _inititalOffset;

    private readonly List<GameObject> _movieConnectionList = new List<GameObject>();

    private readonly Dictionary<string, MovieObject> _movieObjectMap = new Dictionary<string, MovieObject>();

    [Header("Object Path Strings", order = 1)] public string PointPrefabPath =
        "Assets/R62V/UMDSphere/Prefabs/PointPrefab.prefab"; //Set by default. May be changed in the editor.

    private int _prevNumRingsActive;

    private readonly Dictionary<string, Color> _ringColorMap = new Dictionary<string, Color>();

    private readonly List<GameObject> _ringList = new List<GameObject>();

    [Header("Materials", order = 2)]
    public Material RingMaterial;
    public Material CurveMaterial;

    [Header("Data Object", order = 3)] public GameObject Sphere;

    private SphereLayout _sphereLayout = SphereLayout.Sphere;

    // Use this for initialization
    private void Start()
    {
        _cmLoader = gameObject.GetComponent<CMJSONLoader>();
        _cmLoader.LoadData();
        //ringMaterial = new Material(Shader.Find("Standard"));
        RingMaterial = new Material(Shader.Find("Sprites/Default"));
        //curveMaterial = new Material(Shader.Find("Standard"));
        CurveMaterial = new Material(Shader.Find("Sprites/Default"));

        _baseRingColor = new Color(0.5f, 0.5f, 0.5f);

        _centerGrpPosition = new Vector3(0.0f, 0.0f, 0.0f);

        transform.Translate(new Vector3(0.0f, 1.0f, 0.0f));

        _grabObject1 = null;
        _grabObject2 = null;
    }

    public List<GameObject> GetRingsInCollision(Vector3 pos, float maxDist)
    {
        var list = new List<GameObject>();
        var scale = gameObject.transform.localScale.x * 0.5f;
        scale *= scale;

        foreach (var ring in _ringList)
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


        if (_grabObject1 == usto || _grabObject2 == usto) return;

        if (_grabObject1 == null)
        {
            _grabObject1 = usto;
            _activeGrabObject = _grabObject1;
        }
        else if (_grabObject2 == null)
        {
            _grabObject2 = usto;
            _activeGrabObject = _grabObject2;
        }

        if (_grabObject1 != null && _grabObject2 != null)
        {
            _initialScale = gameObject.transform.localScale;
            Vector3 tVec = _grabObject1.CurrPosition - _grabObject2.CurrPosition;
            _initialDist = tVec.magnitude;
            _activeScale = true;
            _activeMove = false;
        }
        else
        {
            //initialRotation = Quaternion.Inverse(gameObject.transform.rotation) * activeGrabObject.currRotation;
            _initialRotation = Quaternion.Inverse(_activeGrabObject.CurrRotation) * gameObject.transform.rotation;
            var tmpVec = gameObject.transform.position - _activeGrabObject.CurrPosition;

            _inititalOffset.Set(
                Vector3.Dot(_activeGrabObject.CurrUpVec, tmpVec),
                Vector3.Dot(_activeGrabObject.CurrRightVec, tmpVec),
                Vector3.Dot(_activeGrabObject.CurrForwardVec, tmpVec)
            );
            _activeMove = true;
            _activeScale = false;
        }
    }

    public void ReleaseSphereWithObject(GameObject obj)
    {
        var usto = obj.GetComponent<UMD_Sphere_TrackedObject>();

        if (usto == null) return;

        if (_grabObject1 == usto)
        {
            _grabObject1 = null;
            _activeScale = false;

            if (_grabObject2 == null) _activeMove = false;
        }

        else if (_grabObject2 == usto)
        {
            _grabObject2 = null;
            _activeScale = false;
            if (_grabObject1 == null) _activeMove = false;
        }
    }

    private void UpdateMove()
    {
        if (_activeMove)
        {
            gameObject.transform.rotation = _activeGrabObject.CurrRotation * _initialRotation;

            gameObject.transform.position = _activeGrabObject.DeviceRay.origin +
                                            _inititalOffset.x * _activeGrabObject.CurrUpVec +
                                            _inititalOffset.y * _activeGrabObject.CurrRightVec +
                                            _inititalOffset.z * _activeGrabObject.CurrForwardVec;
        }
    }

    private void UpdateScale()
    {
        if (_activeScale)
        {
            Vector3 tVec = _grabObject1.CurrPosition - _grabObject2.CurrPosition;

            var scale = tVec.magnitude / _initialDist;

            gameObject.transform.localScale = _initialScale * scale;
        }
    }

    public void ToggleMainLayout()
    {
        switch (_sphereLayout)
        {
            case SphereLayout.Sphere:
                _sphereLayout = SphereLayout.ColumnX;
                break;
            case SphereLayout.ColumnX:
                _sphereLayout = SphereLayout.ColumnY;
                break;
            case SphereLayout.ColumnY:
                _sphereLayout = SphereLayout.ColumnZ;
                break;
            case SphereLayout.ColumnZ:
                _sphereLayout = SphereLayout.Sphere;
                break;
        }

        SetRingLayout(_ringList, _centerGrpPosition, _sphereLayout);
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
            _ringList.Add(ring);

            ring.transform.SetParent(transform);
        }

        SetRingLayout(_ringList, _centerGrpPosition, _sphereLayout);
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
        foreach (var m in _movieObjectMap.Values)
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

        _ringColorMap.Add(ring.name, baseColor);

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

        var ptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PointPrefabPath);

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
            itemText.color = baseColor * _baseRingColor;
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

            if (_movieObjectMap.ContainsKey(movieKey))
                Debug.Log(movieKey + " aready exists");
            else _movieObjectMap.Add(movieKey, mo);


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

        if (_activeRings.Count > 0)
        {
            _prevNumRingsActive = _activeRings.Count;
            HighlightActiveRings();
        }
        else if (_prevNumRingsActive > 0)
        {
            UnhighlightAllRings();
            _prevNumRingsActive = 0;
        }
    }

    public void CreateRingsForYear()
    {
        this.name = "DataObject: Year";

        ClearRings();

        var years = _cmLoader.getAllYears();
        var vals = new string[years.Length];
        var lists = new List<CMData>[years.Length];
        for (var i = 0; i < years.Length; i++)
        {
            vals[i] = "" + years[i];
            lists[i] = _cmLoader.getCMDataForYear(years[i]);
        }
        CreateRings(vals, lists);
    }

    public void CreateRingsForPublisher()
    {
        this.name = "DataObject: Publisher";

        ClearRings();

        var vals = _cmLoader.getAllPublishers();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = _cmLoader.getCMDataForPublisher(vals[i]);
        CreateRings(vals, lists);
    }

    public void CreateRingsForGrouping()
    {
        this.name = "DataObject: Grouping";

        ClearRings();

        var vals = _cmLoader.getAllGroupings();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = _cmLoader.getCMDataForGrouping(vals[i]);
        CreateRings(vals, lists);
    }

    public void CreateRingsForComic()
    {
        this.name = "DataObject: Comic";

        ClearRings();

        var vals = _cmLoader.getAllComics();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = _cmLoader.getCMDataForComic(vals[i]);
        CreateRings(vals, lists);
    }

    public void CreateRingsForDistributor()
    {
        this.name = "DataObject: Distributor";

        ClearRings();

        var vals = _cmLoader.getAllDistributors();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = _cmLoader.getCMDataForDistributor(vals[i]);
        CreateRings(vals, lists);
    }


    public void CreateRingsForStudio()
    {
        this.name = "DataObject: Studio";

        ClearRings();

        var vals = _cmLoader.getAllStudios();
        var lists = new List<CMData>[vals.Length];
        for (var i = 0; i < vals.Length; i++) lists[i] = _cmLoader.getCMDataForStudio(vals[i]);
        CreateRings(vals, lists);
    }

    public void ClearRings()
    {
        MovieConnectionManager connMan;

        foreach (var m in _movieObjectMap.Values)
        {
            connMan = m.connManager;
            connMan.ForceClearAllConnections();
        }

        for (var ring = 0; ring < _ringList.Count; ring++)
            Destroy(_ringList[ring]);

        _ringList.Clear();
        _ringColorMap.Clear();
        _movieObjectMap.Clear();
        _activeRings.Clear();
        _movieConnectionList.Clear();
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
        _movieConnectionList.Add(gObj);
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

        _movieObjectMap.TryGetValue(MovieDBUtils.getMovieDataKey(from), out moFrom);
        _movieObjectMap.TryGetValue(MovieDBUtils.getMovieDataKey(to), out moTo);

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
        //rend.material.shader = Shader.Find("Custom_AlphaBlend");
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

        _movieObjectMap.TryGetValue(mainKey, out fMo);

        var trackList = new List<GameObject>();

        for (var i = 0; i < fMo.cmData.roles.Length; i++)
        {
            if (!fMo.cmData.roles[i].active) continue;
            var list = _cmLoader.getCMDataForActor(fMo.cmData.roles[i].actor);

            foreach (var data in list)
            {
                tKey = MovieDBUtils.getMovieDataKey(data);
                if (mainKey.Equals(tKey)) continue;

                _movieObjectMap.TryGetValue(tKey, out tMo);
                trackList.Add(ConnectMovies(fMo.cmData, tMo.cmData, Color.red));
            }
        }
    }

    public void ClearAllConnections()
    {
        foreach (var gObj in _movieConnectionList) Destroy(gObj);
        _movieConnectionList.Clear();
    }

    public void AddActiveRings(List<GameObject> list)
    {
        _activeRings.AddRange(list);
    }

    private void UnhighlightAllRings()
    {
        RingState rs;
        foreach (var ring in _ringList)
        {
            rs = ring.GetComponent<RingState>();
            rs.UpdateColor();
        }
    }

    private void DimAllRings()
    {
        RingState rs;
        foreach (var ring in _ringList)
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
        foreach (var ring in _activeRings)
        {
            rs = ring.GetComponent<RingState>();
            rs.SetHighlighted();
            rs.UpdateColor();
        }

        _activeRings.Clear();
    }
}