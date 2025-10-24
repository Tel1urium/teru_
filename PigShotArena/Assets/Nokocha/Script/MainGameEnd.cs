using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameEnd : MonoBehaviour
{
    GameManager gameManager;
    private float gameTime = 0f;
    private float gameMaxTime = 120f;
    private bool stopper;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gameManager = GameObject.Find("SceneManagerObj").gameObject.GetComponent<GameManager>();
        stopper = true;
    }

    public void MainGameTimeCount()
    {
        if (gameManager != null && gameManager.state == GameManager.State.Ingame)
        {
            gameTime += Time.deltaTime;
            //Debug.Log("Œo‰ßŽžŠÔF"+gameTime);
        }
    }

    public void MainGameEndCheck()
    {
        if (stopper)
        {
            if (gameTime > gameMaxTime)
            {
                if(ScoreManager.Player1Score != ScoreManager.Player2Score)
                {
                    SceneLoaderWithFade.Instance.LoadSceneWithFade("Result");
                    stopper = false;
                }
                else
                {
                    SceneLoaderWithFade.Instance.LoadSceneWithFade("hikiwake");
                    stopper = false;
                }
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        MainGameTimeCount();
        MainGameEndCheck();
    }
}
