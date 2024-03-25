using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunSO : ScriptableObject
{
    //public ImpactType ImpactType;
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public DamageConfigSO DamageConfig;
    public ShootConfigurationSO ShootConfig;
    public TrailConfigSO TrailConfig;
    public AmmoConfigSO AmmoConfig;
    public AudioConfigSO AudioConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private AudioSource ShootingAudioSource;

    private float LastShootTime;
    private float InitialClickTime;
    private float StopShootingTime;
    private bool LastFrameWantedToShoot;

    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;
    private ObjectPool<Bullet> BulletPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0; // in editor this will not be properly reset, in build it`s fine
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        if (!ShootConfig.IsHitScan)
        {
            BulletPool = new ObjectPool<Bullet>(CreateBullet);
        }
        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
        ShootingAudioSource = Model.GetComponent<AudioSource>();
    }

    public void TryToShoot()
    {
        if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp(
                0,
                (StopShootingTime - InitialClickTime),
                ShootConfig.MaxSpreadTime
            );
            float lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                             / ShootConfig.RecoilRecoverySpeed;
            InitialClickTime = Time.time - Mathf.Lerp(0,lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            

            LastShootTime = Time.time;
            
            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }
            
            ShootSystem.Play();
            AudioConfig.PlayShootingClip(ShootingAudioSource,AmmoConfig.CurrentClipAmmo == 1);

            Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
            //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
            //Quaternion rotation = Quaternion.Euler(spreadAmount);
            //Model.transform.rotation *= rotation;
            
            Vector3 shootDirection = ShootSystem.transform.forward + spreadAmount;

            AmmoConfig.CurrentClipAmmo--;

            if (ShootConfig.IsHitScan)
            {
                DoHitScanShoot(shootDirection);
            }
            else
            {
                DoProjectileShoot(shootDirection);
            }
            
            
        }
    }

    private void DoProjectileShoot(Vector3 shootDirection)
    {
        Bullet bullet = BulletPool.Get();
        bullet.gameObject.SetActive(true);
        bullet.OnCollision += HandleOnBulletColision;
        bullet.transform.position = ShootSystem.transform.position;
        bullet.Spawn(shootDirection * ShootConfig.BulletSpawnForce);

        TrailRenderer trail = TrailPool.Get();
        if (trail != null)
        {
            trail.transform.SetParent(bullet.transform,false);
            trail.transform.localPosition = Vector3.zero;
            trail.emitting = true;
            trail.gameObject.SetActive(true);
        }
    }

    private void HandleOnBulletColision(Bullet bullet, Collision collision)
    {
        TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.transform.SetParent(null,true);
            ActiveMonoBehaviour.StartCoroutine(DeleyedDisableTrail(trail));
        }
        
        bullet.gameObject.SetActive(false);
        BulletPool.Release(bullet);

        if (collision != null)
        {
            ContactPoint contactPoint = collision.GetContact(0);

            HandleBulletImpact(
                Vector3.Distance(contactPoint.point,bullet.SpawnLocation),
                contactPoint.point,
                contactPoint.normal,
                contactPoint.otherCollider
            );
        }
    }

    private IEnumerator DeleyedDisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        trail.emitting = false;
        trail.gameObject.SetActive(false);
        TrailPool.Release(trail);
    }

    // Video 6 - 10:39
    private void HandleBulletImpact(
        float DistanceTraveled,
        Vector3 HitLocation,
        Vector3 HitNormal,
        Collider HitCollider)
    {
        /*SurfaceManager.Instance.HandleImpact(
                HitCollider.gameObject,
                HitLocation,
                HitNormal,
                ImpactType,
                0
            );*/

        if (HitCollider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(DamageConfig.GetDamage(DistanceTraveled));
        }
    }

    private void DoHitScanShoot(Vector3 shootDirection)
    {
        if (Physics.Raycast(
                ShootSystem.transform.position,
                shootDirection,
                out RaycastHit hit,
                float.MaxValue,
                ShootConfig.HitMask
            ))
        {
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    ShootSystem.transform.position,
                    hit.point,
                    hit
                )
            );
        }
        else
        {
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    ShootSystem.transform.position,
                    ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                    new RaycastHit()
                )
            );
        }
    }

    public bool CanReload()
    {
        return AmmoConfig.CanReload();
    }
    
    public void EndReload()
    {
        AmmoConfig.Reload();
    }
    
    public void StartReloading()
    {
        AudioConfig.PlayReloadClip(ShootingAudioSource);
    }
    
    public void Tick(bool WantsToShoot)
    {
        if (WantsToShoot)
        {
            LastFrameWantedToShoot = true;
            TryToShoot();
        }
        else if (!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
        }
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null; // avoid position carry-over from last frame if reused

        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            HandleBulletImpact(distance,EndPoint,Hit.normal,Hit.collider);
        }
        
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        Destroy(instance.gameObject);
        //TrailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = ShadowCastingMode.Off;

        return trail;
    }

    private Bullet CreateBullet()
    {
        return Instantiate(ShootConfig.BulletPrefab);
    }
    
}