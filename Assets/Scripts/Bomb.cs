using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour, IProjectile, IPoolable
{
    public float lifetime {get => _lifetime; set{_lifetime = value;}}
    [SerializeField] private float _lifetime;
    float dieAtTime;
    public float speed {get => _speed; set => _speed = value;}
    [SerializeField] private float _speed;
    public Rigidbody2D body {get => _body; set{}}
    [SerializeField] private Rigidbody2D _body;
    public bool inPool {get => _inPool; set => _inPool = value;}
    bool _inPool = false;
    public event OnReturnToPool onReturnToPool;
    public Transform owner {get; private set;}

    public float explosionRadius;
    public float damage;
    private ParticleSystemPooler fusionExplosionPool;
    private void Update() {
        if(Time.timeSinceLevelLoad >= dieAtTime)
            Collide();
    }

    public void SetExplosionPool(ParticleSystemPooler fusionExplosionPool) => this.fusionExplosionPool = fusionExplosionPool;

    public void Collide()
    {
        PooledParticleSystem fusionExplosion = fusionExplosionPool.pool.Get();
        fusionExplosion.transform.position = transform.position;
        fusionExplosion.Play();

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach(Collider2D target in targets)
        {
            if(target.gameObject == gameObject || target.transform == owner)
                continue;

            if(target.TryGetComponent<IHealth>(out IHealth health))
            {
                if(health is Rock rock)
                    rock.surpressSmallerRocks = true;
                    
                health.TakeDamage(damage, owner);
            }
        }

        onReturnToPool?.Invoke();
    }

    public void Fire(Transform owner, float damage, bool isPlayer = false, bool isJuiced = false)
    {
        this.owner = owner;
        dieAtTime = Time.timeSinceLevelLoad + lifetime;
        body.velocity = transform.up * speed;
        body.angularVelocity = UnityEngine.Random.Range(-90f, 90f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other == null || other.transform == owner)
            return;
        
        if(other.TryGetComponent<IHealth>(out IHealth otherHealth))
        {
            Collide();
        }
    }
}
