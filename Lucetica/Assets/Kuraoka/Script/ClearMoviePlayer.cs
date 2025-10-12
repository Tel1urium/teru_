using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using static EventBus;
public class ClearMoviePlayer : MonoBehaviour
{
    [Header("�{�X�I�u�W�F�N�g�������ɃA�T�C��")]
    public GameObject boss;

    private bool triggered = false;

    void Update()
    {
        // �܂��J�ڂ��Ă��Ȃ� && �{�X�� null �܂��͔�A�N�e�B�u�Ȃ�
        if (!triggered && (boss == null || !boss.activeInHierarchy))
        {
            triggered = true;
            // GameManager�o�R��Title�֖߂�
            GameManager.Instance?.ToTitle();
        }
    }
}
