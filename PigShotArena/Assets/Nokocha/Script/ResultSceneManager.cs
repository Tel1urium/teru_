using UnityEngine;

public class ResultSceneManager : MonoBehaviour
{
    private bool hasStarted = false;

    void Update()
    {
        if (dead.isPlayerDead && !hasStarted)
        {
            hasStarted = true;
            dead.isPlayerDead = false; // �t���O���Z�b�g
            SceneLoaderWithFade.Instance.LoadSceneWithFade("Result");
        }
    }
}
