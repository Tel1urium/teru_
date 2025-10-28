using UnityEngine;
using UnityEngine.UI;

public class TimeUIManager : MonoBehaviour
{
    [Header("参照する MainGameEnd スクリプト")]
    public MainGameEnd mainGameEnd;

    [Header("数字スプライト 0?9")]
    public Sprite[] numberSprites; // index = 数字
    [Header("各数字スプライトのサイズ 0?9")]
    public Vector2[] numberSizes;  // index = 数字ごとのサイズ

    [Header("UI イメージ（左から順に）")]
    public Image digitHundreds;
    public Image digitTens;
    public Image digitOnes;

    [Header("単位スプライトとサイズ")]
    public Sprite unitSprite;
    public Image unitSec;
    public Vector2 unitSize = new Vector2(80, 80);

    private float remainingTime;

    void Start()
    {
        if (unitSec != null && unitSprite != null)
        {
            unitSec.sprite = unitSprite;
            unitSec.rectTransform.sizeDelta = unitSize;
        }
    }

    void Update()
    {
        if (mainGameEnd == null) return;

        // 残り時間 = 最大 - 経過
        remainingTime = Mathf.Clamp(mainGameEnd.GetRemainingTime(), 0f, 999f);

        // 小数切り捨て
        int displayTime = Mathf.FloorToInt(remainingTime);

        int hundreds = displayTime / 100;
        int tens = (displayTime / 10) % 10;
        int ones = displayTime % 10;

        // 各桁を更新
        UpdateDigitImage(digitHundreds, hundreds);
        UpdateDigitImage(digitTens, tens);
        UpdateDigitImage(digitOnes, ones);

        // 100の位を非表示（99秒以下なら）
        if (digitHundreds != null)
        {
            digitHundreds.gameObject.SetActive(displayTime >= 100);
        }
    }

    private void UpdateDigitImage(Image image, int num)
    {
        if (image == null || numberSprites == null || num < 0 || num >= numberSprites.Length)
            return;

        image.sprite = numberSprites[num];

        // サイズ設定：数字ごとのサイズを反映
        if (numberSizes != null && numberSizes.Length > num)
        {
            image.rectTransform.sizeDelta = numberSizes[num];
        }
    }
}