using UnityEngine;

/// <summary>
/// ボム
/// </summary>
public class PlayerBomb : MonoBehaviour
{
    public float radius = 1f;
    public Vector3 velocity = default;

    private void Update()
    {
        this.velocity.y -= 9.81f * 0.5f * Time.deltaTime * Time.deltaTime * 100f;

        var estimate = this.transform.position + this.velocity;

        var point = Vector2.zero;
        var distance = 0f;
        JumpFloodingManager.Get(estimate / 1024f, out point, out distance);
        distance *= 1024f;

        if (distance < radius)
        {
            // 爆発（マップを削り取る）
            const float ScrapeRadius = 25f;
            MapManager.Scrape(point, ScrapeRadius / 1024f);

            // 雑なエフェクト
            var position = this.transform.localPosition;
            var positions = new Vector2[100];
            var velocities = new Vector2[100];
            for (int i = 0; i < 100; ++i)
            {
                positions[i] = position;
                positions[i].x += Random.Range(-ScrapeRadius, ScrapeRadius);
                positions[i].y += Random.Range(-ScrapeRadius, ScrapeRadius);
                float euler = Random.Range(0f, Mathf.PI * 2f);
                velocities[i] = new Vector2(Mathf.Sin(euler), Mathf.Cos(euler));
                velocities[i] *= 2.0f;
            }
            CircleParticleManager.Emit(positions, velocities);

            Destroy(this.gameObject);
        }
        else
        {
            this.transform.localPosition += this.velocity;
        }
    }
}
