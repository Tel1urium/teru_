using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorFilterChanger : MonoBehaviour
{
    public Volume volume; // Inspector��Global Volume���A�^�b�`

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // Volume��Profile����ColorAdjustments���擾
        if (volume.profile.TryGet(out colorAdjustments))
        {
            // �J���[�R�[�h��Color�^�ɕϊ�
            Color newColor;
            if (ColorUtility.TryParseHtmlString("#a0e0ff", out newColor))
            {
                colorAdjustments.colorFilter.value = newColor;
            }
            else
            {
                Debug.LogWarning("�J���[�R�[�h�������ł�");
            }
        }
        else
        {
            Debug.LogWarning("Color Adjustments �� Volume Profile �ɂ���܂���");
        }
    }
}