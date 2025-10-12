using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.PostProcessing.PostProcessResources;

public class CharacterController : MonoBehaviour
{
    private CharacterController _character; // キャラクターコントローラー
    private Animator _animator; // アニメーター
    private AudioSource _audioSource; // オーディオソース
    private float lastSuccessfulAttackTime = -10f; // 最後の成功した攻撃時間
    private Coroutine enhancedDamageCoroutine; // 強化ダメージのコルーチン
    public static CharacterController instance; // インスタンス

    public static CharacterController GetInstance()
    {
        return instance;
    }
    //-----------DAMAGE--------------
    private float normalDamage = 6f; // 通常ダメージ
    private float enhancedDamage = 30f; // 強化ダメージ
    public bool isEnhancedDamage = false; // 強化ダメージフラグ
    private float enhancedDamageDuration = 5f; // 強化ダメージの持続時間
    //---------------HP--------------
    public float characterHealth = 100f; // 最大体力
    public float currentHealth = 100f; // 現在の体力
    private Slider HPslider; // 体力バー
    public Slider delaySlider; // 遅延体力バー
    public float delaySpeed = 1f; // 遅延速度
    private float targetHealth; // 目標体力
    public Image hpFillImage; // 体力バーのイメージ
    private float damage = 6f; // ダメージ量
    private float currentDamage; // 現在のダメージ
    //--------------UI---------------
    public Image gameOverImage; // ゲームオーバー画面
    public Button restartButton; // リスタートボタン
    public Button returnButton; // 戻るボタン
    //------------SKILLCHARGE--------
    private bool isCharging = false; // チャージ中フラグ
    private float chargeTime = 0f; // チャージ時間
    private float maxChargeTime = 3f; // 最大チャージ時間
    public float[] dodgeDistances = { 5f, 8f, 10f }; // 回避距離
    public float[] invincibilityDurations = { 0.7f, 1.5f, 2f }; // 無敵時間
    private bool isInvincible = false; // 無敵状態フラグ
    //------------SKILLDASH----------
    private bool isDashing; // ダッシュ中フラグ
    public float dashingPower; // ダッシュ力
    public float dashingTime; // ダッシュ時間
    public ParticleSystem chargeParticles; // チャージパーティクル
    //----------COOLDOWN-------------
    private bool canUseAbility = true; // アビリティ使用可能フラグ
    public float abilityCooldown = 3f; // アビリティクールダウン
    //----------SKILLROLL------------
    private bool isRolling; // ローリング中フラグ
    public float rollingPower; // ローリング力
    public float rollingTime; // ローリング時間
    public float brakingPower; // ブレーキ力
    //-------------------------------
    private float bulletCooldown = 0.5f; // 弾のクールダウン
    private float lastBulletTime = 0f; // 最後の弾発射時間
    //-------------------------------
    public ParticleSystem dodgeEffect; // 回避エフェクト
    public ParticleSystem damageParticles; // ダメージエフェクト
    //-------------------------------
    public ParticleSystem attackParticles; // 攻撃エフェクト
    private bool canAttack = true; // 攻撃可能フラグ
    private bool isAttacking; // 攻撃中フラグ
    public float attackTime; // 攻撃時間
    public float attackCooldown; // 攻撃クールダウン
    private bool isBullet;
    //-------------------------------
    public float moveSpeed; // 移動速度
    [SerializeField] private Rigidbody _rb; // リジッドボディ
    [SerializeField] private TrailRenderer _trailRenderer; // トレイルレンダラー
    //private CinemachineImpulseSource shaker;
    public GameObject bulletPrefab; // 弾のプレハブ
    public Transform shotPoint; // 発射地点
    //-------------------------------
    public AudioClip damageSound; // ダメージ音
    public AudioClip dodgeSound; // 回避音
    //-------------------------------
    HitDamage hitDamage; // ヒットダメージ
    void Awake()
    {
        // シングルトンパターンの初期化
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        _character = GetComponent<CharacterController>(); // キャラクターコントローラーを取得
        _animator = GetComponent<Animator>(); // アニメーターを取得
        _audioSource = GetComponent<AudioSource>(); // オーディオソースを取得
        _rb = GetComponent<Rigidbody>(); // リジッドボディを取得
        attackParticles.Stop(); // 攻撃エフェクトを停止
        hitDamage = new HitDamage(); // ヒットダメージの初期化
        HPslider = GameObject.Find("PlayerHPSlider").GetComponent<Slider>(); // HPスライダー取得
        delaySlider = GameObject.Find("PlayerDelayHPSlider").GetComponent<Slider>(); // 遅延スライダー取得
        HPslider.maxValue = characterHealth; // 最大HP設定
        delaySlider.maxValue = characterHealth;

        HPslider.value = currentHealth; // 現在HP設定
        delaySlider.value = currentHealth;

        targetHealth = currentHealth; // ターゲットHP設定
        hpFillImage = HPslider.fillRect.GetComponent<Image>(); // HPバーの色設定

        gameOverImage.gameObject.SetActive(false); // ゲームオーバー画面非表示
        restartButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);

