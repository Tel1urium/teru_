using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �_�b�V�����Ɉ��Ԋu�ŃX�L�����b�V���̃X�i�b�v�V���b�g�i�c���j�𐶐�����
/// �ELungeManager �̊J�n/�I���C�x���g�ɘA��
/// �E�e�X�i�b�v�V���b�g�� 0.1 �b�Ńt�F�[�h�A�E�g���Ĕj��
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(LungeManager))]
public class DashAfterimageSpawner : MonoBehaviour
{
    [Header("�Q��")]
    [Tooltip("�c�������� SkinnedMeshRenderer�B���w��Ȃ�q�K�w���玩�����W")]
    public SkinnedMeshRenderer[] sources;

    [Tooltip("�c���p�̓����}�e���A���i���L���j�BURP/Lit �Ȃǂ� Transparent �𐄏�")]
    public Material ghostMaterial;

    [Header("�����p�����[�^")]
    [Tooltip("�����Ԋu�i�b�j�B��F0.05")]
    public float spawnInterval = 0.05f;

    [Tooltip("�c���̎����i�b�j�B��F0.10")]
    public float lifeTime = 0.10f;

    [Range(0f, 1f)]
    [Tooltip("��������̕s�����x�i0-1�j�B�c��̓t�F�[�h�A�E�g")]
    public float initialAlpha = 0.6f;

    [Tooltip("�t�F�[�h�J�[�u�iTime=0��1 �ɑ΂��� �� ��Z�j�B���ݒ�Ȃ���`")]
    public AnimationCurve alphaCurve;

    [Header("�`��I�v�V����")]
    [Tooltip("�e�̎󂯎��𖳌��ɂ���i���F���ƃR�X�g�΍�j")]
    public bool disableReceiveShadows = true;

    [Tooltip("�e�̓��e�𖳌��ɂ���i���F���ƃR�X�g�΍�j")]
    public bool disableCastShadows = true;

    // ���{��F����
    private LungeManager _lm;
    private PlayerMovement _player; // PlayableGraph �� Evaluate ���g�����߁i�C�Ӂj
    private Coroutine _loopCo;

    private void Awake()
    {
        _lm = GetComponent<LungeManager>();
        _player = GetComponent<PlayerMovement>();

        if (alphaCurve == null || alphaCurve.length == 0)
        {
            // ���{��F���ݒ莞�͐��`�i1��0�j
            alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        }
    }

    private void OnEnable()
    {
        _lm.OnLungeStart += HandleLungeStart;
        _lm.OnLungeFinish += HandleLungeEnd;
        _lm.OnLungeBlocked += HandleLungeEnd;
        _lm.OnLungeTooSteep += HandleLungeEnd;
    }

    private void OnDisable()
    {
        _lm.OnLungeStart -= HandleLungeStart;
        _lm.OnLungeFinish -= HandleLungeEnd;
        _lm.OnLungeBlocked -= HandleLungeEnd;
        _lm.OnLungeTooSteep -= HandleLungeEnd;

        if (_loopCo != null) StopCoroutine(_loopCo);
        _loopCo = null;
    }

    private void HandleLungeStart()
    {
        // ���{��F�\�[�X�����ݒ�Ȃ玩�����W
        if (sources == null || sources.Length == 0)
        {
            sources = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        // ���{��F�������[�v�J�n
        if (_loopCo != null) StopCoroutine(_loopCo);
        _loopCo = StartCoroutine(CoSpawnLoop());
    }

    private void HandleLungeEnd()
    {
        // ���{��F�������[�v��~�i�����c���͊e���Ŏ����Ǘ��j
        if (_loopCo != null) StopCoroutine(_loopCo);
        _loopCo = null;
    }

    private IEnumerator CoSpawnLoop()
    {
        // ���{��F�J�n���ɑ� 1 �񐶐����A���̌�� spawnInterval ����
        while (_lm != null && _lm.IsLunging)
        {
            SpawnGhostNow();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnGhostNow()
    {
        if (ghostMaterial == null || sources == null) return;

        // ���{��FPlayableGraph ���g���Ă���ꍇ�A�]������x�Ă�Ń|�[�Y�����艻
        if (_player != null) _player.EvaluateGraphOnce();

        for (int i = 0; i < sources.Length; ++i)
        {
            var smr = sources[i];
            if (smr == null || !smr.gameObject.activeInHierarchy) continue;

            // ���{��F���݃|�[�Y���x�C�N
            var baked = new Mesh();
            smr.BakeMesh(baked); // SkinnedMeshRenderer ����X�i�b�v�V���b�g�쐬�iUnity 6 ����API�j

            // ���{��F�c�� GameObject ��g�ݗ���
            var go = new GameObject($"Ghost_{smr.name}");
            go.layer = gameObject.layer; // ���C���[�p���i�K�v�ɉ����ĕύX�j

            // ���{��F�e�����̃��[���h�z�u�i���_�� SMR �� Transform ��j
            go.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
            go.transform.localScale = Vector3.one; // BakeMesh �̓X�L���ό`�㒸�_�Ȃ̂� 1 �ŕ`�悵��OK

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = baked;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = ghostMaterial;
            mr.receiveShadows = !disableReceiveShadows;
#if UNITY_6000_0_OR_NEWER
            // ���{��F���e�t���O�iEditor/RenderPipeline �ɂ���ċ���������j
            if (disableCastShadows) mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#endif

            // ���{��F�t�F�[�h�S���̃R���|�[�l���g��t�^
            var fade = go.AddComponent<DashGhostInstance>();
            fade.Init(lifeTime, initialAlpha, alphaCurve, () =>
            {
                // ���{��FMesh �̖����j���i���[�N�h�~�j
                if (mf != null && mf.sharedMesh != null)
                {
                    Destroy(mf.sharedMesh);
                }
                Destroy(go);
            });
        }
    }
}
