using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour, IHealth, IPoolable
{
    public Rigidbody2D body {get => _body; set{}}
    [SerializeField] private Rigidbody2D _body;
    public Collider2D col {get => _col; set{}}
    [SerializeField] private Collider2D _col;
    public float maxSpeed;
    public int size {get; private set;}
    public float maxHealth {get => 1; set{}}
    public float currentHealth {get => _hp; set{_hp = value;}}
    float _hp;
    public bool isDead {get => _isDead; set{}}
    bool _isDead;
    public bool isDying {get => _isDying; set{}}
    bool _isDying;
    public bool inPool {get => _inPool; set{_inPool = value;}}
    bool _inPool = false;

    public event OnReturnToPool onReturnToPool;
    private RockSpawner spawner;

    public float sizeMod = 1.5f;
    public float lifetime;
    float dieAtTime;

    private void Update() {
        if(Time.timeSinceLevelLoad >= dieAtTime)
            onReturnToPool?.Invoke();
    }

    public void Init(int size, RockSpawner spawner)
    {
        dieAtTime = Time.timeSinceLevelLoad + lifetime;

        _isDead = false;
        _isDying = false;
        this.spawner = spawner;
        this.size = size;
        float scale = size / sizeMod;
        transform.localScale = new Vector3(scale, scale, scale);
        _hp = maxHealth;
        
        // This resets the ignored collisions
        col.enabled = false;
        col.enabled = true;
    }

    public void Die()
    {
        if(size > 1)
        {
            int nextSize = size - 1;
            int amountOfNewRocks = UnityEngine.Random.Range(2, 5);
            List<Rock> newRocks = new List<Rock>();

            for(int i = 0; i < amountOfNewRocks; i++)
            {
                Rock newRock = spawner.pooler.pool.Get();
                newRock.Init(nextSize, spawner);
                newRocks.Add(newRock);

                Vector3 posOffset = new Vector3(
                    x: UnityEngine.Random.Range(-sizeMod / 2, sizeMod / 2), 
                    y: UnityEngine.Random.Range(-sizeMod / 2, sizeMod / 2), 
                    z: 10
                );
                Quaternion rot = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180f, 180f));

                int attempts = 0;
                while(Physics2D.OverlapCircleAll(transform.position + posOffset, size / sizeMod).Length > 0 && attempts < 20)
                {
                    // This will attempt to spread out the spawned rocks, but it may be impossible depending on where the first couple are spawned, so we limit the amount of attempts
                    posOffset = new Vector3(
                        x: UnityEngine.Random.Range(-sizeMod / 2, sizeMod / 2), 
                        y: UnityEngine.Random.Range(-sizeMod / 2, sizeMod / 2), 
                        z: 10
                    );
                    rot = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180f, 180f));
                    attempts++;
                }

                newRock.transform.SetPositionAndRotation(transform.position + posOffset, rot);
                newRock.body.angularVelocity = UnityEngine.Random.Range(-180f, 180f);
                newRock.body.velocity = body.velocity + ((Vector2)(newRock.transform.position - transform.position).normalized * UnityEngine.Random.Range(0.33f, maxSpeed));
            }

            List<(Rock, Rock)> pairs = new List<(Rock, Rock)>();

            // Ignore colliding with the other freshly spawned rocks
            foreach(Rock rock in newRocks)
            {
                foreach(Rock r in newRocks)
                {
                    if(r == rock)
                        continue;
                    
                    if(pairs.Contains((rock, r)) || pairs.Contains((r, rock)))
                        continue;
                    
                    pairs.Add((rock, r));
                    Physics2D.IgnoreCollision(rock.col, r.col);
                }
            }
        }
        onReturnToPool?.Invoke();
    }

    public void TakeDamage(float amount)
    {
        StartCoroutine(DieAtEndOfFrame());
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.collider == null)
            return;
        
        if(other.collider.TryGetComponent<IHealth>(out IHealth otherHealth))
        {
            otherHealth.TakeDamage(size);
            StartCoroutine(DieAtEndOfFrame());
        }
    }

    IEnumerator DieAtEndOfFrame()
    {
        if(isDying || _isDead)
            yield break;

        isDying = true;
        yield return new WaitForEndOfFrame();
        Die();
    }
}
