using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rocket : MonoBehaviour, IPoolable, IProjectile
{
    [SerializeField] private AudioSource fireSource;
    public bool inPool {get => _inPool; set => _inPool = value;}
    private float dieAtTime;
    public float lifetime {get => _lifetime; set{_lifetime = value;}}
    [SerializeField] private float _lifetime;
    public float speed {get => _speed; set{_speed = value;}}
    [SerializeField] private float _speed;
    public Rigidbody2D body {get => _body; set{}}
    [SerializeField] private Rigidbody2D _body;

    private bool _inPool = false;
    public event OnReturnToPool onReturnToPool;

    private Transform owner;
    float damage;

    public float radius;

    ParticleSystemPooler explosionPooler;
    bool isJuiced;

    private void Update()
    {
        if(Time.timeSinceLevelLoad >= dieAtTime)
            onReturnToPool?.Invoke();
    }

    public void SetExplosionPooler(ParticleSystemPooler pooler)
    {
        explosionPooler = pooler;
    }

    public void Fire(Transform owner, float damage, bool isPlayer = false, bool isJuiced = false)
    {
        this.owner = owner;
        this.damage = damage;
        this.isJuiced = isJuiced;
        dieAtTime = Time.timeSinceLevelLoad + lifetime;
        body.velocity = transform.up * speed;
        fireSource.Play();
    }

    public void Collide()
    {
        PooledParticleSystem explosion = explosionPooler.pool.Get();
        explosion.transform.position = transform.position;
        Vector3 scale = Vector3.one * (isJuiced ? radius * 1.25f : radius * 0.75f);
        explosion.transform.localScale = scale;
        for(int i = 0; i < explosion.transform.childCount; i++)
        {
            Transform child = explosion.transform.GetChild(i);
            child.localScale = scale;
        }
        explosion.Play();
        onReturnToPool?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other == null || other.transform == owner)
            return;
        
        if(other.TryGetComponent<IHealth>(out IHealth otherHealth))
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius * (isJuiced ? 1.5f : 1f));
            foreach(Collider2D hitCollider in hitColliders)
            {
                if(hitCollider.TryGetComponent<IHealth>(out IHealth hitHealth))
                    hitHealth.TakeDamage(damage, owner);
            }
            Collide();
        }
    }
}
