using UnityEngine;
using UnityEngine.Pool;

public class LaserPooler : MonoBehaviour
{
    [SerializeField] private LaserProjectile laserPrefab;

    public bool collectionChecks = true;
    public int maxPoolSize = 10;
    IObjectPool<LaserProjectile> _pool;

    public IObjectPool<LaserProjectile> pool {get{
        if(_pool == null)
            _pool = new ObjectPool<LaserProjectile>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
        return _pool;
    } set{}}

    LaserProjectile CreatePooledItem()
    {
        LaserProjectile newProj = Instantiate(laserPrefab);
        newProj.gameObject.SetActive(false);

        ReturnLaserToPool returnToPool = newProj.gameObject.AddComponent<ReturnLaserToPool>();
        returnToPool.pool = pool;

        return newProj;
    }

    void OnTakeFromPool(LaserProjectile proj) => proj.gameObject.SetActive(true);

    void OnReturnedToPool(LaserProjectile proj)
    {
        proj.transform.position = Vector3.zero;
        proj.body.velocity = Vector2.zero;
        proj.gameObject.SetActive(false);
    }

    void OnDestroyPoolObject(LaserProjectile proj) => Destroy(proj.gameObject);
}

public class ReturnLaserToPool : MonoBehaviour
{
    LaserProjectile proj;
    public IObjectPool<LaserProjectile> pool;
    private void Start() {
        proj = GetComponent<LaserProjectile>();
        proj.onCollision += OnCollision;
    }

    void OnCollision() => pool.Release(proj);
}