using UnityEngine;

public class WinCon : MonoBehaviour
{

    public static bool WinPlayer = true;
    int p1score,p2score;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p1score = ScoreManager.Player1Score;
        p2score = ScoreManager.Player2Score;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(p1score >  p2score)
        {
            WinPlayer = true;
            Debug.Log("P1‚ÌŸ‚¿");
        }
        else if(p1score < p2score)
        {
            WinPlayer = false;
            Debug.Log("P2‚ÌŸ‚¿");
        }
        else
        {
            Debug.Log("ˆø‚«•ª‚¯");
        }
    }
}
