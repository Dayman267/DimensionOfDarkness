using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TilemapGenerator : MonoBehaviour
{
    public GameObject parentToSet;
    public GameObject referenceObjects;

    public GameObject[] grayTerrains;
    public GameObject[] greenTerrains;
    public GameObject[] yellowTerrains;
    private readonly int height = 100; // must be even
    private readonly List<GameObject> pickedTerrains = new();
    private readonly List<GameObject[]> terrainsArrays = new();

    private readonly int width = 100; // must be even

    public int[,] gameObjectsToSpawn;

    private int objectSize;

    private TerrainGenerator terrainGenerator;

    public void Start()
    {
        CreateTerrainsArrays();
        PickTerrainsAndCreateVersions();
        objectSize = (int)pickedTerrains[0].GetComponent<Terrain>().terrainData.size.x;
        gameObjectsToSpawn = new int[width, height];
        SpawnFirstNineObjects();
    }

    private void OnEnable()
    {
        TilemapGeneratorInPlayer.OnEnteredIntoATile += SpawnGameObjectsOnTriggerEnter;
    }

    private void OnDisable()
    {
        TilemapGeneratorInPlayer.OnEnteredIntoATile -= SpawnGameObjectsOnTriggerEnter;
    }

    private void CreateTerrainsArrays()
    {
        var fields = typeof(TilemapGenerator)
            .GetFields(BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
            if (field.FieldType == typeof(GameObject[]))
                terrainsArrays.Add((GameObject[])field.GetValue(this));
    }

    private void PickTerrainsAndCreateVersions()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        var randomIndex = Random.Range(0, terrainsArrays.Count);

        for (int i = 0, j = 0; i < terrainsArrays[randomIndex].Length; i++, j++)
        {
            pickedTerrains.Add(terrainsArrays[randomIndex][i]);
            pickedTerrains.Add(terrainGenerator.GenerateMirroredTerrain(pickedTerrains[j]));
            j++;
            pickedTerrains[j].SetActive(false);
            pickedTerrains[j].transform.parent = referenceObjects.transform;
            for (var k = 0; k < 3; k++)
            {
                pickedTerrains.Add(terrainGenerator.GenerateRotatedTerrain(pickedTerrains[j]));
                j++;
                pickedTerrains[j].SetActive(false);
                pickedTerrains[j].transform.parent = referenceObjects.transform;
                pickedTerrains.Add(terrainGenerator.GenerateMirroredTerrain(pickedTerrains[j]));
                j++;
                pickedTerrains[j].SetActive(false);
                pickedTerrains[j].transform.parent = referenceObjects.transform;
            }
        }
    }

    private void SpawnFirstNineObjects()
    {
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            if (x == width / 2 && y == height / 2) gameObjectsToSpawn[x, y] = 0;
            else gameObjectsToSpawn[x, y] = -1;

        var isFinished = false;
        for (var x = 1; x < width && !isFinished; x++)
        for (var y = 1; y < height && !isFinished; y++)
        {
            if (gameObjectsToSpawn[x, y] == -1) continue;

            var maxX = width - 1;
            var maxY = height - 1;

            if (x > 0 && gameObjectsToSpawn[x - 1, y] == -1)
                gameObjectsToSpawn[x - 1, y] = Random.Range(0, pickedTerrains.Count);
            if (y > 0 && gameObjectsToSpawn[x, y - 1] == -1)
                gameObjectsToSpawn[x, y - 1] = Random.Range(0, pickedTerrains.Count);
            if (x > 0 && y > 0 && gameObjectsToSpawn[x - 1, y - 1] == -1)
                gameObjectsToSpawn[x - 1, y - 1] = Random.Range(0, pickedTerrains.Count);
            if (x < maxX && gameObjectsToSpawn[x + 1, y] == -1)
                gameObjectsToSpawn[x + 1, y] = Random.Range(0, pickedTerrains.Count);
            if (y < maxY && gameObjectsToSpawn[x, y + 1] == -1)
                gameObjectsToSpawn[x, y + 1] = Random.Range(0, pickedTerrains.Count);
            if (x < maxX && y < maxY && gameObjectsToSpawn[x + 1, y + 1] == -1)
                gameObjectsToSpawn[x + 1, y + 1] = Random.Range(0, pickedTerrains.Count);
            if (x > 0 && y < maxY && gameObjectsToSpawn[x - 1, y + 1] == -1)
                gameObjectsToSpawn[x - 1, y + 1] = Random.Range(0, pickedTerrains.Count);
            if (y > 0 && x < maxX && gameObjectsToSpawn[x + 1, y - 1] == -1)
                gameObjectsToSpawn[x + 1, y - 1] = Random.Range(0, pickedTerrains.Count);
            isFinished = true;
        }

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            if (gameObjectsToSpawn[x, y] == -1) continue;
            var instance = Instantiate(
                pickedTerrains[gameObjectsToSpawn[x, y]],
                new Vector3(x - width / 2, 0, y - height / 2) * objectSize,
                Quaternion.identity,
                parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
    }

    public void SpawnGameObjectsOnTriggerEnter(Vector3 position)
    {
        var positionX = position.x / objectSize + width / 2;
        var positionY = position.z / objectSize + height / 2;
        var maxX = gameObjectsToSpawn.GetLength(0) - 1;
        var maxY = gameObjectsToSpawn.GetLength(1) - 1;
        if (positionX > 0 && gameObjectsToSpawn[(int)positionX - 1, (int)positionY] == -1)
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX - 1, (int)positionY]],
                new Vector3((int)positionX - 1 - width / 2, 0, (int)positionY - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionY > 0 && gameObjectsToSpawn[(int)positionX, (int)positionY - 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX, (int)positionY - 1] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX, (int)positionY - 1]],
                new Vector3((int)positionX - width / 2, 0, (int)positionY - 1 - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionX > 0 && positionY > 0 && gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX - 1, (int)positionY - 1]],
                new Vector3((int)positionX - 1 - width / 2, 0, (int)positionY - 1 - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionX < maxX && gameObjectsToSpawn[(int)positionX + 1, (int)positionY] == -1)
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX + 1, (int)positionY]],
                new Vector3((int)positionX + 1 - width / 2, 0, (int)positionY - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionY < maxY && gameObjectsToSpawn[(int)positionX, (int)positionY + 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX, (int)positionY + 1] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX, (int)positionY + 1]],
                new Vector3((int)positionX - width / 2, 0, (int)positionY + 1 - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionX < maxX && positionY < maxY && gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX + 1, (int)positionY + 1]],
                new Vector3((int)positionX + 1 - width / 2, 0, (int)positionY + 1 - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionX > 0 && positionY < maxY && gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX - 1, (int)positionY + 1]],
                new Vector3((int)positionX - 1 - width / 2, 0, (int)positionY + 1 - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }

        if (positionY > 0 && positionX < maxX && gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1] == -1)
        {
            gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1] = Random.Range(0, pickedTerrains.Count);
            var instance = Instantiate(pickedTerrains[gameObjectsToSpawn[(int)positionX + 1, (int)positionY - 1]],
                new Vector3((int)positionX + 1 - width / 2, 0, (int)positionY - 1 - height / 2) * objectSize,
                Quaternion.identity, parentToSet.transform);
            instance.SetActive(true);
            instance.GetComponent<Terrain>().enabled = true;
            instance.GetComponent<ObjectsGenerator>().enabled = true;
        }
    }
}