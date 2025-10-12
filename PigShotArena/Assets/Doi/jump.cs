using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 5f; // �W�����v�̋���
    bool IsGround = false;
    private Rigidbody rb;
    public float gravity = -9.8f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // �n�ʂɂ��鎞�̂݃W�����v
        if (IsGround)
        {
            jumpForce = 5;

            Jump();
            IsGround = false;
        }
        else
        {
            Gravity();
        }
    }

    void Jump()
    {
        // y�����̑��x�����Z�b�g���Ă����ɗ͂�������
        rb.linearVelocity = new Vector3(0, 0f,0);
        rb.linearVelocity = new Vector3(0, jumpForce, 0);
        
    }
    void Gravity()
    {
        jumpForce += gravity * Time.deltaTime;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("under"))
        {
            IsGround = true;
        }
    }
}