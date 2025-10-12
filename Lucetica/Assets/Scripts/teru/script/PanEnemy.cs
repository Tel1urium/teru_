using System.Xml;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class PanEnemy : Enemy
{
    EStateMachine<PanEnemy> stateMachine;
    [SerializeField] GameObject efe;
    [SerializeField] GameObject attackObject;
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
        stateMachine = new EStateMachine<PanEnemy>(this);
        stateMachine.Add<IdleState>((int)EnemyState.Idle);
        stateMachine.Add<PatrolState>((int)EnemyState.Patrol);
        stateMachine.Add<ChaseState>((int)EnemyState.Chase);
        stateMachine.Add<AttackState>((int)EnemyState.Attack);
        stateMachine.Add<AttackInterbalState>((int)EnemyState.AttackInterbal);
        stateMachine.Add<HitState>((int)EnemyState.Hit);
        stateMachine.Add<DeadState>((int)EnemyState.Dead);
        stateMachine.OnStart((int)EnemyState.Idle);
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
        var go=Instantiate(attackObject);
        go.transform.position=transform.position+ new Vector3(0,0,0);
        
    }
    
    private class IdleState : EStateMachine<PanEnemy>.StateBase
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
    private class PatrolState : EStateMachine<PanEnemy>.StateBase
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
                endPos = Owner.GetRandomNavMeshPoint(startPos, 7f);
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
    private class ChaseState : EStateMachine<PanEnemy>.StateBase
    {
        NavMeshAgent navMeshAgent;
        public override void OnStart()
        {
            Owner.ChangeTexture(1);
            Owner.enemyAnimation.SetTrigger("Walk");
            navMeshAgent = Owner.navMeshAgent;
            navMeshAgent.isStopped = false;
        }
        public override void OnUpdate()
        {
            Owner.transform.LookAt(Owner.playerPos.transform.position);
            if (Owner.GetDistance() <= Owner.attackRange)
            {
                StateMachine.ChangeState((int)EnemyState.Attack);
                navMeshAgent.isStopped = true;
            }
            if(Owner.GetDistance() >= Owner.lookPlayerDir){StateMachine.ChangeState((int)EnemyState.Idle); }
            Vector3 playerPos = Owner.playerPos.transform.position;
            navMeshAgent.SetDestination(playerPos);
        }
        public override void OnEnd()
        {
            Owner.enemyAnimation.ResetTrigger("Walk");
        }
    }
    private class AttackState : EStateMachine<PanEnemy>.StateBase
    {
        public override void OnStart()
        {
            Owner.ChangeTexture(1);
            Owner.enemyAnimation.SetTrigger("Attack");
            Owner.transform.LookAt(Owner.playerPos.transform.position);
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
    private class AttackInterbalState : EStateMachine<PanEnemy>.StateBase
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
    private class HitState : EStateMachine<PanEnemy>.StateBase
    {
        public override void OnStart()
        {
            Owner.ChangeTexture(2);
        }
        public override void OnUpdate()
        {
            if (Owner.AnimationEnd("")) { StateMachine.ChangeState((int)EnemyState.Idle); }
        }
        public override void OnEnd()
        {
            Debug.Log("Hit�͏I���");
        }
    }
    private class DeadState : EStateMachine<PanEnemy>.StateBase
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
