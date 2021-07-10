using UnityEngine;

public class EnemyPooler : Pooler<Enemy>
{
    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private RectTransform healthBarContainer;
    public class EnemyReturn : ReturnToPool<Enemy>{}

    protected override void OnReturnToPool(Enemy item)
    {
        item.healthBar.gameObject.SetActive(false);
        base.OnReturnToPool(item);
    }
    protected override void OnTakeFromPool(Enemy item)
    {
        item.healthBar.gameObject.SetActive(true);
        item.isDead = false;
        item.isDying = false;
        item.HealToFull();
        base.OnTakeFromPool(item);
    }
    protected override Enemy CreatePooledItem()
    {
        Enemy enemy = base.CreatePooledItem();

        HealthBar bar = Instantiate(healthBarPrefab, healthBarContainer);
        bar.SetTarget(enemy.gameObject, enemy);

        enemy.healthBar = bar;

        EnemyReturn enemyReturn = enemy.gameObject.AddComponent<EnemyReturn>();
        enemyReturn.pool = pool;
        return enemy;
    }
}