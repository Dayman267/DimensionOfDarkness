using System.Collections.Generic;
using UnityEngine;

public class ObjectsGenerator : MonoBehaviour
{
    private float objectSize;
    
    public List<GameObject> gameObjectPrefabs;
    
    public int minObjectsPerTile = 1;
    public int maxObjectsPerTile = 1;
    
    public void Start()
    {
        if(gameObjectPrefabs.Count == 0) return;
        
        Terrain terrain = transform.GetComponent<Terrain>();
        objectSize = terrain.terrainData.size.x;
        for(int i = Random.Range(minObjectsPerTile, maxObjectsPerTile+1), k = 0; k<i; k++)
        {
            float randPositionX = Random.Range(0, objectSize);
            float randPositionZ = Random.Range(0, objectSize);
            float positionY = terrain.SampleHeight(new Vector3(
                transform.position.x +randPositionX, 
                0, 
                transform.position.z + randPositionZ));
            Vector3 position = new Vector3(
                randPositionX, 
                positionY, 
                randPositionZ);
            Instantiate(
                gameObjectPrefabs[Random.Range(0, gameObjectPrefabs.Count)], 
                position + terrain.transform.position,
                Quaternion.Euler(0, Random.Range(0f, 360f), 0),
                transform);
        }
        Destroy(this);
    }
}
