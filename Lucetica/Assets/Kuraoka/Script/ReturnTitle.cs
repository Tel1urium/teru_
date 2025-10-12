using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnTitle : MonoBehaviour
{
    private bool _requestedReturn = false; // �{�^���Ń^�C�g���֖߂��t���O

    private void Start()
    {
        // �N������ Result �X�e�[�g�ɑJ��
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            StartCoroutine(WaitForGameManagerAndGameOver());
        }
    }

    private IEnumerator WaitForGameManagerAndGameOver()
    {
        while (GameManager.Instance == null)
            yield return null;

        GameManager.Instance.GameOver();
    }

    // �{�^������Ă�
    public void OnClickReturn()
    {
        if (GameManager.Instance == null) return;

        _requestedReturn = true;       // �^�C�g���ɖ߂��t���O
        GameManager.Instance.ToTitle(); // ��ԑJ�ڂ����s
        StartCoroutine(WaitForTitleState());
    }

    // Title ��ԂɂȂ�܂őҋ@
    private IEnumerator WaitForTitleState()
    {
        while (GameManager.Instance != null &&
               GameManager.Instance.CurrentState != GameState.Title)
        {
            yield return null;
        }

        if (_requestedReturn)
        {
            SceneManager.LoadScene("TestTitleScene");
            _requestedReturn = false;
        }
    }
}