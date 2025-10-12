using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float deathTime = 3.0f;
    // Update is called once per frame
    void Start()
    {
        Destroy(gameObject,deathTime);
    }
    
}
