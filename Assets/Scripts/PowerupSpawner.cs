using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    public List<PowerupPooler> powerupPoolers = new List<PowerupPooler>();
    public Powerup GetRandomPowerup() => 
        powerupPoolers[UnityEngine.Random.Range(0, powerupPoolers.Count)].pool.Get();
}
