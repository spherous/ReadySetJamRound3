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

        //  Choose a random position off the edge of the screen
        int offEdge = UnityEngine.Random.Range(0, 2);
        int posOrNeg = UnityEngine.Random.Range(0, 2);
        float offset = UnityEngine.Random.Range(1, 10);

        (float x, float y) loc = offEdge switch{
            // off height
            0 => (UnityEngine.Random.Range(0f, Screen.width), posOrNeg == 0 ? Screen.height + offset : 0 - offset),
            // off width
            1 => (posOrNeg == 0 ? Screen.width + offset : 0 - offset, UnityEngine.Random.Range(0f, Screen.height)),
            _ => (0,0)
        };
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
