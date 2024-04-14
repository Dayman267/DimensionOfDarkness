using System;
using System.Collections;
using LlamAcademy.ImpactSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunSO : ScriptableObject, ICloneable
{
    public ImpactType ImpactType;
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    //public RuntimeAnimatorController animator;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public DamageConfigSO DamageConfig;
    public ShootConfigurationSO ShootConfig;
    public TrailConfigSO TrailConfig;
    public AmmoConfigSO AmmoConfig;
    public AudioConfigSO AudioConfig;
    public BulletPenetrationConfigSO BulletPenetrationConfig;
    

    public ICollisionHandler[] BulletImpactEffects = Array.Empty<ICollisionHandler>();

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private AudioSource ShootingAudioSource;
    private Camera ActiveCamera;

    private float LastShootTime;
    private float InitialClickTime;
    private float StopShootingTime;
    private bool LastFrameWantedToShoot;

    private ParticleSystem[] ShootSystem;
    private GameObject ShootingStartPoint;
    private ObjectPool<TrailRenderer> TrailPool;
    private ObjectPool<Bullet> BulletPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Camera ActiveCamera = null)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        this.ActiveCamera = ActiveCamera;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        if (!ShootConfig.IsHitScan)
        {
            BulletPool = new ObjectPool<Bullet>(CreateBullet);
        }

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentsInChildren<ParticleSystem>();
        ShootingAudioSource = Model.GetComponent<AudioSource>();
        ShootingStartPoint = GameObject.FindWithTag("ShootingStartPoint");

        //ShootingStartPoint.transform.position = ShootSystem[0].transform.position;
    }
    
    public void Despawn()
    {
        Model.SetActive(false);
        Destroy(Model);
        TrailPool.Clear();
        if(BulletPool != null)
            BulletPool.Clear();

        ShootingAudioSource = null;
        ShootSystem = null;
        ShootingStartPoint = null;
    }

    public void UpdateCamera(Camera ActiveCamera)
    {
        this.ActiveCamera = ActiveCamera;
    }

    public void PlayParticleSystems()
    {
        foreach (ParticleSystem ps in ShootSystem)
        {
            ps.Play();
        }
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
            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;

            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }

            PlayParticleSystems();
            AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);
            
            AmmoConfig.CurrentClipAmmo--;
            
            for (int i = 0; i < ShootConfig.BulletPerShot; i++)
            {
                Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
                //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
                //Quaternion rotation = Quaternion.Euler(spreadAmount);
                //Model.transform.rotation *= rotation;
            
                Ray ray = ActiveCamera.ScreenPointToRay(Mouse.current.position.value);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Vector3 shootDirection = (hit.point - ShootingStartPoint.transform.position) + Model.transform.TransformDirection(spreadAmount);
                    if (ShootConfig.IsHitScan)
                    {
                        DoHitScanShoot(shootDirection, GetRaycastOrigin(), ShootingStartPoint.transform.position);
                    }
                    else
                    {
                        DoProjectileShoot(shootDirection.normalized);
                    }
                }
                else
                {
                    Vector3 shootDirection = ray.direction;
                    if (ShootConfig.IsHitScan)
                    {
                        DoHitScanShoot(shootDirection, GetRaycastOrigin(), ShootingStartPoint.transform.position);
                    }
                    else
                    {
                        DoProjectileShoot(shootDirection.normalized);
                    }
                }
            }
        }
    }

   
    /*public void TryToShoot()
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
            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;

            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }

            PlayParticleSystems();
            AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);

            Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
            //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
            //Quaternion rotation = Quaternion.Euler(spreadAmount);
            //Model.transform.rotation *= rotation;

            //Vector3 shootDirection = ShootingStartPoint.transform.forward + spreadAmount;
            Vector3 shootDirection = Vector3.zero;

            if (ShootConfig.ShootType == ShootType.FromGun)
            {
                shootDirection = ShootingStartPoint.transform.forward + spreadAmount;
            }
            else
            {
                shootDirection = ActiveCamera.transform.forward +
                                 ActiveCamera.transform.TransformDirection(shootDirection);
            }

            AmmoConfig.CurrentClipAmmo--;
            
            Debug.DrawRay(GetRaycastOrigin(), shootDirection * 100f, Color.blue, 1f);

            if (ShootConfig.IsHitScan)
            {
                DoHitScanShoot(shootDirection);
            }
            else
            {
                DoProjectileShoot(shootDirection);
            }
        }
    }*/

     private void DoHitScanShoot(Vector3 shootDirection, Vector3 Origin, Vector3 TrailOrigin, int Iteration = 0)
    {
        if (Physics.Raycast(
                Origin,
                shootDirection,
                out RaycastHit hit,
                float.MaxValue,
                ShootConfig.HitMask
            ))
        {
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    TrailOrigin,
                    hit.point,
                    hit,
                    Iteration
                )
            );
        }
        else
        {
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    ShootingStartPoint.transform.position,
                    TrailOrigin + (shootDirection * TrailConfig.MissDistance),
                    new RaycastHit(),
                    Iteration
                )
            );
        }
    }
    
    private void DoProjectileShoot(Vector3 shootDirection)
    {
        Bullet bullet = BulletPool.Get();
        bullet.gameObject.SetActive(true);
        bullet.OnCollision += HandleOnBulletColision;

        if (ShootConfig.ShootType == ShootType.FromCamera
            && Physics.Raycast(
                GetRaycastOrigin(),
                shootDirection,
                out RaycastHit hit,
                float.MaxValue,
                ShootConfig.HitMask
            ))
        {
            Vector3 directionToHit = (hit.point - ShootingStartPoint.transform.position).normalized;
            Model.transform.forward = directionToHit;
            shootDirection = directionToHit;
        }

        bullet.transform.position = ShootingStartPoint.transform.position;
        bullet.Spawn(shootDirection * ShootConfig.BulletSpawnForce);

        TrailRenderer trail = TrailPool.Get();
        if (trail != null)
        {
            trail.transform.SetParent(bullet.transform, false);
            trail.transform.localPosition = Vector3.zero;
            trail.emitting = true;
            trail.gameObject.SetActive(true);
        }
    }

    public Vector3 GetGunForward()
    {
        return Model.transform.forward;
    }

    private void HandleOnBulletColision(Bullet bullet, Collision collision,int objectsPenetrated)
    {
        TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();
        
        if (collision != null && BulletPenetrationConfig != null &&
            BulletPenetrationConfig.MaxObjectsToPenetrate > objectsPenetrated)
        {
            Vector3 direction = -collision.impulse.normalized;
            ContactPoint contact = collision.GetContact(0);
            Vector3 backCastOrigin = contact.point + direction * BulletPenetrationConfig.MaxPenetrationDepth;
            
            if (Physics.Raycast(
                    backCastOrigin,
                    -direction,
                    out RaycastHit hit,
                    BulletPenetrationConfig.MaxPenetrationDepth,
                    ShootConfig.HitMask
                ))
            {
                direction += new Vector3(
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.x,BulletPenetrationConfig.AccuracyLoss.x),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.y,BulletPenetrationConfig.AccuracyLoss.y),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.z,BulletPenetrationConfig.AccuracyLoss.z)
                );
                bullet.transform.position = hit.point + direction * 0.01f;

                bullet.Rigidbody.velocity = (-collision.impulse / bullet.Rigidbody.mass) + direction;
            }
            else
            {
               DisableTrailAndBullet(trail, bullet);
            }
        }
        else
        {
            DisableTrailAndBullet(trail, bullet);
        }

        if (collision != null)
        {
            ContactPoint contactPoint = collision.GetContact(0);

            HandleBulletImpact(
                Vector3.Distance(contactPoint.point, bullet.SpawnLocation),
                contactPoint.point,
                contactPoint.normal,
                contactPoint.otherCollider,
                objectsPenetrated
            );
        }
    }

    private void DisableTrailAndBullet(TrailRenderer trail, Bullet bullet)
    {
        if (trail != null)
        {
            trail.transform.SetParent(null, true);
            ActiveMonoBehaviour.StartCoroutine(DeleyedDisableTrail(trail));
        }

        bullet.gameObject.SetActive(false);
        BulletPool.Release(bullet);
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
        Collider HitCollider,
        int ObjectsPenetrated = 0)
    {
        SurfaceManager.Instance.HandleImpact(
            HitCollider.gameObject,
            HitLocation,
            HitNormal,
            ImpactType,
            0
        );

        if (HitCollider.TryGetComponent(out IDamageable damageable))
        {
            float maxPercentDamage = 1;
            if (BulletPenetrationConfig != null && ObjectsPenetrated > 0)
            {
                for (int i = 0; i < ObjectsPenetrated; i++)
                {
                    maxPercentDamage *= BulletPenetrationConfig.DamageRetentionPercentage;
                }
            }
            
            damageable.TakeDamage(DamageConfig.GetDamage(DistanceTraveled, maxPercentDamage));
        }

        foreach (ICollisionHandler collisionHandler in BulletImpactEffects)
        {
            collisionHandler.HandleImpact(HitCollider, HitLocation, HitNormal, this);
        }
    }

   
    
    /*private void DoHitScanShoot(Vector3 shootDirection, Vector3 Origin, Vector3 TrailOrigin, int Iteration = 0)
    {
        if (Physics.Raycast(
                GetRaycastOrigin(),
                shootDirection,
                out RaycastHit hit,
                float.MaxValue,
                ShootConfig.HitMask
            ))
        {
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    ShootingStartPoint.transform.position,
                    hit.point,
                    hit
                )
            );
        }
        else
        {
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    ShootingStartPoint.transform.position,
                    ShootingStartPoint.transform.position + (shootDirection * TrailConfig.MissDistance),
                    new RaycastHit()
                )
            );
        }
    }*/

    public Vector3 GetRaycastOrigin()
    {
        Vector3 origin = ShootingStartPoint.transform.position;

        if (ShootConfig.ShootType == ShootType.FromCamera)
        {
            origin = ActiveCamera.transform.position
                     + ActiveCamera.transform.forward
                     * Vector3.Distance(
                         ActiveCamera.transform.position,
                         ShootingStartPoint.transform.position
                     );
        }

        return origin;
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
    
    private float currentChargeTime = 0.0f;
    private bool isCharging = false;
    
    void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0.0f;
    }
    
    void StopCharging()
    {
        isCharging = false;
    }
    
    
    public void Tick(bool WantsToShoot)
    {
        
        if (WantsToShoot)
        {
            if (ShootConfig.IsPreparedShot)
            {
                if (!isCharging)
                    StartCharging();
                else
                {
                    currentChargeTime += Time.deltaTime;
                    if (currentChargeTime >= ShootConfig.chargeTime)
                        TryToShoot();
                        
                    
                }
                
            }
            else
            {
                LastFrameWantedToShoot = true;
                TryToShoot();
            }
            
        }
        else if (!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
        }
        else
        {
            StopCharging();
        }
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, int Iteration = 0)
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
            HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider, Iteration);
        }

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false); 
        //TrailPool.Release(instance);
        Destroy(instance.gameObject);   

        if (BulletPenetrationConfig != null && BulletPenetrationConfig.MaxObjectsToPenetrate > Iteration)
        {
            yield return null;
            Vector3 direction = (EndPoint - StartPoint).normalized;
            Vector3 backCastOrigin = Hit.point + direction * BulletPenetrationConfig.MaxObjectsToPenetrate;

            if (Physics.Raycast(
                    backCastOrigin,
                    -direction,
                    out RaycastHit hit,
                    BulletPenetrationConfig.MaxPenetrationDepth,
                    ShootConfig.HitMask
                ))
            {
                Vector3 penetrationOrigin = hit.point;
                direction += new Vector3(
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.x,BulletPenetrationConfig.AccuracyLoss.x),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.y,BulletPenetrationConfig.AccuracyLoss.y),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.z,BulletPenetrationConfig.AccuracyLoss.z)
                );
                
                DoHitScanShoot(direction, penetrationOrigin, penetrationOrigin, Iteration + 1);
            }
        }
        
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

    public object Clone()
    {
        GunSO config = CreateInstance<GunSO>();
        config.ImpactType = ImpactType;
        config.Type = Type;
        config.Name = Name;
        config.name = name;
        
        config.DamageConfig = DamageConfig.Clone() as DamageConfigSO;
        config.ShootConfig = ShootConfig.Clone() as ShootConfigurationSO;
        config.TrailConfig = TrailConfig.Clone() as TrailConfigSO;
        config.AmmoConfig = AmmoConfig.Clone() as AmmoConfigSO;
        config.AudioConfig = AudioConfig.Clone() as AudioConfigSO;
        config.BulletPenetrationConfig = BulletPenetrationConfig.Clone() as BulletPenetrationConfigSO;

        config.ModelPrefab = ModelPrefab;
        config.SpawnPoint = SpawnPoint;
        config.SpawnRotation = SpawnRotation;

        return config;
    }
}