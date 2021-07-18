using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Camera;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Enemy enemyPrefab;
    private Camera cam;
    [SerializeField] private EnemyPooler pooler;
    public float enemySpawnTime;
    float spawnEnemyAtTime;
    float damage = 1;

    private void Awake() => cam = Camera.main;

    private void Update() {
        if(Time.timeSinceLevelLoad >= spawnEnemyAtTime && !player.isDead && !player.isDying)
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        Enemy newEnemy = pooler.pool.Get();
        (float x, float y) loc = Extensions.GetRandomOffScreenLocation();
        newEnemy.transform.position = cam.ScreenToWorldPoint(new Vector3(loc.x, loc.y, 10), MonoOrStereoscopicEye.Mono);

        newEnemy.transform.localScale = Vector3.one * UnityEngine.Random.Range(1f, 1.333f);
        newEnemy.fireDelay = enemyPrefab.fireDelay * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.fireDistance = enemyPrefab.fireDistance * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.accuracy = UnityEngine.Random.Range(0f, 1f);
        newEnemy.speed = enemyPrefab.speed * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.maxHealth = enemyPrefab.maxHealth * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.damage = UnityEngine.Random.Range(damage * 0.85f, damage * 1.15f);
        newEnemy.HealToFull();
        newEnemy.stoppingDistance = enemyPrefab.stoppingDistance * UnityEngine.Random.Range(0.5f, 1.5f);
        newEnemy.spawnTime = Time.timeSinceLevelLoad;

        spawnEnemyAtTime = Time.timeSinceLevelLoad + enemySpawnTime;
    }

    public void IncDifficulty()
    {
        enemySpawnTime *= 0.985f;
        damage *= 1.015f;
    }
}