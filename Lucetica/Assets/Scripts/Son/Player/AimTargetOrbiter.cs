using UnityEngine;

public class AimTargetOrbiter : MonoBehaviour
{
    [Header("=== �Q�� ===")]
    public Transform player;          // �v���C���[�i���S�j
    public Transform cameraRef;       // �Q�Ƃ���J�����i�ʏ�� Camera.main.transform�j

    [Header("=== �z�u�p�����[�^�i�����ʂł̔z�u�j===")]
    public float backDistance = 0.8f; // �v���C���[����ւ̋����i�J�����O���̋t�����j
    public float shoulderOffset = 0.6f; // �E��(+) / ����(-) �����I�t�Z�b�g�i�J�����E������j
    public float height = 1.5f;       // �v���C���[��������̍������Z

    [Header("=== �X���[�W���O ===")]
    public float positionSmooth = 12f; // �ʒu�̃X���[�Y�Ǐ]�i�傫���قǒǏ]�������j
    public float rotationSmooth = 12f; // �����̃X���[�Y�Ǐ]�i�K�v�ȏꍇ�j
    public bool alignYawToCamera = true; // �����������J�������ʂɍ��킹��i�C�Ӂj

    [Header("=== ���̑� ===")]
    public bool autoDetachFromPlayer = true; // �v���C���[�̎q�̏ꍇ�Ɏ����� unparent ���ĉ�]�̌p�����J�b�g

    // --- �����p ---
    private Vector3 velocity;         // SmoothDamp �p
    private Quaternion rotVelocity;   // ��]��ԁiSlerp ���g���̂Ŗ��g�p�ł�OK�j

    void Awake()
    {
        // �J�������w��Ȃ烁�C���J�������g�p
        if (cameraRef == null && Camera.main != null)
            cameraRef = Camera.main.transform;

        // �v���C���[�̎q�ɂȂ��Ă���ƃv���C���[��]���p�����Ă��܂����߁A�K�v�Ȃ玩���ŊO��
        if (autoDetachFromPlayer && transform.parent != null && player != null && transform.IsChildOf(player))
        {
            transform.SetParent(null, true); // ���[���h���W���ێ����� unparent
        }
    }

    void LateUpdate()
    {
        if (player == null || cameraRef == null) return;

        // --- �J������̐����x�N�g�����쐬�iY�����𗎂Ƃ��Đ��K���j ---
        Vector3 camF = cameraRef.forward;
        camF.y = 0f;
        if (camF.sqrMagnitude < 1e-6f) camF = player.forward; // ��펞�t�H�[���o�b�N
        camF.Normalize();

        Vector3 camR = cameraRef.right;
        camR.y = 0f;
        if (camR.sqrMagnitude < 1e-6f) camR = player.right;   // �t�H�[���o�b�N
        camR.Normalize();

        // --- �]�܂����ʒu�F�v���C���[�ʒu + ��� + ���� + ���� ---
        Vector3 desired =
            player.position
            + (-camF * backDistance)
            + (camR * shoulderOffset)
            + Vector3.up * height;

        // --- �ʒu���X���[�Y�ɒǏ] ---
        float t = (positionSmooth <= 0f) ? 1f : (Time.deltaTime * positionSmooth);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, (positionSmooth <= 0f ? 0f : 1f / positionSmooth));

        // --- �i�C�Ӂj���������̌������J�������ʂ֍��킹�� ---
        if (alignYawToCamera)
        {
            // �J���������O���֌�����i�s�b�`�͌Œ�j
            if (camF.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(camF, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(player.position + Vector3.up * height, transform.position);
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
#endif
}
