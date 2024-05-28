using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
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
            var randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0f; // Keep the enemies on the same level as the player

            var spawnPosition = playerTransform.position + randomOffset;

            if (NavMesh.SamplePosition(spawnPosition, out var hit, spawnRadius, NavMesh.AllAreas))
            {
                var randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                var enemyInstance = Instantiate(randomEnemyPrefab, hit.position, Quaternion.identity);

                // Передача игрока в EnemyMovement
                
                if (enemyInstance.TryGetComponent<EnemyMovement>(out var enemyMovement))
                {
                    enemyMovement.Player = playerTransform;
                }

                // Убедитесь, что NavMeshAgent активен
                
                if (enemyInstance.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
                {
                    navMeshAgent.enabled = true;
                }
            }
            else
            {
                Debug.LogWarning("Unable to find valid NavMesh position for spawning enemy.");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /*public Transform Player;
    public int NumberOfEnemiesToSpawn = 5;
    public float SpawnDelay = 1f;
    public List<Enemy> EnemyPrefabs = new List<Enemy>();
    public SpawnMethod EnemySpawnMethod = SpawnMethod.RoundRobin;

    private NavMeshTriangulation Triangulation;
    private Dictionary<int, ObjectPool.ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool.ObjectPool>();

    private void Awake()
    {
        for (int i = 0; i < EnemyPrefabs.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.ObjectPool.CreateInstance(EnemyPrefabs[i], NumberOfEnemiesToSpawn));
        }
    }

    private void Start()
    {
        Triangulation = NavMesh.CalculateTriangulation();
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds Wait = new WaitForSeconds(SpawnDelay);

        int SpawnedEnemies = 0;

        while (SpawnedEnemies < NumberOfEnemiesToSpawn)
        {
            if (EnemySpawnMethod == SpawnMethod.RoundRobin)
            {
                SpawnRoundRobinEnemy(SpawnedEnemies);
            }
            else if (EnemySpawnMethod == SpawnMethod.Random)
            {
                SpawnRandomEnemy();
            }

            SpawnedEnemies++;
            yield return Wait;
        }
    }

    private void SpawnRoundRobinEnemy(int SpawnedEnemies)
    {
        int SpawnIndex = SpawnedEnemies % EnemyPrefabs.Count;
        DoSpawnEnemy(SpawnIndex);
    }

    private void SpawnRandomEnemy()
    {
        DoSpawnEnemy(Random.Range(0, EnemyPrefabs.Count));
    }

    private void DoSpawnEnemy(int SpawnIndex)
    {
        var poolableObject = EnemyObjectPools[SpawnIndex].GetObject();

        if (poolableObject != null)
        {
            Enemy enemy = poolableObject.GetComponent<Enemy>();

            int VertexIndex = Random.Range(0, Triangulation.vertices.Length);

            NavMeshHit Hit;
            if (NavMesh.SamplePosition(Triangulation.vertices[VertexIndex], out Hit, 2f, NavMesh.AllAreas))
            {
                enemy.Agent.Warp(Hit.position);
                enemy.Agent.enabled = true;
                enemy.Movement.Player = Player;
                enemy.Movement.StartChasing();
            }
            else
            {
                Debug.LogError($"Unable to place NavMeshAgent on NavMesh. Tried to use {Triangulation.vertices[VertexIndex]}");
            }
        }
        else
        {
            Debug.LogError($"Unable to fetch enemy of type {SpawnIndex} from object pool. Out of objects?");
        }
    }

    public enum SpawnMethod
    {
        RoundRobin,
        Random
        // Other spawn methods can be added here
    }*/
}