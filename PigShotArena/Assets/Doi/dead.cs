using UnityEngine;
using UnityEngine.SceneManagement;
public class dead : MonoBehaviour
{
   
    public static bool isPlayerDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPlayerDead = false;
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player1"))
        {
            Debug.Log("Dead");
            player1.alive1 = false;
            isPlayerDead = true;
        }
        else
        {
            Debug.Log("Dead");
            Debug.Log("p@layer2");
            player1.alive1 = true;
            isPlayerDead = true;
            //SceneManager.LoadScene("Result");
        }
        
    }
}

