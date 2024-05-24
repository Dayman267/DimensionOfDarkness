using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject GenerateRotatedTerrain(GameObject exampleTerrain)
    {
        return GenerateTerrain(exampleTerrain.GetComponent<Terrain>(), 1, exampleTerrain);
    }

    public GameObject GenerateMirroredTerrain(GameObject exampleTerrain)
    {
        return GenerateTerrain(exampleTerrain.GetComponent<Terrain>(), 2, exampleTerrain);
    }

    private GameObject GenerateTerrain(Terrain exampleTerrain, int variant, GameObject exampleObject)
    {
        var terrainPrefab = new GameObject();
        var terrain = terrainPrefab.AddComponent<Terrain>();
        terrain.terrainData = GenerateTerrainData(exampleTerrain, variant);
        terrain.gameObject.AddComponent<TerrainCollider>().terrainData = terrain.terrainData;
        terrain.materialTemplate = exampleTerrain.materialTemplate;

        terrain.terrainData.terrainLayers = exampleTerrain.terrainData.terrainLayers;
        terrain.terrainData.RefreshPrototypes();

        var boxCollider = terrainPrefab.AddComponent<BoxCollider>();
        var exampleBoxCollider = exampleObject.GetComponent<BoxCollider>();

        boxCollider.isTrigger = exampleBoxCollider.isTrigger;
        boxCollider.size = exampleBoxCollider.size;
        boxCollider.center = exampleBoxCollider.center;

        terrainPrefab.tag = "Terrain";
        terrainPrefab.layer = 6;

        var objectsGenerator = terrainPrefab.AddComponent<ObjectsGenerator>();
        objectsGenerator.gameObjectPrefabs = new List<GameObject>();
        var exampleObjectsGenerator = exampleObject.GetComponent<ObjectsGenerator>();
        for (var i = 0; i < exampleObjectsGenerator.gameObjectPrefabs.Count; i++)
            objectsGenerator.gameObjectPrefabs.Add(exampleObjectsGenerator.gameObjectPrefabs[i]);
        objectsGenerator.maxObjectsPerTile = exampleObjectsGenerator.maxObjectsPerTile;
        objectsGenerator.minObjectsPerTile = exampleObjectsGenerator.minObjectsPerTile;

        return terrainPrefab;
    }

    private TerrainData GenerateTerrainData(Terrain exampleTerrain, int variant)
    {
        var terrainData = new TerrainData();
        var exampleTerrainData = exampleTerrain.terrainData;
        var exampleTerrainHeights = exampleTerrainData.GetHeights(
            0,
            0,
            exampleTerrain.terrainData.heightmapResolution,
            exampleTerrain.terrainData.heightmapResolution);
        terrainData.heightmapResolution = exampleTerrainData.heightmapResolution;

        if (variant == 1) terrainData.SetHeights(0, 0, RotateHeights(exampleTerrainHeights, exampleTerrain));
        else if (variant == 2) terrainData.SetHeights(0, 0, MirrorHeights(exampleTerrainHeights, exampleTerrain));
        terrainData.size = exampleTerrainData.size;
        return terrainData;
    }

    private float[,] MirrorHeights(float[,] original, Terrain exampleTerrain)
    {
        var heightmapResolution = exampleTerrain.terrainData.heightmapResolution;
        var heights = new float[heightmapResolution, heightmapResolution];
        for (var x = 0; x < heightmapResolution; x++)
        for (var y = 0; y < heightmapResolution; y++)
            heights[x, y] = original[y, x];
        return heights;
    }

    private float[,] RotateHeights(float[,] original, Terrain exampleTerrain)
    {
        var heightmapResolution = exampleTerrain.terrainData.heightmapResolution;
        var heights = new float[heightmapResolution, heightmapResolution];

        for (var x = 0; x < heightmapResolution; x++)
        for (var y = 0; y < heightmapResolution; y++)
            heights[y, heightmapResolution - 1 - x] = original[x, y];
        return heights;
    }
}