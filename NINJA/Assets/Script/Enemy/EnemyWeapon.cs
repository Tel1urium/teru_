using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public Collider weaponCollider;
    public float attackDuration;
    private bool canAttack = true;
    public float attackCooldown;
    // Start is called before the first frame update
    void Start()
    {
        weaponCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
