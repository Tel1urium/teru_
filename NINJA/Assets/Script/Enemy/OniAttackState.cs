using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Unity.Burst.CompilerServices;

public class OniAttackState : MonoBehaviour
{
    public float YokoAttackSpan = 3.0f;
    public float TateAttackSpan = 5.0f;
    public float RotateSpeed = 3.0f;
    public float TateAccumulateTime = 1.0f;//�c�U��������܂ł̗��ߎ���
    public float YokoAccumulateTime = 1.0f;//���U��������܂ł̗��ߎ���
    [Range(0, 10)] public float probability = 5.0f;//�U���̊m��
    public bool attack { get; set; } = false;
    public AudioClip attackSound;
    public float searchAngle = 10.0f;

    private OniStatus oniStatus;
    private float attackTime = 0.0f;
    private RaycastHit hit;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        oniStatus = GetComponent<OniStatus>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (oniStatus.oniState != OniStatus.State.Attack) return;
        //���̋�������v���C���[�����ꂽ��ANormalState�֑J��
        /*if (oniStatus.dist > oniStatus.enemyStopD)
        {
            oniStatus.oniState = OniStatus.State.Normal;
            return;
        }*/
        //------------------------------------------------------------
        Physics.Raycast(this.transform.position + new Vector3(0, 0.5f, 0), transform.forward, out hit);
        //���o�ɓ����Ă��Ȃ���΁A�v���C���[�̕����������i�̂����������j
        if (!attack)
        {
            if (!(AttackAngle(searchAngle) || (hit.transform.gameObject.tag == "Player")))
            {
                RotateSpeed = 3.0f;
                Quaternion rotation = Quaternion.LookRotation(new Vector3(oniStatus.player.transform.position.x, transform.position.y,
                    oniStatus.player.transform.position.z) - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotateSpeed * Time.deltaTime);
                oniStatus.animator.SetFloat("rotation", RotateSpeed);
            }
        }
        else { oniStatus.animator.SetFloat("rotation", 0); }
        //�T�[�`����p�x���������甭��
        if (!attack && (AttackAngle(searchAngle) || (hit.transform.gameObject.tag == "Player")))
        {
            attackTime = 0;
            attack = true;
            oniStatus.animator.ResetTrigger("AttackTrigger");
            int half = UnityEngine.Random.Range(1, 11);
            if (half <= probability)
            {
                oniStatus.animator.SetTrigger("YokoAttackTrigger");
            }
            else
            {
                oniStatus.animator.SetTrigger("TateAttackTrigger");
            }
        }
        attackTime += Time.deltaTime;
    }

    public bool AttackAngle(float searchAngles)
    {
        //��l���̕���
        Vector3 playerDirection = oniStatus.player.transform.position - transform.position;
        //�G�̑O������̎�l���̕���
        float angle = Vector3.Angle(transform.forward, playerDirection);
        if (angle < searchAngles) { return true; }
        return false;
    }

    public void TateAccumulate()
    {
        oniStatus.animator.SetTrigger("AttackTrigger");
    }
    public void YokoAccumulate()
    {
        oniStatus.animator.SetTrigger("AttackTrigger");
    }
    public void TateAttackEnd()
    {
        attack = false;
        oniStatus.oniState = OniStatus.State.Normal;
    }
    public void YokoAttackEnd()
    {
        attack = false;
        oniStatus.oniState = OniStatus.State.Normal;
    }

    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
    public void AttackSound()
    {
        audioSource.PlayOneShot(attackSound);
    }
}
