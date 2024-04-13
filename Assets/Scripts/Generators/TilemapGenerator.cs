using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class TilemapGenerator : MonoBehaviour
{
    public GameObject parentToSet;
    
    public GameObject[] grayTerrains;
    public GameObject[] greenTerrains;
    public GameObject[] yellowTerrains;
    private List<GameObject[]> terrainsArrays = new();
    private List<GameObject> pickedTerrains1 = new();
    private GameObject[] pickedTerrains;

    private TerrainGenerator terrainGenerator;
    
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
        CreateTerrainsArrays();
        PickTerrainsAndCreateVersions();
        objectSize = (int)pickedTerrains1[0].GetComponent<Terrain>().terrainData.size.x;
        gameObjectsToSpawn = new int[width, height];
        SpawnFirstNineObjects();
    }

    private void CreateTerrainsArrays()
    {
        FieldInfo[] fields = typeof(TilemapGenerator)
            .GetFields(BindingFlags.Instance | BindingFlags.Public);
        
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(GameObject[]))
            {
                terrainsArrays.Add((GameObject[])field.GetValue(this));
            }
        }
    }

    private void PickTerrainsAndCreateVersions()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        int randomIndex = UnityEngine.Random.Range(0, terrainsArrays.Count);
        
        for (int i = 0, j = 0; i < terrainsArrays[randomIndex].Length; i++, j++)
        {
            pickedTerrains1.Add(terrainsArrays[randomIndex][i]);
            pickedTerrains1.Add(terrainGenerator.GenerateMirroredTerrain(pickedTerrains1[j]));
            j++;
            Destroy(pickedTerrains1[j]);
            for (int k = 0; k < 3; k++)
            {
                pickedTerrains1.Add(terrainGenerator.GenerateRotatedTerrain(pickedTerrains1[j]));
                j++;
                Destroy(pickedTerrains1[j]);
                pickedTerrains1.Add(terrainGenerator.GenerateMirroredTerrain(pickedTerrains1[j]));
                j++;
                Destroy(pickedTerrains1[j]);
            }
        }
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
                
                if (x > 0 && gameObjectsToSpawn[x - 1,y] == -1)
                {
                    gameObjectsToSpawn[x - 1,y] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (y > 0 && gameObjectsToSpawn[x,y - 1] == -1)
                {
                    gameObjectsToSpawn[x,y - 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (x > 0 && y > 0 && gameObjectsToSpawn[x - 1,y - 1] == -1) 
                {
                    gameObjectsToSpawn[x - 1,y - 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (x < maxX && gameObjectsToSpawn[x + 1,y] == -1) 
                {
                    gameObjectsToSpawn[x + 1,y] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (y < maxY && gameObjectsToSpawn[x,y + 1] == -1) 
                {
                    gameObjectsToSpawn[x,y + 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (x < maxX && y < maxY && gameObjectsToSpawn[x + 1,y + 1] == -1) 
                {
                    gameObjectsToSpawn[x + 1,y + 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (x > 0 && y < maxY && gameObjectsToSpawn[x - 1,y + 1] == -1) 
                {
                    gameObjectsToSpawn[x - 1,y + 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                if (y > 0 && x < maxX && gameObjectsToSpawn[x + 1,y - 1] == -1)
                {
                    gameObjectsToSpawn[x + 1,y - 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
                }
                isFinished = true;
            }
        }
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameObjectsToSpawn[x, y] == -1) continue;
                GameObject instance = Instantiate(
                    pickedTerrains1[gameObjectsToSpawn[x, y]],
                    new Vector3(x-width/2, 0, y-height/2) * objectSize, 
                    Quaternion.identity, 
                    parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
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
            gameObjectsToSpawn[(int)positionX - 1,(int)positionY] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX - 1,(int)positionY]],
                new Vector3((int)positionX - 1-width/2, 0, (int)positionY-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionY > 0 && gameObjectsToSpawn[(int)positionX, (int)positionY - 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX, (int)positionY - 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX,(int)positionY - 1]],
                new Vector3((int)positionX -width/2, 0, (int)positionY - 1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionX > 0 && positionY > 0 && gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX - 1,(int)positionY -1]],
                new Vector3((int)positionX - 1-width/2, 0, (int)positionY-1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionX < maxX && gameObjectsToSpawn[(int)positionX + 1, (int)positionY] == -1) 
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX + 1,(int)positionY]],
                new Vector3((int)positionX + 1-width/2, 0, (int)positionY-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionY < maxY && gameObjectsToSpawn[(int)positionX, (int)positionY + 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX, (int)positionY + 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX,(int)positionY+1]],
                new Vector3((int)positionX-width/2, 0, (int)positionY+1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionX < maxX && positionY < maxY && gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX + 1,(int)positionY+1]],
                new Vector3((int)positionX + 1-width/2, 0, (int)positionY+1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionX > 0 && positionY < maxY && gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX - 1,(int)positionY+1]],
                new Vector3((int)positionX - 1-width/2, 0, (int)positionY+1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
        if (positionY > 0 && positionX < maxX && gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1] == -1) 
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1] = UnityEngine.Random.Range(0, pickedTerrains1.Count);
            GameObject instance = Instantiate(pickedTerrains1[gameObjectsToSpawn[(int)positionX + 1,(int)positionY-1]],
                new Vector3((int)positionX + 1-width/2, 0, (int)positionY-1-height/2) * objectSize,
                Quaternion.identity, parentToSet.transform);
                instance.GetComponent<Terrain>().enabled = true;
                instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
    }
}
