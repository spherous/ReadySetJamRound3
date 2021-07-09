using UnityEngine;

public class RockPooler : Pooler<Rock>
{
    public class RockReturn : ReturnToPool<Rock> {}
    protected override void OnReturnToPool(Rock item)
    {
        item.body.velocity = Vector2.zero;
        base.OnReturnToPool(item);
    }
    protected override Rock CreatePooledItem()
    {
        Rock rock = base.CreatePooledItem();
        RockReturn rockReturn = rock.gameObject.AddComponent<RockReturn>();
        rockReturn.pool = pool;
        return rock;
    }
}