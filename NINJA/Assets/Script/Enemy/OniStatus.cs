using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class OniStatus : MonoBehaviour
{
    public enum State
    {
        Normal,
        Attack,
        Die,
        skill,
        LeanBack,
    }

    //---------HP---------------
    public float health = 100f;
    public Slider HPslider;
    public Slider delaySlider;
    public float currentHealth = 100f;
    public float delaySpeed = 1f;
    private float targetHealth;
    public Image hpFillImage;

    private float lastHitTime = -1f;
    private float hitCooldown = 0.5f;
    //---------DAMAGE---------------
    private float lastSuccessfulDodgeTime = -10f;
    private float enhancedDamageDuration = 5f;
    private float enhancedDamageMultiplier = 5f;
    private Coroutine enhancedDamageCoroutine;
    private float remainingInvincibleTime = 0.0f;
    public bool isEnhancedDamage = false;
    public float normalDamage = 6f;
    //--------------------------
    //private変数＆関数
    private CinemachineImpulseSource shaker;
    [SerializeField] private ParticleSystem damageParticle;
    private AudioSource audioSource;
    //----------------UI---------------------
    public Image stageClearImage;
    public Button nextButton;
    public Button titleButton;
    public GameObject fadePanel;
    //---------------------------------------
    public float enemyStopD = 5.0f;
    public float invincibleTime = 0.4f;//無敵時間
    public State oniState = State.Normal;
    public Animator animator;
    public NavMeshAgent agent { get; set; }
    public GameObject player;
    public AudioClip damageSound;
    public bool navgationEnabled = false;
    public float dist { get; set; }
    [Header("鬼を倒した後の余韻の時間")] public float AfterDeathTime = 3.0f;//鬼が倒された後、どれぐらいでシーン遷移するか
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        agent.stoppingDistance = enemyStopD;
        dist = Vector3.Distance(player.transform.position, transform.position);
        HPslider = GameObject.Find("EnemyHPSlider").GetComponent<Slider>();
        delaySlider = GameObject.Find("EnemyDelayHPSlider").GetComponent<Slider>();

        HPslider.maxValue = health;
        delaySlider.maxValue = health;

        // スライダーの現在値の設定
        HPslider.value = currentHealth;
        delaySlider.value = currentHealth;

        targetHealth = currentHealth;
        hpFillImage = HPslider.fillRect.GetComponent<Image>();
        shaker = FindObjectOfType<CinemachineImpulseSource>();

        stageClearImage?.gameObject.SetActive(false);
        nextButton?.gameObject.SetActive(false);
        titleButton?.gameObject.SetActive(false);
        fadePanel.GetComponent<FadeInOut>().FadeInStart(0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        if (navgationEnabled)
        {
            if (agent.enabled == true)
            {
                agent.SetDestination(player.transform.position);
            }
        }
        else { return; }
        dist = Vector3.Distance(player.transform.position, transform.position);
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);//共通
        //UpdateHPBarColor();
        HPslider.value = targetHealth;
        if (delaySlider.value > HPslider.value)
        {
            delaySlider.value -= delaySpeed * Time.deltaTime * health;
        }
        remainingInvincibleTime -= Time.deltaTime;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
        {
            Debug.Log("leanback");
        }
    }

    /*private void UpdateHPBarColor()
    {
        float hpPercent = targetHealth / health;
        Color customGreen = new Color(69f / 255f, 174f / 255f, 71f / 255f);
        Color customYellow = new Color(217f / 255f, 174f / 255f, 71f / 255f);
        Color customRed = new Color(217f / 255f, 71f / 255f, 71f / 255f);
        if (hpPercent > 0.7f)
        {
            hpFillImage.color = customGreen;
        }
        else if (hpPercent > 0.3f)
        {
            hpFillImage.color = customYellow;
        }
        else
        {
            hpFillImage.color = customRed;
        }
    }*/
    public void NotifyDodgeSuccess()
    {
        if (enhancedDamageCoroutine != null)
        {
            StopCoroutine(enhancedDamageCoroutine);
        }
        enhancedDamageCoroutine = StartCoroutine(ActivateEnhancedDamage());
        lastSuccessfulDodgeTime = Time.time;
    }

    public IEnumerator ActivateEnhancedDamage()
    {
        isEnhancedDamage = true;
        float startTime = Time.time;

        while (Time.time - startTime < 5f)
        {
            if (!isEnhancedDamage) yield break;
            yield return null;
            if (Time.time - lastSuccessfulDodgeTime < 5f)
            {
                startTime = Time.time;
                lastSuccessfulDodgeTime = Time.time;
            }
        }

        yield return new WaitForSeconds(enhancedDamageDuration - 5f);
        isEnhancedDamage = false;
        enhancedDamageCoroutine = null;
    }
    public void TakeDamage(float damage)
    {

        targetHealth -= damage;
        targetHealth = Mathf.Clamp(targetHealth, 0, health);
        currentHealth = targetHealth;
        damageParticle.Play();
        HPslider.value = targetHealth;
        if (targetHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator ResetEnhancedDamage()
    {
        yield return new WaitForSeconds(enhancedDamageDuration);
        isEnhancedDamage = false;

    }
    private void Die()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<OniSkillState>().StopTheAction();
        damageParticle.Stop();
        //animation再生
        animator.SetTrigger("DieTrigger");
        oniState = State.Die;
        //animation再生後画面遷移。animationEventで制御
        //アニメーションイベントでStartAfterDeathを呼ぶ
    }

    //プレイヤーとの処理
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Weapon")
        {
            if (remainingInvincibleTime <= 0.0f)
            {
                float damage = normalDamage;
                CharacterController playerController = other.GetComponentInParent<CharacterController>();
                if (playerController != null && playerController.isEnhancedDamage)
                {
                    damage *= 5f;
                    if (oniState != State.skill && oniState != State.Die)
                    {
                        LeanBack();
                    }
                    else if (animator.GetCurrentAnimatorStateInfo(0).IsName("rushed") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("Landing") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("LandEntry"))
                    {
                        LeanBack();
                        Debug.Log("leanback");
                    }
                }

                shaker.GenerateImpulse();
                targetHealth -= damage;
                targetHealth = Mathf.Clamp(targetHealth, 0, health);
                damageParticle.Play();
                audioSource.PlayOneShot(damageSound);
                HPslider.value = targetHealth;

                if (targetHealth <= 0)
                {
                    if (oniState != State.Die)
                    {
                        Die();
                    }
                }
                remainingInvincibleTime = invincibleTime;
            }
        }

    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            if (remainingInvincibleTime <= 0.0f)
            {
                float damage = 3f;
                targetHealth -= damage;
                damageParticle.Play();
                audioSource.PlayOneShot(damageSound);
                HPslider.value = targetHealth;
                targetHealth = Mathf.Clamp(targetHealth, 0, health);
                if (targetHealth <= 0)
                {
                    if (oniState != State.Die)
                    {
                        Die();
                    }
                }
                Destroy(other.gameObject);
                remainingInvincibleTime = invincibleTime;
            }
        }
    }

    public void ToNormal()
    {
        oniState = State.Normal;
    }

    private void LeanBack()
    {
        //のけぞる処理。アニメーションが終わったら、ノーマルステートへ
        animator.SetTrigger("LeanBackTrigger");
        oniState = State.LeanBack;
        GetComponent<OniAttackState>().attack = false;
        GetComponent<SwordController>().AttackNotEnabled();
        //アニメーションイベントでToNormalを呼び出して、ノーマルステートに変える
    }

    public void StartAfterDeath()
    {
        StartCoroutine(AfterDeath());
    }

    private IEnumerator AfterDeath()
    {
        fadePanel.GetComponent<FadeInOut>().FadeOutStart(0.05f);
        yield return new WaitForSeconds(AfterDeathTime);
        SceneManager.LoadScene("Stage_1_Clear");
        yield break;
    }

}
