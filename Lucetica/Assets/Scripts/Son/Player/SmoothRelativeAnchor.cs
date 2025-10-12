using UnityEngine;

/// <summary>
/// �J�����p�A���J�[�i���ΒǏ]�{���[���h�Ɨ��^�p�j
/// �E�J�n���Ƀ^�[�Q�b�g�i�v���C���[�j����u���Έʒu/��]�v���L�^
/// �E�ȍ~�̓��[���h��ԂœƗ����A���t���[���ő��ΖڕW�փX���[�Y�Ǐ]
/// �E�A�j���h��/�q�K�w�̃m�C�Y���Ւf���A�_�b�V�����̋}�ςɂ��Ǐ]�i�����ێ�
/// </summary>
[DefaultExecutionOrder(50)]
public class SmoothRelativeAnchor : MonoBehaviour
{
    [Header("=== �Ǐ]�Ώ� ===")]
    public Transform target;                 // �v���C���[���̊
    public bool detachOnStart = true;        // �J�n���ɐe�q�֌W���������邩

    [Header("=== ���΃I�t�Z�b�g ===")]
    public Vector3 initialLocalOffset = new Vector3(0f, 1.6f, -0.5f); // �����̑��Έʒu�i�^�[�Q�b�g��j
    public Vector3 initialLocalEulerOffset = Vector3.zero;            // �����̑��Ή�]�i�I�C���[�j

    [Header("=== �Ǐ]�X���[�Y ===")]
    [Tooltip("�ʒu�̃X���[�Y���ԁi�b�j�B�������قǃL�r�L�r")]
    public float positionSmoothTime = 0.12f;
    [Tooltip("��]�̑J�ڌW���B0=���� / 1=�S�����Ȃ��B����: 0.1?0.3")]
    [Range(0f, 1f)] public float rotationLerpFactor = 0.15f;
    [Tooltip("�傫�����ꂽ�ꍇ�̃X�i�b�v�����im�j�B0�ȉ��Ŗ���")]
    public float snapDistance = 6f;
    [Tooltip("�ʒu�X���[�Y�̍ő呬�x�im/s�j�B0�ȉ��Ŗ�����")]
    public float maxPositionSpeed = 0f;

    [Header("=== �\���i�C�Ӂj===")]
    [Tooltip("�^�[�Q�b�g�̑��x�𐄒肵�āA�������ǂ��i�_�b�V�����ɗL���j")]
    public bool usePrediction = true;
    [Tooltip("��ǂݎ��ԁi�b�j�B0.03?0.1 ���x")]
    public float predictionTime = 0.06f;
    [Tooltip("���x�̃��[�p�X�i0=����/1=�Ǐ]�����j�B����: 0.15?0.35")]
    [Range(0f, 1f)] public float velocitySmoothing = 0.2f;

