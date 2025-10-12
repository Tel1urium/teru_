using UnityEngine;

public class BoundReflect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("反射ー");
        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null) return;

        // �ڐG�_����@���x�N�g�����擾
        ContactPoint contact = collision.contacts[0];
        Vector3 inVelocity = otherRb.linearVelocity;
        Vector3 normal = contact.normal;

        // ���˃x�N�g�����v�Z
        Vector3 reflected = Vector3.Reflect(inVelocity, normal);

        // ���ˌ�̑��x��K�p�i���x�ێ� or �����j
        otherRb.linearVelocity = reflected.normalized * inVelocity.magnitude;

    }
}
