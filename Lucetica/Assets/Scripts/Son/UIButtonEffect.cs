using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


[RequireComponent(typeof(Image))]
public class UIButtonEffect_Unscaled : MonoBehaviour,
    ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISubmitHandler
{
    // === �Q�� ===
    public Image targetImage;

    // === ���J���[�ێ� ===
    public Color originalColor;

    // === ��ԃt���O ===
    private bool isSelected = false; // UI��őI������Ă��邩
    private bool isBlinking = false; // �_�ł��s����
    private bool isPressed = false;  // �������i�}�E�X�j��

    [Header("�_�Őݒ�iTimeScale�����j")]
    [Tooltip("�_�Ŏ��̍ŏ��A���t�@�l")]
    [Range(0f, 1f)] public float minAlpha = 0.01f;
    [Tooltip("�_�ő��x�iHz�j")]
    public float blinkSpeedHz = 1f;

    [Header("�������̈Â��W��")]
    [Tooltip("RGB�ɏ�Z����W���B0=�^����, 1=�ω��Ȃ�")]
    [Range(0f, 1f)] public float darkenFactor = 0.6f;

    private void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
            originalColor = targetImage.color;
        }
    }

    private void OnEnable()
    {
        // �O�̂��ߏ�����
        ResetVisualImmediate();
    }

    private void OnDisable()
    {
        // ���������Ɍ����ڂ�߂�
        ResetVisualImmediate();
    }

    private void Update()
    {
        // === �_�ł� Time.unscaledTime ���g�p ===
        if (isBlinking && !isPressed)
        {
            float t = (Mathf.Sin(Time.unscaledTime * blinkSpeedHz * 2f * Mathf.PI) + 1f) * 0.5f;

            float targetA = Mathf.Lerp(minAlpha, originalColor.a, t);

            Color c = targetImage.color;
            c.a = targetA;
            targetImage.color = c;
        }
    }

    // === �����ڂ𑦍��Ɍ��� ===
    private void ResetVisualImmediate()
    {
        isBlinking = false;
        isPressed = false;
        targetImage.color = originalColor;
    }

    // === �I�����ꂽ ===
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        if (!isPressed) isBlinking = true;
    }

    // === �I������ ===
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        ResetVisualImmediate();
    }

    // === �}�E�X�������� ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        isSelected = true;
        if (!isPressed) isBlinking = true;
    }

    // === �}�E�X���o�� ===
    public void OnPointerExit(PointerEventData eventData)
    {
        isSelected = false;
        ResetVisualImmediate();
    }

    // === �}�E�X���� ===
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        isBlinking = false; // �����Ă���Ԃ͓_�Œ�~

        // RGB ���������ĈÂ�����B�A���t�@�͎��F���̂��ߌ��ɖ߂�
        Color c = targetImage.color;
        c.r = originalColor.r * darkenFactor;
        c.g = originalColor.g * darkenFactor;
        c.b = originalColor.b * darkenFactor;
        c.a = originalColor.a;
        targetImage.color = c;
    }

    // === �}�E�X���� ===
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        if (isSelected)
        {
            isBlinking = true;

            Color c = targetImage.color;
            c.a = originalColor.a;
            targetImage.color = c;
        }
        else
        {
            ResetVisualImmediate();
        }
    }

    // === Submit ===
    public void OnSubmit(BaseEventData eventData)
    {
        StartCoroutine(FlashDarkOnSubmit_Unscaled());
    }


    private IEnumerator FlashDarkOnSubmit_Unscaled()
    {
        // �Â�����
        Color c = targetImage.color;
        c.r = originalColor.r * darkenFactor;
        c.g = originalColor.g * darkenFactor;
        c.b = originalColor.b * darkenFactor;
        c.a = originalColor.a; // �A���t�@�͂�������\��
        targetImage.color = c;

        yield return new WaitForEndOfFrame();

        if (isSelected)
        {
            isPressed = false;
            isBlinking = true;

            Color c2 = targetImage.color;
            c2.a = originalColor.a;
            targetImage.color = c2;
        }
        else
        {
            ResetVisualImmediate();
        }
    }
}
