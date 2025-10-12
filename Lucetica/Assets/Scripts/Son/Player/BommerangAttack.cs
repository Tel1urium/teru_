using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class BommerangAttack : MonoBehaviour
{
    // === �q�b�g���ɓn���_���[�W��� ===
    [SerializeField] public DamageData damageData = new DamageData(2);

    // === ���������u�Ԃɒe��j�����邩 ===
    [FormerlySerializedAs("�����j��")]
    [SerializeField] public bool isDestroyedOnHit = true;

    // === �����蔻��Ώۂ̃��C���[ ===
    [SerializeField] public LayerMask hitLayer;

    // === �q�b�g���̃G�t�F�N�g ===
    [SerializeField] public GameObject hitEffect;

    // === �e�̎��� ===
    [SerializeField] public float lifetime = 1f;

    // === ���� Collider �ւ̍ăq�b�g�Ԋu ===
    [SerializeField] public double hitInterval = -1;

    // ---- �q�b�g���� ----
    private readonly Dictionary<Collider, double> _lastHitTimePerCollider = new Dictionary<Collider, double>(32);

    // ----------------------------------------------------------------------
    // ������ ��������u�n�`�Ǐ]�iY�̂ݕ␳�j�v�̓����ݒ� ������
    // ----------------------------------------------------------------------
    [Header("=== �n�`�Ǐ]�iY �␳�j ===")]
    [Tooltip("�ڒn��Ɏg���q�I�u�W�F�N�g��� CapsuleCollider")]
    [SerializeField] private CapsuleCollider sourceCapsule;

    [Tooltip("�n�ʂ���̖ڕW�I�t�Z�b�g")]
    [SerializeField] private float hoverHeight = 0.02f;

    [Tooltip("������J�n�I�t�Z�b�g")]
    [SerializeField] private float upCastExtra = 0.2f;

    [Tooltip("�������̃��C����")]
    [SerializeField] private float downCastExtra = 0.6f;

    [Tooltip("��Ԃ̃X���[�Y����")]
    [SerializeField] private float smoothTime = 0.05f;

    [Tooltip("�n�ʃ��C���[")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("�f�o�b�O�p�̉����i���C�\���Ȃǁj")]
    [SerializeField] private bool drawGizmos = false;

    private float _yVelocity;

    private void Awake()
    {
        if (sourceCapsule == null)
        {
            sourceCapsule = GetComponentInChildren<CapsuleCollider>();
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnDisable()
    {
        _lastHitTimePerCollider.Clear();
    }

    private void LateUpdate()
    {
        // === �n�`�Ǐ]�FAnimator ��ړ��Ŏq���������u��v�� Y ��␳ ===
        AdjustYByGround();
    }

    private void OnTriggerStay(Collider other)
    {
        // --- 1) ���C���[�t�B���^ ---
        if (!IsInLayerMask(other.gameObject.layer, hitLayer))
            return;

        // --- 2) ����t���[�����ɖ����ȓ����� ---
        if (other.attachedRigidbody != null && other.attachedRigidbody.gameObject == this.gameObject)
            return;

        // --- 3) ��x���� or �N�[���_�E������ ---
        if (!CanHitNow(other, Time.timeAsDouble))
            return;

        // --- 4) �_���[�W�K�p�iIDamageable ������O��B�Ȃ���ΐe���T���j ---
        var dmgTarget = other.GetComponent<Enemy>();
        if (dmgTarget == null) dmgTarget = other.GetComponentInParent<Enemy>();
        if (dmgTarget != null)
        {
            // �_���[�W��K�p
            dmgTarget.TakeDamage(damageData);

            // �q�b�g�����̍X�V
            _lastHitTimePerCollider[other] = Time.timeAsDouble;

            // �q�b�g�G�t�F�N�g����
            SpawnHitVFX(other);

            // �������j���t���O�������Ă���Ȃ玩��
            if (isDestroyedOnHit)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("BommerangAttack: No damageable component found on " + other.name);
        }
    }

    // === �_���[�W���̐ݒ� ===
    public void SetDamage(DamageData dmg)
    {
        damageData = dmg;
    }

    // === �w�背�C���[�� LayerMask �Ɋ܂܂�邩 ===
    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    // === ���� Collider �ɍ��q�b�g���Ă悢���i�Ԋu�`�F�b�N�j ===
    private bool CanHitNow(Collider col, double now)
    {
        // ���l�F��x�����q�b�g����
        if (hitInterval < 0)
        {
            if (_lastHitTimePerCollider.ContainsKey(col)) return false; // ���ɓ�������
            return true;
        }

        // �񕉒l�F�N�[���_�E������
        if (!_lastHitTimePerCollider.TryGetValue(col, out double last))
            return true; // ����

        return (now - last) >= hitInterval;
    }

    // === �q�b�g���� VFX ���� ===
    private void SpawnHitVFX(Collider other)
    {
        if (hitEffect == null) return;

        // ������ʒu�̐���
        var myPos = transform.position;
        var closest = other.ClosestPoint(myPos);
        

        var pos = other.transform.position;

        // �����͍U���̐i�s����
        /*Quaternion rot;
        var dir = (closest - myPos);
        if (dir.sqrMagnitude > 1e-6f) rot = Quaternion.LookRotation(dir);
        else rot = transform.rotation;*/
        var rot = transform.rotation;

        var vfx = Instantiate(hitEffect, pos, rot);
        // �����ŕK�v�Ȃ玩���j����e�q�t���Ȃǂ��s��
    }

    // ----------------------------------------------------------------------
    // �n�`�Ǐ]�iY �␳�j�{��
    // �E�q�� CapsuleCollider �̒ꋅ���S����ɁA�n�ʂƂ̋������v�����A
    //   ���[�g�I�u�W�F�N�g�� Y �݂̂�␳����
    // ----------------------------------------------------------------------
    private void AdjustYByGround()
    {
        if (sourceCapsule == null) return;

        // --- ���[���h�֕ϊ� ---
        Vector3 worldCenter = sourceCapsule.transform.TransformPoint(sourceCapsule.center);

        // ���l�X�P�[���Ή��i���a�͍ő厲���̗p�j
        float sx = Mathf.Abs(sourceCapsule.transform.lossyScale.x);
        float sy = Mathf.Abs(sourceCapsule.transform.lossyScale.y);
        float sz = Mathf.Abs(sourceCapsule.transform.lossyScale.z);
        float worldRadius = sourceCapsule.radius * Mathf.Max(sx, sy, sz);
        float worldHeight = Mathf.Max(sourceCapsule.height * sy, worldRadius * 2f);

        // �J�v�Z���́g������h
        Vector3 up = sourceCapsule.transform.TransformDirection(Vector3.up).normalized;

        // �ꋅ���S
        float half = worldHeight * 0.5f;
        Vector3 bottomSphereCenter = worldCenter - up * (half - worldRadius);

        // ���C�̊J�n�_�i�ꋅ���S��菭����j
        Vector3 rayStart = bottomSphereCenter + up * upCastExtra;
        Vector3 rayDir = -up;
        float rayLen = upCastExtra + downCastExtra + worldRadius + hoverHeight;

        // �n�ʃ��C�L���X�g
        if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, rayLen, groundMask, QueryTriggerInteraction.Ignore))
        {
            // �ڕW�F�ꋅ���S���u�n�� + hoverHeight�v�ɗ���悤�ɂ���
            Vector3 targetBottomCenter = hit.point + up * hoverHeight;

            Vector3 pos = transform.position;
            float deltaAlongUp = Vector3.Dot((targetBottomCenter - bottomSphereCenter), up);
            float targetY = pos.y + deltaAlongUp;

            // Y �̂݃X���[�Y�␳
            float newY = Mathf.SmoothDamp(pos.y, targetY, ref _yVelocity, smoothTime);
            transform.position = new Vector3(pos.x, newY, pos.z);

            if (drawGizmos)
            {
                Debug.DrawLine(rayStart, hit.point, Color.green);
            }
        }
        else
        {
            // �n�ʂ����o�ł��Ȃ��i�󒆂Ȃǁj�BY�͕ێ�
            if (drawGizmos)
            {
                Debug.DrawLine(rayStart, rayStart + rayDir * rayLen, Color.yellow);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || sourceCapsule == null) return;

        Vector3 worldCenter = sourceCapsule.transform.TransformPoint(sourceCapsule.center);
        float sx = Mathf.Abs(sourceCapsule.transform.lossyScale.x);
        float sy = Mathf.Abs(sourceCapsule.transform.lossyScale.y);
        float sz = Mathf.Abs(sourceCapsule.transform.lossyScale.z);
        float worldRadius = sourceCapsule.radius * Mathf.Max(sx, sy, sz);
        float worldHeight = Mathf.Max(sourceCapsule.height * sy, worldRadius * 2f);
        Vector3 up = sourceCapsule.transform.TransformDirection(Vector3.up).normalized;
        float half = worldHeight * 0.5f;
        Vector3 bottomSphereCenter = worldCenter - up * (half - worldRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(bottomSphereCenter, worldRadius * 0.2f);
    }
}