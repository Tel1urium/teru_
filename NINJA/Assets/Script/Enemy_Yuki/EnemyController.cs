using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.Rendering;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Unity.VisualScripting;
using static UnityEngine.Rendering.PostProcessing.PostProcessResources;
using Cinemachine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Chase, //í«ê’
        Leave,//ì¶ëñ
        Idle, //ë“ã@
        Atk1,//çUåÇÇP
        Atk2,//çUåÇ2
        Atk3,//çUåÇ3
        Atk4,//çUåÇ4
        Die
    };
    public PlayableDirector[] timeLine;
    public GameObject Atk1;
    public GameObject Atk2;
    public GameObject Atk3;
    public GameObject Atk4;
    public EnemyState state;
    private Transform target;
    private Vector3 player;
    private NavMeshAgent navMeshAgent;
    private Vector3 vec;
    public static bool attack = false;
    public int atk_cnt = 1;
    public static float now = 0;
    int mAtk3 = 15;
    int mAtk1 = 8;
    public GameObject sensor;
    public Collider collider;
    //---------HP---------------
    public float health = 250f;
    public Slider HPslider;
    public Slider delaySlider;
    public float currentHealth;
    public float delaySpeed = 1f;
    private float targetHealth;
    public Image hpFillImage;

    private float lastHitTime = -1f;
    private float hitCooldown = 0.5f;
    private bool isTakingDamage = false;
    ////---------DAMAGE---------------
    private float lastSuccessfulDodgeTime = -10f;
    public float normalDamage = 6f;
    private float enhancedDamageMultiplier = 5f;
    private float enhancedDamageDuration = 5f;
    private Coroutine enhancedDamageCoroutine;
    public bool isEnhancedDamage = false;
    //--------------------------
    private CinemachineImpulseSource shaker;
    [SerializeField] private ParticleSystem damageParticle;
    private AudioSource audioSource;
    public GameObject Yuki;
    public Image stageClearImage;
    //public Button nextButton;
    public Button titleButton;
    public AudioClip damageSound;
    //----------------------------
    int cnt = 0;
    public int chage = 0;
    InField inField;
    //-------------------------------------------
    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetState(EnemyState.Idle);
        //--------------SOUND------------------
        audioSource = GetComponent<AudioSource>();
        //--------------HPUI-------------------
        HPslider = GameObject.Find("EnemyHPSlider").GetComponent<Slider>();
        delaySlider = GameObject.Find("EnemyDelayHPSlider").GetComponent<Slider>();
        
        HPslider.maxValue = health;
        delaySlider.maxValue = health;

        HPslider.value = currentHealth;
        delaySlider.value = currentHealth;

        targetHealth = currentHealth;
        hpFillImage = HPslider.fillRect.GetComponent<Image>();
        //----------UI-------------------------
        shaker = GetComponentInParent<CinemachineImpulseSource>();
        stageClearImage.gameObject.SetActive(false);
        //nextButton.gameObject.SetActive(false);
        titleButton.gameObject.SetActive(false);
        //-------------------------------------
        inField = new InField();
    }
    void Update()
    {
        //ìGÇÃå¸Ç´ÇÉvÉåÉCÉÑÅ[ÇÃï˚å¸Ç…è≠ÇµÇ∏Ç¬ïœçX
        {
            var dir = (GetLookPlayer() - transform.position).normalized;
            dir.y = 0;
            Quaternion setRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, navMeshAgent.angularSpeed * 0.1f * Time.deltaTime);

        }
        if (targetHealth <= 0)
        {
           SetState(EnemyState.Die);
        }

        if (state == EnemyState.Chase)//í«Ç§èÛë‘
        {
            if (target == null)
            {
                SetState(EnemyState.Idle);
            }
            else
            {
                SetVec(target.position);
                navMeshAgent.speed = 1f;
                navMeshAgent.SetDestination(GetVec());
            }
        }
        if (state == EnemyState.Leave)//ì¶Ç∞ÇÈèÛë‘
        {
            if (target == null)
            {
                SetState(EnemyState.Idle);
            }
            else
            {
                var dir=(transform.position-target.position).normalized;
                dir.y = 0;

                SetVec(transform.position + dir * 3.0f);
                if(dir.x >= 3f || dir.z >= 3f)
                {
                    navMeshAgent.speed = 0f;
                }
                else
                {
                    navMeshAgent.speed = 0.8f;
                }
                navMeshAgent.SetDestination(GetVec());
            }
            
        }
        if (state == EnemyState.Idle)
        {
            ChangeState();
        }

        now++;
        if (now > 80*4)
        {
            now = 0;
        }
        Debug.Log(state);
        //--------------------HP--------------------
        //UpdateHPBarColor();
        HPslider.value = targetHealth;
        if (delaySlider.value > HPslider.value)
        {
            delaySlider.value -= delaySpeed * Time.deltaTime * health;
        }
    }
    public void Attack_Stop(EnemyState state)//âΩâÒçUåÇÇ∑ÇÈÇ©ÇÃï™äÚ
    {
        attack = false;

        if (GetState() == EnemyState.Idle)
        {
            SetState(EnemyState.Leave);
        }
        else
        {
            float percent = 0.0f;
            if (atk_cnt == 1) percent = 30f;
            else if (atk_cnt == 2) percent = 70f;
            else if (atk_cnt == 3) percent = 100f;
            if(Probability(percent))
            {
                SetState(EnemyState.Idle);
                atk_cnt = 1;
            }
            else
            {
                SetState(state);
                atk_cnt++;
            }
        }
    }

    public static bool Probability(float fPersent)//ämóßîªíËópÉÅÉ\ÉbÉh
    {
        float fProbabilityRate = UnityEngine.Random.value * 100;
        if (fPersent == 100f && fProbabilityRate == fPersent)
        {
            return true;
        }
        else if(fPersent > fProbabilityRate)
        {
            return true ;
        }
        else
        {
            return false ;
        }
    }

    public void SetState(EnemyState tmpState)
    {
        state= tmpState;

        if (tmpState == EnemyState.Idle)
        {
            navMeshAgent.isStopped = true;
            timeLine[4].Play();
            cnt = 0;
        }
        else if(tmpState == EnemyState.Chase)
        {
            navMeshAgent.isStopped = false;
            timeLine[4].Play();
        }
        else if (tmpState == EnemyState.Leave)
        {
            navMeshAgent.isStopped = false;
            timeLine[4].Play();
           
        }
        else if (tmpState == EnemyState.Atk1)
        {
            navMeshAgent.isStopped = true;
            //attack = true;
            timeLine[0].Play();
            now = 0;
        }
        else if (tmpState == EnemyState.Atk2)
        {
            navMeshAgent.isStopped = true;
            //attack = true;
            timeLine[1].Play();
            now = 0;
        }
        else if (tmpState == EnemyState.Atk3)
        {
            navMeshAgent.isStopped = true;
            //attack = true;
            timeLine[2].Play();
            now = 0;
        }
        else if (tmpState == EnemyState.Atk4)
        {
            navMeshAgent.isStopped = true;
            //attack = true;
            timeLine[3].Play();
            now = 0;
        }
        else if (tmpState == EnemyState.Die)
        {
            navMeshAgent.isStopped = true;
            Destroy(sensor);
            timeLine[5].Play();
        }
        

    }
    public EnemyState GetState()
    {
        return state;
    }
    public void SetTarget(Transform tagetObj)
    {
        target = tagetObj;
    }
    public void SetLookPlayer(Vector3 playerObj)
    {
        player=playerObj;
    }
    public Vector3 GetLookPlayer()
    {
        return player;
    }
    public void SetVec(Vector3 pos)
    {
        vec = pos;
    }
    public Vector3 GetVec()
    {
        return vec;
    }

    public void Atk1_Motion()
    {
        if(!attack)
        {
            attack = true;
            for (int i = 0; i < mAtk1; i++)
            {
                GameObject go = Instantiate(Atk1);
                var rb = go.GetComponent<Rigidbody>();
                go.transform.rotation = Quaternion.Euler(0, i * 45, 0);
                go.transform.position = this.transform.position;
                go.transform.position += go.transform.forward * 1.5f;
                //rb.AddForce(go.transform.forward * 1000);
            }
        }
        Attack_Stop(EnemyState.Atk1);
    }

    public void Atk2_Motion()
    {
        if(!attack)
        {
            attack = true;
            for (int i = 0; i < 3; i++)
            {
                GameObject go = Instantiate(Atk2);
                go.transform.position = this.transform.position + 
                    new Vector3(2*Mathf.Cos((45 + i * 45) * Mathf.Deg2Rad), 2*Mathf.Sin((30 + i * 60) * Mathf.Deg2Rad), 0) + transform.up * 2;
            }
        }
        Attack_Stop(EnemyState.Atk2);
    }

    public void Atk3_Motion()
    {
        if ((!attack))
        {
            attack = true;
            for (int i = 0; i < mAtk3; i++)
            {
                GameObject go = Instantiate(Atk3);
                float rndX = Random.Range(target.position.x - 10.0f, target.position.x + 11.0f);//--------------
                float rndZ = Random.Range(target.position.z - 10.0f, target.position.z + 11.0f);//--------------
                go.transform.position = new Vector3(rndX, 15, rndZ);
            }
        }
        Attack_Stop(EnemyState.Atk3);
    }
    public void Atk4_Motion()
    {
        if (!attack)
        {
            attack = true;
            GameObject go = Instantiate(Atk4);
            go.transform.position = this.transform.position;
        }
        Attack_Stop(EnemyState.Atk4);
    }

    public void ChangeState()
    {
        if (inField.GetInField())
        {
            if (cnt > chage)
            {
                int rand = Random.Range(0, 3);
                if (!attack)
                {
                    switch (rand)
                    {
                        case 0: SetState(EnemyState.Atk1); break;
                        case 1: SetState(EnemyState.Atk2); break;
                        case 2: SetState(EnemyState.Atk3); break;
                    }
                }
                cnt = 0;
            }
            cnt++;
        }
        
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
    private IEnumerator ResetEnhancedDamage()
    {
        yield return new WaitForSeconds(enhancedDamageDuration);
        isEnhancedDamage = false;

    }
    private void Die()
    {
        //Destroy(Yuki);
        //SceneManager.LoadScene("Stage_2_Clear");
        Destroy(collider);
        stageClearImage.gameObject.SetActive(true);
        titleButton.gameObject.SetActive(true);
        Invoke("ChangeScene", 1f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Weapon")
        {
            float damage = normalDamage;
            CharacterController playerController = other.GetComponentInParent<CharacterController>();
            if (playerController != null && playerController.isEnhancedDamage)
            {
                damage *= 5f;
                timeLine[6].Play();
            }

            shaker.GenerateImpulse();
            targetHealth -= damage;
            targetHealth = Mathf.Clamp(targetHealth, 0, health);
            damageParticle.Play();
            audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;

            if (targetHealth <= 0)
            {
                Die();
            }
        }
       
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            float damage = 3f;
            targetHealth -= damage;
            damageParticle.Play();
            audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            targetHealth = Mathf.Clamp(targetHealth, 0, health);
            if (targetHealth <= 0)
            {
                Die();
            }
            Destroy(other.gameObject);
        }
    }
    public void ChangeScene()
    {
        FadeManager.Instance.LoadScene("Stage_2_Clear", 2f);
    }
    private void UpdateHPBarColor()
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
    }
}
