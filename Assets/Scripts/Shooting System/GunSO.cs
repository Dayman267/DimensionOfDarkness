using System;
using System.Collections;
using LlamAcademy.ImpactSystem;
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
    private Camera ActiveCamera;

    private MonoBehaviour ActiveMonoBehaviour;


    public ICollisionHandler[] BulletImpactEffects = Array.Empty<ICollisionHandler>();
    private ObjectPool<Bullet> BulletPool;
    private Transform BulletPoolParent;

    private float currentChargeTime;
    private float InitialClickTime;
    private bool isCharging;
    private bool LastFrameWantedToShoot;

    private float LastShootTime;
    private GameObject Model;
    private AudioSource ShootingAudioSource;
    private GameObject ShootingStartPoint;
    private float StopShootingTime;
    private ObjectPool<TrailRenderer> TrailPool;

    private GameObject TrailPoolParent;

    private ParticleSystem[] VFX_System;

    public object Clone()
    {
        var config = CreateInstance<GunSO>();
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

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Camera ActiveCamera = null)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        this.ActiveCamera = ActiveCamera;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        if (!ShootConfig.IsHitScan) BulletPool = new ObjectPool<Bullet>(CreateBullet);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        TrailPoolParent = GameObject.FindWithTag("TrailPool");
        BulletPoolParent = GameObject.FindWithTag("BulletsPool").transform;

        VFX_System = GameObject.FindWithTag("VFX_System").GetComponentsInChildren<ParticleSystem>();
        ShootingAudioSource = Model.GetComponent<AudioSource>();
        ShootingStartPoint = GameObject.FindWithTag("ShootingStartPoint");


        var vfxSystem = GameObject.FindWithTag("VFX_System");
        if (vfxSystem != null)
            ShootingStartPoint.transform.position = vfxSystem.transform.position;
        else
            Debug.LogError("Object with tag 'VFX_System' not found!");
    }

    public void Despawn()
    {
        Model.SetActive(false);
        Destroy(Model);
        TrailPool.Clear();
        if (BulletPool != null)
            BulletPool.Clear();

        ShootingAudioSource = null;
        VFX_System = null;
        ShootingStartPoint = null;
    }

    public void UpdateCamera(Camera ActiveCamera)
    {
        this.ActiveCamera = ActiveCamera;
    }

    public void PlayParticleSystems()
    {
        foreach (var ps in VFX_System) ps.Play();
    }

    public void TryToShoot()
    {
        if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            var lastDuration = Mathf.Clamp(
                0,
                StopShootingTime - InitialClickTime,
                ShootConfig.MaxSpreadTime
            );
            var lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                           / ShootConfig.RecoilRecoverySpeed;
            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;

            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                if (!ShootingAudioSource.isPlaying)
                    AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }

            PlayParticleSystems();
            AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);
            Crosshair.OnShotFired();
            AmmoConfig.CurrentClipAmmo--;

            for (var i = 0; i < ShootConfig.BulletPerShot; i++)
            {
                var spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
                //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
                //Quaternion rotation = Quaternion.Euler(spreadAmount);
                //Model.transform.rotation *= rotation;

                var ray = ActiveCamera.ScreenPointToRay(Mouse.current.position.value);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ShootConfig.HitMask))
                {
                    var shootDirection = hit.point - ShootingStartPoint.transform.position +
                                         Model.transform.TransformDirection(spreadAmount);
                    if (ShootConfig.IsHitScan)
                        DoHitScanShoot(shootDirection, GetRaycastOrigin(), ShootingStartPoint.transform.position);
                    else
                        DoProjectileShoot(shootDirection.normalized);
                }
                else
                {
                    var shootDirection = ray.direction;
                    if (ShootConfig.IsHitScan)
                        DoHitScanShoot(shootDirection, GetRaycastOrigin(), ShootingStartPoint.transform.position);
                    else
                        DoProjectileShoot(shootDirection.normalized);
                }
            }
        }
    }

    private void DoHitScanShoot(Vector3 shootDirection, Vector3 Origin, Vector3 TrailOrigin, int Iteration = 0)
    {
        if (Physics.Raycast(
                Origin,
                shootDirection,
                out var hit,
                float.MaxValue,
                ShootConfig.HitMask
            ))
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    TrailOrigin,
                    hit.point,
                    hit,
                    Iteration
                )
            );
        else
            ActiveMonoBehaviour.StartCoroutine(
                PlayTrail(
                    ShootingStartPoint.transform.position,
                    TrailOrigin + shootDirection * TrailConfig.MissDistance,
                    new RaycastHit(),
                    Iteration
                )
            );
    }

    private void DoProjectileShoot(Vector3 shootDirection)
    {
        var bullet = BulletPool.Get();
        bullet.gameObject.SetActive(true);
        bullet.OnCollision += HandleOnBulletColision;

        if (ShootConfig.ShootType == ShootType.FromCamera
            && Physics.Raycast(
                GetRaycastOrigin(),
                shootDirection,
                out var hit,
                float.MaxValue,
                ShootConfig.HitMask
            ))
        {
            var directionToHit = (hit.point - ShootingStartPoint.transform.position).normalized;
            Model.transform.forward = directionToHit;
            shootDirection = directionToHit;
        }

        bullet.transform.position = ShootingStartPoint.transform.position;
        bullet.Spawn(shootDirection * ShootConfig.BulletSpawnForce);

        var trail = TrailPool.Get();
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

    private void HandleOnBulletColision(Bullet bullet, Collision collision, int objectsPenetrated)
    {
        var trail = bullet.GetComponentInChildren<TrailRenderer>();

        if (collision != null && BulletPenetrationConfig != null &&
            BulletPenetrationConfig.MaxObjectsToPenetrate > objectsPenetrated)
        {
            var direction = -collision.impulse.normalized;
            var contact = collision.GetContact(0);
            var backCastOrigin = contact.point + direction * BulletPenetrationConfig.MaxPenetrationDepth;

            if (Physics.Raycast(
                    backCastOrigin,
                    -direction,
                    out var hit,
                    BulletPenetrationConfig.MaxPenetrationDepth,
                    ShootConfig.HitMask
                ))
            {
                direction += new Vector3(
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.x, BulletPenetrationConfig.AccuracyLoss.x),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.y, BulletPenetrationConfig.AccuracyLoss.y),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.z, BulletPenetrationConfig.AccuracyLoss.z)
                );
                bullet.transform.position = hit.point + direction * 0.01f;

                bullet.Rigidbody.velocity = -collision.impulse / bullet.Rigidbody.mass + direction;
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
            var contactPoint = collision.GetContact(0);

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
                for (var i = 0; i < ObjectsPenetrated; i++)
                    maxPercentDamage *= BulletPenetrationConfig.DamageRetentionPercentage;

            damageable.TakeDamage(DamageConfig.GetDamage(DistanceTraveled, maxPercentDamage));
        }

        foreach (var collisionHandler in BulletImpactEffects)
            collisionHandler.HandleImpact(HitCollider, HitLocation, HitNormal, this);
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
        var origin = ShootingStartPoint.transform.position;

        if (ShootConfig.ShootType == ShootType.FromCamera)
            origin = ActiveCamera.transform.position
                     + ActiveCamera.transform.forward
                     * Vector3.Distance(
                         ActiveCamera.transform.position,
                         ShootingStartPoint.transform.position
                     );

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

    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0.0f;
    }

    private void StopCharging()
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
                {
                    StartCharging();
                }
                else
                {
                    currentChargeTime += Time.deltaTime;
                    if (currentChargeTime >= ShootConfig.chargeTime) TryToShoot();
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

    /*private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, int Iteration = 0)
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
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.x, BulletPenetrationConfig.AccuracyLoss.x),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.y, BulletPenetrationConfig.AccuracyLoss.y),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.z, BulletPenetrationConfig.AccuracyLoss.z)
                );

                DoHitScanShoot(direction, penetrationOrigin, penetrationOrigin, Iteration + 1);
            }
        }
    }*/

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, int Iteration = 0)
    {
        var instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.localPosition = StartPoint;
        yield return null; // avoid position carry-over from last frame if reused

        instance.emitting = true;

        var distance = Vector3.Distance(StartPoint, EndPoint);
        var remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.localPosition = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - remainingDistance / distance)
            );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.localPosition = EndPoint;

        if (Hit.collider != null) HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider, Iteration);

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);

        if (BulletPenetrationConfig != null && BulletPenetrationConfig.MaxObjectsToPenetrate > Iteration)
        {
            yield return null;
            var direction = (EndPoint - StartPoint).normalized;
            var backCastOrigin = Hit.point + direction * BulletPenetrationConfig.MaxPenetrationDepth;

            if (Physics.Raycast(
                    backCastOrigin,
                    -direction,
                    out var hit,
                    BulletPenetrationConfig.MaxPenetrationDepth,
                    ShootConfig.HitMask
                ))
            {
                var penetrationOrigin = hit.point;
                direction += new Vector3(
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.x, BulletPenetrationConfig.AccuracyLoss.x),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.y, BulletPenetrationConfig.AccuracyLoss.y),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.z, BulletPenetrationConfig.AccuracyLoss.z)
                );

                DoHitScanShoot(direction, penetrationOrigin, penetrationOrigin, Iteration + 1);
            }
        }
    }

    private TrailRenderer CreateTrail()
    {
        var instance = new GameObject("Bullet Trail");
        var trail = instance.AddComponent<TrailRenderer>();
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
        return Instantiate(ShootConfig.BulletPrefab, BulletPoolParent);
    }
}