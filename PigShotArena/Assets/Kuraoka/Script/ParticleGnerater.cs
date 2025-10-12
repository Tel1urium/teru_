using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleGnerater : MonoBehaviour
{
    [SerializeField] private Transform target; // プレイヤーを追う対象
    [SerializeField] private float attractionRadius = 5f;
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float smoothness = 0.2f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (target == null) return; // プレイヤーがまだ生成されていない場合はスキップ

        int max = ps.main.maxParticles;
        if (max == 0) return;

        if (particles == null || particles.Length < max)
            particles = new ParticleSystem.Particle[max];

        int count = ps.GetParticles(particles);
        Vector3 center = target.position + Vector3.up * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 toCenter = center - particles[i].position;
            float dist = toCenter.magnitude;

            if (dist < attractionRadius)
            {
                Vector3 dir = toCenter.normalized;
                float t = Mathf.Clamp01(1f - dist / attractionRadius);
                float speed = Mathf.Lerp(0.5f, maxSpeed, t);
                particles[i].velocity = Vector3.Lerp(particles[i].velocity, dir * speed, smoothness);
            }
        }

        ps.SetParticles(particles, count);
    }

    // プレイヤーを外部からセットする関数
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}