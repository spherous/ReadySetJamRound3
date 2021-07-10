using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour, IProjectile, IPoolable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trail;
    public Color playerColor;
    public Color enemyColor;
    private float dieAtTime;
    public float lifetime {get => _lifetime; set{_lifetime = value;}}
    [SerializeField] private float _lifetime;

    public float speed {get => _speed; set{_speed = value;}}
    [SerializeField] private float _speed;

    public Rigidbody2D body {get => _body; set{}}

    public bool inPool {get => _inPool; set{_inPool = value;}}
    bool _inPool = false;

    [SerializeField] private Rigidbody2D _body;

    public event OnReturnToPool onReturnToPool;
    private Transform owner;

    private void Update() {
        if(Time.timeSinceLevelLoad >= dieAtTime)
        {
            Collide();
        }
    }

    public void Collide()
    {
        trail.Clear();
        onReturnToPool?.Invoke();
    }

    public void Fire(Transform owner, bool isPlayer = false)
    {
        trail.Clear();
        this.owner = owner;
        dieAtTime = Time.timeSinceLevelLoad + lifetime;
        body.velocity = transform.up * speed;
        Color color = isPlayer ? playerColor : enemyColor;
        spriteRenderer.color = color;
        trail.startColor = color;
        trail.endColor = color;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other == null || other.transform == owner)
            return;

        if(other.TryGetComponent<IHealth>(out IHealth otherHealth))
        {
            otherHealth.TakeDamage(1, owner);
            Collide();
        }
    }
}