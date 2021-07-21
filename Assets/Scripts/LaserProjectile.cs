using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour, IProjectile, IPoolable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trail;
    public Color playerColor;
    public Color juicedColor;
    public Color enemyColor;
    private float dieAtTime;
    public float lifetime {get => _lifetime; set{_lifetime = value;}}
    [SerializeField] private float _lifetime;

    public float speed {get => _speed; set{_speed = value;}}
    [SerializeField] private float _speed;

    public Rigidbody2D body {get => _body; set{}}
    [SerializeField] private Rigidbody2D _body;

    public bool inPool {get => _inPool; set{_inPool = value;}}
    bool _inPool = false;


    public event OnReturnToPool onReturnToPool;
    private Transform owner;
    float damage;

    private void Update()
    {
        if(Time.timeSinceLevelLoad >= dieAtTime)
            Collide();
    }

    public void Collide()
    {
        trail.Clear();
        onReturnToPool?.Invoke();
    }

    public void Fire(Transform owner, float damage, bool isPlayer = false, bool isJuiced = false)
    {
        trail.Clear();
        this.owner = owner;
        this.damage = damage;
        dieAtTime = Time.timeSinceLevelLoad + lifetime;
        body.velocity = transform.up * speed;
        Color color = isPlayer ? isJuiced ? juicedColor : playerColor : enemyColor;
        spriteRenderer.color = color;
        trail.startColor = color;
        trail.endColor = color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other == null || other.transform == owner)
            return;

        if(other.TryGetComponent<IHealth>(out IHealth otherHealth))
        {
            otherHealth.TakeDamage(damage, owner);
            Collide();
        }
    }
}