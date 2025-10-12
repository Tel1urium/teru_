using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InField : MonoBehaviour
{
    public static bool inField = false;

    public bool GetInField()//ステージ入った時のtrueを返す
    {
        return inField;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            inField = true;
        }
    }
}
