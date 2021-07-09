using UnityEngine;

public class LaserPooler : Pooler<LaserProjectile>
{
    public class LaserReturn : ReturnToPool<LaserProjectile> {}
    protected override void OnReturnToPool(LaserProjectile item)
    {
        item.body.velocity = Vector2.zero;
        base.OnReturnToPool(item);
    }
    protected override LaserProjectile CreatePooledItem()
    {
        LaserProjectile proj = base.CreatePooledItem();
        LaserReturn laserReturn = proj.gameObject.AddComponent<LaserReturn>();
        laserReturn.pool = pool;
        return proj;
    }
}