using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyballController : MonoBehaviour
{
    public GameObject ball;
    public float thrust = 10f;
    Rigidbody rb_ball;

    float power;
    MeshRenderer ballMeshRenderer;
    Color ballColor;

    public Transform playerTransform;  

    public float spawnOffset = 1.5f; 
    private GameObject currentBall;  

    // Start is called before the first frame update
    void Start()
    {
        power = 0f;
        ballColor = new Color(0f, 0.2f, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            
            if (playerTransform != null)
            {
                
                if (currentBall != null)
                {
                    Destroy(currentBall);
                }

                
                currentBall = Instantiate(ball, playerTransform.position + playerTransform.forward * spawnOffset, playerTransform.rotation);
                rb_ball = currentBall.GetComponent<Rigidbody>();
                ballMeshRenderer = currentBall.GetComponent<MeshRenderer>();
                rb_ball.isKinematic = true;

                StartCoroutine("ChargeUp");
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            
            StopCoroutine("ChargeUp");

            if (currentBall != null)
            {
                rb_ball.isKinematic = false;
             
                Vector3 throwDirection = playerTransform.forward;  
               
                rb_ball.AddForce(throwDirection * thrust * power, ForceMode.Impulse);

                currentBall = null;
            }

            power = 0f;
        }

        if (currentBall != null)
        {
            currentBall.transform.position = playerTransform.position + playerTransform.forward * spawnOffset;
            currentBall.transform.rotation = playerTransform.rotation;  // プレイヤーの向きに合わせる
        }
    }

    IEnumerator ChargeUp()
    {
        while (true)
        {
            power += 0.03f;
            ballColor.r = power;
            ballMeshRenderer.material.SetColor("_BaseColor", ballColor);

            if (power >= 1f)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
}
