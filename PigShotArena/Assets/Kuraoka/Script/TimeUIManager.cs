using UnityEngine;
using UnityEngine.UI;

public class TimeUIManager : MonoBehaviour
{
    [Header("�Q�Ƃ��� MainGameEnd �X�N���v�g")]
    public MainGameEnd mainGameEnd;

    [Header("�����X�v���C�g 0?9")]
    public Sprite[] numberSprites; // index = ����
    [Header("�e�����X�v���C�g�̃T�C�Y 0?9")]
    public Vector2[] numberSizes;  // index = �������Ƃ̃T�C�Y

    [Header("UI �C���[�W�i�����珇�Ɂj")]
    public Image digitHundreds;
    public Image digitTens;
    public Image digitOnes;

    [Header("�P�ʃX�v���C�g�ƃT�C�Y")]
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

        // �c�莞�� = �ő� - �o��
        remainingTime = Mathf.Clamp(mainGameEnd.GetRemainingTime(), 0f, 999f);

        // �����؂�̂�
        int displayTime = Mathf.FloorToInt(remainingTime);

        int hundreds = displayTime / 100;
        int tens = (displayTime / 10) % 10;
        int ones = displayTime % 10;

        // �e�����X�V
        UpdateDigitImage(digitHundreds, hundreds);
        UpdateDigitImage(digitTens, tens);
        UpdateDigitImage(digitOnes, ones);

        // 100�̈ʂ��\���i99�b�ȉ��Ȃ�j
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

        // �T�C�Y�ݒ�F�������Ƃ̃T�C�Y�𔽉f
        if (numberSizes != null && numberSizes.Length > num)
        {
            image.rectTransform.sizeDelta = numberSizes[num];
        }
    }
}