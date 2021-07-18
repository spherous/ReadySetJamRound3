using UnityEngine;

public class FuelPooler : Pooler<FuelCanister>
{
    public class FuelReturn : ReturnToPool<FuelCanister> {}

    protected override void OnReturnToPool(FuelCanister item)
    {
        item.body.velocity = Vector2.zero;
        base.OnReturnToPool(item);
    }

    protected override FuelCanister CreatePooledItem()
    {
        FuelCanister fuel = base.CreatePooledItem();
        FuelReturn fuelReturn = fuel.gameObject.AddComponent<FuelReturn>();
        fuelReturn.pool = pool;
        return fuel;
    }
}