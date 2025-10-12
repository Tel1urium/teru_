using UnityEngine;

public class PlayerCollisionSE : MonoBehaviour
{
    public AudioClip seClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = seClip;
    }

    void OnCollisionEnter(Collision collision)
    {
        // ‘Šè‚Ìƒ^ƒO‚ª "player2" ‚Ìê‡‚¾‚¯SE‚ğ–Â‚ç‚·
        if (collision.gameObject.CompareTag("player2"))
        {
            audioSource.Play();
        }
    }
}
