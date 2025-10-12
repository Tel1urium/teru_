using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private TMP_Text loadingText;

    private void Awake()
    {
        fadeCanvas.alpha = 1f; // ���X�^�[�g
    }

    public IEnumerator PlayLoadingAnimation()
    {
        // (1) ���̂܂܏����ҋ@
        yield return new WaitForSeconds(0.5f);

        // (2) �e�L�X�g�A�j���[�V���� (2�b)
        yield return StartCoroutine(AnimateText(2.0f));

        // (3) �e�L�X�g�����č��w�i�̂�
        loadingText.enabled = false;

        // (4) ���̂܂܏����ҋ@
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator AnimateText(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            loadingText.text = "Loading" + new string('.', (int)(time * 3) % 4);
            yield return null;
        }
    }
}
