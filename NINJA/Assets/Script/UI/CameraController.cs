using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class CameraController : MonoBehaviour
{
    public float posX, posY, posZ;
    public float rotationSpeed;
    public float originalAngle = -90.0f;

    [SerializeField] private Transform player;

    // Update is called once per frame
    void Update()
    {

        originalAngle += Input.GetAxisRaw("RightHorizontal") *rotationSpeed * Time.deltaTime;
        if(originalAngle > 180.0f) { originalAngle = -179.0f ; }
        else if(originalAngle < -180.0f) { originalAngle = 179.0f; }
        float rad = originalAngle * 2 * Mathf.PI / 360;
        transform.position = player.position + new Vector3(Mathf.Cos(rad) * posX, posY, Mathf.Sin(rad) * posZ);

        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
    }
}
