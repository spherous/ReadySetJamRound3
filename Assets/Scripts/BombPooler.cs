using UnityEngine;

public class BombPooler : Pooler<Bomb>
{
    public class BombReturn : ReturnToPool<Bomb> {}
    [SerializeField] private Player player;
    [SerializeField] private ParticleSystemPooler fusionExplosionPool;
    protected override void OnReturnToPool(Bomb item)
    {
        player.activeBomb = null;
        item.body.velocity = Vector2.zero;
        base.OnReturnToPool(item);
    }

    protected override Bomb CreatePooledItem()
    {
        Bomb bomb = base.CreatePooledItem();
        bomb.SetExplosionPool(fusionExplosionPool);
        BombReturn bombReturn = bomb.gameObject.AddComponent<BombReturn>();
        bombReturn.pool = pool;
        return bomb;
    }
}