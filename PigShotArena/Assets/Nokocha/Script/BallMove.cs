using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class BallMove : MonoBehaviour
{
    public float moveForce = 5f;            //�{�[���ɗ^�����
    public float moveStopCheck = 0.5f;      //�~�܂���������
    public float waitTime = 2f;             //�~�܂��Ă��瓮���o���܂ł̎���

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

        //�����_���ȕ����ɗ͂�������
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        rb.AddForce(randomDir * moveForce, ForceMode.Impulse);

        MoveCheck = false;
    }
}
