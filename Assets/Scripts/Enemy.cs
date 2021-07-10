using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour, IHealth, IPoolable
{
    [SerializeField] private Collider2D col;
    public float maxHealth {get => _maxHP; set => _maxHP = value;}
    [SerializeField] private float _maxHP;
    public float currentHealth {get => _hp; set => _hp = value;}
    float _hp;
    public bool isDead {get => _isDead; set => _isDead = value;}
    bool _isDead = false;
    public bool isDying {get => _isDying; set => _isDying = value;}
    bool _isDying = false;
    public bool inPool {get => _inPool; set => _inPool = value;}
    bool _inPool = false;

    public event OnHealthChanged onHealthChanged;
    public event OnReturnToPool onReturnToPool;
    public HealthBar healthBar;
    public float speed;
    Player player;
    LaserPooler laserPooler;

    public float fireDelay;
    float fireAtTime;
    public float fireDistance;

    public float stoppingDistance;

    public ContactFilter2D colliderCastFilter;
    Score score;

    private void Awake() 
    {
        player = GameObject.FindObjectOfType<Player>();
        laserPooler = GameObject.FindObjectOfType<LaserPooler>();
        score = GameObject.FindObjectOfType<Score>();
    }
    private void Start() => currentHealth = maxHealth;

    private void Update() {
        if((player.transform.position - transform.position).magnitude <= fireDistance && Time.timeSinceLevelLoad >= fireAtTime)
            Fire();
    }

    private void Fire()
    {
        fireAtTime = Time.timeSinceLevelLoad + fireDelay;

        if(player.isDying || player.isDead)
            return;

        List<RaycastHit2D> results = new List<RaycastHit2D>();
        Vector3 playerPositionInLocalSpace = player.transform.position - transform.position;
        int amountHit = col.Cast(playerPositionInLocalSpace.normalized, colliderCastFilter, results, playerPositionInLocalSpace.magnitude);

        if(amountHit > 0)
        {
            RaycastHit2D firstHit = results[0];
            // Debug.Log(firstHit.collider.gameObject.name);
            if(firstHit.collider.TryGetComponent<Enemy>(out Enemy otherEnemy))
            {
                // If an enemy is the first thing between us and the player, don't fire.
                return;
            }
        }

        LaserProjectile laser = laserPooler.pool.Get();
        laser.transform.SetPositionAndRotation(transform.position, transform.rotation);
        laser.Fire(transform);
    }

    private void FixedUpdate()
    {
        if(isDead || isDying)    
            return;
        
        Move();
    }

    private void Move()
    {
        Vector3 playerPositionInLocalSpace = player.transform.position - transform.position;
        Vector3 direction = playerPositionInLocalSpace.normalized;

        // Stop short of player
        if(playerPositionInLocalSpace.magnitude <= stoppingDistance)
        {
            transform.rotation = Quaternion.LookRotation(transform.forward, direction);
            return;
        }

        transform.position = transform.position + (direction * speed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.LookRotation(transform.forward, direction);
    }

    public void Die()
    {
        _isDead = true;
        onReturnToPool?.Invoke();
    }

    public void TakeDamage(float amount, Transform damagedBy)
    {
        if(isDying || isDead)
            return;

        float oldHP = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(amount), 0, maxHealth);

        if(oldHP != currentHealth)
            onHealthChanged?.Invoke(currentHealth);

        if(currentHealth == 0)
        {
            // Regardless if the player killed an enemy or a rock, the player still gets score
            // But an enemy killing an enemy does not count
            if(!damagedBy.gameObject.TryGetComponent<Enemy>(out Enemy otherEnemy))
                score.Add(2);
            
            Die();
        }
    }
    public void HealToFull()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
    }
}
