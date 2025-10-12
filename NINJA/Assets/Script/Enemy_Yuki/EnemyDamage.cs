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


public class EnemyDamage : MonoBehaviour
{
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
    private void Start()
    {
        HPslider = GameObject.Find("EnemyHPSlider").GetComponent<Slider>();
        delaySlider = GameObject.Find("EnemyDelayHPSlider").GetComponent<Slider>();
        audioSource = GetComponent<AudioSource>();

        HPslider.maxValue = health;
        delaySlider.maxValue = health;

        HPslider.value = currentHealth;
        delaySlider.value = currentHealth;

        targetHealth = currentHealth;
        hpFillImage = HPslider.fillRect.GetComponent<Image>();

        shaker = GetComponentInParent<CinemachineImpulseSource>();
        stageClearImage.gameObject.SetActive(false);
        //nextButton.gameObject.SetActive(false);
        titleButton.gameObject.SetActive(false);
    }
    private void Update()
    {   
        UpdateHPBarColor();
        HPslider.value = targetHealth;
        if (delaySlider.value > HPslider.value)
        {
            delaySlider.value -= delaySpeed * Time.deltaTime*health;
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
        Destroy(Yuki);
        SceneManager.LoadScene("Stage_2_Clear");
        stageClearImage.gameObject.SetActive(true);
        titleButton.gameObject.SetActive(true);
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
        if (other.tag == "Bullet")
        {
            float damage = 3f;
            TakeDamage(damage);
            Destroy(other.gameObject);
        }
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
    
