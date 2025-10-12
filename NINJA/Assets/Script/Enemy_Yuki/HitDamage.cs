using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDamage : MonoBehaviour
{
    public float damage;
    CharacterController controller;
    private void Start()
    {
        controller = new CharacterController();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            controller.AddDamage(damage);
        }
    }
}
