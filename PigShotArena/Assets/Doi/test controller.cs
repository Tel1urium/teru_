using UnityEngine;

public class testcontroller : MonoBehaviour
{
    public float moveForce = 10f; // �ړ��Ɏg���͂̑傫��
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // ���͂��擾
        float h = Input.GetAxis("Horizontal"); // A,D
        float v = Input.GetAxis("Vertical");   // W,S

        // �J�����̌����Ɋ֌W�Ȃ��A�I�u�W�F�N�g�̃��[�J���O�㍶�E�ɓ�����
        Vector3 moveDir = transform.right * h + transform.forward * v;

        // �͂�������i���������j
        rb.AddForce(moveDir.normalized * moveForce);
    }
}
