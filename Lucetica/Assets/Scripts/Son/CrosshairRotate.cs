using UnityEngine;

public class CrosshairRotate : MonoBehaviour
{
    [Header("=== ��]���x ===")]
    public float rotationSpeed = 100f; // 1�b������̉�]�p�x

    void Update()
    {
        // === Z���𒆐S�ɉ�]������ ===
        transform.Rotate(Vector3.forward, rotationSpeed * Time.unscaledDeltaTime);
    }
}
