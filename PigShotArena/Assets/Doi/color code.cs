using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorFilterChanger : MonoBehaviour
{
    public Volume volume; // InspectorでGlobal Volumeをアタッチ

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // VolumeのProfileからColorAdjustmentsを取得
        if (volume.profile.TryGet(out colorAdjustments))
        {
            // カラーコードをColor型に変換
            Color newColor;
            if (ColorUtility.TryParseHtmlString("#a0e0ff", out newColor))
            {
                colorAdjustments.colorFilter.value = newColor;
            }
            else
            {
                Debug.LogWarning("カラーコードが無効です");
            }
        }
        else
        {
            Debug.LogWarning("Color Adjustments が Volume Profile にありません");
        }
    }
}