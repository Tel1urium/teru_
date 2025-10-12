using System.Xml;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class ChikenEnemy : Enemy
{
    EStateMachine<ChikenEnemy> stateMachine;
    [SerializeField] Collider attackCollider;
    private enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        AttackInterbal,
        Hit,
        Dead
    }
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        nowHp = maxHp;
        stateMachine = new EStateMachine<ChikenEnemy>(this);
        stateMachine.Add<IdleState>((int)EnemyState.Idle);
        stateMachine.Add<PatrolState>((int)EnemyState.Patrol);
        stateMachine.Add<ChaseState>((int)EnemyState.Chase);
        stateMachine.Add<AttackState>((int)EnemyState.Attack);
        stateMachine.Add<AttackInterbalState>((int)EnemyState.AttackInterbal);
        stateMachine.Add<HitState>((int)EnemyState.Hit);
        stateMachine.Add<DeadState>((int)EnemyState.Dead);
        stateMachine.OnStart((int)EnemyState.Idle);
        attackCollider.enabled = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (nowHp <= 0) { stateMachine.ChangeState((int)EnemyState.Dead); }
        stateMachine.OnUpdate();
    }
    public override void OnAttackSet()
    {
        attackCollider.enabled = true;
    }
    public override void OnAttackEnd()
    {
        attackCollider.enabled = false;
    }
    private class IdleState : EStateMachine<ChikenEnemy>.StateBase
    {
        float cDis;
        public override void OnStart()
        {
            Owner.ChangeTexture(0);
            Owner.enemyAnimation.SetTrigger("Idle");
            cDis = Owner.lookPlayerDir;
        }
        public override void OnUpdate()
        {
            float playerDis = Owner.GetDistance();
            var playerDir = Owner.playerPos.transform.position - Owner.transform.position;
            var angle = Vector3.Angle(Owner.transform.forward, playerDir);
            if (playerDis <= cDis && angle <= Owner.angle) { StateMachine.ChangeState((int)EnemyState.Chase); }
            else { StateMachine.ChangeState((int)EnemyState.Patrol); }
        }
        public override void OnEnd()
        {
            Owner.enemyAnimation.ResetTrigger("Idle");
        }
    }
    private class PatrolState : EStateMachine<ChikenEnemy>.StateBase
    {
        NavMeshAgent navMeshAgent;
        float cDis;
        Vector3 endPos;
        Vector3 startPos;
        bool goingToEnd = true;
        bool firstInit = true;
        public override void OnStart()
        {
            Owner.ChangeTexture(0);
            navMeshAgent = Owner.navMeshAgent;
            navMeshAgent.isStopped = false;
            if (firstInit)
            {
                startPos = Owner.transform.position;
                endPos = Owner.GetRandomNavMeshPoint(startPos,10f);
                firstInit = false;
            }

            cDis = Owner.lookPlayerDir;
        }
        public override void OnUpdate()
        {
            Owner.enemyAnimation.SetTrigger("Walk");
            float playerDis = Owner.GetDistance();
            var playerDir = Owner.playerPos.transform.position - Owner.transform.position;
            var angle = Vector3.Angle(Owner.transform.forward, playerDir);
            if (playerDis <= cDis && angle <= Owner.angle)            // �v���C���[���o
            {
                StateMachine.ChangeState((int)EnemyState.Chase);
                return;
            }
            // �p�g���[��
            Vector3 targetPos = goingToEnd ? endPos : startPos;
            navMeshAgent.SetDestination(targetPos);
            // ��������
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                goingToEnd = !goingToEnd;
                StateMachine.ChangeState((int)EnemyState.Idle);
            }
        }
        public override void OnEnd()
        {
            Owner.enemyAnimation.ResetTrigger("Walk");
        }

    }
    private class ChaseState : EStateMachine<ChikenEnemy>.StateBase
    {
        NavMeshAgent navMeshAgent;
        public override void OnStart()
        {
            Owner.ChangeTexture(1);
            Owner.enemyAnimation.SetTrigger("Run");
            navMeshAgent = Owner.navMeshAgent;
            navMeshAgent.isStopped = false;
        }
        public override void OnUpdate()
        {
            if (Owner.GetDistance() <= Owner.attackRange)
            {
                StateMachine.ChangeState((int)EnemyState.Attack);
                navMeshAgent.isStopped = true;
            }
            if (Owner.GetDistance() >= Owner.lookPlayerDir) { StateMachine.ChangeState((int)EnemyState.Idle); }
            Vector3 playerPos = Owner.playerPos.transform.position;
            navMeshAgent.SetDestination(playerPos);
        }
        public override void OnEnd()
        {
            Owner.enemyAnimation.ResetTrigger("Run");
        }
    }
    private class AttackState : EStateMachine<ChikenEnemy>.StateBase
    {
        public override void OnStart()
        {
            Owner.enemyAnimation.SetTrigger("Attack");
            Owner.transform.LookAt(Owner.playerPos.transform.position);
            Owner.navMeshAgent.isStopped = true;
        }
        public override void OnUpdate()
        {
            if (Owner.AnimationEnd("Attack")) { StateMachine.ChangeState((int)EnemyState.AttackInterbal); }
        }
        public override void OnEnd()
        {
            Owner.enemyAnimation.ResetTrigger("Attack");
        }
    }
    private class AttackInterbalState : EStateMachine<ChikenEnemy>.StateBase
    {
        float time;
        public override void OnStart()
        {
            Owner.ChangeTexture(1);
            Owner.enemyAnimation.SetTrigger("Idle");
        }
        public override void OnUpdate()
        {
            time += Time.deltaTime;
            if (time > Owner.attackSpeed) { StateMachine.ChangeState((int)EnemyState.Idle); time = 0; }
        }
        public override void OnEnd()
        {
            Owner.enemyAnimation.ResetTrigger("Idle");
        }
    }
    private class HitState : EStateMachine<ChikenEnemy>.StateBase
    {
        public override void OnStart()
        {
            Owner.ChangeTexture(2);
            Debug.Log("Hit����");
        }
        public override void OnUpdate()
        {
            if (Owner.AnimationEnd("")) { StateMachine.ChangeState((int)EnemyState.Idle);}
        }
        public override void OnEnd()
        {
            Debug.Log("Hit�͏I���");
        }
    }
    private class DeadState : EStateMachine<ChikenEnemy>.StateBase
    {
        public override void OnStart()
        {
            Owner.ChangeTexture(2);
            Owner.enemyAnimation.SetTrigger("Dead");
            Debug.Log("Dead����");
        }
        public override void OnUpdate()
        {
            if (Owner.AnimationEnd("Dead"))
            {
                Owner.OnDead();
            }
        }
        public override void OnEnd()
        {
            Debug.Log("Dead�͏I���");
        }
    }
}
