using UnityEngine;
using static UnityEngine.Camera;

public class EnemyPooler : Pooler<Enemy>
{
    [SerializeField] private Camera cam;
    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private RectTransform healthBarContainer;
    [SerializeField] private ParticleSystemPooler sparksPooler;
    [SerializeField] private ParticleSystemPooler explosionPooler;
    public class EnemyReturn : ReturnToPool<Enemy>{}

    protected override void OnReturnToPool(Enemy item)
    {
        item.healthBar.gameObject.SetActive(false);
        item.gameObject.SetActive(false);
        item.transform.position = cam.ScreenToWorldPoint(new Vector3(-50, -50, 10), MonoOrStereoscopicEye.Mono);
        // base.OnReturnToPool(item);
    }
    protected override void OnTakeFromPool(Enemy item)
    {
        item.isDead = false;
        item.isDying = false;
        item.healthBar.gameObject.SetActive(true);
        item.HealToFull();
        base.OnTakeFromPool(item);
    }
    protected override Enemy CreatePooledItem()
    {
        Enemy enemy = base.CreatePooledItem();
        enemy.SetSparksPooler(sparksPooler);
        enemy.SetExplosionPooler(explosionPooler);
        HealthBar bar = Instantiate(healthBarPrefab, healthBarContainer);
        bar.SetTarget(enemy.gameObject, enemy);

        enemy.healthBar = bar;

        EnemyReturn enemyReturn = enemy.gameObject.AddComponent<EnemyReturn>();
        enemyReturn.pool = pool;
        return enemy;
    }
}