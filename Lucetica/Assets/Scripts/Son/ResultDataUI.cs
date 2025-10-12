using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// �N���A���ʂ̊eUI�������o���ȈՃv���[���^�[
/// - �e�v�f���Ƃ�: �x��(delay) �� duration �� from��to �Ɉړ�
/// - �C�[�W���O�A�K�v���̂� SetActive(true)
/// - ��X�P�[�����Ԃ��g�p�i���ʉ�ʂ� timeScale=0 �ł������j
/// </summary>
public class ResultDataUI : MonoBehaviour
{
    [Serializable]
    public class ClearUI
    {
        [Tooltip("�\���Ώۂ�UI���[�g")]
        public GameObject ui;
        [Tooltip("�J�n�ʒu�i���[�J���j")]
        public Vector3 from;
        [Tooltip("�I���ʒu�i���[�J���j")]
        public Vector3 to;

        [Tooltip("�\�����J�n����܂ł̒x���b")]
        public float delay = 0f;
        [Tooltip("�ړ��ɂ����鎞�ԁi�b�j")]
        public float duration = 0.4f;

        [Tooltip("�i�s�ɑ΂����ԁi0��1�j")]
        public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("�C�ӁF�t�F�[�h")]
        [Tooltip("CanvasGroup ������� 0��1 �Ƀt�F�[�h")]
        public bool fadeIfCanvasGroup = true;

        // ---- �����^�C�� ----
        [NonSerialized] public bool started;
        [NonSerialized] public bool finished;
        [NonSerialized] public float startTimeUnscaled; // �J�n�����i��X�P�[���b�j
        [NonSerialized] public CanvasGroup cg;          // ����Η��p
    }

    [Header("�\���L���[")]
    public List<ClearUI> clearUIs = new List<ClearUI>();

    // ��X�P�[�����Ԃ̊�i�L��������0�j
    private float _t0;

    private void OnEnable()
    {
        // === �������i���ʉ�ʂɓ������u�ԁj ===
        _t0 = Time.realtimeSinceStartup; // ��X�P�[�����Ԃ̑��

        if (clearUIs == null || clearUIs.Count == 0) return;

        foreach (var ui in clearUIs)
        {
            if (ui.ui == null) continue;

            // �ʒu�� from �ɃZ�b�g�A���������\��
            var tr = ui.ui.transform as RectTransform;
            tr.localPosition = ui.from;
            ui.ui.SetActive(false);

            // �\�Ȃ� CanvasGroup ���擾�i�t�F�[�h�p�j
            ui.cg = ui.ui.GetComponent<CanvasGroup>();
            if (ui.fadeIfCanvasGroup && ui.cg != null)
            {
                ui.cg.alpha = 0f;
            }

            ui.started = false;
            ui.finished = false;
            ui.startTimeUnscaled = 0f;
        }
    }

    private void Update()
    {
        if (clearUIs == null || clearUIs.Count == 0)
        {
            enabled = false; // �Ď��s�v�Ȃ玩����~
            return;
        }

        // ��X�P�[���b�i����UI�� timeScale �̉e�����󂯂Ȃ��O��j
        float now = Time.realtimeSinceStartup;
        float elapsed = now - _t0;

        bool anyAlive = false;

        // for ���g���A�v�f�̃����^�C����Ԃ������߂���悤�ɂ���
        for (int i = 0; i < clearUIs.Count; i++)
        {
            var item = clearUIs[i];
            if (item == null || item.ui == null) continue;

            if (item.finished) continue; // �����ς�

            // �܂��J�n�����ɒB���Ă��Ȃ�
            if (elapsed < item.delay)
            {
                anyAlive = true;
                continue;
            }

            // �J�n�������i1�񂾂��j
            if (!item.started)
            {
                item.ui.SetActive(true);
                item.started = true;
                item.startTimeUnscaled = now;
            }

            // �i�s�x�i0��1�j
            float t = item.duration <= 0f ? 1f : Mathf.Clamp01((now - item.startTimeUnscaled) / item.duration);
            float eval = item.easing != null ? item.easing.Evaluate(t) : t;

            // ���`�ł͂Ȃ� curve �]���� from��to ���ԁi���t���[�� from ���g���̂ł͂Ȃ��A�Œ�̋N�_���g���j
            var tr = item.ui.transform as RectTransform;
            tr.localPosition = Vector3.LerpUnclamped(item.from, item.to, eval);

            // �t�F�[�h�i�C�Ӂj
            if (item.fadeIfCanvasGroup && item.cg != null)
            {
                item.cg.alpha = eval;
            }

            if (t >= 1f)
            {
                item.finished = true;
            }
            else
            {
                anyAlive = true;
            }
        }

        // �S���I������� Update ���~�߂Ė��ʂȕ��ׂ������
        if (!anyAlive)
        {
            enabled = false;
        }
    }

    /// <summary>
    /// �u���ցv�{�^��
    /// </summary>
    public void OnNextButtonClick()
    {
        GameManager.Instance?.ToEndRoll();
    }
}
