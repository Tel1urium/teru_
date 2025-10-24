
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    EStateMachine<Player> stateMachine;
    [SerializeField] float accelaration;
    [SerializeField] PlayerInput action;
    [SerializeField] float rotateSpeed;
    [SerializeField] float mChragePow;
    [SerializeField] float moveForce;
    [SerializeField] float maxSpeed;
    [SerializeField] float drag;  // 抵抗（慣性調整）
    [SerializeField] float cDrag;  // 抵抗（慣性調整）
    [SerializeField] private ParticleSystem chargeEffect;
    float h,v;
    Rigidbody rb;
    InputAction move;
    InputAction charge;
    InputAction rize;
    float bTime;
    float cTime;
    float chargePow;
    GameManager gameManager;
    Vector3 moveDir;
    public LayerMask collisionMask;
    public Gamepad assignedGamepad;
    float yPos;
    float yCurrent;
    bool Dead = false;
    Vector3 firstPos;
    enum State
    {
        Idle,
        Move,
        Charge,
        Fire,
        Bound,
        Die,
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        rb.linearDamping = drag;   // 慣性の減衰を設定
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        move = action.actions["Move"];
        charge = action.actions["Charge"];
        rize = action.actions["Rize"];
        stateMachine = new EStateMachine<Player>(this);
        stateMachine.Add<IdleState>((int)State.Idle);
        stateMachine.Add<MoveState>((int)State.Move);
        stateMachine.Add<ChargeState>((int)State.Charge);
        stateMachine.Add<FireState>((int)State.Fire);
        stateMachine.Add<BoundState>((int)State.Bound);
        stateMachine.Add<DieState>((int)State.Die);
        stateMachine.OnStart((int)State.Idle);
        yCurrent =this.transform.position.y;
        firstPos = this.transform.position;
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        if (gameManager == null || gameManager.GetGameState() != GameManager.State.Ingame)
            return; // ゲームが始まっていないので物理処理もしない
        yPos = this.transform.position.y;
        if(yCurrent < yPos)this.transform.position = new Vector3(this.transform.position.x, yCurrent, this.transform.position.z);
        else if(yCurrent > yPos) yCurrent = yPos;
        CollisionPredictionAndReflect();
        stateMachine.OnUpdate();
        //gameObject.SetActive(Dead);
    }

    void LateUpdate()
    {
        if (chargeEffect == null) return;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[chargeEffect.main.maxParticles];
        int count = chargeEffect.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 dirToCenter = (transform.position - particles[i].position).normalized;
            particles[i].velocity = dirToCenter * 3f; // 中心に向けて吸引（速度3）
        }

        chargeEffect.SetParticles(particles, count);
    }
    
    private class IdleState : EStateMachine<Player>.StateBase
    {
        public override void OnStart()
        {
            Debug.Log("Idle");
        }
        public override void OnUpdate()
        {
            if (Owner.Dead) { StateMachine.ChangeState((int)State.Die); }
            if (Owner.move.ReadValue<Vector2>() != new Vector2(0, 0)) { StateMachine.ChangeState((int)State.Move); }
            if (Owner.charge.IsPressed()) { StateMachine.ChangeState((int)State.Charge); }
        }
        public override void OnEnd()
        {
            
        }
    }
    private class MoveState : EStateMachine<Player>.StateBase
    {
        public override void OnStart()
        {
            Debug.Log("Move");
        }
        public override void OnUpdate()
        {
            if (Owner.Dead) { StateMachine.ChangeState((int)State.Die); }
            if (Owner.charge.IsPressed()) { StateMachine.ChangeState((int)State.Charge); }
            if (Owner.move.activeControl?.device != Owner.assignedGamepad &&
            Owner.charge.activeControl?.device != Owner.assignedGamepad)
            {
                Debug.Log("nun");
                return;
            }
            if(Owner.move.ReadValue<Vector2>()==new Vector2(0,0)) { StateMachine.ChangeState((int)State.Idle); }
            Owner.Angle();
            Owner.OnMove();
        }
    }
    private class ChargeState : EStateMachine<Player>.StateBase
    {
        public override void OnStart()
        {
            Debug.Log("Chage");
        }
        public override void OnUpdate()
        {
            if (Owner.Dead) { StateMachine.ChangeState((int)State.Die); }
            Owner.rb.linearVelocity *= Owner.cDrag;
            Owner.chargePow += Time.deltaTime * 20;
            if (Owner.chargePow >= Owner.mChragePow)
            {
                Owner.chargePow = Owner.mChragePow;
            }
            if (Owner.move.ReadValue<Vector2>() != new Vector2(0, 0)) { StateMachine.ChangeState((int)State.Charge); }
            if (Owner.move.activeControl?.device != Owner.assignedGamepad &&
          Owner.charge.activeControl?.device != Owner.assignedGamepad)
            {
                return;
            }
            if (!Owner.charge.IsPressed()) { StateMachine.ChangeState((int)State.Fire); }
              if (!Owner.chargeEffect.isPlaying)
              {
                  Owner.chargeEffect.gameObject.SetActive(true);
                  Owner.chargeEffect.Play();
              }
              
             
            Owner.Angle();
            if (Owner.moveDir.sqrMagnitude > 0.01f)
            {
                Owner.UpdateRotation(Owner.moveDir);
            }
            if (Owner.charge.IsPressed()) { StateMachine.ChangeState((int)State.Charge); }
        }
        public override void OnEnd()
        {

            Owner.rb.linearDamping = Owner.drag;
        }

    }
    private class BoundState : EStateMachine<Player>.StateBase
    {
        public override void OnStart()
        {
            Debug.Log("Bound");
        }
        public override void OnUpdate()
        {
            if (Owner.Dead) { StateMachine.ChangeState((int)State.Die); }
            Owner.bTime += Time.deltaTime;
            if (Owner.bTime >= 0.5f)
            {
                 if (Owner.rb.linearVelocity.magnitude < 0.1f)
                 {
                     StateMachine.ChangeState((int)State.Idle);  //停止していればIdleへ
                 }
                 else
                 {
                     StateMachine.ChangeState((int)State.Move);  //動いていればMoveへ戻す
                 }
             }
        }
        public override void OnEnd()
        {

        }
    }
    private class FireState : EStateMachine<Player>.StateBase
    {
        public override void OnStart()
        {
            Debug.Log("Fire");
        }
        public override void OnUpdate()
        {
            //if (Owner.chargeEffect.isPlaying)
            //    Owner.chargeEffect.Stop();
            //Vector3 velocity = Owner.rb.linearVelocity;
            //Vector3 moveDirFromVelocity = velocity.normalized;
            //Owner.rb.AddForce(moveDirFromVelocity * Owner.chargePow, ForceMode.VelocityChange);
            //Owner.chargePow = 0;
            //StateMachine.ChangeState((int)State.Bound);
            if (Owner.Dead) { StateMachine.ChangeState((int)State.Die); }
            if (Owner.chargeEffect.isPlaying)
                Owner.chargeEffect.Stop();

            Vector3 fireDirection = Owner.moveDir;
            if (fireDirection.sqrMagnitude < 0.01f)
            {
                fireDirection = Owner.rb.linearVelocity.normalized;
            }
            else
            {
                fireDirection.Normalize();
            }

            Owner.rb.AddForce(fireDirection * Owner.chargePow, ForceMode.VelocityChange);
            Owner.chargePow = 0;
            StateMachine.ChangeState((int)State.Bound);
        }
        public override void OnEnd()
        {

        }
    }
    private class DieState : EStateMachine<Player>.StateBase
    {
        public override void OnStart()
        {
            
        }
        public override void OnUpdate()
        {
            if (Owner.chargeEffect.isPlaying) { Owner.chargeEffect.Stop(); }
            Owner.rb.linearVelocity = Vector3.zero;
            Owner.rb.linearVelocity *= Owner.cDrag;
            //Destroy(Owner.gameObject);
            if (Owner.rize.IsPressed()) { StateMachine.ChangeState((int)State.Idle); }
        }
        public override void OnEnd()
        {
            if (Owner.gameObject.CompareTag("ball1")) { ScoreManager.Player1Score -= 2; }
            if (Owner.gameObject.CompareTag("ball2")) { ScoreManager.Player2Score -= 2; }
            Owner.yPos = Owner.firstPos.y;
            Owner.yCurrent = Owner.firstPos.y;
            Owner.transform.position = Owner.firstPos;
            Owner.Dead = false;
        }

    }
    public void OnMove()
    {
        if (moveDir.sqrMagnitude > 0.01f)
        {
            // 最大速度制限
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(moveDir * moveForce, ForceMode.Acceleration);
            }
        }
    }
    void Angle()
    {
        var inputAxis = action.actions["Move"].ReadValue<Vector2>();
        h = inputAxis.x;
        v = inputAxis.y;   
        //カメラの正面を取得
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        //カメラの右側を取得
        Vector3 camRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;
        //移動方向を格納
        moveDir = camForward * v + camRight * h;
        moveDir.Normalize();

    }
  

    public void CollisionPredictionAndReflect()
    {
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < 0.01f) return;

        Vector3 direction = velocity.normalized;
        Ray ray = new Ray(transform.position, Vector3.zero);
        var sphereRadius = 0.7f;
        RaycastHit hit;
        var rayLength = 0.00000f;
        if (Physics.SphereCast(ray, sphereRadius, out hit, rayLength, collisionMask))
        {
            Vector3 hitNormal = hit.normal;
            Vector3 reflected = Vector3.Reflect(velocity, hitNormal);

            rb.linearVelocity = Vector3.zero; // 一度停止
            rb.AddForce(reflected.normalized * 50, ForceMode.VelocityChange);

            Debug.DrawRay(transform.position, direction * hit.distance, Color.red, 0.2f);
            Debug.DrawRay(hit.point, hitNormal, Color.yellow, 0.2f);

        }
        else
        {
            Debug.DrawRay(transform.position, direction * rayLength, Color.green, 0.1f);
        }
    }
    public void UpdateRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
    }
    public void OnHit()
    {
        Dead = true;
    }
    public float GetChargePow() => chargePow;
}
