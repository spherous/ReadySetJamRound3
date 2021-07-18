using UnityEngine;

public class PowerupPooler : Pooler<Powerup>
{
    public class PowerupReturn : ReturnToPool<Powerup>{}
    protected override void OnReturnToPool(Powerup item)
    {
        item.body.velocity = Vector2.zero;
        base.OnReturnToPool(item);
    }
    protected override Powerup CreatePooledItem()
    {
        Powerup powerup = base.CreatePooledItem();
        PowerupReturn powerupReturn = powerup.gameObject.AddComponent<PowerupReturn>();
        powerupReturn.pool = pool;
        return powerup;
    }
}