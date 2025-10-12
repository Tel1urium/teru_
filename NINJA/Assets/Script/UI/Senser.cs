using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Senser : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Collider col;
    // Start is called before the first frame update
    void Start()
    {
        col.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            col.enabled = true;
            enemy.GetComponent<OniStatus>().navgationEnabled = true;
        }
    }
}
