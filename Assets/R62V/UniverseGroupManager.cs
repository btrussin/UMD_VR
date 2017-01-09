using UnityEngine;
using System.Collections;

public class UniverseGroupManager : MonoBehaviour {

    GameObject currUniverse = null;

    public GameObject universe1;
    public GameObject universe2;
    public GameObject universe3;

    public Material activeMaterial;
    public Material inactiveMaterial;

    GameObject currBall = null;

    int universeLayerMask;
    int ballLayerMask;

    // Use this for initialization
    void Start () {
        universeLayerMask = 1 << LayerMask.NameToLayer("BallUniverse");
        ballLayerMask = 1 << LayerMask.NameToLayer("BallGraph");

        universe1.transform.Translate(-2.0f, 1.0f, 2.0f);
        universe2.transform.Translate( 0.0f, 1.0f, 2.0f);
        universe3.transform.Translate( 2.0f, 1.0f, 2.0f);


    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void releaseBall()
    {
        if (currBall != null)
        {
            currBall.GetComponent<Renderer>().material = inactiveMaterial;
        }
    }

    public GameObject getUniverse(Ray ray, out Vector3 intPt)
    {
        RaycastHit hitInfo;

        intPt = new Vector3(0.0f, 0.0f, 0.0f);

        if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, 30.0f, universeLayerMask))
        {
            currUniverse = hitInfo.collider.gameObject;
            intPt = hitInfo.point;
        }
        else currUniverse = null;

        return currUniverse;

    }

    public GameObject getBall(Ray ray, out Vector3 intPt)
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, 30.0f, ballLayerMask))
        {
            releaseBall();

            currBall = hitInfo.collider.gameObject;
            Debug.Log("Hit Ball: ");
            Renderer r = hitInfo.collider.gameObject.GetComponent<Renderer>();
            r.material = activeMaterial;

            intPt = hitInfo.point;
            return currBall;
        }
        intPt = new Vector3(0.0f, 0.0f, 0.0f);
        return null;
    }
}
