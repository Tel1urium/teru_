using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField] private Collider swordCollider;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AttackEnabled()
    {
        swordCollider.enabled = true;
    }
    public void AttackNotEnabled()
    {
        swordCollider.enabled = false;
    }
}
