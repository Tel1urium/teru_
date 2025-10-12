using Unity.VisualScripting;
using UnityEngine;

public class OnHitDamage : MonoBehaviour
{
    [SerializeField ]private Enemy enemy;
    [SerializeField]private float damage;
    public void OnTriggerEnter(Collider other)
    {
        // �v���C���[�ւ̃_���[�W����
        var player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            float finalDamage = (enemy != null) ? enemy.GetDamage() : damage;
            DamageData damageData = new DamageData(finalDamage);
            player.TakeDamage(damageData);
        }
        // �G�ւ̃_���[�W����
        var otherEnemy = other.GetComponent<Enemy>();
        if (otherEnemy != null && otherEnemy != enemy) // �����ȊO��Enemy
        {
            float finalDamage = (enemy != null) ? enemy.GetDamage() : damage;
            DamageData damageData = new DamageData(finalDamage);
            otherEnemy.TakeDamage(damageData);
        }
    }
}
