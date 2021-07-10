using UnityEngine;
using static UnityEngine.Camera;

public class RockSpawner : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public RockPooler pooler;
    public float rockSpawnDelay;
    private float spawnAtTime;

    private void Update() {
        if(Time.timeSinceLevelLoad >= spawnAtTime)
            SpawnRock();
    }

    public void SpawnRock()
    {
        Rock rock = pooler.pool.Get();
        rock.Init(UnityEngine.Random.Range(2, 5), this);

        (float x, float y) loc = Extensions.GetRandomOffScreenLocation();
        rock.transform.position = cam.ScreenToWorldPoint(new Vector3(loc.x, loc.y, 10), MonoOrStereoscopicEye.Mono);

        // Choose a target somewhere on screen, send rock in direction of target
        (float x, float y) screenTarget = (UnityEngine.Random.Range(100f, Screen.width - 100), UnityEngine.Random.Range(100f, Screen.height - 100));
        Vector3 worldTarget = cam.ScreenToWorldPoint(new Vector3(screenTarget.x, screenTarget.y, 10));
        rock.body.velocity = (worldTarget - rock.transform.position).normalized * UnityEngine.Random.Range(0.8f, rock.maxSpeed);

        // Randomize rotation
        rock.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-180f, 180f));
        rock.body.angularVelocity = UnityEngine.Random.Range(-180f, 180f);

        spawnAtTime = Time.timeSinceLevelLoad + rockSpawnDelay;
    }
}
