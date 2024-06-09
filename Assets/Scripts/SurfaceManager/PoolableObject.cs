using UnityEngine;
using UnityEngine.Pool;

public class PoolableObject : MonoBehaviour
{
    public ObjectPool<GameObject> Parent;

    protected virtual void OnDisable()
    {
        if (Parent != null) Parent.Release(gameObject);
    }
}