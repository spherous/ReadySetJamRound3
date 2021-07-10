using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Camera;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    private Camera cam;
    [SerializeField] private EnemyPooler pooler;
    public float enemySpawnTime;
    float spawnEnemyAtTime;

    private void Awake() => cam = Camera.main;

    private void Update() {
        if(Time.timeSinceLevelLoad >= spawnEnemyAtTime)
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        Enemy newEnemy = pooler.pool.Get();
        (float x, float y) loc = Extensions.GetRandomOffScreenLocation();
        newEnemy.transform.position = cam.ScreenToWorldPoint(new Vector3(loc.x, loc.y, 10), MonoOrStereoscopicEye.Mono);
        
        // Randomize FireDelay, Stopping Distance, Speed, HP, Size
        newEnemy.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.75f, 1.25f);
        newEnemy.fireDelay = enemyPrefab.fireDelay * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.fireDistance = enemyPrefab.fireDistance * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.speed = enemyPrefab.speed * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.maxHealth = enemyPrefab.maxHealth * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.HealToFull();

        spawnEnemyAtTime = Time.timeSinceLevelLoad + enemySpawnTime;
    }
}