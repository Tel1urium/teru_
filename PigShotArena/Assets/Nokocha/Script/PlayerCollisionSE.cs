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
        // ����̃^�O�� "player2" �̏ꍇ����SE��炷
        if (collision.gameObject.CompareTag("player2"))
        {
            audioSource.Play();
        }
    }
}
