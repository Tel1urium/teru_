using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1.0f; // �Œ�1�b

    private void Awake()
    {
        fadeCanvas.alpha = 0f; // �f�t�H���g����
    }

    private void Start()
    {
        fadeCanvas.alpha = 1f; // �V�[���J�n���͍�
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0f;
    }

    public IEnumerator FadeOut()
    {
        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 1f;
    }
}
