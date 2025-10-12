using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // 弾の移動速度
    public float moveSpeed;

    // トレイルレンダラー（弾が移動する際の軌跡を表示）
    private TrailRenderer _trailRenderer;

    // 弾のリジッドボディ（物理演算）
    Rigidbody rb;

    // 弾の最大移動距離
    public float maxDistance = 5f;

    // 弾が発射された位置
    private Vector3 startPosition;

    void Start()
    {
        // 弾の発射位置を保存
        startPosition = transform.position;

        // トレイルレンダラーを取得
        _trailRenderer = GetComponent<TrailRenderer>();

        // 弾の移動を開始する（前方にmoveSpeedの速度で進む）
        GetComponent<Rigidbody>().velocity = transform.forward * moveSpeed;
    }

    void Update()
    {
        // 弾が発射されてから進んだ距離を計算
        float distanceTravelled = Vector3.Distance(transform.position, startPosition);

        // トレイルを表示する
        _trailRenderer.emitting = true;

        // 最大移動距離を超えたら弾を削除
        if (distanceTravelled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