        currentDamage = damage; // ダメージ初期化
    }

    // 毎フレーム呼び出される
    void Update()
    {
        if (isRolling || isAttacking || isDashing || isBullet) return; // アクション中は動作を中断

        // キャラクター移動処理
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * vertical + camRight * horizontal;
        moveDir.Normalize();

        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDir); // キャラクターの向きを移動方向に設定
            _animator.SetBool("isRun", true); // 走りアニメーションを設定
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World); // 実際に移動
        }
        else
        {
            _animator.SetBool("isRun", false); // アイドル状態
        }

        if (Input.GetButton("Bullet") && !isBullet) // 弾を撃つ
        {
            StartBullet(); // 弾発射処理
            
        }

        if (Input.GetButton("Attack") && canAttack && !isBullet) // 弾が発射中でないことを確認
        {
            StartAttack(); // 攻撃処理
            
        }

        if (Input.GetButton("Skill_Dash") && canUseAbility)
        {
            if (!isCharging)
            {
                StartCoroutine(ChargeOrRoll()); // ダッシュまたはローリング開始
            }
        }

        // HPバーの更新
        UpdateHPBarColor();
        HPslider.value = targetHealth;
        if (delaySlider.value > HPslider.value)
        {
            delaySlider.value -= delaySpeed * Time.deltaTime * characterHealth; // 遅延スライダーの更新
        }
    }

    public void StartBullet()
    {
        if (!isBullet) // 弾が発射中でない場合のみ発射
        {
            _animator.SetBool("isBullet", true);
            StartCoroutine(Bullet()); // 弾を発射するコルーチンを開始
        }
    }

    public void StartAttack()
    {
        if (!isAttacking && !isBullet) // 弾発射中ではない場合にのみ攻撃処理を開始
        {
            _animator.SetBool("isAttack", true);
            StartCoroutine(Attack()); // 攻撃を発動
        }
    }

    public IEnumerator Bullet()
    {
        isBullet = true;
        // 弾のクールダウンが終わっていない場合は何もしない
        if (Time.time - lastBulletTime < bulletCooldown)
        {
            yield break;
        }

        // 最後に弾を発射した時間を更新
        lastBulletTime = Time.time;
        // 弾を発射
        Instantiate(bulletPrefab, shotPoint.position, this.transform.rotation);
        // アニメーションの長さを取得
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        // 弾のアニメーションが終了するまで待機
        yield return new WaitForSeconds(animationLength);
        // 弾のアニメーションを停止
        _animator.SetBool("isBullet", false);
        isBullet = false;
    }

    private IEnumerator ChargeOrRoll()
    {
        // 能力を使用できない場合は処理を中断
        if (!canUseAbility) yield break;
        canUseAbility = false;
        isCharging = true;
        chargeTime = 0f;

        // チャージアニメーションを開始
        _animator.SetBool("isCharge", true);
        chargeParticles.Play();

        // ダッシュボタンを押し続けている間、チャージ時間を加算
        while (Input.GetButton("Skill_Dash"))
        {
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
            _animator.SetFloat("ChargeTime", chargeTime / maxChargeTime);

            if (Input.GetButtonUp("Skill_Dash"))
            {
                break;
            }

            yield return null;
        }
        isCharging = false;
        chargeParticles.Stop();

        // チャージ時間が短ければロール、長ければダッシュ
        if (chargeTime < 2f)
        {
            _animator.SetBool("isRoll", true);
            yield return Roll();
        }
        else
        {
            _animator.SetBool("isDash", true);
            yield return Dash();
        }

        _animator.SetBool("isCharge", false);
        _animator.SetBool("isRoll", false);
        _animator.SetBool("isDash", false);

        // 次の能力を使うまで待機
        yield return new WaitForSeconds(abilityCooldown);
        canUseAbility = true;
    }
    private IEnumerator Roll()
    {
        isRolling = true;
        _animator.SetBool("isRoll", true);

        // 重力を無効化して、前方に移動
        _rb.useGravity = false;
        _rb.velocity = transform.forward * rollingPower;

        // ロールのエフェクトを表示
        _trailRenderer.emitting = true;

        // ロールアニメーションの長さを取得
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float rollAnimationDuration = stateInfo.length * 0.4f;

        // アニメーションが終了するまで待機
        yield return new WaitForSeconds(rollAnimationDuration);

        // ロール終了後、移動を停止し重力を再び有効化
        _rb.velocity = Vector3.zero;
        _rb.useGravity = true;
        _trailRenderer.emitting = false;
        _animator.SetBool("isRoll", false);
        isRolling = false;
    }
    private IEnumerator Dash()
    {
        isDashing = true;

        // チャージレベルに基づいて回避距離と無敵時間を決定
        int chargeLevel = Mathf.FloorToInt(chargeTime);
        chargeLevel = Mathf.Min(chargeLevel, dodgeDistances.Length - 1);

        float dodgeDistance = dodgeDistances[chargeLevel];
        float invincibilityTime = invincibilityDurations[chargeLevel];

        // ダッシュ中に無敵状態を開始
        StartCoroutine(ActivateInvincibility(invincibilityTime));

        // ダッシュアニメーションを開始
        _animator.SetBool("isDash", true);
        _rb.useGravity = false;
        _rb.velocity = transform.forward * dodgeDistance;
        _trailRenderer.emitting = true;

        // ダッシュ時間が経過するまで待機
        yield return new WaitForSeconds(dodgeDistance / (dashingPower * 3f));

        // ダッシュ終了後、移動を停止し重力を再び有効化
        _rb.velocity = Vector3.zero;
        _rb.useGravity = true;
        _trailRenderer.emitting = false;
        _animator.SetBool("isDash", false);
        isDashing = false;
    }
    private IEnumerator ActivateInvincibility(float duration)
    {
        // 無敵時間が経過するまで待機
        yield return new WaitForSeconds(duration);
    }

    public void TriggerAttackFromAnimation()
    {
        // 攻撃可能なら攻撃を開始
        if (canAttack)
        {
            StartCoroutine(Attack());
        }
    }
    private IEnumerator Attack()
    {
        // 攻撃を開始できないように設定
        canAttack = false;
        isAttacking = true;
        // 攻撃アニメーションを開始
        

        // 攻撃のパーティクルエフェクトを再生
        if (attackParticles != null && !attackParticles.isPlaying)
        {
            attackParticles.Play();
        }
        // 攻撃時間だけ待機
        yield return new WaitForSeconds(attackTime);

        // 攻撃終了後、アニメーションと状態をリセット
        isAttacking = false;
        _animator.SetBool("isAttack", false);

        // 攻撃のパーティクルエフェクトを停止
        if (attackParticles.isPlaying)
        {
            attackParticles.Stop();
        }

        // 攻撃クールダウンの間、再び攻撃可能にするまで待機
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    public void OnDodgeSuccess()
    {
        // 回避成功時に強化ダメージを有効化
        StartCoroutine(ActivateEnhancedDamage());
    }
    private IEnumerator ActivateEnhancedDamage()
    {
        // 強化ダメージを有効化
        isEnhancedDamage = true;

        // 強化ダメージの持続時間だけ待機
        yield return new WaitForSeconds(enhancedDamageDuration);

        // 強化ダメージを無効化
        isEnhancedDamage = false;
        enhancedDamageCoroutine = null;
    }
    public void DamageToEnemy(Collider enemyWeaponCollider)
    {
        // 攻撃した敵のコライダーから敵オブジェクトを取得
        GameObject enemy = enemyWeaponCollider.gameObject;

        // 敵のステータスを取得
        OniStatus enemyHealth = enemy.GetComponentInParent<OniStatus>();
        EnemyController enemyYuki=enemy.GetComponentInParent<EnemyController>();

        // 敵が存在し、ステータスが有効ならダメージを与える
        if (enemyHealth != null||enemyYuki != null)
        {
            // 強化ダメージ状態かどうかで与えるダメージを決定
            float damage = isEnhancedDamage ? enhancedDamage : normalDamage;
            enemyHealth.TakeDamage(damage);
            enemyYuki.TakeDamage(damage);
        }
    }
    public void NotifyDodgeSuccess()
    {
        // 強化ダメージのコルーチンが実行中なら停止
        if (enhancedDamageCoroutine != null)
        {
            StopCoroutine(enhancedDamageCoroutine);
        }

        // 強化ダメージを再度有効化
        enhancedDamageCoroutine = StartCoroutine(ActivateEnhancedDamage());
        lastSuccessfulAttackTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 無敵状態ならダメージを受けない
        if (isInvincible) return;

        // ローリングやダッシュ中の場合
        if (isRolling || isDashing)
        {
            // 特定のタグの敵武器に当たった場合
            if (other.CompareTag("EnemyWeapon") || other.CompareTag("EnemyWepon1") || other.CompareTag("EnemyWepon2") || other.CompareTag("EnemyWepon3") || other.CompareTag("OniWepon") || other.CompareTag("OniRush") || other.CompareTag("OniJump"))
            {
                // 回避成功時の処理
                NotifyDodgeSuccess();

                // 回避エフェクト再生
                if (dodgeEffect != null && !dodgeEffect.isPlaying)
                {
                    dodgeEffect.Play();
                    _audioSource.PlayOneShot(dodgeSound);
                }

                // 敵にダメージを与える
                DealDamageToEnemy(other);
                return;
            }
        }
        // 攻撃を受けた時、強化ダメージを無効化
        lastSuccessfulAttackTime = -10f;
        isEnhancedDamage = false;

        // 強化ダメージのコルーチンを停止
        if (enhancedDamageCoroutine != null)
        {
            StopCoroutine(enhancedDamageCoroutine);
            enhancedDamageCoroutine = null;
        }

        // 各武器のダメージ処理
        if (other.tag == "EnemyWeapon")
        {
            // 雪女武器からのダメージ
            targetHealth -= 20;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                FadeManager.Instance.LoadScene("Stage_2_GameOver", 2f);
                Destroy(gameObject);
            }
        }

        if (other.tag == "OniWepon")
        {
            // 鬼の武器からのダメージ
            targetHealth -= 20;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                Destroy(gameObject);
                SceneManager.LoadScene("Stage_1_GameOver");
            }
        }

        if (other.tag == "EnemyWepon1")
        {
            // 敵武器1からのダメージ
            targetHealth -= 30;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                FadeManager.Instance.LoadScene("Stage_2_GameOver", 2f);
                Destroy(gameObject);
            }
        }

        if (other.tag == "EnemyWepon2")
        {
            // 敵武器2からのダメージ
            targetHealth -= 15;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                FadeManager.Instance.LoadScene("Stage_2_GameOver", 2f);
                Destroy(gameObject);
            }
        }

        if (other.tag == "EnemyWepon3")
        {
            // 敵武器3からのダメージ
            targetHealth -= 10;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                FadeManager.Instance.LoadScene("Stage_2_GameOver", 2f);
                Destroy(gameObject);
            }
        }

        if (other.tag == "OniRush")
        {
            // 鬼の突進からのダメージ
            targetHealth -= 40;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                Destroy(gameObject);
                SceneManager.LoadScene("Stage_1_GameOver");
            }
        }

        if (other.tag == "OniJump")
        {
            // 鬼のジャンプ攻撃からのダメージ
            targetHealth -= 50;
            targetHealth = Mathf.Clamp(targetHealth, 0, characterHealth);
            currentHealth = targetHealth;
            damageParticles.Play();
            _audioSource.PlayOneShot(damageSound);
            HPslider.value = targetHealth;
            if (targetHealth <= 0)
            {
                // ゲームオーバー処理
                Destroy(gameObject);
                SceneManager.LoadScene("Stage_1_GameOver");
            }
        }
    }

    public void DealDamageToEnemy(Collider enemyWeaponCollider)
    {
        // 敵の武器のコライダーを取得
        GameObject enemy = enemyWeaponCollider.gameObject;

        // OniStatusとEnemyControllerを親オブジェクトから取得
        OniStatus enemyHealth = enemy.GetComponentInParent<OniStatus>();
        EnemyController yukiHealth = enemy.GetComponentInParent<EnemyController>();

        // 反射ダメージの設定（ここでの値は例です）
        float reflectDamage = 20f; // 実際のダメージ値に置き換えてください

        // OniStatusが存在すればダメージを与える
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(reflectDamage);
            StartCoroutine(SlowTimeCoroutine()); // 時間を遅くするコルーチンを開始
        }

        // EnemyControllerが存在すればダメージを与える
        if (yukiHealth != null)
        {
            yukiHealth.TakeDamage(reflectDamage);
            StartCoroutine(SlowTimeCoroutine()); // 時間を遅くするコルーチンを開始
        }
    }

    private IEnumerator SlowTimeCoroutine()
    {
        // 無敵状態を有効化
        isInvincible = true;

        // ゲームの時間を遅くする（50%スピード）
        Time.timeScale = 0.5f;

        // 0.5秒間待機
        yield return new WaitForSeconds(0.5f);

        // 時間を元に戻す
        Time.timeScale = 1f;
        isInvincible = false; // 無敵状態を解除
    }

    private void UpdateHPBarColor()
    {
        // 現在のHPの割合を計算
        float hpPercent = targetHealth / characterHealth;

        // HPに応じた色を設定
        Color customGreen = new Color(69f / 255f, 174f / 255f, 71f / 255f); // 緑
        Color customYellow = new Color(217f / 255f, 174f / 255f, 71f / 255f); // 黄色
        Color customRed = new Color(217f / 255f, 71f / 255f, 71f / 255f); // 赤

        // HPが70%以上の場合、緑色に設定
        if (hpPercent > 0.7f)
        {
            hpFillImage.color = customGreen;
        }
        // HPが30%〜70%の場合、黄色に設定
        else if (hpPercent > 0.3f)
        {
            hpFillImage.color = customYellow;
        }
        // HPが30%以下の場合、赤色に設定
        else
        {
            hpFillImage.color = customRed;
        }
    }

    public void AddDamage(float dm)
    {
        // ダメージを受け取る
        targetHealth -= dm;
        UpdateHPBarColor(); // ダメージ後、HPバーの色を更新
    }
}
