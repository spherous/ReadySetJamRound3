using UnityEngine;
using static UnityEngine.Camera;

public class EnergyCell : MonoBehaviour, IPoolable
{
    Camera cam;
    public bool inPool {get => _inPool; set => _inPool = value;}
    bool _inPool;
    public event OnReturnToPool onReturnToPool;
    public Rigidbody2D body {get => _body; set{}}
    [SerializeField] private Rigidbody2D _body;

    public float chargeAmount;

    private void Awake() => cam = Camera.main;
    private void FixedUpdate() => ScreenWrap();

    private void ScreenWrap()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position, MonoOrStereoscopicEye.Mono);
        transform.position = cam.ScreenToWorldPoint(screenPos.GetScreenWrapPosition(), MonoOrStereoscopicEye.Mono);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other == null)
            return;
        
        if(other.TryGetComponent<Player>(out Player player))
        {
            player.ChargeBattery(chargeAmount);
            onReturnToPool?.Invoke();
        }
        else if(other.TryGetComponent<PickupRadius>(out PickupRadius pickupRadius))
        {
            if(pickupRadius.player.SpendCharge(2))
            {
                pickupRadius.player.ChargeBattery(chargeAmount);
                onReturnToPool?.Invoke();
            }
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
