using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectsGenerator : MonoBehaviour
{
    private GameObject parentToSet;
    
    private float objectSize;
    
    public GameObject[] gameObjectPrefabs;
    
    [SerializeField] private int minObjectsPerTile = 1;
    [SerializeField] private int maxObjectsPerTile = 1;
    
    public void Start()
    {
        if(gameObjectPrefabs.Length == 0) return;
        
        parentToSet = GameObject.FindWithTag("Objects");
        objectSize = (int)(transform.GetChild(0).gameObject.transform.lossyScale.x *
                           Mathf.Sqrt(transform.childCount)/2);
        for(int i = Random.Range(minObjectsPerTile, maxObjectsPerTile+1), k = 0; k<i; k++)
        {
            float randPositionX = Random.Range(-objectSize, objectSize+1);
            float randPositionY = Random.Range(-objectSize, objectSize+1);
            Vector3 position = new Vector3(transform.position.x + randPositionX, 0, transform.position.z + randPositionY);
            Instantiate(
                gameObjectPrefabs[Random.Range(0, gameObjectPrefabs.Length)], 
                position,
                Quaternion.identity,
                parentToSet.transform);
        }
        Destroy(this);
    }
}
