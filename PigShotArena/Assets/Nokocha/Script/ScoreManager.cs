using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager instance;

    public static int Player1Score;
    public static int Player2Score;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player1Score = 0;
        Player2Score = 0;
    }

    void Awake()
    {
        //if(instance == null)
        //{
        //    instance = this;
        //    //DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ball1"))
        {
            Player1Score += 1;
            Debug.Log("player1スコア" + Player1Score);
            //触れたボールを削除
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("ball2"))
        {
            Player2Score += 1;
            Debug.Log("player2スコア" + Player2Score);
            Destroy(other.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
