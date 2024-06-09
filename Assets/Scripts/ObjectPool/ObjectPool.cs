using System.Collections.Generic;
using UnityEngine;

namespace ObjectPool
{
    public class ObjectPool
    {
        private readonly List<PoolableObject> AvailableObjectsPool;
        private readonly PoolableObject Prefab;
        private readonly int Size;

        private ObjectPool(PoolableObject Prefab, int Size)
        {
            this.Prefab = Prefab;
            this.Size = Size;
            AvailableObjectsPool = new List<PoolableObject>(Size);
        }

        public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
        {
            var pool = new ObjectPool(Prefab, Size);

            var poolGameObject = new GameObject(Prefab + " Pool");
            pool.CreateObjects(poolGameObject);

            return pool;
        }

        private void CreateObjects(GameObject parent)
        {
            for (var i = 0; i < Size; i++)
            {
                var poolableObject = Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
                poolableObject.Parent = this;
                poolableObject.gameObject
                    .SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
            }
        }

        public PoolableObject GetObject()
        {
            var instance = AvailableObjectsPool[0];

            AvailableObjectsPool.RemoveAt(0);

            instance.gameObject.SetActive(true);

            return instance;
        }

        public void ReturnObjectToPool(PoolableObject Object)
        {
            AvailableObjectsPool.Add(Object);
        }
    }
}