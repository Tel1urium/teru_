using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NumberSpriteData
{
    public Sprite sprite;        // �����X�v���C�g
    public Vector2 size = new Vector2(100, 100); // �\���T�C�Y�i�C�ӎw��j
}

public class ScoreUI : MonoBehaviour
{
    [Header("�����X�v���C�g�ݒ� (0?9)")]
    public NumberSpriteData[] numberSprites = new NumberSpriteData[10];

    [Header("Player1 �X�R�AUI")]
    public Image player1TensImage;
    public Image player1OnesImage;

    [Header("Player2 �X�R�AUI")]
    public Image player2TensImage;
    public Image player2OnesImage;

    private int prevP1Score = -1;
    private int prevP2Score = -1;

    void Update()
    {
        int p1 = Mathf.Clamp(ScoreManager.Player1Score, 0, 70);
        int p2 = Mathf.Clamp(ScoreManager.Player2Score, 0, 70);

        if (p1 != prevP1Score)
        {
            UpdateScoreUI(p1, player1TensImage, player1OnesImage);
            prevP1Score = p1;
        }

        if (p2 != prevP2Score)
        {
            UpdateScoreUI(p2, player2TensImage, player2OnesImage);
            prevP2Score = p2;
        }
    }

    void UpdateScoreUI(int score, Image tensImage, Image onesImage)
    {
        int tens = score / 10;
        int ones = score % 10;

        SetImageSpriteAndSize(tensImage, tens);
        SetImageSpriteAndSize(onesImage, ones);
    }

    void SetImageSpriteAndSize(Image img, int num)
    {
        if (num < 0 || num >= numberSprites.Length) return;

        img.sprite = numberSprites[num].sprite;

        // �X�v���C�g���ƂɎw�肵���T�C�Y��K�p
        RectTransform rect = img.GetComponent<RectTransform>();
        rect.sizeDelta = numberSprites[num].size;
    }
}
