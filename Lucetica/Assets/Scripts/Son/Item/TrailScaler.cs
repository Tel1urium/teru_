using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailScaler : MonoBehaviour
{
    // === Trail �̑������w��iInspector �Őݒ�\�j ===
    [Tooltip("Trail �̑S�̓I�ȑ����{���i�f�t�H���g1�j")]
    public float width = 5.0f;

    // === Trail �̐擪�`�����܂ł̑����J�[�u ===
    [Tooltip("Trail �̒����ɉ����������ω��iX=�ʒu, Y=�{���j")]
    public AnimationCurve widthCurve = AnimationCurve.Linear(0, 1, 1, 1);

    // TrailRenderer �ւ̎Q��
    private TrailRenderer trail;

    private void Awake()
    {
        // TrailRenderer �R���|�[�l���g���擾
        trail = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        // Start ���ɐݒ��K�p
        ApplyTrailWidth();
    }

    /// <summary>
    /// TrailRenderer �ɑ����ݒ��K�p����֐�
    /// </summary>
    public void ApplyTrailWidth()
    {
        trail.widthMultiplier = width;  // �S�̔{��
        trail.widthCurve = widthCurve;  // �����J�[�u
    }
}
