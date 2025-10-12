using UnityEngine;

public class testcontroller : MonoBehaviour
{
    public float moveForce = 10f; // 移動に使う力の大きさ
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 入力を取得
        float h = Input.GetAxis("Horizontal"); // A,D
        float v = Input.GetAxis("Vertical");   // W,S

        // カメラの向きに関係なく、オブジェクトのローカル前後左右に動かす
        Vector3 moveDir = transform.right * h + transform.forward * v;

        // 力を加える（物理挙動）
        rb.AddForce(moveDir.normalized * moveForce);
    }
}
