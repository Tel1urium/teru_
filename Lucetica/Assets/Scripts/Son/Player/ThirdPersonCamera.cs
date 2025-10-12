using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using static EventBus;
using UnityEngine.InputSystem.LowLevel;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("=== �Ǐ]�^�[�Q�b�g ===")]
    public Transform target;      // �v���C���[�̒ʏ펞�s�{�b�g
    public Transform aimTarget;   // ���b�N�I�����̌��z���s�{�b�g

    [Header("=== �J������]�Ƌ��� ===")]
    public Vector2 rotationSpeed = new Vector2(400f, 300f); // �J������]���x
    public Vector2 pitchClamp = new Vector2(-30f, 60f);     // �����p�x�̐���
    public float defaultDistance = 6.5f;                    // �ʏ틗��
    public float minDistance = 1f;                          // �ŏ�����
    public LayerMask collisionMask;                         // �J�����Փ˃��C���[

    [Header("=== �J�����{�f�B�Փ� ===")]
    [Tooltip("�J�����{�̂̔��a")]
    public float cameraBodyRadius = 0.25f;

    [Tooltip("�q�b�g�ʂ���̗]��")]
    public float cameraBodyPadding = 0.05f;

    [Tooltip("�񕪒T���̔�����")]
    public int bodyResolveIterations = 6;

    [Header("=== �f�o�C�X�ʊ��x�{�� ===")]
    public float mouseSensitivityMultiplier = 0.3f;
    public float gamepadSensitivityMultiplier = 1.0f;

    [Header("=== ���b�N�I���ݒ� ===")]
    public LayerMask enemyLayer;        // �G���C���[
    public float maxLockDistance = 25f; // ���b�N�Ώۂ̍ő勗��
    [Range(10f, 120f)]
    public float lockFovAngle = 60f;    // ���ʎ���p
    public float switchInputThreshold = 0.3f; // ���͂Ń^�[�Q�b�g�ؑւ�臒l
    public float switchCooldown = 0.35f;      // �A���ؑւ̃N�[���_�E��

    [Header("=== �Ǐ]�X�s�[�h (�ʏ�ƃ��b�N��) ===")]
    public float pivotLerpSpeed_Normal = 8f;  // �ʏ펞�s�{�b�g���
    public float pivotLerpSpeed_Locked = 3f;  // ���b�N���s�{�b�g���
    public float smoothSpeed_Normal = 8f;     // �ʏ펞�J�����ʒu���
    public float smoothSpeed_Locked = 3f;     // ���b�N���J�����ʒu���

    [Header("=== ���b�N�I����]��� ===")]
    public float lockSwitchRotateSpeed = 6f;  // �ؑ֒���̉�]��ԑ��x

    [Header("=== �q�G�����L�[�΍� ===")]
    public bool autoDetachFromPlayer = true;  // �v���C���[�z���Ȃ玩���ŊO��

    [Header("=== �ؑ֎��̈ړ��u�����h ===")]
    public float switchMoveBlendTime = 0.15f; // �^�[�Q�b�g�ؑ֎��̈ʒu�u�����h����

    // ����
    private float yaw;
    private float pitch;
    private Transform cam;
    private Vector3 currentVelocity;     // �J�����ʒu�p SmoothDamp ���x

    private InputSystem_Actions inputActions;
    private Vector2 lookInput;
    private bool lockPressedThisFrame;

    private bool isLocked;
    private Transform lockedEnemy;
    private float switchTimer;
    private bool needsSmoothRotate;
    private Quaternion smoothLockRotation;

    // �s�{�b�g���
    private Vector3 followPivot;
    private Vector3 pivotVelocity;

    // �ؑ֎��̈ʒu�u�����h
    private bool switchMoveActive;
    private float switchMoveRemain;
    private float switchMoveTotal;
    private Vector3 camPosOnSwitch;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;

        // �����p�x
        var angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;

        // ����
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        inputActions.Player.Lock.performed += _ => lockPressedThisFrame = true;

        // �����s�{�b�g
        followPivot = target != null ? target.position : transform.position;

        // �v���C���[�z������O��
        if (autoDetachFromPlayer && target != null)
        {
            Transform root = target.root;
            if (root != null && transform.IsChildOf(root))
            {
                transform.SetParent(null, true); // ���[���h���W���ێ�
            }
        }
    }
    private void OnEnable()
    {
        PlayerEvents.ChangeCameraTarget += StartOrbitAround;
    }


    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
        PlayerEvents.ChangeCameraTarget -= StartOrbitAround;
    }

    void LateUpdate()
    {
        if (cam == null || target == null) return;

        float dt = Time.deltaTime;
        switchTimer -= dt;

        bool gamepad = IsGamepadUsed();
        float sensMul = gamepad ? gamepadSensitivityMultiplier : mouseSensitivityMultiplier;

        // �񃍃b�N���͎蓮��]
        if (!isLocked)
        {
            yaw += lookInput.x * rotationSpeed.x * dt * sensMul;
            pitch -= lookInput.y * rotationSpeed.y * dt * sensMul;
            pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);
        }

        // ���b�N�L�[
        if (lockPressedThisFrame)
        {
            lockPressedThisFrame = false;
            if (!isLocked) { TryAcquireLock(); }
            else { CancelLock(); }
        }

        // ���b�N���̗L�����ƍ��E�ؑ�
        if (isLocked)
        {
            if (!IsValidLockTarget(lockedEnemy))
            {
                CancelLock();
            }
            else
            {
                float horiz = lookInput.x;
                if (Mathf.Abs(horiz) > switchInputThreshold && switchTimer <= 0f)
                {
                    SwitchLockTarget(horiz > 0f ? +1 : -1);
                    switchTimer = switchCooldown;
                }
            }
        }

        // �s�{�b�g���
        float pivotSpeed = isLocked ? pivotLerpSpeed_Locked : pivotLerpSpeed_Normal;
        Vector3 desiredPivot = (isLocked && aimTarget != null) ? aimTarget.position : target.position;
        followPivot = Vector3.SmoothDamp(
            followPivot, desiredPivot, ref pivotVelocity,
            1f / Mathf.Max(0.0001f, pivotSpeed)
        );

        // ���O�ʒu
        transform.position = followPivot;

        // ����
        if (isLocked && lockedEnemy != null)
        {
            Vector3 refPos = (aimTarget != null ? aimTarget.position : followPivot);
            Vector3 dir = GetAimPoint(lockedEnemy) - refPos;

            if (dir.sqrMagnitude > 1e-6f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

                if (needsSmoothRotate)
                {
                    smoothLockRotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lockSwitchRotateSpeed);
                    transform.rotation = smoothLockRotation;

                    var e = smoothLockRotation.eulerAngles;
                    yaw = e.y;
                    float rp = e.x > 180f ? e.x - 360f : e.x;
                    pitch = Mathf.Clamp(rp, pitchClamp.x, pitchClamp.y);

                    if (Quaternion.Angle(transform.rotation, targetRot) < 0.5f)
                        needsSmoothRotate = false;
                }
                else
                {
                    transform.rotation = targetRot;

                    var e = targetRot.eulerAngles;
                    yaw = e.y;
                    float rp = e.x > 180f ? e.x - 360f : e.x;
                    pitch = Mathf.Clamp(rp, pitchClamp.x, pitchClamp.y);
                }
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // === �����m�� ===

        // 1) �������Ղ���̂�����΁A���̎�O�܂ŏk�߂�
        float losDistance = defaultDistance;
        if (Physics.Raycast(followPivot, -transform.forward, out RaycastHit losHit, defaultDistance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            // �Փ˖ʂ��班���]�������
            losDistance = Mathf.Clamp(losHit.distance - Mathf.Max(cameraBodyRadius, cameraBodyPadding), minDistance, defaultDistance);
        }

        // 2) ���@�{�f�B�i���j���ŏI�ʒu�ŏd�ˍ��킹�`�F�b�N���A�d�Ȃ��Ă���΁u�^�[�Q�b�g���ցv�k�߂ĉ��
        float actualDistance = ResolveCameraBodyDistanceBinary(
            followPivot,
            transform,      // ���O�� forward/up/right ���g��
            losDistance,    // �܂������l����̋�������]�l�Ƃ��ēn��
            minDistance,
            cameraBodyRadius,
            cameraBodyPadding,
            collisionMask
        );

        // �J�����ʒu�X�V (�ؑ֎��u�����h��D��)
        float moveSpeed = isLocked ? smoothSpeed_Locked : smoothSpeed_Normal;
        Vector3 finalPos = followPivot - transform.forward * actualDistance;

        if (switchMoveActive)
        {
            switchMoveRemain -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(switchMoveRemain / switchMoveTotal);
            float eased = 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // �C�[�Y�A�E�g
            cam.position = Vector3.Lerp(camPosOnSwitch, finalPos, eased);

            if (switchMoveRemain <= 0f)
            {
                switchMoveActive = false;
                currentVelocity = Vector3.zero; // ������؂�
                cam.position = finalPos;
            }
        }
        else
        {
            cam.position = Vector3.SmoothDamp(
                cam.position, finalPos, ref currentVelocity,
                1f / Mathf.Max(0.0001f, moveSpeed)
            );
        }

        // �����_
        if (isLocked && lockedEnemy != null)
        {
            cam.LookAt(GetAimPoint(lockedEnemy));
        }
        else
        {
            cam.LookAt(target.position + Vector3.up * 0.5f);
        }
    }

    private bool IsGamepadUsed()
    {
        return Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
    }

    // ���b�N�擾
    private void TryAcquireLock()
    {
        Transform best = FindBestTargetInFov();
        if (best != null)
        {
            isLocked = true;
            lockedEnemy = best;
            needsSmoothRotate = true;

            // �C�x���g���o
            PlayerEvents.OnAimTargetChanged?.Invoke(lockedEnemy.gameObject);
            UIEvents.OnAimPointChanged?.Invoke(ResolveAimPointTransform(lockedEnemy));
        }
    }

    // ���b�N����
    private void CancelLock()
    {
        if (!isLocked) return;
        isLocked = false;
        lockedEnemy = null;
        needsSmoothRotate = false;

        // �C�x���g���o
        PlayerEvents.OnAimTargetChanged?.Invoke(null);
        UIEvents.OnAimPointChanged?.Invoke(null);
    }

    // ���E�ؑ�
    private void SwitchLockTarget(int dirSign)
    {
        if (!isLocked || lockedEnemy == null) return;

        List<Transform> candidates = GatherTargetsInFov();
        if (candidates.Count == 0) return;

        candidates = candidates
            .Where(t => t != lockedEnemy && IsValidLockTarget(t))
            .OrderBy(t => AngleFromCameraForward(t))
            .ToList();

        if (candidates.Count == 0) return;

        Vector3 right = transform.right;
        var sideList = candidates
            .Where(t =>
            {
                Vector3 to = (GetAimPoint(t) - cam.position).normalized;
                float side = Mathf.Sign(Vector3.Dot(to, right)); // �E��+1, ����-1
                return side == Mathf.Sign(dirSign);
            })
            .ToList();

        Transform next = (sideList.Count > 0 ? sideList : candidates).First();

        if (next != null)
        {
            lockedEnemy = next;
            needsSmoothRotate = true;

            // �ʒu�u�����h�J�n
            switchMoveActive = true;
            switchMoveTotal = Mathf.Max(0.0001f, switchMoveBlendTime);
            switchMoveRemain = switchMoveTotal;
            camPosOnSwitch = cam.position;

            currentVelocity = Vector3.zero;

            // �C�x���g���o
            PlayerEvents.OnAimTargetChanged?.Invoke(lockedEnemy.gameObject);
            UIEvents.OnAimPointChanged?.Invoke(ResolveAimPointTransform(lockedEnemy));
        }
    }

    // FOV ���̍œK�^�[�Q�b�g
    private Transform FindBestTargetInFov()
    {
        List<Transform> list = GatherTargetsInFov();
        if (list.Count == 0) return null;

        Transform best = null;
        float bestAngle = float.MaxValue;
        float bestDist = float.MaxValue;

        foreach (var t in list)
        {
            float ang = AngleFromCameraForward(t);
            float dist = Vector3.Distance(target.position, t.position);

            if (ang < bestAngle || (Mathf.Approximately(ang, bestAngle) && dist < bestDist))
            {
                best = t;
                bestAngle = ang;
                bestDist = dist;
            }
        }
        return best;
    }

    // �����W
    private List<Transform> GatherTargetsInFov()
    {
        var results = new List<Transform>();
        Collider[] hits = Physics.OverlapSphere(target.position, maxLockDistance, enemyLayer);
        if (hits == null || hits.Length == 0) return results;

        float half = lockFovAngle * 0.5f;
        Vector3 fwd = transform.forward;

        foreach (var c in hits)
        {
            Transform t = c.transform;
            if (!IsValidLockTarget(t)) continue;

            Vector3 to = (GetAimPoint(t) - cam.position).normalized;
            float ang = Vector3.Angle(fwd, to);
            if (ang <= half) results.Add(t);
        }
        return results;
    }

    // �J�����O������̊p�x
    private float AngleFromCameraForward(Transform t)
    {
        Vector3 to = (GetAimPoint(t) - cam.position).normalized;
        return Vector3.Angle(transform.forward, to);
    }

    // �L����
    private bool IsValidLockTarget(Transform t)
    {
        if (t == null) return false;
        if (!t.gameObject.activeInHierarchy) return false;
        float d = Vector3.Distance(target.position, t.position);
        if (d > maxLockDistance) return false;
        return true;
    }

    // �_���_�̃��[���h���W
    private Vector3 GetAimPoint(Transform t)
    {
        var ap = t.GetComponentInChildren<AimPointMarker>();
        if (ap != null) return ap.transform.position;

        if (t.TryGetComponent<Collider>(out var col))
            return col.bounds.center;

        return t.position + Vector3.up * 1.0f;
    }

    // UI �p�ɓn���g�����X�t�H�[��
    private Transform ResolveAimPointTransform(Transform t)
    {
        if (t == null) return null;
        var ap = t.GetComponentInChildren<AimPointMarker>();
        if (ap != null) return ap.transform;
        return t;
    }

    private float ResolveCameraBodyDistanceBinary(
    Vector3 pivot,
    Transform rig,
    float desiredDistance,
    float minDist,
    float bodyRadius,
    float padding,
    LayerMask mask
)
    {
        // ��]�������ŏ���菬������΂��̂܂�
        desiredDistance = Mathf.Max(minDist, desiredDistance);

        // �w�苗�������ɔ�d�Ȃ�Ȃ�I��
        if (!Physics.CheckSphere(pivot - rig.forward * desiredDistance, bodyRadius, mask, QueryTriggerInteraction.Ignore))
            return desiredDistance;

        // [lo .. hi] �͈̔͂Łu�ő�̔�d�Ȃ苗���v��T������
        float lo = minDist;           // ��Ɂg��d�Ȃ�h���֊񂹂Ă����i�S�[���j
        float hi = desiredDistance;   // ���́g�d�Ȃ�h�ł��邱�Ƃ��������Ă����[

        // lo ���d�Ȃ��Ă���\���͒Ⴂ���A�ی��Ƃ��Ċm�F���d�Ȃ��Ă�����X�ɋl�߂�
        if (Physics.CheckSphere(pivot - rig.forward * lo, bodyRadius, mask, QueryTriggerInteraction.Ignore))
        {
            // �ق�̏�����O�ցi����̋ɒ[�P�[�X�j
            lo = minDist;
        }

        for (int i = 0; i < Mathf.Max(1, bodyResolveIterations); i++)
        {
            float mid = (lo + hi) * 0.5f;
            Vector3 midPos = pivot - rig.forward * mid;
            bool overlap = Physics.CheckSphere(midPos, bodyRadius, mask, QueryTriggerInteraction.Ignore);

            if (overlap)
            {
                // �܂��d�Ȃ��Ă��� �� ����ɓ����ցi�������k�߂�j
                hi = mid;
            }
            else
            {
                // ��d�Ȃ� �� ���������O�֎����i�ő�̔�d�Ȃ苗����T���j
                lo = mid;
            }
        }

        // �ʂ��班���]�������ĕԂ��i�u����p�̒i���ɋ�������j
        float solved = Mathf.Max(minDist, lo - padding);
        return solved;
    }

    private void StartOrbitAround(Transform focus, float durationSec, float rotateSpeed)
    {
        if (focus == null) return;
        StopAllCoroutines();
        StartCoroutine(CoOrbitAround(focus, durationSec, rotateSpeed));
    }

    private System.Collections.IEnumerator CoOrbitAround(Transform focus, float durationSec, float rotateSpeed)
    {
        // --- ���͂𖳌��� ---
        if (inputActions != null) inputActions.Disable();

        Transform originalTarget = target;
        target = focus;

        float timer = 0f;

        while (timer < durationSec)
        {
            timer += Time.deltaTime;

            // �u�E���́v��͋[ �� yaw ���E�����ɐi��
            lookInput = new Vector2(+1f, 0f) * rotateSpeed;

            // �����͐����ɌŒ肵�����ꍇ�� pitch �� 0 �ɋ߂Â���
            pitch = Mathf.Lerp(pitch, 0f, Time.deltaTime * 2f);

            yield return null;
        }

        // --- ���� target �ɖ߂� ---
        target = originalTarget;

        // �Ǐ]�����R�ɖ߂����߂� 0.5�b���炢���̓[���ő҂�
        float backTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < backTime)
        {
            elapsed += Time.deltaTime;
            lookInput = Vector2.zero; // ���͖����ɂ��ĒǏ]������������
            yield return null;
        }

        // --- ���͂��ĊJ ---
        if (inputActions != null) inputActions.Enable();
    }
}