using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public delegate void CollisionEvent(Bullet Bullet, Collision Collision, int ObjectsPenetrated);

    [field: SerializeField] public Vector3 SpawnLocation { get; private set; }
    
    [field: SerializeField] public Vector3 SpawnVelocity { get; private set; }

    [SerializeField] private float DeleyedDisableTime = 2f;

    //[SerializeField] private GameObject effectPrefab;
    [SerializeField] private float effectDuration = 2f;
    private int ObjectsPenetrated;

    public Rigidbody Rigidbody { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        OnCollision = null;
    }

    private void OnCollisionEnter(Collision Collision)
    {
        OnCollision?.Invoke(this, Collision, ObjectsPenetrated);
        ObjectsPenetrated++;

        /*if (effectPrefab != null)
        {
            // Создаем экземпляр эффекта
            GameObject effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            // Запускаем корутину для его удаления через effectDuration секунд
            StartCoroutine(DisableEffect(effectInstance, effectDuration));
        }*/
    }

    public event CollisionEvent OnCollision;

    public void Spawn(Vector3 SpawnForce)
    {
        /*if (effectPrefab != null)
        {
            GameObject effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            effectInstance.transform.parent = transform;
        }*/

        ObjectsPenetrated = 0;
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        SpawnVelocity = SpawnForce * Time.fixedDeltaTime / Rigidbody.mass;
        StartCoroutine(DeleyedDisable(DeleyedDisableTime));
    }

    private IEnumerator DeleyedDisable(float Time)
    {
        yield return null;
        yield return new WaitForSeconds(Time);
        OnCollisionEnter(null);
    }

    private IEnumerator DisableEffect(GameObject effectInstance, float duration)
    {
        // Ждем заданное время
        yield return new WaitForSeconds(duration);
        // После ожидания отключаем эффект
        if (effectInstance != null) Destroy(effectInstance);
    }
}