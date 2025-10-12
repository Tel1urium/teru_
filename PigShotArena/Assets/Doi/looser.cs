using UnityEngine;

public class looser : MonoBehaviour
{
    public float maxAngle = 30f;
    // 振動の速さ
    public float speed = 2f;

    // 初期回転角を記憶
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Mathf.Sinで-1〜1の間を揺らす
        float angle = maxAngle * Mathf.Sin(Time.time * speed);

        // 初期回転を基準にZ軸回転（2D風に左右に振るならZ軸）
        transform.rotation = initialRotation * Quaternion.Euler(angle, 0, 0);
    }
}
