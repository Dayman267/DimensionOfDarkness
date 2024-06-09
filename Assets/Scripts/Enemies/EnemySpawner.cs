using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemyScriptableObject> enemies = new();
    public float spawnRadius = 10f;
    public float spawnInterval = 5f;
    
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            int numberOfEnemiesToSpawn = Random.Range(1, 3);

            for (int i = 0; i < numberOfEnemiesToSpawn; i++)
            {
                var randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = 0f; // Keep the enemies on the same level as the player

                var spawnPosition = playerTransform.position + randomOffset;

                if (NavMesh.SamplePosition(spawnPosition, out var hit, spawnRadius, NavMesh.AllAreas))
                {
                    var randomEnemySO = enemies[Random.Range(0, enemies.Count)];
                    var enemyInstance = Instantiate(randomEnemySO.Prefab.gameObject, hit.position, Quaternion.identity);

                    // Setup enemy with ScriptableObject configurations
                    randomEnemySO.SetupEnemy(enemyInstance.GetComponent<Enemy>());
                    
                    if (enemyInstance.TryGetComponent<EnemyMovement>(out var enemyMovement))
                    {
                        enemyMovement.Player = playerTransform;
                    }
                    
                    if (enemyInstance.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
                    {
                        navMeshAgent.enabled = true;
                    }
                }
                else
                {
                    Debug.LogWarning("Unable to find valid NavMesh position for spawning enemy.");
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    public IEnumerator SpawnSpecificEnemy(EnemyScriptableObject enemySO)
    {
        var randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0f; // Keep the enemies on the same level as the player

        var spawnPosition = playerTransform.position + randomOffset;

        if (NavMesh.SamplePosition(spawnPosition, out var hit, spawnRadius, NavMesh.AllAreas))
        {
            var enemyInstance = Instantiate(enemySO.Prefab.gameObject, hit.position, Quaternion.identity);

            // Setup enemy with ScriptableObject configurations
            enemySO.SetupEnemy(enemyInstance.GetComponent<Enemy>());
            
            if (enemyInstance.TryGetComponent<EnemyMovement>(out var enemyMovement))
            {
                enemyMovement.Player = playerTransform;
            }
            
            if (enemyInstance.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
            {
                navMeshAgent.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning("Unable to find valid NavMesh position for spawning enemy.");
        }

        yield return null;
    }
}
