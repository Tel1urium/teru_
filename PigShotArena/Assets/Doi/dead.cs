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
        var player=other.GetComponent<Player>();
        if (player != null)
        {
            player.OnHit();
        }
    }
}

