using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
[Serializable]
public struct DamageData
{
    public float damageAmount; // �_���[�W��
    // ���̃_���[�W�֘A���i�����Ȃǁj��ǉ��\
    public DamageData(float damage)
    {
        damageAmount = damage;
    }
}
public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject[] weaponDrops; // �h���b�v����镐��̃v���n�u�i2��ށj
    [SerializeField] protected Texture[] textures;
    [SerializeField] protected float maxHp;
    [SerializeField] protected float maxSpeed;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected int attackDamage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float lookPlayerDir;
    [SerializeField] protected float angle;
    [SerializeField] protected GameObject playerPos;
    [SerializeField] protected Animator enemyAnimation;
    [SerializeField] protected GameObject thisObject;
    [SerializeField] protected GameObject deathEffect;
    protected float nowHp;
    protected float nowSpeed;
    protected float distance;
    protected NavMeshAgent navMeshAgent;
    protected AnimatorStateInfo info;
    protected bool animetionEnd;


    protected GameObject TestTarget;

    private bool _isDead; // �d�����S�h�~�t���O

    void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        playerPos=EventBus.PlayerEvents.GetPlayerObject?.Invoke();
    }

    protected virtual void UpdateTestTarget()
    {
        TestTarget = EventBus.PlayerEvents.GetPlayerObject?.Invoke();
        if (TestTarget != null)
        {
            Debug.Log($"Enemy: Target{TestTarget.name} found");
        }
        else
        {
            Debug.Log("Enemy: No target found");
        }
    }
    protected virtual void OnDamage()
    {
        nowHp-=5;
    }
    public virtual void OnAttackSet(){  }
    public virtual void OnAttackEnd() { }
    public virtual void OnSumon() { }
    public virtual int TakeDamage(DamageData dmg)
    {
        nowHp -= (int)dmg.damageAmount;
        //if (nowHp <= 0)
        //{
        //    OnDead();
        //}
        return (int)dmg.damageAmount;
    }
    public virtual void OnDead()
    {
        if (_isDead) return;
        _isDead = true;
        DropWeapon();
        Instantiate(deathEffect, this.gameObject.transform.position, Quaternion.identity);
        EventBus.EnemyEvents.OnEnemyDeath?.Invoke(this.gameObject);
        Destroy(gameObject);
    }
    protected float GetDistance()
    {
        Vector3 offset = playerPos.transform.position - transform.position;
        offset.y = 0; 
        return offset.magnitude;
        
    }
    void DropWeapon()
    {
        // �z�񂪋�Ȃ�h���b�v���Ȃ�
        if (weaponDrops == null || weaponDrops.Length == 0) return;
        int index = UnityEngine.Random.Range(0, weaponDrops.Length); // �����_���I��
        Instantiate(weaponDrops[index], transform.position, Quaternion.identity);

    }
    public bool AnimationEnd(string stateName)
    {
        // ���݂̃X�e�[�g�����擾
        AnimatorStateInfo stateInfo = enemyAnimation.GetCurrentAnimatorStateInfo(0);

        // �X�e�[�g�����n�b�V�������Ĕ�r
        int stateHash = Animator.StringToHash("Base Layer." + stateName);

        // �Y���X�e�[�g�ł��� normalizedTime >=1 �Ȃ�I���Ƃ݂Ȃ�
        if (stateInfo.fullPathHash == stateHash && stateInfo.normalizedTime >= 1f)
        {
            return true;
        }

        return false;
    }
    public int GetDamage()
    {
        return attackDamage;
    }
    public Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 rand = center + new Vector3(
                UnityEngine.Random.Range(-radius, radius),
                0,
                UnityEngine.Random.Range(-radius, radius)
            );
            rand.y = center.y; // �������Œ�

            NavMeshHit hit;
            if (NavMesh.SamplePosition(rand, out hit, radius, NavMesh.AllAreas))
                return hit.position;
        }

        return center;
    }
    protected void ChangeTexture(int index)
    {
        if (index < 0 || index >= textures.Length){index=0; }
        Texture newTexture = textures[index];
        Renderer[] renderers = thisObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.mainTexture = newTexture;
            }
        }
    }
    
    public static bool Probability(float fPersent)//�m������p���\�b�h
    {
        float fProbabilityRate = UnityEngine.Random.value * 100;
        if (fPersent == 100f && fProbabilityRate == fPersent)
        {
            return true;
        }
        else if (fPersent > fProbabilityRate)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
