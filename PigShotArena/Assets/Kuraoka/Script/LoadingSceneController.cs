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
        fadeCanvas.alpha = 1f; // 黒スタート
    }

    public IEnumerator PlayLoadingAnimation()
    {
        // (1) 黒のまま少し待機
        yield return new WaitForSeconds(0.5f);

        // (2) テキストアニメーション (2秒)
        yield return StartCoroutine(AnimateText(2.0f));

        // (3) テキスト消して黒背景のみ
        loadingText.enabled = false;

        // (4) 黒のまま少し待機
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
