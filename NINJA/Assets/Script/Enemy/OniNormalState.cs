using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class OniNormalState : MonoBehaviour
{
    private OniStatus oniStatus;

    public float speed = 3.5f;
    public float skillSpan = 10.0f;//�Z�̊Ԋu

    [SerializeField] float searchAngle = 10.0f;
    private float skillTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        oniStatus = GetComponent<OniStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        if (oniStatus.oniState != OniStatus.State.skill && oniStatus.navgationEnabled)
        {
            skillTime += Time.deltaTime;
        }

        if (oniStatus.oniState != OniStatus.State.Normal) return;

        if (skillTime > skillSpan)
        {
            skillTime = 0.0f;
            oniStatus.oniState = OniStatus.State.skill;
            return;
        }
        else if (oniStatus.dist <= oniStatus.enemyStopD)//���̋����Ƀv���C���[����������AAttackState�֑J��
        {
            oniStatus.agent.speed = 0;
            oniStatus.oniState = OniStatus.State.Attack;
            return;
        }
        //------------------------------------------------------------------
        if (!GetComponent<OniAttackState>().attack)
        {
            oniStatus.agent.speed = speed;
        }
        else { oniStatus.agent.speed = 0; }

        //�v���C���[�̕����������i�̂����������j
        float RotateSpeed = 2.0f;
        Quaternion rotation = Quaternion.LookRotation(new Vector3(oniStatus.player.transform.position.x, transform.position.y,
            oniStatus.player.transform.position.z) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotateSpeed * Time.deltaTime);
    }

    //���E�ɓ��������ǂ���
    public bool ChaseAngle(float searchAngles)
    {
        //��l���̕���
        Vector3 playerDirection = oniStatus.player.transform.position - transform.position;
        //�G�̑O������̎�l���̕���
        float angle = Vector3.Angle(transform.forward, playerDirection);
        if (angle < searchAngles) { return true; }
        return false;
    }


#if UNITY_EDITOR
    //�@�T�[�`����p�x�\��
    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawSolidArc(transform.position, Vector3.up, Quaternion.Euler(0f, -searchAngle, 0f) * transform.forward, searchAngle * 2f, 5);
        Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), transform.forward * 100);
    }
#endif
}
