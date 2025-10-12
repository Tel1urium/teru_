using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class BallMove : MonoBehaviour
{
    public float moveForce = 5f;            //ƒ{[ƒ‹‚É—^‚¦‚é—Í
    public float moveStopCheck = 0.5f;      //~‚Ü‚Á‚½‚©Œ©‚é
    public float waitTime = 2f;             //~‚Ü‚Á‚Ä‚©‚ç“®‚«o‚·‚Ü‚Å‚ÌŠÔ

    private Rigidbody rb;
    private bool MoveCheck = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        if (rb.velocity.magnitude < moveStopCheck && !MoveCheck)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    System.Collections.IEnumerator WaitAndMove()
    {
        MoveCheck = true;
        yield return new WaitForSeconds(waitTime);

        //ƒ‰ƒ“ƒ_ƒ€‚È•ûŒü‚É—Í‚ğ‰Á‚¦‚é
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        rb.AddForce(randomDir * moveForce, ForceMode.Impulse);

        MoveCheck = false;
    }
}
