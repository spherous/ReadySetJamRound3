using UnityEngine;

public class Powerup : MonoBehaviour, IPoolable
{
    public Rigidbody2D body;
    public PowerupType type;

    public bool inPool {get => _inPool; set => _inPool = value;}
    private bool _inPool = false;
    public event OnReturnToPool onReturnToPool;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other == null)
            return;

        if(other.TryGetComponent<Player>(out Player player))
        {
            player.CollectPowerup(type);
            onReturnToPool?.Invoke();
        }
        else if(other.TryGetComponent<Rock>(out Rock rock))
        {
            if(rock.size >= 2)
            {
                rock.Break();
                onReturnToPool?.Invoke();
            }
        }
    }
}