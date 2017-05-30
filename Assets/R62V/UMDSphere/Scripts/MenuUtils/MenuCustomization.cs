using UnityEngine;
using System.Collections;

public class MenuCustomization : MonoBehaviour {

    public GameObject menu;
    public Camera cam;

    float lScale;
    Vector3 currentSize;

    //Recent points
    float minX, minZ;
    float maxX, maxZ;
    bool usedVertical = false;

    void Awake()
    {
        ResetStartingStats();
        currentSize = menu.transform.localScale;
    }

    void ResetStartingStats()
    {
        minX = 0.25f;
        minZ = 0.15f;
        maxX = 0.25f * 3;
        maxZ = 0.15f * 3.1f;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            {
                DecreaseWindowSize();
            }

        if (Input.GetKey(KeyCode.RightArrow))
            {
                IncreaseWindowSize();
            }

        float distance = Mathf.Abs(Vector3.Distance(cam.transform.position, menu.transform.position));
        if (Input.GetAxis("Vertical") < 0 && distance > 0.25f)
        {
            menu.transform.Translate(Vector3.up * 2f * Time.deltaTime);
            float aSize = Mathf.Atan(1f / (2f * distance));
            lScale = .2f * (1f / aSize);
            menu.transform.localScale = lScale * currentSize;
            Debug.Log(menu.transform.localScale.x);
            minX = menu.transform.localScale.x;
            minZ = menu.transform.localScale.z;
            maxX = menu.transform.localScale.x * 3f;
            maxZ = menu.transform.localScale.z * 3f;
            usedVertical = true;
            //currentSize = menu.transform.localScale;
        }
        else if (Input.GetAxis("Vertical") > 0 && distance < 25f)
        {
            menu.transform.Translate(Vector3.up * -2f * Time.deltaTime);
            float aSize = Mathf.Atan(1f / (2f * distance));
            lScale = .2f * (1f / aSize);
            menu.transform.localScale = lScale * currentSize;
            Debug.Log(menu.transform.localScale.x);
            minX = menu.transform.localScale.x;
            minZ = menu.transform.localScale.z;
            maxX = menu.transform.localScale.x * 3f;
            maxZ = menu.transform.localScale.z * 3f;
            usedVertical = true;
            //currentSize = menu.transform.localScale;
        }


        //Configuration options
        if (Input.GetKeyDown(KeyCode.Return))
        {
            transform.parent.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            usedVertical = false;
            Application.LoadLevel(0);
        }
    }

    void DecreaseWindowSize()
    {
        Vector3 tempLocalScale = currentSize;
        if (transform.localScale.x >= minX)
        {
            tempLocalScale.x -= Time.deltaTime * 0.1f;
        }

        if (transform.localScale.z >= minZ) {
            tempLocalScale.z -= Time.deltaTime * 0.1f;
        }

        currentSize = tempLocalScale;
        transform.localScale = tempLocalScale;
    }

    void IncreaseWindowSize()
    {
        Vector3 tempLocalScale = currentSize;
        if (transform.localScale.x <= maxX)
        {
            tempLocalScale.x += Time.deltaTime * 0.1f;
        }

        if (transform.localScale.z <= maxZ)
        {
            tempLocalScale.z += Time.deltaTime * 0.1f;
        }

        currentSize = tempLocalScale;
        transform.localScale = tempLocalScale;
    }

}
