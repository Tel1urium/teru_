using UnityEngine;

public class BoundReflect : MonoBehaviour
{
    Rigidbody rb;
    public LayerMask collisionMask;
    float yPos;
    float yCurrent;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        yCurrent = this.transform.position.y;
    }
    private void Update()
    {
        yPos = this.transform.position.y;
        if (yCurrent < yPos) this.transform.position = new Vector3(this.transform.position.x, yCurrent, this.transform.position.z);
        else if (yCurrent > yPos) yCurrent = yPos;
        CollisionPredictionAndReflect();
    }
    public void CollisionPredictionAndReflect()
    {
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < 0.0001f) return;

        Vector3 direction = velocity.normalized;
        Ray ray = new Ray(transform.position, Vector3.zero);
        var sphereRadius = 0.7f;
        RaycastHit hit;
        var rayLength = 0.00000f;
        if (Physics.SphereCast(ray, sphereRadius, out hit, rayLength, collisionMask))
        {
            Vector3 hitNormal = hit.normal;
            Vector3 reflected = Vector3.Reflect(velocity, hitNormal);

            rb.linearVelocity = Vector3.zero; // 一度停止
            rb.AddForce(reflected.normalized * 50, ForceMode.VelocityChange);

            Debug.DrawRay(transform.position, direction * hit.distance, Color.red, 0.2f);
            Debug.DrawRay(hit.point, hitNormal, Color.yellow, 0.2f);

        }
        else
        {
            Debug.DrawRay(transform.position, direction * rayLength, Color.green, 0.1f);
        }
    }

}
