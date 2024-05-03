using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class BulletCaseSpawner : MonoBehaviour
{
    [SerializeField]private GameObject bulletPrefab;
    [SerializeField]private float rotationSpeed = 20f;
    [SerializeField]private float lifeTime = 30f;

    public void SpawnBullet(Transform bulletCasesParent)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, bulletCasesParent);
        
        bullet.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * rotationSpeed;
      
        StartCoroutine(DestroyBulletAfterTime(bullet, lifeTime));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
