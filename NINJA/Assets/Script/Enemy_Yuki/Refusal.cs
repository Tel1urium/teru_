using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refusal : MonoBehaviour
{
    public float refusalPow;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            Transform transform = rb.transform;
            rb.AddForce(-transform.forward * refusalPow, ForceMode.Impulse);
        }
    }
}
