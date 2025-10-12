using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using System.Reflection;
using static EventBus;

public enum PlayerState
{
    Idle,        // �ҋ@
    Move,        // ����
    Hit,         // ��e
    Pickup,      // �s�b�N�A�b�v
    SwitchWeapon,// ����؂�ւ�
    Attack,      // �U��
    Skill,       // �X�L��
    Dash,        // �_�b�V��
    Dead,        // ���S
    Falling      // ����
}

public enum PlayerTrigger
{
    MoveStart,
    MoveStop,
    GetHit,
    StartPickup,
    EndPickup,
    SwitchWeapon,
    AttackInput,
    AttackUp,
    Hold,
    DashInput,
    Die,
    NoGround,
    Grounded,
    SkillInput
}



[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // ====== ���͌n ======
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    [HideInInspector] public bool attackHeld;                // �������ςȂ��t���O
    [HideInInspector] public bool attackPressedThisFrame;    // ���̃t���[���ɉ������ꂽ��

    

    // ====== �U������ ======
    [Header("�U������")]
    [Tooltip("�Z������")]
    public float shortPressMax = 0.15f;
    [Tooltip("�X�L���̒�����")]
    public float skillHoldTime = 1.0f;
    //public float skillHoldThreshold = 0.35f;
    
    // �����t���O
    private bool holdUIShown = false;     // ������UI�\������
    private bool skillTriggered = false;
    private double attackPressStartTime = -1.0;

    [Header("�X�e�[�^�X")]
    public float maxHealth = 50;
    private float currentHealth = 50;
    public float CurrentHealth => currentHealth;


    // ====== �ړ��ݒ� ======
    [Header("�ړ��ݒ�")]
    public float moveSpeed = 5f;           // �ړ����x
    public float gravity = -9.81f;         // �d�͉����x
    public float rotationSpeed = 360f;     // ��]���x�i�x/�b�j

    [Header("�_�b�V���ݒ�")]
    public float dashSpeed = 10f;           // �_�b�V�����x
    public float dashDistance = 3f;       // �_�b�V������
    public float dashCooldown = 1.5f;     // �_�b�V���̃N�[���^�C��
    private float dashCooldownTimer = -Mathf.Infinity; // �_�b�V���̃N�[���^�C���Ǘ��p�^�C�}�[
    public float dashInvincibilityTime = 0.3f; // �_�b�V�����G����
    private float dashTimer = 0f;        // ���G���ԊǗ��p�^�C�}�[
    public bool IsDashInvincible => dashTimer > 0f;
    public float dashFreezeAtSeconds = 0.02f;
    public AudioClip dashSound;

    [Header("��e�ݒ�")]
    public float hitInvincibilityTime = 0.8f; // ��e���G����
    private float hitTimer = 0f;         // ��e���G���ԊǗ��p�^�C�}�[
    public bool IsHitInvincible => hitTimer > 0f;
    public bool IsInvincible => IsDashInvincible || IsHitInvincible;
    public GameObject hitEffectPrefab; // ��e�G�t�F�N�g
    public AudioClip hitSound;

    // ====== �ڒn���� ======
    [Header("�ڒn����")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // ====== ����E���f�� ======
    [Header("����֘A")]
    public GameObject weaponBoxR;          // �E��̕���e
    public GameObject weaponBoxL;          // ����̕���e
    public WeaponInstance fist;            // �f��
    public GameObject wpBrokeEffect;      // ����j��G�t�F�N�g
    public bool isHitboxVisible = true; // �q�b�g�{�b�N�X����

    // ����C���x���g��
    public PlayerWeaponInventory weaponInventory = new PlayerWeaponInventory();

    [Header("�v���C���[���f��")]
    public GameObject playerModel;
    public Animator playerAnimator;

    // ====== �A�j���[�V�����iPlayableGraph�j======
    [Header("�A�j���[�V�����N���b�v")]
    public AnimationClip idleClip;
    public AnimationClip moveClip;
    public AnimationClip dashClip;
    public AnimationClip fallClip;
    public AnimationClip hitClip;
    public AnimationClip dieClip;
    public PlayableGraph playableGraph;
    public AnimationMixerPlayable mixer;
    private AnimationClipPlayable idlePlayable;
    private AnimationClipPlayable movePlayable;
    private AnimationMixerPlayable actionSubMixer;

    // ���C�����C���[�p�̒ǉ�Playable
    private AnimationClipPlayable fallPlayable;
    private AnimationClipPlayable hitPlayable;
    private AnimationClipPlayable deadPlayable;
    private AnimationClipPlayable dashPlayable;
    // �A�N�V�����X���b�g�̃_�~�[�i���ڑ����̌����߁j
    private AnimationClipPlayable actionPlaceholder;

    // ���C�����C���[�̃t�F�[�h�p�R���[�`��
    private Coroutine mainLayerFadeCo;


    // ================== ��Ԋԃu�����h�ݒ� ==================
    // ��(from��to)�̃u�����h���Ԃ��㏑������G���g��
    // ���C�����C���[�̃X���b�g�ԍ��iMixer���͂̌Œ芄�蓖�āj
    public enum MainLayerSlot
    {
        Idle = 0, // �A�C�h��
        Move = 1, // �ړ�
        Action = 2, // �U��/�X�L��
        Falling = 3, // ����
        Hit = 4, // ��e
        Dead = 5,  // ���S
        Dash = 6
    }
    [System.Serializable]
    public class StateBlendEntry
    {
        public PlayerState from;                 // ��FMove
        public PlayerState to;                   // ��FHit
        [Min(0f)] public float duration = 0.12f; // ��F0.06f�i��e�͑����j
    }

    //�uto��ԁv�P�ʂ̃f�t�H���g�u�����h�ifrom����v���̃t�H�[���o�b�N�j
    [System.Serializable]
    public class StateDefaultBlend
    {
        public PlayerState to;                   // ��FHit
        [Min(0f)] public float duration = 0.12f; // ��F0.06f
    }

    [Header("��Ԋԃu�����h�i�ρj")]
    [Tooltip("�S�̂̋K��l")]
    public float defaultStateCrossfade = 0.12f;

    [Header("Dash/Hit���荞�݃u�����h")]
    public float interruptCrossfade = 0.05f;

    [Tooltip("�����from��to�ŏ㏑���������u�����h���ԁi�C�ӌ��j")]
    public List<StateBlendEntry> blendOverrides = new List<StateBlendEntry>();

    [Tooltip("to��Ԃ��Ƃ̊���u�����h�ifrom����v���Ɏg�p�j")]
    public List<StateDefaultBlend> toStateDefaults = new List<StateDefaultBlend>();

    // ���{��F���߂̘_����ԁi�u�����h�v�Z�p�j
    public PlayerState lastBlendState = PlayerState.Idle;

    [Header("�T�E���h")]
    public PlayerAudioManager audioManager;

    // ====== ������� ======


    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    public bool IsGrounded => isGrounded;
    private Transform mainCam;
    private StateMachine<PlayerState, PlayerTrigger> _fsm;
    private GameObject lockOnTarget = null; // ���b�N�I���Ώ�
    public GameObject GetLockOnTarget => lockOnTarget;
    private Coroutine rotateYawCo; // ���ݐi�s���̐�����]�R���[�`��

    private static FieldInfo _fiCurrentHealth;
    private Coroutine _rumbleCo;

    // ====== ���C�t�T�C�N�� ======
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        PlayerEvents.GetPlayerObject = () => this.gameObject; // �v���C���[�I�u�W�F�N�g�̒�
    }

    private void OnEnable()
    {
        // --- ���͍w�� ---
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // �U���L�[
        inputActions.Player.Attack.performed += ctx =>
        {
            // �����J�n
            attackHeld = true;
            attackPressedThisFrame = false;
            attackPressStartTime = Time.timeAsDouble;
            holdUIShown = false;
            skillTriggered = false;
        };

        // �U���L�[�������u�Ԃɔ���
        inputActions.Player.Attack.canceled += ctx =>
        {
            attackHeld = false;

            double held = (attackPressStartTime < 0.0) ? 0.0 : (Time.timeAsDouble - attackPressStartTime);
            attackPressStartTime = -1.0;

            // ���S�Ȃǂ͖���
            if (_fsm.CurrentState == PlayerState.Dead) return;

            // Skill�����΂ŗ������ꍇ�̂݁A�Z����������s��
            if (!skillTriggered)
            {
                if (held <= shortPressMax)
                {
                    // �Z�����R���{�U��
                    attackPressedThisFrame = true;
                    _fsm.ExecuteTrigger(PlayerTrigger.AttackInput);
                }
                else
                {
                    if (holdUIShown) UIEvents.OnAttackHoldUI?.Invoke(false);
                }
            }
            else
            {
                // Skill�͊��ɔ��΍ς݁FUI�̌�Еt��
                UIEvents.OnAttackHoldUI?.Invoke(false);
            }

            // ��Ԃ��N���A
            holdUIShown = false;
            skillTriggered = false;
        };

        inputActions.Player.SwitchWeapon.performed += ctx => OnSwitchWeaponInput();
        inputActions.Player.SwitchWeapon2.performed += ctx => OnSwitchWeaponInput2();
        inputActions.Player.SwitchHitbox.performed += ctx => { isHitboxVisible = !isHitboxVisible; };
        inputActions.Player.Dash.performed += ctx => OnDashInput();

        // --- UIEvents �̍w�ǁF�����ؑ�/�j��/�ϋv ---
        UIEvents.OnRightWeaponSwitch += HandleRightWeaponSwitch;   // (weapons, from, to)

        PlayerEvents.OnAimTargetChanged += (newTarget) =>
        {
            lockOnTarget = newTarget;
        };

        PlayerEvents.ApplyHP += HandleApplyHP;
        PlayerEvents.ApplyLoadoutInstances += HandleApplyLoadoutInstances;

        PlayerEvents.OnGamepadShake += GamepadRumbleOnce;
        PlayerEvents.OnGamepadShakeCurve += GamepadRumble;


    }

    private void OnDisable()
    {
        // ���͉���
        inputActions?.Disable();

        // �w�ǉ���
        UIEvents.OnRightWeaponSwitch -= HandleRightWeaponSwitch;

        if (PlayerEvents.GetPlayerObject != null)
        {
            PlayerEvents.GetPlayerObject = null;
        }
        PlayerEvents.ApplyHP -= HandleApplyHP;
        PlayerEvents.ApplyLoadoutInstances -= HandleApplyLoadoutInstances;

        PlayerEvents.OnGamepadShake -= GamepadRumbleOnce;
        PlayerEvents.OnGamepadShakeCurve -= GamepadRumble;

    }

    private void Start()
    {
        mainCam = Camera.main.transform;

        // --- PlayableGraph ������ ---
        playableGraph = PlayableGraph.Create("PlayerGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // ���{��F���[�g���[�V�����͐���R�[�h���ň������ߖ�����
        playerAnimator.applyRootMotion = false;

        // ���{��F�e�N���b�v�̃��b�v���[�h�i�����͍ŏI�t���[���ێ��j
        idleClip.wrapMode = WrapMode.Loop;
        moveClip.wrapMode = WrapMode.Loop;
        fallClip.wrapMode = WrapMode.ClampForever;    // �� �ύX�FLoop��ClampForever
        hitClip.wrapMode = WrapMode.ClampForever;
        dieClip.wrapMode = WrapMode.ClampForever;
        dashClip.wrapMode = WrapMode.ClampForever;

        idlePlayable = AnimationClipPlayable.Create(playableGraph, idleClip);
        movePlayable = AnimationClipPlayable.Create(playableGraph, moveClip);
        fallPlayable = AnimationClipPlayable.Create(playableGraph, fallClip);
        hitPlayable = AnimationClipPlayable.Create(playableGraph, hitClip);
        deadPlayable = AnimationClipPlayable.Create(playableGraph, dieClip);
        dashPlayable = AnimationClipPlayable.Create(playableGraph, dashClip);

        // ���{��F���C���~�L�T�[��6���́iIdle/Move/Action/Falling/Hit/Dead�j
        mixer = AnimationMixerPlayable.Create(playableGraph, 7);

        // 0:Idle, 1:Move
        mixer.ConnectInput((int)MainLayerSlot.Idle, idlePlayable, 0, 1f);
        mixer.ConnectInput((int)MainLayerSlot.Move, movePlayable, 0, 0f);

        // 2:Action�i�����̓_�~�[��ڑ��B�U��/�X�L�����Ɏq�~�L�T�[�֍����ւ��j
        actionSubMixer = AnimationMixerPlayable.Create(playableGraph, 2);
        actionSubMixer.SetInputCount(2);
        mixer.ConnectInput((int)MainLayerSlot.Action, actionSubMixer, 0, 0f);

        // 3:Fall, 4:Hit, 5:Dead
        mixer.ConnectInput((int)MainLayerSlot.Falling, fallPlayable, 0, 0f);
        mixer.ConnectInput((int)MainLayerSlot.Hit, hitPlayable, 0, 0f);
        mixer.ConnectInput((int)MainLayerSlot.Dead, deadPlayable, 0, 0f);
        mixer.ConnectInput((int)MainLayerSlot.Dash, dashPlayable, 0, 0f);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", playerAnimator);
        playableOutput.SetSourcePlayable(mixer);
        playableGraph.Play();

        // --- FSM ������ ---
        SetupStateMachine();

        // --- ���C�t������ ---
        currentHealth = maxHealth;
        UIEvents.OnPlayerHpChange?.Invoke((int)currentHealth, (int)maxHealth);
        PlayerEvents.OnPlayerSpawned?.Invoke(this.gameObject);
    }

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
            playableGraph.Destroy();
    }

    private void Update()
    {
        // �ڒn����
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // �n��ł͐������x�������ȕ��l�ɌŒ肵�A�ڒn������m��
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -4f; // �� ����ɂ������ȉ����t���ŕ��V��h�~
        }

        // ���n���o �� Falling�ցiDead/Falling �����O�j
        if (!isGrounded && _fsm.CurrentState != PlayerState.Dead && _fsm.CurrentState != PlayerState.Falling)
        {
            _fsm.ExecuteTrigger(PlayerTrigger.NoGround);
        }

        if (isGrounded && _fsm.CurrentState == PlayerState.Falling)
        {
            _fsm.ExecuteTrigger(PlayerTrigger.Grounded);
        }

        // �n�ʂ��� Idle/Move ���͈ړ����͂Ŋm���ɑJ�ڂ��쓮�i�ی��j
        if (isGrounded && (_fsm.CurrentState == PlayerState.Idle || _fsm.CurrentState == PlayerState.Move))
        {
            if (moveInput.sqrMagnitude > 0.01f)
                _fsm.ExecuteTrigger(PlayerTrigger.MoveStart);
            else
                _fsm.ExecuteTrigger(PlayerTrigger.MoveStop);
        }

        // �d��/�ړ��i�d�͂͏펞���Z�j
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ���G���ԍX�V
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer < 0f) dashTimer = 0f;
        }
        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer < 0f) hitTimer = 0f;
        }
        // === �������Ď� ===
        if (attackHeld && attackPressStartTime >= 0.0)
        {
            double held = Time.timeAsDouble - attackPressStartTime;

            // (1) ������UI�̋N��
            if (held > shortPressMax && !holdUIShown)
            {
                UIEvents.OnAttackHoldUI?.Invoke(true);   // �i��UI�Ȃǂ�\���J�n
                holdUIShown = true;
            }

            // (2) �i���ʒm
            if (holdUIShown)
            {
                float p = Mathf.Clamp01((float)((held - shortPressMax) / Mathf.Max(0.0001f, (skillHoldTime - shortPressMax))));
                UIEvents.OnAttackHoldProgress?.Invoke(p);
            }

            // (3) Skill�������l���B
            if (!skillTriggered && held >= skillHoldTime)
            {
                skillTriggered = true;
                // === �X�L�����́i��ԑJ�ځj ===
                if (CanUseSkill())
                {

                    UIEvents.OnAttackHoldCommitted?.Invoke();
                    _fsm.ExecuteTrigger(PlayerTrigger.SkillInput);
                    UIEvents.OnAttackHoldUI?.Invoke(false);
                }
                else {
                    UIEvents.OnAttackHoldDenied?.Invoke();
                }
            }
        }
        // �X�e�[�g�X�V
        bool pressedNow = attackPressedThisFrame;
        _fsm.Update(Time.deltaTime);
        attackPressedThisFrame = false && pressedNow;
    }

    // ====== FSM �\�z ======
    private void SetupStateMachine()
    {
        _fsm = new StateMachine<PlayerState, PlayerTrigger>(this, PlayerState.Idle);

        _fsm.RegisterState(PlayerState.Idle, new PlayerIdleState(this));
        _fsm.RegisterState(PlayerState.Move, new PlayerMoveState(this));
        _fsm.RegisterState(PlayerState.Attack, new PlayerAttackState(this));
        _fsm.RegisterState(PlayerState.Skill, new PlayerSkillState(this));
        _fsm.RegisterState(PlayerState.Falling, new PlayerFallingState(this));
        _fsm.RegisterState(PlayerState.Hit, new PlayerHitState(this));
        _fsm.RegisterState(PlayerState.Dead, new PlayerDeadState(this));
        _fsm.RegisterState(PlayerState.Dash, new PlayerDashState(this));

        // Locomotion
        _fsm.AddTransition(PlayerState.Idle, PlayerState.Move, PlayerTrigger.MoveStart);
        _fsm.AddTransition(PlayerState.Move, PlayerState.Idle, PlayerTrigger.MoveStop);

        // Attack / Skill
        _fsm.AddTransition(PlayerState.Idle, PlayerState.Attack, PlayerTrigger.AttackInput);
        _fsm.AddTransition(PlayerState.Move, PlayerState.Attack, PlayerTrigger.AttackInput);
        _fsm.AddTransition(PlayerState.Idle, PlayerState.Skill, PlayerTrigger.SkillInput);
        _fsm.AddTransition(PlayerState.Move, PlayerState.Skill, PlayerTrigger.SkillInput);
        _fsm.AddTransition(PlayerState.Attack, PlayerState.Idle, PlayerTrigger.MoveStop);
        _fsm.AddTransition(PlayerState.Attack, PlayerState.Move, PlayerTrigger.MoveStart);
        _fsm.AddTransition(PlayerState.Attack, PlayerState.Skill, PlayerTrigger.SkillInput);
        _fsm.AddTransition(PlayerState.Skill, PlayerState.Idle, PlayerTrigger.MoveStop);
        _fsm.AddTransition(PlayerState.Skill, PlayerState.Move, PlayerTrigger.MoveStart);

        // Falling�iDead �ȊO �� Falling�j
        _fsm.AddTransition(PlayerState.Idle, PlayerState.Falling, PlayerTrigger.NoGround);
        _fsm.AddTransition(PlayerState.Move, PlayerState.Falling, PlayerTrigger.NoGround);
        //_fsm.AddTransition(PlayerState.Attack, PlayerState.Falling, PlayerTrigger.NoGround);
        //_fsm.AddTransition(PlayerState.Skill, PlayerState.Falling, PlayerTrigger.NoGround);
        _fsm.AddTransition(PlayerState.Hit, PlayerState.Falling, PlayerTrigger.NoGround);

        // Falling �� Idle
        _fsm.AddTransition(PlayerState.Falling, PlayerState.Idle, PlayerTrigger.Grounded);

        // Hit�i�ǂ̏�Ԃ���ł���e�\�B���G����TakeDamage���Œe���j
        _fsm.AddTransition(PlayerState.Idle, PlayerState.Hit, PlayerTrigger.GetHit);
        _fsm.AddTransition(PlayerState.Move, PlayerState.Hit, PlayerTrigger.GetHit);
        _fsm.AddTransition(PlayerState.Attack, PlayerState.Hit, PlayerTrigger.GetHit);
        //_fsm.AddTransition(PlayerState.Skill, PlayerState.Hit, PlayerTrigger.GetHit);
        _fsm.AddTransition(PlayerState.Falling, PlayerState.Hit, PlayerTrigger.GetHit);
        _fsm.AddTransition(PlayerState.Dash, PlayerState.Hit, PlayerTrigger.GetHit);
        _fsm.AddTransition(PlayerState.Hit, PlayerState.Idle, PlayerTrigger.MoveStop);
        _fsm.AddTransition(PlayerState.Hit, PlayerState.Move, PlayerTrigger.MoveStart);

        // Dash�i�C�ӂ̏�Ԃ���_�b�V���B��������Dash�X�e�[�g�j
        _fsm.AddTransition(PlayerState.Idle, PlayerState.Dash, PlayerTrigger.DashInput);
        _fsm.AddTransition(PlayerState.Move, PlayerState.Dash, PlayerTrigger.DashInput);
        _fsm.AddTransition(PlayerState.Attack, PlayerState.Dash, PlayerTrigger.DashInput);
        //_fsm.AddTransition(PlayerState.Skill, PlayerState.Dash, PlayerTrigger.DashInput);
        _fsm.AddTransition(PlayerState.Dash, PlayerState.Idle, PlayerTrigger.MoveStop);
        _fsm.AddTransition(PlayerState.Dash, PlayerState.Move, PlayerTrigger.MoveStart);

        // Dead�i���B��FHit �� Dead�j
        _fsm.AddTransition(PlayerState.Hit, PlayerState.Dead, PlayerTrigger.Die);
    }

    // ====== ���̓n���h�� ======
    public void HandleMovement(float deltaTime)
    {
        float inputX = moveInput.x;
        float inputZ = moveInput.y;

        Vector3 camForward = mainCam.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = mainCam.right; camRight.y = 0f; camRight.Normalize();

        Vector3 moveDir = camRight * inputX + camForward * inputZ;
        moveDir = (moveDir.sqrMagnitude > 1e-4f) ? moveDir.normalized : Vector3.zero;

        if (moveDir.sqrMagnitude > 0f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * deltaTime);
            controller.Move(moveDir * moveSpeed * deltaTime);
        }
    }
    public void CheckMoveInput()
    {
        // �� Input �n���c���Ă���ꍇ�̌݊��`�F�b�N�i�K�v�Ȃ�폜�j
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f)
        {
            _fsm.ExecuteTrigger(PlayerTrigger.MoveStart);
        }
    }
    public void CheckMoveStop()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(inputX) < 0.1f && Mathf.Abs(inputZ) < 0.1f)
        {
            _fsm.ExecuteTrigger(PlayerTrigger.MoveStop);
        }
    }
    private void OnSwitchWeaponInput()
    {
        if (_fsm.CurrentState == PlayerState.Attack || _fsm.CurrentState == PlayerState.Skill)
        {
            Debug.Log("Cannot switch weapon while attacking or during skill!");
            return;
        }
        if (!weaponInventory.TrySwitchRight())
        {
            Debug.Log("No usable weapon for right hand.");
        }
    }

    private void OnSwitchWeaponInput2()
    {
        if (_fsm.CurrentState == PlayerState.Attack || _fsm.CurrentState == PlayerState.Skill)
        {
            Debug.Log("Cannot switch weapon while attacking or during skill!");
            return;
        }
        if (!weaponInventory.TrySwitchRight(-1))
        {
            Debug.Log("No usable weapon for left hand.");
        }
    }
    private void OnDashInput()
    {
        if(IsDashCooldownReady()) _fsm.ExecuteTrigger(PlayerTrigger.DashInput);
    }

    // ====== ���f�������i�C�x���g�h���u���j ======



    // �E��̐ؑփC�x���g
    private void HandleRightWeaponSwitch(List<WeaponInstance> list, int from, int to)
    {
        // �Efrom->to �̕ω����󂯂āA�E��̕��탂�f�������ւ���
        if (from == to) return; // �ω��Ȃ�
        ApplyHandModel(HandType.Main, list, to);
    }

    // �w���̕��탂�f�����X�V
    private void ApplyHandModel(HandType hand, List<WeaponInstance> list, int toIndex)
    {
        // �E���s�̎q��S�폜���AtoIndex ���L���Ȃ�V�����v���n�u���C���X�^���X������
        GameObject box = (hand == HandType.Main) ? weaponBoxR : weaponBoxL;

        // �����q�I�u�W�F�N�g�j��
        for (int i = box.transform.childCount - 1; i >= 0; --i)
        {
            Transform c = box.transform.GetChild(i);
            if (c) Destroy(c.gameObject);
        }

        if (toIndex < 0 || toIndex >= list.Count) return;
        var inst = list[toIndex];
        if (inst == null || inst.template == null || inst.template.modelPrefab == null) return;

        // �V�K�C���X�^���X��
        GameObject newWeapon = Instantiate(inst.template.modelPrefab, box.transform);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;
    }

    // ====== ���[�e�B���e�B ======

    public void HandleFalling()
    {
        _fsm.ExecuteTrigger(PlayerTrigger.NoGround);
    }

    public WeaponInstance GetMainWeapon()
    {
        return weaponInventory.GetWeapon(HandType.Main);
    }

    public void ToIdle()
    {
        _fsm.ExecuteTrigger(PlayerTrigger.MoveStop);
    }


    // === ���E�� ===
    public void PickUpWeapon(WeaponItem weapon)
    {
        if (weapon == null) return;
        weaponInventory.AddWeapon(weapon);
        Debug.Log("PickUp: " + weapon.weaponName);

        // ���񑕔��̎������i�E�肪��Ȃ�E��ɑ����j
        if (weaponInventory.GetWeapon(HandType.Main) == null)
        {
            weaponInventory.TrySwitchRight(); // �C�x���g�ŉE�胂�f�������������
        }
        /*else if (weaponInventory.GetWeapon(HandType.Sub) == null)
        {
            weaponInventory.TrySwitchLeft(); // �C�x���g�ō��胂�f�������������
        }*/
    }

    // === �_���[�W�E�� ===
    public void TakeDamage(DamageData damage)
    {
        if (damage.damageAmount <= 0) return;
        if (_fsm.CurrentState == PlayerState.Dead) return; // ���S��͖���

        if (_fsm.CurrentState == PlayerState.Skill || IsInvincible) // ���G���Ԓ��͖���
        {
            GamepadRumbleOnce(0.3f, 0.2f);
            return;
        }


        currentHealth = Mathf.Max(0, currentHealth - damage.damageAmount);
        Debug.Log($"Player took {damage.damageAmount} damage. Current HP: {currentHealth}/{maxHealth}");
        UIEvents.OnPlayerHpChange?.Invoke((int)currentHealth, (int)maxHealth);

        // ���S����͔�e���[�V�����̏I�Ղōs���i�����ł͑�Dead�ɂ��Ȃ��j
        if (_fsm.CurrentState != PlayerState.Hit)
        {
            _fsm.ExecuteTrigger(PlayerTrigger.GetHit);
            hitTimer = hitInvincibilityTime; // ��e���G���ԃ��Z�b�g
            // ��e�G�t�F�N�g
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);
                PlayerEvents.PlayClipByPart(PlayerAudioPart.Mouth, hitSound,1f,1f,0f);
            }
            GamepadRumbleOnce(0.8f, 0.35f);
        }
    }

    // ====== �A�j���[�V�������� ======
    // ���C�����C���[�̎w��X���b�g�փN���X�t�F�[�h�i���̓��͂�0�ցj
    public void BlendToMainSlot(MainLayerSlot target, float duration)
    {
        if (mainLayerFadeCo != null) StopCoroutine(mainLayerFadeCo);
        mainLayerFadeCo = StartCoroutine(CoBlendToMainSlot(target, duration));
    }

    private System.Collections.IEnumerator CoBlendToMainSlot(MainLayerSlot target, float duration)
    {
        int inputCount = mixer.GetInputCount();
        if (duration <= 0f)
        {
            for (int i = 0; i < inputCount; ++i)
                mixer.SetInputWeight(i, (i == (int)target) ? 1f : 0f);
            yield break;
        }

        // ���ݏd�݂̃X�i�b�v�V���b�g
        float[] start = new float[inputCount];
        for (int i = 0; i < inputCount; ++i) start[i] = mixer.GetInputWeight(i);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float w = Mathf.Clamp01(t / duration);
            for (int i = 0; i < inputCount; ++i)
            {
                float dst = (i == (int)target) ? 1f : 0f;
                mixer.SetInputWeight(i, Mathf.Lerp(start[i], dst, w));
            }
            yield return null;
        }
        for (int i = 0; i < inputCount; ++i)
            mixer.SetInputWeight(i, (i == (int)target) ? 1f : 0f);
    }
    // 
    private MainLayerSlot MapStateToSlot(PlayerState s)
    {
        switch (s)
        {
            case PlayerState.Idle: return MainLayerSlot.Idle;
            case PlayerState.Move: return MainLayerSlot.Move;
            case PlayerState.Attack: return MainLayerSlot.Action; // �q�~�L�T�[
            case PlayerState.Skill: return MainLayerSlot.Action; // �q�~�L�T�[
            case PlayerState.Falling: return MainLayerSlot.Falling;
            case PlayerState.Hit: return MainLayerSlot.Hit;
            case PlayerState.Dead: return MainLayerSlot.Dead;
            case PlayerState.Dash: return MainLayerSlot.Dash;
            default: return MainLayerSlot.Idle;
        }
    }

    // �u�����h���Ԃ̉����i1.��from to > 2.to���� > 3.�S�̊���j
    public float ResolveBlendDuration(PlayerState from, PlayerState to)
    {

        if (to == PlayerState.Hit || to == PlayerState.Dash)
            return Mathf.Max(0f, interruptCrossfade);
        // 1) �ʃI�[�o�[���C�h����
        for (int i = 0; i < blendOverrides.Count; ++i)
        {
            var e = blendOverrides[i];
            if (e != null && e.from == from && e.to == to) return Mathf.Max(0f, e.duration);
        }
        // 2) to��Ԃ̊���
        for (int i = 0; i < toStateDefaults.Count; ++i)
        {
            var d = toStateDefaults[i];
            if (d != null && d.to == to) return Mathf.Max(0f, d.duration);
        }
        // 3) �S�̊���
        return Mathf.Max(0f, defaultStateCrossfade);
    }

    // �_����Ԃփu�����h�i�Ăяo������to��Ԃ����n���΂悢�j
    public void BlendToState(PlayerState toState)
    {
        var slot = MapStateToSlot(toState);
        float dur = ResolveBlendDuration(lastBlendState, toState);

        // �����̃��C���w�t�F�[�hAPI���g�p
        BlendToMainSlot(slot, dur);

        // ����̂��߂ɋL�^
        lastBlendState = toState;
    }

    public AnimationMixerPlayable GetActionSubMixer() => actionSubMixer;
    public void EvaluateGraphOnce() { if (playableGraph.IsValid()) playableGraph.Evaluate(0f); }
    public bool HasMoveInput() => moveInput.sqrMagnitude > 0.01f;
    public void ExecuteTriggerExternal(PlayerTrigger t)
    {
        _fsm.ExecuteTrigger(t);
    }
    // Hit �N���b�v�� Playable �� 0 �b����Đ�������
    public void ResetHitClipPlayable()
    {
        if (hitPlayable.IsValid())
        {
            hitPlayable.SetTime(0.0);
            hitPlayable.SetSpeed(1.0);
            hitPlayable.Play();
            EvaluateGraphOnce(); // 0�b�߂��̌����ڂ𑦔��f
        }
    }

    // Dead �N���b�v�� Playable �� 0 �b����Đ�������
    public void ResetDeadClipPlayable()
    {
        if (deadPlayable.IsValid())
        {
            deadPlayable.SetTime(0.0);
            deadPlayable.SetSpeed(1.0);
            deadPlayable.Play();
            EvaluateGraphOnce(); // �����ɏ����t���[����
        }
    }

    // ���{��F�_�b�V���p�̃A�j���� 0 �b����Đ�������
    public void ResetDashClipPlayable()
    {
        if (dashPlayable.IsValid())
        {
            dashPlayable.SetTime(0.0);
            dashPlayable.SetSpeed(1.0);
            dashPlayable.Play();
            EvaluateGraphOnce();
        }
    }

    // ���{��F�_�b�V���̖��G���J�n�i�X�e�[�g����Ăԁj
    public void StartDashInvincibility()
    {
        // �^�C�}�[�����FUpdate �Ō���������
        dashTimer = dashInvincibilityTime;
    }

    // ���{��F�_�b�V���̖��G���I���i�X�e�[�g����Ăԁj
    public void EndDashInvincibility()
    {
        dashTimer = 0f;
    }

    // ���{��F�_�b�V���̃N�[���^�C���L�^�i�I�����ɌĂԁj
    public void MarkDashCooldown()
    {
        dashCooldownTimer = Time.time;
        Invoke(nameof(EnableDashUI), dashCooldown);
    }
    private void EnableDashUI()
    {
        UIEvents.OnDashUIChange?.Invoke(true);
    }

    // ���{��F�_�b�V���\���i���͒i�K�̃K�[�h�Ɏg�p�j
    public bool IsDashCooldownReady()
    {
        return Time.time >= (dashCooldownTimer + dashCooldown);
    }

    // ���{��F�J�������΂̈ړ����͕����B�����͂Ȃ琳�ʂ�Ԃ�
    public Vector3 ResolveDashDirectionWorld(float minInputSqr = 0.01f)
    {
        Vector3 dir;
        if (this.moveInput.sqrMagnitude >= minInputSqr)
        {
            Vector3 camF = mainCam.forward; camF.y = 0f; camF.Normalize();
            Vector3 camR = mainCam.right; camR.y = 0f; camR.Normalize();
            dir = (camR * moveInput.x + camF * moveInput.y);
            if (dir.sqrMagnitude < 1e-6f) dir = transform.forward; else dir.Normalize();
        }
        else
        {
            dir = transform.forward;
        }
        return dir;
    }
    // Dash �N���b�v�̒�����Ԃ��i���ݒ莞�̓t�F�C���Z�[�t�j
    public double GetDashClipLength()
    {
        return (dashClip != null) ? (double)dashClip.length : 0.2;
    }

    // Dash �N���b�v�̌��ݎ���
    public double GetDashPlayableTime()
    {
        return dashPlayable.IsValid() ? dashPlayable.GetTime() : 0.0;
    }

    // Dash �N���b�v�̎����Ƒ��x�̐ݒ�
    public void SetDashPlayableTime(double t) { if (dashPlayable.IsValid()) { dashPlayable.SetTime(t); } }
    public void SetDashPlayableSpeed(double s) { if (dashPlayable.IsValid()) { dashPlayable.SetSpeed(s); } }
    /// <summary>
    /// �w��́u���������v�ցA�w�莞�ԂŃv���C���[�̌�������]������
    /// - XZ���ʃx�N�g�����g�p (Y�����͖���)
    /// - ��]���ɍēx�Ă΂ꂽ�ꍇ�A�O�̉�]���~���ĐV������]���J�n
    /// </summary>
    /// <param name="horizontalDir">�������� (XZ����)�B��: ���b�N�I���Ώ� - ���� �̃x�N�g��</param>
    /// <param name="durationSec">��]�ɗv����b���B0�ȉ��Ȃ瑦����]</param>
    public void RotateYawOverTime(Vector3 horizontalDir, float durationSec)
    {
        // --- ������ (Y������0�ɂ��āAXZ���ʂɎˉe) ---
        horizontalDir.y = 0f;
        if (horizontalDir.sqrMagnitude < 1e-6f)
        {
            // �����ȕ��� (�[���x�N�g��) �������Ȃ�
            return;
        }
        horizontalDir.Normalize();

        // �ڕW���[�p���Z�o
        float targetYaw = Quaternion.LookRotation(horizontalDir, Vector3.up).eulerAngles.y;

        // �i�s���̉�]������Β�~
        if (rotateYawCo != null)
        {
            StopCoroutine(rotateYawCo);
            rotateYawCo = null;
        }

        // ����0�ȉ�
        if (durationSec <= 0f)
        {
            var e = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
            return;
        }

        // �R���[�`���ŃX���[�Y��]
        rotateYawCo = StartCoroutine(CoRotateYawTo(targetYaw, durationSec));
    }

    /// <summary>
    /// �����p: ���[�p�֎��ԃu�����h��]
    /// - �����̉�]����ڕW���[�p�� LerpAngle ���
    /// - �o�ߒ��ɐV�K��]�������� StopCoroutine �����O��
    /// </summary>
    private System.Collections.IEnumerator CoRotateYawTo(float targetYaw, float durationSec)
    {
        // ���[�p
        float startYaw = transform.rotation.eulerAngles.y;
        float t = 0f;

        // LerpAngle
        while (t < durationSec)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / durationSec);
            float currentYaw = Mathf.LerpAngle(startYaw, targetYaw, p);
            transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
            yield return null;
        }

        // �ŏI�X�i�b�v
        transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        rotateYawCo = null;
    }

    /// <summary>
    /// ���݃��b�N�I�����̓G�ɑ΂���u�������� (XZ)�v���擾����
    /// - ���b�N�I���Ώۂ����� / �߂�����ꍇ�� false ��Ԃ�
    /// </summary>
    /// <param name="dirXZ">���K���ς݂� XZ �����x�N�g�� (Y=0)</param>
    public bool TryGetLockOnHorizontalDirection(out Vector3 dirXZ)
    {
        dirXZ = Vector3.zero;

        if (lockOnTarget == null) return false;

        // ������
        Vector3 v = lockOnTarget.transform.position - transform.position;
        v.y = 0f;

        if (v.sqrMagnitude < 1e-6f) return false;

        dirXZ = v.normalized;
        return true;
    }
    // === �ړ����̗͂L���ƃ��[���h�������擾 ===

    public bool TryGetMoveDirectionWorld(float minInputSqr, out Vector3 dir)
    {
        // �J�������΂ŎZ�o�iResolveDashDirectionWorld �Ɠ����̍��W�n�j
        if (this.moveInput.sqrMagnitude >= minInputSqr)
        {
            Vector3 camF = mainCam.forward; camF.y = 0f; camF.Normalize();
            Vector3 camR = mainCam.right; camR.y = 0f; camR.Normalize();
            dir = (camR * moveInput.x + camF * moveInput.y);
            if (dir.sqrMagnitude < 1e-6f) { dir = transform.forward; return false; }
            dir.Normalize();
            return true;
        }
        dir = transform.forward;
        return false;
    }


    public void SetHandModelVisible(HandType hand, bool visible)
    {
        GameObject box = (hand == HandType.Main) ? weaponBoxR : weaponBoxL;
        if (box == null) return;

        // �q�����ׂĂ� Renderer ��񋓂��Đؑ�
        var renderers = box.GetComponentsInChildren<Renderer>(includeInactive: true);
        for (int i = 0; i < renderers.Length; ++i)
        {
            
            renderers[i].enabled = visible;
        }
    }

    /// <summary>
    /// �E��iMain�j�̕��탂�f�����B��/�\������Ȉ�API
    /// </summary>
    public void HideMainHandModel() => SetHandModelVisible(HandType.Main, false);
    public void ShowMainHandModel() => SetHandModelVisible(HandType.Main, true);

    private bool CanEnterSkillNow()
    {
        var w = GetMainWeapon();
        var list = w?.template?.finisherAttack;
        var finisher = (list != null && list.Count > 0) ? list[0] : null;
        if (finisher == null || finisher.animation == null) return false;
        return w.currentDurability >= finisher.durabilityCost;
    }

    private void HandleApplyHP(int current, int max)
    {
        maxHealth = Mathf.Max(1, max);
        if (_fiCurrentHealth == null)
        {
            _fiCurrentHealth = typeof(PlayerMovement)
                .GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        if (_fiCurrentHealth != null)
        {
            _fiCurrentHealth.SetValue(this, (float)Mathf.Clamp(current, 0, (int)maxHealth));
        }

        UIEvents.OnPlayerHpChange?.Invoke(Mathf.Clamp(current, 0, (int)maxHealth), (int)maxHealth);
    }

    private void HandleApplyLoadoutInstances(List<WeaponInstance> instances, int mainIndex)
    {
        if (weaponInventory == null) return;
        weaponInventory.ApplyLoadoutInstances(instances, mainIndex);
    }
    private bool CanUseSkill()
    {
        var w = GetMainWeapon();
        var list = w?.template?.finisherAttack;
        var finisher = (list != null && list.Count > 0) ? list[0] : null;
        if (finisher == null || finisher.animation == null) return false;
        return w.currentDurability >= finisher.durabilityCost;
    }

    public void DisablePlayerInput()
    {
        if (inputActions != null)
        {
            inputActions.Disable(); // ���ׂĂ̓��͂��~
        }
    }

    public void EnablePlayerInput()
    {
        if (inputActions != null)
        {
            inputActions.Enable(); // ���͂��ĊJ
        }
    }
    // ====== �Q�[���p�b�h�U�� ======
    private void GamepadRumbleOnce(float intensity, float durationSec)
    {
        GamepadRumble(intensity, intensity, durationSec);
    }

    /// <summary>
    /// ���݂̃p�b�h�U���𑦎���~
    /// </summary>
    private void CancelGamepadRumble(Gamepad pad = null)
    {
        pad ??= Gamepad.current;
        if (_rumbleCo != null)
        {
            StopCoroutine(_rumbleCo);
            _rumbleCo = null;
        }
        if (pad != null)
        {
            // ���{��F���ׂẴ��[�^�[���~
            pad.SetMotorSpeeds(0f, 0f);
            pad.PauseHaptics();   // �O�̂���
            pad.ResetHaptics();   // �f�o�C�X��Ԃ�������
        }
    }
    public void GamepadRumble(float low, float high, float durationSec)
    {
        bool unscaled = true;
        Gamepad pad = null;
        // �f�o�C�X�擾�i���ڑ��Ȃ疳���j
        pad ??= Gamepad.current;
        if (pad == null) return;

        // ���x�̃N�����v
        low = Mathf.Clamp01(low);
        high = Mathf.Clamp01(high);
        durationSec = Mathf.Max(0f, durationSec);

        // �����̐U���𒆒f�i�㏑���J�n�j
        if (_rumbleCo != null)
        {
            StopCoroutine(_rumbleCo);
            _rumbleCo = null;
        }

        // �R���[�`���J�n
        _rumbleCo = StartCoroutine(CoRumble(pad, low, high, durationSec, unscaled));
    }

    // --- �U������ ---
    private System.Collections.IEnumerator CoRumble(Gamepad pad, float low, float high, float durationSec, bool unscaled)
    {
        // �J�n���ɐݒ�
        pad.SetMotorSpeeds(low, high);

        if (durationSec > 0f)
        {
            if (unscaled)
                yield return new WaitForSecondsRealtime(durationSec); // ���Ԓ�~�����J�E���g
            else
                yield return new WaitForSeconds(durationSec);
        }

        // �I�����ɒ�~�i�㏑���������ꍇ�� StopCoroutine �ł����ɗ��Ȃ��j
        pad.SetMotorSpeeds(0f, 0f);
        pad.PauseHaptics();
        pad.ResetHaptics();

        _rumbleCo = null;
    }
}