    // --- ������� ---
    private Vector3 _localOffset;            // ���ۂɎg�����Έʒu
    private Quaternion _localRotOffset;      // ���ۂɎg�����Ή�]
    private Vector3 _vel;                    // SmoothDamp �p�̑��x
    private Vector3 _lastTargetPos;
    private Vector3 _targetVel;              // ����^�[�Q�b�g���x
    private bool _initialized;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[SmoothRelativeAnchor] target ���ݒ�");
            enabled = false;
            return;
        }

        // --- �������΃I�t�Z�b�g�i�����w�� or �V�[����̌��ݑ��΁j ---
        // ���łɃ^�[�Q�b�g�̎q�Ŕz�u����Ă���ꍇ�́A���̃��[�J�����̗p���₷��
        if (transform.parent == target)
        {
            _localOffset = transform.localPosition;
            _localRotOffset = transform.localRotation;
        }
        else
        {
            _localOffset = initialLocalOffset;
            _localRotOffset = Quaternion.Euler(initialLocalEulerOffset);
        }

        // --- �e�q�֌W�̉����i���[���h��Ԃցj ---
        if (detachOnStart)
        {
            // ���݂̃��[���h�ϊ���ێ������܂ܐe���O��
            transform.SetParent(null, true);
        }

        // --- �����ʒu/��]�̊m��i���΂��烏�[���h�֔��f�j ---
        var desired = ComputeDesiredPose(Time.deltaTime);
        transform.position = desired.position;
        transform.rotation = desired.rotation;

        _lastTargetPos = target.position;
        _initialized = true;
    }

    void LateUpdate()
    {
        if (!_initialized || target == null) return;

        // --- �^�[�Q�b�g���x�̐��� ---
        var rawVel = (target.position - _lastTargetPos) / Mathf.Max(Time.deltaTime, 1e-6f);
        _targetVel = Vector3.Lerp(rawVel, _targetVel, Mathf.Clamp01(velocitySmoothing));
        _lastTargetPos = target.position;

        // --- ���ΖڕW�̎Z�o ---
        var desired = ComputeDesiredPose(Time.deltaTime);

        // --- �勗���X�i�b�v�i���[�v/���A���̈��S��j ---
        if (snapDistance > 0f)
        {
            float dist = Vector3.Distance(transform.position, desired.position);
            if (dist > snapDistance)
            {
                transform.position = desired.position;
                transform.rotation = desired.rotation;
                _vel = Vector3.zero;
                return;
            }
        }

        // --- �ʒu���X���[�Y�ɒǏ]�iSmoothDamp�j ---
        float smooth = Mathf.Max(0.0001f, positionSmoothTime);
        if (maxPositionSpeed > 0f)
            transform.position = Vector3.SmoothDamp(transform.position, desired.position, ref _vel, smooth, maxPositionSpeed);
        else
            transform.position = Vector3.SmoothDamp(transform.position, desired.position, ref _vel, smooth);

        // --- ��]�͎w���I�Ɋɂ₩�ɕ�ԁiSlerp�j ---
        float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(1f - rotationLerpFactor), Time.deltaTime * 60f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired.rotation, t);
    }

    /// <summary>
    /// ���ΖڕW�̃��[���h�p�����Z�o
    /// �^�[�Q�b�g�̌��ݎp�� �~ ���΃I�t�Z�b�g
    /// �\�����L���Ȃ��ǂ݈ʒu��
    /// </summary>
    private (Vector3 position, Quaternion rotation) ComputeDesiredPose(float dt)
    {
        // ���Ή�]��K�p�����u���Έʒu�v
        Vector3 local = _localRotOffset * _localOffset;
        // �^�[�Q�b�g�p���ɏ悹��
        Vector3 pos = target.TransformPoint(local);
        Quaternion rot = target.rotation * _localRotOffset;

        // �\���i�C�Ӂj
        if (usePrediction && predictionTime > 0f)
        {
            pos += _targetVel * predictionTime;
        }
        return (pos, rot);
    }

    // ====== ���JAPI ======

    /// <summary>
    /// ���݂̃^�[�Q�b�g�̃A���J�[�֌W����u���΃I�t�Z�b�g�v���Čv�Z���ċL�^
    /// �E�V�����J�����\�}����ɂ��������Ɏg�p
    /// </summary>
    public void RefreshRelativeOffset()
    {
        if (target == null) return;
        // �^�[�Q�b�g��Ԃ֋t�ϊ�
        _localOffset = target.InverseTransformPoint(transform.position);
        // ��]�̑���
        _localRotOffset = Quaternion.Inverse(target.rotation) * transform.rotation;
    }

    /// <summary>
    /// �����e�q�֌W�������i���[���h��Ԃŕێ��j
    /// </summary>
    public void DetachNow()
    {
        if (transform.parent != null)
            transform.SetParent(null, true);
    }

    /// <summary>
    /// �^�[�Q�b�g�̎q�ɖ߂��i�f�o�b�O/�ꎞ�p�r�j
    /// </summary>
    public void AttachBack(bool keepWorld = true)
    {
        if (target == null) return;
        transform.SetParent(target, keepWorld);
    }

    /// <summary>
    /// �ڕW���΂𒼐ڃZ�b�g�i�O���V�X�e������\�}��؂�ւ������ꍇ�j
    /// </summary>
    public void SetRelativeOffset(Vector3 localOffset, Quaternion localRotOffset)
    {
        _localOffset = localOffset;
        _localRotOffset = localRotOffset;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f);
    }
}
