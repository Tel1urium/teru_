using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Collider weaponCollider;  // 武器のコライダー
    public float attackDuration;     // 攻撃の持続時間
    private bool isAttacking = false; // 現在攻撃中かどうか
    public float attackCooldown;     // 攻撃クールダウン時間
    private bool canAttack = true;   // 攻撃可能かどうか

    private void Start()
    {
        weaponCollider.enabled = false; // 初期状態で武器のコライダーは無効にする
    }

    void Update()
    {
        // 攻撃ボタンが押されていて、攻撃可能で、かつ現在攻撃していない場合に攻撃を開始
        if (Input.GetButton("Attack") && canAttack && !isAttacking)
        {
            StartCoroutine((SwingWeapon())); // 攻撃処理をコルーチンで開始

        }
    }
    void StartSwingWeapon()
    {
        StartCoroutine(SwingWeapon());
    }

    private IEnumerator SwingWeapon()
    {
        canAttack = false;  // 攻撃中は他の攻撃をできないようにする
        isAttacking = true; // 攻撃中フラグを立てる

        yield return new WaitForSeconds(0.3f); // 攻撃開始を0.3秒遅らせる

        weaponCollider.enabled = true; // コライダーを有効化（攻撃開始）

        yield return new WaitForSeconds(attackDuration);  // 攻撃の持続時間を待機（ここは変えない）

        weaponCollider.enabled = false; // 攻撃終了後にコライダーを無効にする

        yield return new WaitForSeconds(attackCooldown);  // 攻撃後のクールダウン時間を待機

        isAttacking = false;  // 攻撃終了フラグを解除
        canAttack = true;     // 攻撃可能フラグを立てる
    }

}
