using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RedPoint : MonoBehaviour
{
    public GameObject dangerJumpArea;
    public float time;
    bool point;
    RaycastHit hit;
    GameObject area;
    // Start is called before the first frame update
    void Start()
    {
        point = true;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position + new Vector3(0,-4,0), transform.up*-1);
        Debug.DrawRay(ray.origin, ray.direction*1000, Color.red, 3, false);
        if (point)
        {
            //int layerMask = 1 << 7;
            //layerMask = ~layerMask;
            Physics.Raycast(ray, out hit);
            GameObject area = Instantiate(dangerJumpArea);
            area.transform.position = hit.point;
            area.transform.localScale = new Vector3(1.5f, 0.01f, 1.5f);
            Destroy(area, time);
            point = false;
        }
        
    }
}
