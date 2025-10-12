using System;
using UnityEngine;

/// <summary>
/// �c�� 1 �̂̎����E�t�F�[�h�Ǘ�
/// �EMaterialPropertyBlock �� �� ���X�V�i�C���X�^���X���R�X�g����j
/// �E�����Ŏ����j���i�R�[���o�b�N�j
/// </summary>
[DisallowMultipleComponent]
public class DashGhostInstance : MonoBehaviour
{
    private float _life;
    private float _age;
    private float _initialAlpha;
    private AnimationCurve _curve;
    private Action _onDispose;

    private MeshRenderer _mr;
    private MaterialPropertyBlock _mpb;

    // ���{��FURP/Lit �Ȃǂ̑�\�I�J���[�������ɒT��
    private static readonly int[] _colorIDs = new int[]
    {
        Shader.PropertyToID("_BaseColor"), // URP
        Shader.PropertyToID("_Color")      // Built-in/Standard
    };

    public void Init(float life, float initialAlpha, AnimationCurve curve, Action onDispose)
    {
        _life = Mathf.Max(0.01f, life);
        _initialAlpha = Mathf.Clamp01(initialAlpha);
        _curve = curve;
        _onDispose = onDispose;

        _mr = GetComponent<MeshRenderer>();
        if (_mr == null) _mr = gameObject.AddComponent<MeshRenderer>();
        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        // ���{��F�J�n���ɏ��� �� �𔽉f
        ApplyAlpha(1f);
    }

    private void Update()
    {
        _age += Time.deltaTime;
        float t01 = Mathf.Clamp01(_age / _life);
        ApplyAlpha(1f - _curve.Evaluate(t01)); // ���{��F�J�[�u�� 1��0 �z��i���`�Ȃ炻�̂܂܁j

        if (_age >= _life)
        {
            _onDispose?.Invoke();
        }
    }

    private void ApplyAlpha(float factor)
    {
        if (_mr == null) return;

        _mr.GetPropertyBlock(_mpb);

        // ���{��F�ŏ��Ɍ��������J���[�v���p�e�B�� �� ��K�p
        for (int i = 0; i < _colorIDs.Length; ++i)
        {
            if (_mr.sharedMaterial != null && _mr.sharedMaterial.HasProperty(_colorIDs[i]))
            {
                Color c = _mr.sharedMaterial.GetColor(_colorIDs[i]);
                c.a = _initialAlpha * Mathf.Clamp01(factor);
                _mpb.SetColor(_colorIDs[i], c);
                break;
            }
        }
        _mr.SetPropertyBlock(_mpb);
    }
}
