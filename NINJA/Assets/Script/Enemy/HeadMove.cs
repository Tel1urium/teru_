using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HeadMove : MonoBehaviour
{
    public float range = 180.0f;
    public float yMin, yMax;
    public float xMin, xMax;

    [SerializeField] private Transform player;
    [SerializeField] private Transform head;
    private OniStatus oniStatus;
    // Start is called before the first frame update
    void Start()
    {
        oniStatus = GetComponent<OniStatus>();
    }

    private void LateUpdate()
    {
        //ålŒö‚Ì•ûŒü
        Vector3 playerDirection = player.transform.position - transform.position;
        //“G‚Ì‘O•û‚©‚ç‚ÌålŒö‚Ì•ûŒü
        float angle = Vector3.Angle(transform.forward, playerDirection);

        if (angle <= range &&head != null)
        {
            if (oniStatus.oniState != OniStatus.State.Attack && oniStatus.oniState != OniStatus.State.skill)
            {
                Quaternion rotation = Quaternion.LookRotation(player.position - head.position);
                Vector3 euler = rotation.eulerAngles;
                Vector3 bodyEuler = transform.rotation.eulerAngles;
                //euler.z = -90.0f;
                euler.y = Mathf.Clamp(euler.y, bodyEuler.y + yMin, bodyEuler.y + yMax);//Y²‚Ì§Œä
                euler.x = Mathf.Clamp(euler.x, bodyEuler.x + xMin, bodyEuler.x + xMax);//X²‚Ì§Œä
                //head.rotation = Quaternion.Euler(euler);
                float RotateSpeed = 10.0f;
                head.rotation = Quaternion.Slerp(head.rotation, Quaternion.Euler(euler), RotateSpeed * Time.deltaTime);
            }
        }
    }
}
