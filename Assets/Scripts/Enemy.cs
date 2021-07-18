using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour, IHealth, IPoolable
{
    [SerializeField] private Collider2D col;
    [SerializeField] private AudioSource laserSource;
    [SerializeField] private AudioSource damagedSource;
    public List<AudioClip> damagedClips = new List<AudioClip>();
    private ParticleSystemPooler sparksPooler;
    private ParticleSystemPooler explosionPooler;
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

    public float accuracy;

    public float spawnTime;

    public List<Transform> firePoints = new List<Transform>();
    int lastFirePointIndex;
    public float rotationSpeed;
    EnemySpawner spawner;
    RockSpawner rockSpawner;
    PowerupSpawner powerupSpawner;
    EnergyCellPooler energyPooler;
    FuelPooler fuelPooler;
    public float damage;
    public float chanceToDropPowerup;

    private void Awake() 
    {
        player = GameObject.FindObjectOfType<Player>();
        laserPooler = GameObject.FindObjectOfType<LaserPooler>();
        spawner = GameObject.FindObjectOfType<EnemySpawner>();
        rockSpawner = GameObject.FindObjectOfType<RockSpawner>();
        powerupSpawner = GameObject.FindObjectOfType<PowerupSpawner>();
        energyPooler = GameObject.FindObjectOfType<EnergyCellPooler>();
        fuelPooler = GameObject.FindObjectOfType<FuelPooler>();
        // sparksPooler = GameObject.FindObjectOfType<ParticleSystemPooler>();
        score = GameObject.FindObjectOfType<Score>();
    }
    private void Start() => currentHealth = maxHealth;

    private void Update() {
        Vector3 playerPositionInLocalSpace = player.transform.position - transform.position;
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        int amountHit = col.Cast(playerPositionInLocalSpace, colliderCastFilter, results, playerPositionInLocalSpace.magnitude);
        RaycastHit2D nearestHit = results.FirstOrDefault();
        
        if(nearestHit.collider != null && nearestHit.collider.TryGetComponent<Rock>(out Rock rock) && Time.timeSinceLevelLoad >= fireAtTime)
            Fire(nearestHit.point - (Vector2)transform.position);
        else if(playerPositionInLocalSpace.magnitude <= fireDistance && Time.timeSinceLevelLoad >= fireAtTime)
            Fire(playerPositionInLocalSpace);

    }

    public void SetSparksPooler(ParticleSystemPooler sparksPooler) => this.sparksPooler = sparksPooler;
    public void SetExplosionPooler(ParticleSystemPooler explosionPooler) => this.explosionPooler = explosionPooler;

    private void Fire(Vector3 direction)
    {
        fireAtTime = Time.timeSinceLevelLoad + fireDelay;

        if(player.isDying || player.isDead)
            return;


        List<RaycastHit2D> results = new List<RaycastHit2D>();
        // Vector3 playerPositionInLocalSpace = player.transform.position - transform.position;
        
        // Enemy is not roughly facing player, hold fire
        if(Vector3.Dot(transform.up, direction) <= 0.75f)
            return;
        
        int amountHit = col.Cast(direction.normalized, colliderCastFilter, results, direction.magnitude);

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
        Vector3 eulerRot = transform.rotation.eulerAngles + new Vector3(0, 0, UnityEngine.Random.Range(-15f * (1f - accuracy), 15f * (1f - accuracy)));

        int firePointIndex = lastFirePointIndex + 1 > firePoints.Count - 1 ? 0 : lastFirePointIndex + 1;
        Transform firePoint = firePoints[firePointIndex];
        lastFirePointIndex = firePointIndex;
        laser.transform.SetPositionAndRotation(firePoint.position, Quaternion.Euler(eulerRot));
        laserSource.Play();
        laser.Fire(transform, damage);
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
            // transform.rotation = Quaternion.LookRotation(transform.forward, direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.forward, direction), rotationSpeed * Time.deltaTime);
            return;
        }

        transform.position = transform.position + (direction * speed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.forward, direction), rotationSpeed * Time.deltaTime);
    }

    public void Die()
    {
        PooledParticleSystem explosion = explosionPooler.pool.Get();
        explosion.transform.position = transform.position;
        explosion.transform.localScale = transform.localScale;
        explosion.Play();

        // Enemies drop fuel on death
        float aliveTime = Time.timeSinceLevelLoad - spawnTime;
        float refuelAmount = Mathf.Clamp(500 - aliveTime, 0, 500);
        
        if(refuelAmount != 0)
        {
            FuelCanister fuel = fuelPooler.pool.Get();
            fuel.transform.SetPositionAndRotation(
                position: transform.position + new Vector3(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 0),
                rotation: Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180f, 180f))
            );
            fuel.refuelAmount = refuelAmount;
            fuel.body.velocity = ((Vector2)transform.up + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized * UnityEngine.Random.Range(0f, 1f);
            fuel.body.angularVelocity = UnityEngine.Random.Range(-90f, 90f);
        }

        // Enemies drop energy cells on death
        EnergyCell cell = energyPooler.pool.Get();
        cell.transform.SetPositionAndRotation(
            position: transform.position + new Vector3(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 0),
            rotation: Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180f, 180f))
        );
        cell.chargeAmount = 15;
        cell.body.velocity = ((Vector2)transform.up + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized * UnityEngine.Random.Range(0f, 1f);
        cell.body.angularVelocity = UnityEngine.Random.Range(-90f, 90f);
        
        // Enemies have a chance to drop a powerup on death
        float chanceRoll = UnityEngine.Random.Range(0f, 1f);
        if(chanceRoll <= chanceToDropPowerup)
        {
            Powerup powerup = powerupSpawner.GetRandomPowerup();
            powerup.transform.SetPositionAndRotation(
                position: transform.position + new Vector3(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.2f, 0.8f), 0),
                rotation: Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180f, 180f))
            );
            powerup.body.velocity = ((Vector2)transform.up + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized * UnityEngine.Random.Range(0f, 1f);
            powerup.body.angularVelocity = UnityEngine.Random.Range(-90f, 90f);
        }

        spawner?.IncDifficulty();
        rockSpawner?.IncDifficulty();
        _isDead = true;
        onReturnToPool?.Invoke();
    }

    public void TakeDamage(float amount, Transform damagedBy)
    {
        if(isDying || isDead)
            return;

        PooledParticleSystem sparks = sparksPooler.pool.Get();
        sparks.transform.position = transform.position;
        sparks.Play();

        float oldHP = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(amount), 0, maxHealth);

        if(oldHP != currentHealth)
        {
            damagedSource.PlayOneShot(damagedClips[UnityEngine.Random.Range(0, damagedClips.Count)]);
            onHealthChanged?.Invoke(currentHealth);
        }

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
