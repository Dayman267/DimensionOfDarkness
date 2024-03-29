using Unity.AI.Navigation;
using UnityEngine;

public class TilemapGenerator : MonoBehaviour
{
    public GameObject parentToSet;
    
    public GameObject[] gameObjectPrefabs;
    
    public int[,] gameObjectsToSpawn;
    
    private int width = 100;         // must be even
    private int height = 100;        // must be even

    private int objectSize;
    

    private void OnEnable()
    {
        TilemapGeneratorInPlayer.OnEnteredIntoATile += SpawnGameObjectsOnTriggerEnter;
    }

    private void OnDisable()
    {
        TilemapGeneratorInPlayer.OnEnteredIntoATile -= SpawnGameObjectsOnTriggerEnter;
    }

    public void Start()
    {
        objectSize = (int)gameObjectPrefabs[0].GetComponent<Terrain>().terrainData.size.x;
        gameObjectsToSpawn = new int[width, height];
        
        SpawnFirstNineObjects();
    }

    private void SpawnFirstNineObjects()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == width / 2 && y == height / 2) gameObjectsToSpawn[x, y] = 0;
                else gameObjectsToSpawn[x, y] = -1;
            }
        }

        bool isFinished = false;
        for (int x = 1; x < width && !isFinished; x++)
        {
            for (int y = 1; y < height && !isFinished; y++)
            {
                if (gameObjectsToSpawn[x, y] == -1) continue;

                int maxX = width - 1;
                int maxY = height - 1;

                if (x > 0 && gameObjectsToSpawn[x - 1, y] == -1)
                {
                    gameObjectsToSpawn[x - 1, y] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (y > 0 && gameObjectsToSpawn[x, y - 1] == -1)
                {
                    gameObjectsToSpawn[x, y - 1] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (x > 0 && y > 0 && gameObjectsToSpawn[x - 1, y - 1] == -1)
                {
                    gameObjectsToSpawn[x - 1, y - 1] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (x < maxX && gameObjectsToSpawn[x + 1, y] == -1)
                {
                    gameObjectsToSpawn[x + 1, y] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (y < maxY && gameObjectsToSpawn[x, y + 1] == -1)
                {
                    gameObjectsToSpawn[x, y + 1] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (x < maxX && y < maxY && gameObjectsToSpawn[x + 1, y + 1] == -1)
                {
                    gameObjectsToSpawn[x + 1, y + 1] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (x > 0 && y < maxY && gameObjectsToSpawn[x - 1, y + 1] == -1)
                {
                    gameObjectsToSpawn[x - 1, y + 1] = Random.Range(0, gameObjectPrefabs.Length);
                }

                if (y > 0 && x < maxX && gameObjectsToSpawn[x + 1, y - 1] == -1)
                {
                    gameObjectsToSpawn[x + 1, y - 1] = Random.Range(0, gameObjectPrefabs.Length);
                }

                isFinished = true;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameObjectsToSpawn[x, y] == -1) continue;
                Instantiate(
                    gameObjectPrefabs[gameObjectsToSpawn[x, y]],
                    new Vector3(x - width / 2, 0, y - height / 2) * objectSize,
                    Quaternion.identity,
                    parentToSet.transform);
            }
        }
    }

    public void SpawnGameObjectsOnTriggerEnter(Vector3 position)
    {
        float positionX = (position.x / objectSize) + width/2;
        float positionY = (position.z / objectSize) + height/2;
    
        int maxX = gameObjectsToSpawn.GetLength(0) - 1;
        int maxY = gameObjectsToSpawn.GetLength(1) - 1;
    
        if (positionX > 0 && gameObjectsToSpawn[(int)positionX - 1, (int)positionY] == -1)
        {
            gameObjectsToSpawn[(int)positionX - 1,(int)positionY] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX - 1,(int)positionY]],
                new Vector3((int)positionX - 1-width/2, 0, (int)positionY-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionY > 0 && gameObjectsToSpawn[(int)positionX, (int)positionY - 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX, (int)positionY - 1] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX,(int)positionY - 1]],
                new Vector3((int)positionX -width/2, 0, (int)positionY - 1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionX > 0 && positionY > 0 && gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX - 1,(int)positionY -1]],
                new Vector3((int)positionX - 1-width/2, 0, (int)positionY-1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionX < maxX && gameObjectsToSpawn[(int)positionX + 1, (int)positionY] == -1) 
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX + 1,(int)positionY]],
                new Vector3((int)positionX + 1-width/2, 0, (int)positionY-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionY < maxY && gameObjectsToSpawn[(int)positionX, (int)positionY + 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX, (int)positionY + 1] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX,(int)positionY+1]],
                new Vector3((int)positionX-width/2, 0, (int)positionY+1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionX < maxX && positionY < maxY && gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX + 1,(int)positionY+1]],
                new Vector3((int)positionX + 1-width/2, 0, (int)positionY+1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionX > 0 && positionY < maxY && gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX - 1,(int)positionY+1]],
                new Vector3((int)positionX - 1-width/2, 0, (int)positionY+1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        if (positionY > 0 && positionX < maxX && gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1] = Random.Range(0, 3);
            Instantiate(gameObjectPrefabs[gameObjectsToSpawn[(int)positionX + 1,(int)positionY-1]],
                new Vector3((int)positionX + 1-width/2, 0, (int)positionY-1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
        }
        
    }
}
