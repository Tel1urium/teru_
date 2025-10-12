using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ReturnToGame : MonoBehaviour
{
    [Header("�_�ł�����UI")]
    public GameObject blinkingUI; // Canvas �̎q�I�u�W�F�N�g�Ȃ�

    [Header("�_�Őݒ�")]
    [Tooltip("1��̓_�łɂ����鎞�ԁi�b�j")]
    public float fadeDuration = 0.5f;

    [Header("�X�e�[�W���")]
    public string stageSceneName = "NormalStage1_Forest"; // �X�e�[�W1
    public string bossSceneName = "SampleScene 1";        // �{�X��
    public bool isBossStage = false;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        if (blinkingUI != null)
        {
            // CanvasGroup ��ǉ����ē����x����
            canvasGroup = blinkingUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = blinkingUI.AddComponent<CanvasGroup>();

            StartCoroutine(FadeBlinkUI());
        }
    }

    private IEnumerator FadeBlinkUI()
    {
        while (true)
        {
            // �t�F�[�h�A�E�g
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
            // �t�F�[�h�C��
            yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        }
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = end;
    }

    // �{�^������Ă�
    public void OnClickReturnToGame()
    {
        string targetScene=isBossStage? bossSceneName:stageSceneName;
        //�V�[�����[�h
        SceneManager.LoadScene(targetScene);
        if(GameManager.Instance!=null)
        {
            GameManager.Instance.StartGame();
        }
    }
}
