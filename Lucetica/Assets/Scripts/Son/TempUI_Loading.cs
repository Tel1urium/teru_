using UnityEngine;
using static EventBus;
using System.Collections.Generic;
using DG.Tweening;

public class TempUI_Loading : MonoBehaviour
{
    public static TempUI_Loading Instance { get; private set; }
    public GameObject loadingUI;

    [Header("�ǂ̏�ԂŃ��[�f�B���OUI���o����")]
    public List<GameState> loadingStates = new List<GameState>();

    [Header("LOADING �̊e�����I�u�W�F�N�g�i���Ԓʂ�j")]
    public List<GameObject> Loadings = new List<GameObject>();

    [Header("�A�j���ݒ�")]
    public float jumpPower = 30f;      // �W�����v����(px)
    public float jumpDuration = 0.30f; // 1�����̍��v���ԁi���0.5/����0.5�j
    public float letterDelay = 0.05f;  // �ׂ̕����ւ̒x��
    public float waveInterval = 0.20f; // 1�g�Ǝ��g�̊Ԋu

    private Sequence waveSeq;                  // �ė��p���郁�C���V�[�P���X
    private readonly List<RectTransform> _rts = new(); // �L���b�V��
    private Vector2[] _basePos;                // �����ʒu

    private void Awake()
    {
        // �V���O���g���ۏ�
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // DOTween�e�ʂ��Ɋm�ہiGC����̈ꏕ�j
        DOTween.SetTweensCapacity(200, 50);
    }

    private void OnEnable()
    {
        SystemEvents.OnGameStateChange += HandlePreLoading;
        SystemEvents.OnSceneLoadComplete += HandleLoadingComplete;

        // --- ��x���������i���^�[�Ə������W���m��j ---
        CacheRectsAndBasePositions();
        BuildSequenceIfNeeded(); // �V�[�P���X����x�������
    }

    private void OnDisable()
    {
        SystemEvents.OnGameStateChange -= HandlePreLoading;
        SystemEvents.OnSceneLoadComplete -= HandleLoadingComplete;

        // ���������͒�~�������ʒu��
        PauseAndReset();
    }

    private void HandlePreLoading(GameState state)
    {
        if (!loadingUI) return;

        if (loadingStates.Contains(state))
        {
            loadingUI.SetActive(true);
            // --- �Đ��iKill�����Ďn���j ---
            if (waveSeq != null)
            {
                // �� �L��������ɏu���Đ��J�n
                waveSeq.Restart();
            }
        }
        else
        {
            loadingUI.SetActive(false);
            PauseAndReset();
        }
    }

    private void HandleLoadingComplete()
    {
        if (!loadingUI) return;
        loadingUI.SetActive(false);
        PauseAndReset();
    }

    // ==== �������� ====

    /// <summary>
    /// �q����RectTransform�Ə������W���L���b�V��
    /// </summary>
    private void CacheRectsAndBasePositions()
    {
        _rts.Clear();
        for (int i = 0; i < Loadings.Count; i++)
        {
            if (Loadings[i] == null) continue;
            var rt = Loadings[i].GetComponent<RectTransform>();
            if (rt != null) _rts.Add(rt);
        }

        _basePos = new Vector2[_rts.Count];
        for (int i = 0; i < _rts.Count; i++)
        {
            _basePos[i] = _rts[i].anchoredPosition;
        }
    }

    /// <summary>
    /// �g�V�[�P���X����x�����\�z�iAutoKill���Ȃ��j
    /// </summary>
    private void BuildSequenceIfNeeded()
    {
        if (waveSeq != null) return;

        // ����������Ύ~�߂�i��{���Ȃ��j
        waveSeq = DOTween.Sequence()
            // Unscaled�X�V�Ń��[�f�B���O����Time.timeScale�e�����󂯂Ȃ�
            .SetUpdate(true)
            // �j�����Ȃ��Ŏg����
            .SetAutoKill(false);

        // 1�g�̒����i�S�����̃W�����v�{�Ō�̊Ԋu�j
        float half = jumpDuration * 0.5f;

        // --- �e�����̃W�����v���u�}���v����1�{�̔g�ɂ��� ---
        for (int i = 0; i < _rts.Count; i++)
        {
            var rt = _rts[i];
            float startAt = i * letterDelay; // �������Ƃ̊J�n�I�t�Z�b�g

            // �� �Y�i����v�Z�ł͂Ȃ����[�J���ϐ��ɑޔ��j
            float baseY = _basePos[i].y;

            // ��聨����̘A�������Sequence��
            var oneJump = DOTween.Sequence()
                // ���FOutQuad�Ńt���b��
                .Append(rt.DOAnchorPosY(baseY + jumpPower, half).SetEase(Ease.OutQuad))
                // ����FInQuad�ŃX�b�Ɨ�����
                .Append(rt.DOAnchorPosY(baseY, half).SetEase(Ease.InQuad));

            // ���C���Ɏ��ԑ}���i���̂������őS����1�{�̔g���Ԏ��ɏ��j
            waveSeq.Insert(startAt, oneJump);
        }

        // �g�̏I�[�ɑҋ@�𑫂��āA�������[�v
        float waveLength = (_rts.Count > 0 ? (_rts.Count - 1) * letterDelay : 0f) + jumpDuration + waveInterval;
        waveSeq.AppendInterval(Mathf.Max(0f, waveLength - waveSeq.Duration()));
        waveSeq.SetLoops(-1, LoopType.Restart);

        // �ŏ��͒�~���Ă����i�K�v�ȂƂ���Restart�j
        waveSeq.Pause();
    }

    /// <summary>
    /// �V�[�P���X���~�߂đS�����������ʒu�ɖ߂�
    /// </summary>
    private void PauseAndReset()
    {
        if (waveSeq != null) waveSeq.Pause();

        if (_rts != null && _basePos != null)
        {
            int n = Mathf.Min(_rts.Count, _basePos.Length);
            for (int i = 0; i < n; i++)
            {
                if (_rts[i] != null)
                    _rts[i].anchoredPosition = _basePos[i]; // �ʒu���m���Ƀ��Z�b�g
            }
        }
    }
}
