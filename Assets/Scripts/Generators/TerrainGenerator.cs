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
        GameObject terrainPrefab = new GameObject();
        Terrain terrain = terrainPrefab.AddComponent<Terrain>();
        terrain.terrainData = GenerateTerrainData(exampleTerrain, variant);
        terrain.gameObject.AddComponent<TerrainCollider>().terrainData = terrain.terrainData;
        terrain.materialTemplate = exampleTerrain.materialTemplate;

        terrain.terrainData.terrainLayers = exampleTerrain.terrainData.terrainLayers;
        terrain.terrainData.RefreshPrototypes();

        terrain.terrainData.detailPrototypes = exampleTerrain.terrainData.detailPrototypes;
        terrain.terrainData.RefreshPrototypes();

        for (int i = 0; i < exampleTerrain.terrainData.detailPrototypes.Length; i++)
        {
            int[,] detailLayer = null;
            if (variant == 1)
            {
                detailLayer = RotateMatrix(exampleTerrain.terrainData.GetDetailLayer(
                    0, 0, 
                    exampleTerrain.terrainData.detailWidth, exampleTerrain.terrainData.detailHeight, 
                    i));
            }
            else if (variant == 2)
            {
                detailLayer = MirrorMatrix(exampleTerrain.terrainData.GetDetailLayer(
                    0, 0, 
                    exampleTerrain.terrainData.detailWidth, exampleTerrain.terrainData.detailHeight, 
                    i));
            }
            terrain.terrainData.SetDetailLayer(0, 0, i, detailLayer);
        }

        BoxCollider boxCollider = terrainPrefab.AddComponent<BoxCollider>();
        BoxCollider exampleBoxCollider = exampleObject.GetComponent<BoxCollider>();

        boxCollider.isTrigger = exampleBoxCollider.isTrigger;
        boxCollider.size = exampleBoxCollider.size;
        boxCollider.center = exampleBoxCollider.center;

        terrainPrefab.tag = "Terrain";
        terrainPrefab.layer = 6;
        
        ObjectsGenerator objectsGenerator = terrainPrefab.AddComponent<ObjectsGenerator>();
        objectsGenerator.gameObjectPrefabs = new List<GameObject>();
        ObjectsGenerator exampleObjectsGenerator = exampleObject.GetComponent<ObjectsGenerator>();
        for (int i = 0; i < exampleObjectsGenerator.gameObjectPrefabs.Count; i++)
        {
            objectsGenerator.gameObjectPrefabs.Add(exampleObjectsGenerator.gameObjectPrefabs[i]);
        }
        objectsGenerator.maxObjectsPerTile = exampleObjectsGenerator.maxObjectsPerTile;
        objectsGenerator.minObjectsPerTile = exampleObjectsGenerator.minObjectsPerTile;
        
        return terrainPrefab;
    }

    private TerrainData GenerateTerrainData(Terrain exampleTerrain, int variant)
    {
        TerrainData terrainData = new TerrainData();
        TerrainData exampleTerrainData = exampleTerrain.terrainData;
        float[,] exampleTerrainHeights = exampleTerrainData.GetHeights(
            0,
            0,
            exampleTerrain.terrainData.heightmapResolution,
            exampleTerrain.terrainData.heightmapResolution);
        terrainData.heightmapResolution = exampleTerrainData.heightmapResolution;

        terrainData.SetDetailResolution(exampleTerrainData.detailResolution, exampleTerrainData.detailResolutionPerPatch);

        if (variant == 1) terrainData.SetHeights(0, 0, RotateMatrix(exampleTerrainHeights));
        else if (variant == 2) terrainData.SetHeights(0, 0, MirrorMatrix(exampleTerrainHeights));
        terrainData.size = exampleTerrainData.size;
        return terrainData;
    }

    private T[,] RotateMatrix<T>(T[,] original)
    {
        int width = original.GetLength(0);
        int height = original.GetLength(1);
        T[,] rotated = new T[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotated[y, width - 1 - x] = original[x, y];
            }
        }
        return rotated;
    }

    private T[,] MirrorMatrix<T>(T[,] original)
    {
        int width = original.GetLength(0);
        int height = original.GetLength(1);
        T[,] mirrored = new T[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mirrored[x, y] = original[y, x];
            }
        }
        return mirrored;
    }
}