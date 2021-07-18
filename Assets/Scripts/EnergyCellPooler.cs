using UnityEngine;

public class EnergyCellPooler : Pooler<EnergyCell>
{
    public class EnergyReturn : ReturnToPool<EnergyCell> {}

    protected override void OnReturnToPool(EnergyCell item)
    {
        item.body.velocity = Vector2.zero;
        base.OnReturnToPool(item);
    }

    protected override EnergyCell CreatePooledItem()
    {
        EnergyCell cell = base.CreatePooledItem();
        EnergyReturn cellReturn = cell.gameObject.AddComponent<EnergyReturn>();
        cellReturn.pool = pool;
        return cell;
    }
}