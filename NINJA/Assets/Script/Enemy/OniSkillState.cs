using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Cinemachine;

public class OniSkillState : MonoBehaviour
{
    private OniStatus oniStatus;
    private OniAttackState oniAttackState;
    public float attackDist = 5.0f;//�U�����鋗��
    public float t = 3.0f;//��Ԏ���
    public float gravity = 30.0f;//�d��
    public float rushSpeed = 15.0f;//�ːi�X�s�[�h
    public float rushDist = 20.0f;//�ːi���鋗��
    public float skillCoolDown = 3.0f;//�X�L���̃N�[���_�E��
    public float rushIdel = 1.5f;//�ːi���n�܂�܂ł̗��ߎ���
    public float jumpIdel = 1.0f;//�W�����v���n�܂�܂ł̗��ߎ���
    public bool isGrounded = true;
    [Range(0, 10)] public float probability = 5.0f;//�X�L���̊m��
    public AudioClip jumpSound;
    public AudioClip tackleSound;
    [Header("�t�B�[���h�̒��S�̍��W")] public Vector3 fieldCenter;//�t�B�[���h�̒��S

    [SerializeField, Tooltip("�X�L�������邱�Ƃ�m�点��p�[�e�B�N��")] private ParticleSystem dangerParticle;//�X�L�����g�����Ƃ�m�点��p�[�e�B�N��
    [SerializeField] private GameObject dangerJumpArea;//�댯�ȃG���A�̃G�t�F�N�g
    [SerializeField] private GameObject dangerRushArea;//�댯�ȃG���A�̃G�t�F�N�g
    [SerializeField] private CinemachineImpulseSource shaker;
    //[SerializeField, Header("�t�B�[���h�̒��S�̍��W")] private Transform fieldCenter;//�t�B�[���h�̒��S
    private AudioSource audioSource;
    private GameObject shockWave;//�Ռ��g
    private GameObject rush;//�ːi���̃G�t�F�N�g�ƃR���C�_�[
    private Collider jumpCollider;
    RaycastHit hit;
    private Rigidbody rBody;
    private Vector3 velocity;
    private bool jumpFlag = false;
    private bool rushFlag = false;
    private bool rushFinish = false;
    private float skillTime = 0f;
    private Vector3 pastPos;

