using UnityEngine;

public class effect : MonoBehaviour
{
    public GameObject boingEffectPrefab;
    public AudioClip boingSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player1"))
        {
            if (boingEffectPrefab != null)
            {
                Instantiate(boingEffectPrefab, transform.position, Quaternion.identity);
            }
            if (boingSound != null)
            {
                AudioSource.PlayClipAtPoint(boingSound, transform.position);
            }
        }
    }
}
