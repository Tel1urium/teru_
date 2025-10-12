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
    public float attackDist = 5.0f;//攻撃する距離
    public float t = 3.0f;//飛ぶ時間
    public float gravity = 30.0f;//重力
    public float rushSpeed = 15.0f;//突進スピード
    public float rushDist = 20.0f;//突進する距離
    public float skillCoolDown = 3.0f;//スキルのクールダウン
    public float rushIdel = 1.5f;//突進が始まるまでの溜め時間
    public float jumpIdel = 1.0f;//ジャンプが始まるまでの溜め時間
    public bool isGrounded = true;
    [Range(0, 10)] public float probability = 5.0f;//スキルの確率
    public AudioClip jumpSound;
    public AudioClip tackleSound;
    [Header("フィールドの中心の座標")] public Vector3 fieldCenter;//フィールドの中心

    [SerializeField, Tooltip("スキルが来ることを知らせるパーティクル")] private ParticleSystem dangerParticle;//スキルを使うことを知らせるパーティクル
    [SerializeField] private GameObject dangerJumpArea;//危険なエリアのエフェクト
    [SerializeField] private GameObject dangerRushArea;//危険なエリアのエフェクト
    [SerializeField] private CinemachineImpulseSource shaker;
    //[SerializeField, Header("フィールドの中心の座標")] private Transform fieldCenter;//フィールドの中心
    private AudioSource audioSource;
    private GameObject shockWave;//衝撃波
    private GameObject rush;//突進時のエフェクトとコライダー
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

    //突進スキル
    private void Rush()
    {
        //ランダムで何かのSkillが発動、自分じゃない場合、待機。
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

    //スキルの待ち時間と、スキルを選ぶコルーチン
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
                oniStatus.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f//再生完了すると1以上になる
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
                //プレイヤーの位置にdangerAreaエフェクトを生成
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
                //vector3のプレイヤーとの距離
                Vector3 vecDist = oniStatus.player.transform.position - transform.position;
                //vecDistをxとzに分解する
                //0で割るとinfinityになる
                velocity.x = vecDist.x / t;
                velocity.z = vecDist.z / t;
                //t秒飛んで目的の場所へ
                velocity.y = (0.5f * gravity * t);
                //プレイヤーの位置にdangerAreaエフェクトを生成
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
                 oniStatus.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f//再生完了すると1以上になる
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

    //地面のチェック
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
                    Vector3 fieldCenterOutsideDir = (hit.transform.position - fieldCenter).normalized;//プレイヤーからフィールドの中心への方向
                    fieldCenterOutsideDir.y = 0;
                    hit.transform.position += fieldCenterOutsideDir;
                }
                else
                {
                    Vector3 fieldCenterDir = (hit.transform.position - fieldCenter).normalized;//プレイヤーからフィールドの中心への方向
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
        //　Cubeのレイを疑似的に視覚化
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position - new Vector3(0,0.5f,0), new Vector3(1.6f, 0.5f, 1.6f));//この値に2倍した数にする
    }*/
}
