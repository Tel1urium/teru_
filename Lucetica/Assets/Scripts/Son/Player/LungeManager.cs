using System;
using UnityEngine;

/// <summary>
/// LungeManager for CharacterController�i�P�̔Łj
/// �E�U���ːi�^�_�b�V���̓����R���|�[�l���g�i�O���͊J�n���Ɉ�x�����Ăяo���j
/// �ECharacterController.Move ���厲�ɁA���O�� Physics.CapsuleCast �ŋ����N���b�v�i詐��΍�j
/// �ECC �� slopeLimit / stepOffset / skinWidth �����p�i�Ζ�/�i���Ή��j
///
/// �d�v�d�l�FlungeDistance == 0f �́u�ːi���Ȃ��v�i�������ł͂Ȃ��j
/// </summary>
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class LungeManager : MonoBehaviour
{
    // ===== �������[�h =====
    public enum LungeAim { Forward, ToTarget, CustomDir }

    [Header("�Q��")]
    [Tooltip("�O���̊�i���ݒ�Ȃ玩����Transform�j")]
    public Transform facingSource;

    [Header("�K�{�FCharacterController")]
    public CharacterController controller;

    [Header("�Փ˔��背�C���[�i�n�`/�ǂȂǁj")]
    public LayerMask environmentMask = ~0;

    [Header("����/���l����p�����[�^")]
    [Tooltip("�v���X�C�[�v�������獷�������ŏ��}�[�W��")]
    public float skin = 0.03f;
    [Tooltip("�n�ʋz�������i�����ȕ����������j")]
    public float groundSnapDistance = 0.08f;
    [Tooltip("�X�C�[�v���̔��a�c���i�����l�j")]
    public float radiusInflation = 0.01f;
    [Tooltip("�i���⏕�F�K�v�Ȃ�X�C�[�v�O�Ɉꎞ�I�ɏグ��ʁiCC.stepOffset �ƕ��p�j")]
    public float preStepLift = 0.0f; // 0 ��CC�ɔC����



    // ==== �����^�C���ړ���� ====
    private bool isLunging;
    private Vector3 moveDir = Vector3.zero;  // ���K�������i�����j
    private float baseSpeed = 0f;            // m/s
    private float? maxTime = null;           // �b�inull=���Ԑ����Ȃ��j
    private float? maxDistance = null;       // m�inull=���������Ȃ��j�������łŎg�p
    private AnimationCurve accelCurve = null;

    private float elapsed = 0f;              // �o�ߎ���
    private float movedDistance = 0f;        // �~�ϋ���

    // ==== �O���ʒm ====
    public event Action OnLungeStart;     // �ːi�J�n
    public event Action OnLungeFinish;    // ����I���i����/���ԁj
    public event Action OnLungeBlocked;   // �Փˁi���O�X�C�[�v�ŋ������l�܂����j
    public event Action OnLungeTooSteep;  // �Ζʌ��E�i���֎~�j

    public bool IsLunging => isLunging;
    public Vector3 LastUsedDirection => moveDir;

    private void Reset()
    {
        if (!facingSource) facingSource = transform;
        controller = GetComponent<CharacterController>();
        // CC �̐����ݒ�i�K�v�ɉ�����Inspector�ŏ㏑���j
        controller.minMoveDistance = 0f;          // �����ړ����E��
        controller.detectCollisions = true;
        // slopeLimit�i�x�j�AstepOffset�AskinWidth �̓v���W�F�N�g��ɍ��킹�Đݒ�
    }

    private void Awake()
    {
        if (!facingSource) facingSource = transform;
        if (!controller) controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        EventBus.PlayerEvents.LungeByDistance += StartLungeByDistance;
        EventBus.PlayerEvents.LungeByTime += StartLungeByTime;
    }
    private void OnDisable()
    {
        EventBus.PlayerEvents.LungeByDistance -= StartLungeByDistance;
        EventBus.PlayerEvents.LungeByTime -= StartLungeByTime;
        ForceCancel(false);
    }

    // ======================================================================
    // ���JAPI�F�����x�[�X�i�����j  �� distance <= 0f �� �񔭓�
    // ======================================================================
    public bool StartLungeByDistance(
        LungeAim aim,
        Vector3 toTargetPos,
        Vector3 customDir,
        float speed,
        float distance,
        AnimationCurve curve = null)
    {
        if (distance <= 0f) return false; // �� �d�v�F0�͓ːi���Ȃ�
        if (!TryResolveDirection(aim, toTargetPos, customDir, out Vector3 dir)) return false;

        moveDir = dir;
        baseSpeed = Mathf.Max(0.01f, speed);
        maxDistance = distance;
        maxTime = null;
        accelCurve = curve;

        BeginLunge();
        return true;
    }

    // ======================================================================
    // ���JAPI�F���ԃx�[�X�i�K�v���̂݁j
    // ======================================================================
    public bool StartLungeByTime(
        LungeAim aim,
        Vector3 toTargetPos,
        Vector3 customDir,
        float speed,
        float timeSeconds,
        AnimationCurve curve = null)
    {
        if (timeSeconds <= 0f) return false;
        if (!TryResolveDirection(aim, toTargetPos, customDir, out Vector3 dir)) return false;

        moveDir = dir;
        baseSpeed = Mathf.Max(0.01f, speed);
        maxTime = timeSeconds;
        maxDistance = null;
        accelCurve = curve;

        BeginLunge();
        return true;
    }

    /// <summary>�O�����狭���L�����Z���i�X�e�[�g�J�ڎ��̕ی��j</summary>
    public void ForceCancel(bool invokeFinish = true)
    {
        isLunging = false;
        elapsed = 0f;
        movedDistance = 0f;
        if (invokeFinish) OnLungeFinish?.Invoke();
    }

    private void BeginLunge()
    {
        isLunging = true;
        elapsed = 0f;
        movedDistance = 0f;
        OnLungeStart?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!isLunging) return;

        float dt = Time.fixedDeltaTime;

        // === ���x����i�������J�[�u�Ή��j ===
        float speed = baseSpeed;
        if (accelCurve != null)
        {
            float denom = Mathf.Max(1e-4f, maxTime ?? 1f);
            float t01 = Mathf.Clamp01(elapsed / denom);
            speed *= Mathf.Max(0f, accelCurve.Evaluate(t01));
        }

        // === �ړ��\��x�N�g���i�����j ===
        Vector3 delta = moveDir * speed * dt;

        // === ��肪�Ζʌ��E�𒴂��邩�ȈՔ���iCC �� slopeLimit �𑸏d�j ===
        // CC �͓����ŏ������邪�A"������~" �̃Q�[�����[������ꂽ���ꍇ�Ɏ��O����
        // ���p���������鎞�͒��f�i�v���C���[��Y�͊O���̏d�͌n�ŊǗ��O��j
        // �� delta ����������������ꍇ���� slopeLimit �𒴂������Ȃ��~
        if (controller.slopeLimit < 89.9f && Vector3.Dot(delta, Vector3.up) > 0f)
        {
            // �ȈՁF�ڒn�ߖT�Ȃ�����֎~�i�����Ȓn�ʖ@�����K�v�Ȃ� SphereCast �ŏE���j
            if (controller.isGrounded)
            {
                StopTooSteep();
                return;
            }
        }

        // === �i���⏕�i�K�v�Ȃ�ꎞ�I�Ɏ����グ�Ă���X�C�[�v�j ===
        Vector3 startPos = transform.position;
        if (preStepLift > 0f && controller.isGrounded)
        {
            // CC.Move �ŏグ��ƏՓ˔��肪����̂ŁA���O�X�C�[�v�ʒu���������グ��
            startPos += Vector3.up * preStepLift;
        }

        // === ���O�X�C�[�v�iPhysics.CapsuleCast�j�ŋ����N���b�v�i詐��h�~�j ===
        float allowed = ClipByCapsuleCast(startPos, delta, out RaycastHit hit);
        bool blocked = false;
        Vector3 move = delta;

        if (allowed < delta.magnitude)
        {
            move = delta.normalized * Mathf.Max(0f, allowed - skin);
            blocked = true;
        }

        // === ���ړ��FCharacterController.Move ===
        // �ECC ������/�i��/�Ζʂ��������
        // �EY�����̐���i�d��/���n�j�̓Q�[�����̕ʌn����
        controller.Move(move);

        // === �n�ʋz���i�����ȕ��������GCC.isGrounded �� false �ł��ߖT�Ȃ牺����j ===
        if (!controller.isGrounded && groundSnapDistance > 0f)
        {
            if (ProbeGroundDistance(out float down))
            {
                // CC.Move �ł݈̂ʒu��K�p
                controller.Move(Vector3.down * down);
            }
        }

        // === �i���X�V����~���� ===
        movedDistance += move.magnitude;
        elapsed += dt;

        if (blocked)
        {
            StopBlocked();
            return;
        }
        if (maxDistance.HasValue && movedDistance >= maxDistance.Value - 1e-4f)
        {
            StopFinished();
            return;
        }
        if (maxTime.HasValue && elapsed >= maxTime.Value - 1e-6f)
        {
            StopFinished();
            return;
        }
    }

    // ==== ��~�n���h�� ====
    private void StopFinished()
    {
        isLunging = false;
        OnLungeFinish?.Invoke();
    }
    private void StopBlocked()
    {
        isLunging = false;
        OnLungeBlocked?.Invoke();
    }
    private void StopTooSteep()
    {
        isLunging = false;
        OnLungeTooSteep?.Invoke();
    }

    // ======================================================================
    // ���O�X�C�[�v�iCapsuleCast�j? CC �̌`������[���h�ɓW�J���Ďg�p
    // ======================================================================
    private float ClipByCapsuleCast(Vector3 startPos, Vector3 delta, out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        if (delta.sqrMagnitude <= 1e-8f) return 0f;

        // CC �̃J�v�Z�������[���h�ցiY���A���C���O��j
        GetControllerCapsule(out Vector3 p0, out Vector3 p1, out float r);

        // startPos �ɉ��ړ��i�X�C�[�v�̌��_���������グ�铙�ɑΉ��j
        Vector3 off = startPos - transform.position;
        p0 += off; p1 += off;

        float dist = delta.magnitude;
        Vector3 dir = delta / dist;
        float rr = r + radiusInflation;

        if (Physics.CapsuleCast(p0, p1, rr, dir, out hitInfo, dist + skin,
                                environmentMask, QueryTriggerInteraction.Ignore))
        {
            return hitInfo.distance;
        }
        return dist;
    }

    // CC �̃��[���h�`��i�[�_�Ɣ��a�j
    private void GetControllerCapsule(out Vector3 p0, out Vector3 p1, out float r)
    {
        // CharacterController �̓��[�J�� center/height/radius ������
        // �J�v�Z���[�_�� CC �� up �����ɉ����Čv�Z
        var t = controller.transform;
        Vector3 up = t.up;

        // lossyScale �ɂ�锼�a�̃X�P�[���iX/Z�̕��ς��̗p�j
        float sx = Mathf.Abs(t.lossyScale.x);
        float sz = Mathf.Abs(t.lossyScale.z);
        float sy = Mathf.Abs(t.lossyScale.y);

        r = controller.radius * ((sx + sz) * 0.5f);
        float h = Mathf.Max(controller.height * sy, r * 2f);

        Vector3 center = t.TransformPoint(controller.center);
        float half = Mathf.Max(0f, h * 0.5f - r);

        p0 = center + up * half; // ��[
        p1 = center - up * half; // ���[
    }

    // �n�ʂ܂ł̋���
    private bool ProbeGroundDistance(out float downDistance)
    {
        downDistance = 0f;
        GetControllerCapsule(out Vector3 p0, out Vector3 p1, out float r);
        Vector3 center = (p0 + p1) * 0.5f;

        if (Physics.SphereCast(center + Vector3.up * 0.02f, r * 0.95f, Vector3.down,
                               out RaycastHit hit, groundSnapDistance,
                               environmentMask, QueryTriggerInteraction.Ignore))
        {
            // �����]�T�����ĉ����鋗�����Z�o
            downDistance = Mathf.Max(0f, hit.distance - skin * 0.5f);
            return downDistance > 1e-3f;
        }
        return false;
    }

    // ======================================================================
    // ����
    // ======================================================================
    private bool TryResolveDirection(LungeAim aim, Vector3 toTargetPos, Vector3 customDir, out Vector3 dir)
    {
        dir = Vector3.zero;
        switch (aim)
        {
            case LungeAim.Forward:
                dir = (facingSource ? facingSource.forward : transform.forward);
                break;
            case LungeAim.ToTarget:
                dir = (toTargetPos - transform.position);
                dir.y = 0f;
                break;
            case LungeAim.CustomDir:
                dir = customDir;
                break;
        }
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) return false;
        dir.Normalize();
        return true;
    }
}
