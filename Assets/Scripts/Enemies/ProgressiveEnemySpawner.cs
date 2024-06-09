using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProgressiveEnemySpawner : MonoBehaviour
{
    public enum SpawnMethod
    {
        RoundRobin,
        Random
    }

    public const int TileSize = 10;
    public Transform Player;
    public float SpawnDensityPerTile = 0.5f;
    public int MaxEnemies = 5;

    public Vector3Int NavMeshSize = new(40, 10, 40);
    public List<EnemyScriptableObject> Enemies = new();
    public SpawnMethod EnemySpawnMethod = SpawnMethod.RoundRobin;
    [SerializeField] private AreaFloorBaker FloorBaker;
    [SerializeField] private LayerMask EnemyMask;
    private readonly Dictionary<int, ObjectPool.ObjectPool> EnemyObjectPools = new();
    private readonly HashSet<Vector3> SpawnedTiles = new();
    public HashSet<Enemy> AliveEnemies = new();
    private Collider[] EnemyColliders;
    private int SpawnedEnemies;

    private void Awake()
    {
        for (var i = 0; i < Enemies.Count; i++)
            EnemyObjectPools.Add(i, ObjectPool.ObjectPool.CreateInstance(Enemies[i].Prefab, MaxEnemies));

        FloorBaker.OnNavMeshUpdate += HandleNavMeshUpdate;
    }

    private void Start()
    {
        EnemyColliders = new Collider[MaxEnemies];
    }

    private void SpawnEnemiesOnNewTiles(Vector3 currentTilePosition)
    {
        if (SpawnedEnemies >= MaxEnemies) return;

        for (var x = -1 * NavMeshSize.x / TileSize / 2; x <= NavMeshSize.x / TileSize / 2; x++)
        for (var z = -1 * NavMeshSize.z / TileSize / 2; z <= NavMeshSize.z / TileSize / 2; z++)
        {
            var tilePosition = new Vector3(currentTilePosition.x + x, currentTilePosition.y,
                currentTilePosition.z + z);
            var enemiesSpawnedForTile = 0;

            if (!SpawnedTiles.Contains(tilePosition))
                while (enemiesSpawnedForTile + Random.value < SpawnDensityPerTile && SpawnedEnemies < MaxEnemies)
                {
                    SpawnEnemyOnTile(tilePosition);
                    enemiesSpawnedForTile++;
                    SpawnedEnemies++;
                }
        }
    }

    private void SpawnEnemyOnTile(Vector3 TilePosition)
    {
        if (EnemySpawnMethod == SpawnMethod.Random) SpawnRandomEnemy(TilePosition);
    }

    private void SpawnRandomEnemy(Vector3 TilePosition)
    {
        DoSpawnEnemy(Random.Range(0, Enemies.Count), TilePosition);
    }

    private void DoSpawnEnemy(int SpawnIndex, Vector3 TilePosition)
    {
        var poolableObject = EnemyObjectPools[SpawnIndex].GetObject();

        if (poolableObject != null)
        {
            var enemy = poolableObject.GetComponent<Enemy>();
            Enemies[SpawnIndex].SetupEnemy(enemy);


            NavMeshHit Hit;
            if (NavMesh.SamplePosition(TilePosition * TileSize +
                                       new Vector3(Random.Range(-TileSize / 2, TileSize / 2), 0,
                                           Random.Range(-TileSize / 2, TileSize / 2)),
                    out Hit, 5f, -1))
            {
                enemy.Agent.Warp(Hit.position);
                // enemy needs to get enabled and start chasing now.
                enemy.Movement.Player = Player;
                enemy.Agent.enabled = true;
                enemy.Movement.Spawn();
                AliveEnemies.Add(enemy);
                // enemy.OnDie += HandleEnemyDeath;
                // enemy.Health.OnDeath += HandleEnemyDeath;
            }
            else
            {
                Debug.LogError(
                    $"Unable to place NavMeshAgent on NavMesh. Tried to spawn on tile {TilePosition}");
                SpawnedEnemies--;
                SpawnedTiles.Remove(TilePosition);
                enemy.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError($"Unable to fetch enemy of type {SpawnIndex} from object pool. Out of objects?");
            SpawnedEnemies--;
            SpawnedTiles.Remove(TilePosition);
        }
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        AliveEnemies.Remove(enemy);
    }

    private void HandleNavMeshUpdate(Bounds Bounds)
    {
        int Hits = Physics.OverlapBoxNonAlloc(Bounds.center, Bounds.extents, EnemyColliders, Quaternion.identity, EnemyMask.value);

        Enemy enemyComponent;
        Enemy[] enemyArray = new Enemy[Hits];
        for (int i = 0; i < Hits; i++)
        {
            if (EnemyColliders[i].gameObject.TryGetComponent<Enemy>(out enemyComponent))
            {
                enemyArray[i] = enemyComponent;
                enemyComponent.Agent.enabled = true;
            }
        }

        HashSet<Enemy> outOfBoundsEnemies = new HashSet<Enemy>(AliveEnemies);

        outOfBoundsEnemies.ExceptWith(enemyArray);

        foreach (Enemy enemy in outOfBoundsEnemies)
        {
            enemy.Agent.enabled = false;
        }

        Vector3 currentTilePosition = new Vector3(
            Mathf.FloorToInt(Player.transform.position.x) / TileSize,
            Mathf.FloorToInt(Player.transform.position.y) / TileSize,
            Mathf.FloorToInt(Player.transform.position.z) / TileSize
        );

        if (!SpawnedTiles.Contains(currentTilePosition))
        {
            SpawnedTiles.Add(currentTilePosition);
        }

        SpawnEnemiesOnNewTiles(currentTilePosition);
    }
}