    private Coroutine _skillIdelCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        oniStatus = GetComponent<OniStatus>();
        oniAttackState = GetComponent<OniAttackState>();
        rBody = GetComponent<Rigidbody>();
        rBody.isKinematic = true;
        audioSource = GetComponent<AudioSource>();
        shockWave = transform.GetChild(1).gameObject;
        shockWave.SetActive(false);
        rush = transform.GetChild(2).gameObject;
        rush.SetActive(false);
        jumpCollider = transform.GetChild(3).gameObject.GetComponent<Collider>();
        jumpCollider.enabled = false;
        _skillIdelCoroutine = StartCoroutine(SkillIdleCoroutine());
    }

    private void FixedUpdate()
    {
        if (jumpFlag)
        {
            Jump();
        }
        if (rushFlag)
        {
            Rush();
        }
    }

    //�ːi�X�L��
    private void Rush()
    {
        //�����_���ŉ�����Skill�������A��������Ȃ��ꍇ�A�ҋ@�B
        if (skillTime > rushIdel)
        {
            dangerParticle.Stop();
            Vector3 velocity = transform.forward * rushSpeed;
            velocity.y = 0;
            rBody.velocity = velocity;
            audioSource.PlayOneShot(tackleSound);
            if (rBody.velocity.magnitude > 0) { rush.SetActive(true); }
            Vector3 dist = transform.position - pastPos;
            if (dist.magnitude >= rushDist || skillTime >= rushIdel + 2)
            {
                rushFinish = true;
            }
            oniStatus.animator.SetFloat("rBodyMoveSpeed", rBody.velocity.magnitude);
        }
        skillTime += Time.deltaTime;
    }

    //�X�L���̑҂����ԂƁA�X�L����I�ԃR���[�`��
    private IEnumerator SkillIdleCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => oniStatus.navgationEnabled);
            yield return new WaitUntil(() =>
                oniStatus.oniState == OniStatus.State.skill
            );
            oniAttackState.attack = false;
            yield return new WaitUntil(() =>
                oniStatus.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f//�Đ����������1�ȏ�ɂȂ�
            );
            dangerParticle.Play();
            oniStatus.agent.speed = 0;
            oniStatus.agent.enabled = false;
            rBody.isKinematic = false;
            int dice = UnityEngine.Random.Range(1, 11);
            if (oniStatus.dist >= 15.0f) { probability = 8; }
            if (dice <= probability)
            {
                rushFlag = true;
                pastPos = transform.position;
                //�v���C���[�̈ʒu��dangerArea�G�t�F�N�g�𐶐�
                GameObject area = Instantiate(dangerRushArea);
                area.transform.position = new Vector3((transform.position + transform.forward * (rushDist / 2)).x, 0.1f,
(transform.position + transform.forward * (rushDist / 2)).z);
                area.transform.rotation = Quaternion.Euler(area.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, area.transform.rotation.eulerAngles.z);
                area.transform.localScale = new Vector3(rush.GetComponent<BoxCollider>().size.y, rushDist, area.transform.localScale.z);
                oniStatus.animator.SetTrigger("RushEntry");
                yield return new WaitUntil(() => rushFinish);
                oniStatus.animator.SetFloat("rBodyMoveSpeed", 0);
                rush.SetActive(false);
                Destroy(area);
                rushFlag = false;
                rushFinish = false;
            }
            else
            {
                jumpFlag = true;
                oniStatus.animator.SetTrigger("JumpIdel");
                yield return new WaitForSeconds(jumpIdel);
                //vector3�̃v���C���[�Ƃ̋���
                Vector3 vecDist = oniStatus.player.transform.position - transform.position;
                //vecDist��x��z�ɕ�������
                //0�Ŋ����infinity�ɂȂ�
                velocity.x = vecDist.x / t;
                velocity.z = vecDist.z / t;
                //t�b���ŖړI�̏ꏊ��
                velocity.y = (0.5f * gravity * t);
                //�v���C���[�̈ʒu��dangerArea�G�t�F�N�g�𐶐�
                GameObject area = Instantiate(dangerJumpArea);
                area.transform.position = new Vector3(oniStatus.player.transform.position.x, 0.1f, oniStatus.player.transform.position.z);
                area.transform.localScale = new Vector3(shockWave.GetComponent<SphereCollider>().radius * 2, area.transform.localScale.y,
                    shockWave.GetComponent<SphereCollider>().radius * 2);
                Destroy(area, t);
                yield return new WaitForSeconds(t);
                jumpFlag = false;
                audioSource.PlayOneShot(jumpSound);
                shaker.GenerateImpulse();
                oniStatus.animator.ResetTrigger("Jump");
                shockWave.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                shockWave.SetActive(false);
            }
            rBody.isKinematic = true;
            jumpCollider.enabled = false;
            rBody.useGravity = true;
            skillTime = 0f;
            yield return new WaitForSeconds(skillCoolDown);
            oniStatus.agent.enabled = true;
            yield return new WaitUntil(() =>
                 oniStatus.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f//�Đ����������1�ȏ�ɂȂ�
            );
            oniStatus.oniState = OniStatus.State.Normal;
        }
    }

    private void Jump()
    {
        if (skillTime > jumpIdel)
        {
            dangerParticle.Stop();
            rBody.useGravity = false;
            isGrounded = false;
            oniStatus.animator.SetBool("IsGrounded", isGrounded);
            velocity.y -= (gravity * Time.deltaTime);
            rBody.velocity = velocity;
            oniStatus.animator.SetTrigger("Jump");
            if (skillTime >= jumpIdel + (t / 2.0f))
            {
                jumpCollider.enabled = true;
                BoxCast();
            }
        }
        else
        {
            transform.LookAt(new Vector3(oniStatus.player.transform.position.x, transform.position.y, oniStatus.player.transform.position.z));
        }
        oniStatus.animator.SetFloat("JumpPower", rBody.velocity.y);
        skillTime += Time.deltaTime;
    }

    //�n�ʂ̃`�F�b�N
    private void OnTriggerStay(Collider other)
    {
        if (isGrounded) { return; }
        if (oniStatus.animator.GetFloat("JumpPower") <= 0.1f && other.tag == "Ground")
        {
            isGrounded = true;
        }
        else { isGrounded = false; }
        oniStatus.animator.SetBool("IsGrounded", isGrounded);
    }

    void BoxCast()
    {
        if (Physics.BoxCast(transform.position, new Vector3(3.2f, 1.0f, 3.2f) * 0.5f, -transform.up, out hit, Quaternion.identity, 2))
        {
            if (hit.transform.tag == "Player")
            {
                float dist = Vector3.Distance(hit.transform.position, fieldCenter);//position
                if (dist <= 1.0f)
                {
                    Vector3 fieldCenterOutsideDir = (hit.transform.position - fieldCenter).normalized;//�v���C���[����t�B�[���h�̒��S�ւ̕���
                    fieldCenterOutsideDir.y = 0;
                    hit.transform.position += fieldCenterOutsideDir;
                }
                else
                {
                    Vector3 fieldCenterDir = (hit.transform.position - fieldCenter).normalized;//�v���C���[����t�B�[���h�̒��S�ւ̕���
                    fieldCenterDir.y = 0;
                    hit.transform.position -= fieldCenterDir;
                }
            }
        }
    }

    public void StopTheAction()
    {
        StopCoroutine(_skillIdelCoroutine);
        rushFlag = false;
        jumpFlag = false;
        shockWave.SetActive(false);
        rush.SetActive(false);
        dangerParticle.Stop();
    }

    /*void OnDrawGizmos()
    {
        //�@Cube�̃��C���^���I�Ɏ��o��
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position - new Vector3(0,0.5f,0), new Vector3(1.6f, 0.5f, 1.6f));//���̒l��2�{�������ɂ���
    }*/
}
