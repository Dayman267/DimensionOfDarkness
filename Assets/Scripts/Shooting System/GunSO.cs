using System;
using System.Collections;
using LlamAcademy.ImpactSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunSO : ScriptableObject, ICloneable
{
    public ImpactType ImpactType;
    public GunType Type;
    public string Name;

    public GameObject ModelPrefab;

    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public bool AutoShootAnimationEnable = false;

    public DamageConfigSO DamageConfig;
    public ShootConfigurationSO ShootConfig;
    public TrailConfigSO TrailConfig;
    public AmmoConfigSO AmmoConfig;
    public AudioConfigSO AudioConfig;
    public BulletPenetrationConfigSO BulletPenetrationConfig;
    
    public ICollisionHandler[] BulletImpactEffects = Array.Empty<ICollisionHandler>();

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private AudioSource GunAudioSource;
    private AudioSource ChargingAudioSource;
    private Camera ActiveCamera;
    private Animator animator;
    public AnimatorOverrideController animatorOverrideController;

    private float LastShootTime;
    private float InitialClickTime;
    private float StopShootingTime;
    private bool LastFrameWantedToShoot;

    private GameObject TrailPoolParent;
    private Transform BulletPoolParent;
    private Transform BulletCasesParent;

    private ParticleSystem[] Shoot_VFX;
    private ParticleSystem[] ChargeShoot_VFX;
    private Transform ShootingStartPoint;
    private ObjectPool<TrailRenderer> TrailPool;
    private ObjectPool<Bullet> BulletPool;
    private BulletCaseSpawner bulletCaseSpawner;

    public static event Action OnAutoShootAnimationEnable;
    public static event Action OnAutoShootAnimationDiasble;
    public static event Action OnSingleShootAnimationEnable;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour, Transform BulletPoolParent,
        Transform BulletCasesParent, Transform ShootingStartPoint, Camera ActiveCamera = null)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        this.ActiveCamera = ActiveCamera;
        this.BulletPoolParent = BulletPoolParent;
        this.BulletCasesParent = BulletCasesParent;
        this.ShootingStartPoint = ShootingStartPoint;

        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        if (!ShootConfig.IsHitScan)
        {
            BulletPool = new ObjectPool<Bullet>(CreateBullet);
        }

        Model = Instantiate(ModelPrefab, Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        GunAudioSource = Model.GetComponent<AudioSource>();
        bulletCaseSpawner = Model.GetComponentInChildren<BulletCaseSpawner>();
        Transform shootFXSystem = Model.GetComponentInChildren<Shooting_VFX_System_Mark>().transform;
        Shoot_VFX = shootFXSystem.GetComponentsInChildren<ParticleSystem>();
        animator = Model.GetComponentInParent<Animator>();

        animator.runtimeAnimatorController = animatorOverrideController;

        if (DamageConfig.IsChargedShot)
        {
            Transform chargingFXSystem = Model.GetComponentInChildren<Charging_VFX_System_Mark>().transform;
            ChargeShoot_VFX = chargingFXSystem.GetComponentsInChildren<ParticleSystem>();
            ChargingAudioSource = chargingFXSystem.GetComponent<AudioSource>();
        }

        if (shootFXSystem != null)
        {
            ShootingStartPoint.position = shootFXSystem.transform.position;
        }
        else
        {
            Debug.LogError("Object with tag 'VFX_System' not found!");
        }
    }

    public void Despawn()
    {
        Model.SetActive(false);
        Destroy(Model);
        TrailPool.Clear();
        if (BulletPool != null)
            BulletPool.Clear();

        GunAudioSource = null;
        Shoot_VFX = null;
        ChargeShoot_VFX = null;
        ShootingStartPoint = null;
        animatorOverrideController = null;
    }

    private void PlayParticleSystems(ParticleSystem[] particleSystem)
    {
        foreach (ParticleSystem ps in particleSystem)
        {
            ps.Play();
        }
    }

    private bool IsEmptyClipCheck()
    {
        if (AmmoConfig.CurrentClipAmmo == 0)
        {
            if (!GunAudioSource.isPlaying)
                AudioConfig.PlayOutOfAmmoClip(GunAudioSource);
            return true;
        }

        return false;
    }


    private bool isClipEmpty = false;

    private void TryToShoot()
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

            isClipEmpty = IsEmptyClipCheck();

            if (isClipEmpty)
            {
                OnAutoShootAnimationDiasble?.Invoke();
                return;
            }

            if (PlayerController.IsPlayerHasIdleState() && !isClipEmpty)
            {
                if (AutoShootAnimationEnable)
                    OnAutoShootAnimationEnable?.Invoke();
                else
                    OnSingleShootAnimationEnable?.Invoke();
            }

            PlayParticleSystems(Shoot_VFX);
            AudioConfig.PlayShootingClip(GunAudioSource, AmmoConfig.CurrentClipAmmo == 1);
            Crosshair.OnShotFired();
            AmmoConfig.CurrentClipAmmo--;

            for (int i = 0; i < ShootConfig.BulletPerShot; i++)
            {
                Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
                //Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
                //Quaternion rotation = Quaternion.Euler(spreadAmount);
                //Model.transform.rotation *= rotation;

                Ray ray = ActiveCamera.ScreenPointToRay(Mouse.current.position.value);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ShootConfig.HitMask))
                {
                    Vector3 shootDirection = (hit.point - ShootingStartPoint.transform.position) +
                                             Model.transform.TransformDirection(spreadAmount);
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

            if (bulletCaseSpawner != null)
                bulletCaseSpawner.SpawnBullet(BulletCasesParent);
        }
    }

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
                    TrailOrigin,
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
    bullet.OnCollision += HandleOnBulletCollision;

    if (ShootConfig.ShootType == ShootType.FromCamera &&
        Physics.Raycast(
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
        Transform transform;
        (transform = trail.transform).SetParent(bullet.transform, false);
        transform.localPosition = Vector3.zero;
        trail.emitting = true;
        trail.gameObject.SetActive(true);
    }
    
    if(TrailConfig.BulletParticlePrefab != null)
    {
        GameObject particleInstance = Instantiate(TrailConfig.BulletParticlePrefab, bullet.transform.position, Quaternion.identity);
        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
        particleSystem.Play();
        
        ActiveMonoBehaviour.StartCoroutine(UpdateParticleSystem(particleSystem, bullet, trail));
    }
}

private IEnumerator UpdateParticleSystem(ParticleSystem particleSystem, Bullet bullet, TrailRenderer trail)
{
    while (bullet.gameObject.activeSelf)
    {
        particleSystem.transform.position = bullet.transform.position;
        yield return null;
    }

    // Останавливаем и уничтожаем Particle System
    particleSystem.Stop();
    Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);

    if (trail != null)
    {
        trail.transform.SetParent(null, true);
        ActiveMonoBehaviour.StartCoroutine(DelayedDisableTrail(trail));
    }
}

    private void HandleOnBulletCollision(Bullet bullet, Collision collision, int objectsPenetrated)
    {
        TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();

        if (collision != null && BulletPenetrationConfig != null &&
            BulletPenetrationConfig.MaxObjectsToPenetrate > objectsPenetrated)
        {
            Vector3 direction = (bullet.transform.position - bullet.SpawnLocation).normalized;
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
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.x, BulletPenetrationConfig.AccuracyLoss.x),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.y, BulletPenetrationConfig.AccuracyLoss.y),
                    Random.Range(-BulletPenetrationConfig.AccuracyLoss.z, BulletPenetrationConfig.AccuracyLoss.z)
                );
                bullet.transform.position = hit.point + direction * 0.01f;

                bullet.Rigidbody.velocity = bullet.SpawnVelocity - direction;
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
            ActiveMonoBehaviour.StartCoroutine(DelayedDisableTrail(trail));
        }

        bullet.gameObject.SetActive(false);
        BulletPool.Release(bullet);
    }

    private IEnumerator DelayedDisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        trail.emitting = false;
        GameObject gameObject;
        (gameObject = trail.gameObject).SetActive(false);
        //TrailPool.Release(trail);
        Destroy(gameObject);
    }
    
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

            damageable.TakeDamage(DamageConfig.GetDamage(DistanceTraveled, maxPercentDamage, chargedDamageMultiplier));
        }

        foreach (ICollisionHandler collisionHandler in BulletImpactEffects)
        {
            collisionHandler.HandleImpact(HitCollider, HitLocation, HitNormal, this);
        }
    }
    
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
        AudioConfig.PlayReloadClip(GunAudioSource);
    }

    private float currentChargeTime = 0.0f;
    private bool isCharging = false;
    private float chargingDamageMultiplier = 1.0f;
    private float chargedDamageMultiplier = 1.0f;

    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0.0f;
        chargingDamageMultiplier = 1.0f;
        if (ChargeShoot_VFX != null && !IsEmptyClipCheck())
        {
            AudioConfig.PlayChargingShotClip(ChargingAudioSource);
            PlayParticleSystems(ChargeShoot_VFX);
        }
    }

    private void StopCharging()
    {
        currentChargeTime = 0;
        isCharging = false;
        if (ChargeShoot_VFX != null)
            ChargingAudioSource.Stop();
    }

    private void ChargedShoot()
    {
        chargedDamageMultiplier = chargingDamageMultiplier;
        TryToShoot();
        if (DamageConfig.IsChargedShot) StopCharging();
    }


    public void CallTick(bool WantsToShoot)
    {
        Tick(WantsToShoot);
    }

    private void Tick(bool WantsToShoot)
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

                    if (DamageConfig.IsChargedShot)
                        chargingDamageMultiplier = Mathf.Lerp(1.0f, DamageConfig.maxChargedDamageMultiplier,
                            currentChargeTime / ShootConfig.chargeTime);

                    if (currentChargeTime >= ShootConfig.chargeTime)
                    {
                        ChargedShoot();
                    }
                }
            }
            else
            {
                LastFrameWantedToShoot = true;
                TryToShoot();
            }
        }
        else if (!WantsToShoot && DamageConfig.IsChargedShot && currentChargeTime > ShootConfig.chargeTime * 0.3)
        {
            ChargedShoot();
            OnAutoShootAnimationDiasble?.Invoke();
        }
        else if (!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
            OnAutoShootAnimationDiasble?.Invoke();
        }
        else
        {
            StopCharging();
            OnAutoShootAnimationDiasble?.Invoke();
        }
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, int Iteration = 0)
{
    TrailRenderer instance = CreateTrail();
    instance.gameObject.SetActive(true);
    instance.transform.position = StartPoint;

    // Создаем экземпляр Particle System, если ParticlePrefab не равен null
    GameObject particleInstance = null;
    ParticleSystem particleSystem = null;
    if (TrailConfig.BulletParticlePrefab != null)
    {
        particleInstance = Instantiate(TrailConfig.BulletParticlePrefab, StartPoint, Quaternion.identity);
        particleSystem = particleInstance.GetComponent<ParticleSystem>();
        particleSystem.Play();
    }

    yield return null; // avoid position carry-over from last frame if reused

    instance.emitting = true;
    float distance = Vector3.Distance(StartPoint, EndPoint);
    float remainingDistance = distance;
    while (remainingDistance > 0)
    {
        Vector3 currentPosition = Vector3.Lerp(
            StartPoint,
            EndPoint,
            Mathf.Clamp01(1 - (remainingDistance / distance))
        );

        instance.transform.position = currentPosition;
        if (particleInstance != null)
            particleInstance.transform.position = currentPosition;

        remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;
        yield return null;
    }

    if (particleInstance != null)
    {
        // Обновляем позицию Particle System
        instance.transform.position = EndPoint;
        particleInstance.transform.position = EndPoint;
    }

    if (Hit.collider != null)
    {
        HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider, Iteration);
    }

    yield return new WaitForSeconds(TrailConfig.Duration);
    yield return null;

    instance.emitting = false;
    instance.gameObject.SetActive(false);
    Destroy(instance.gameObject);

    // Останавливаем и уничтожаем Particle System, если ParticlePrefab не равен null
    if (particleInstance != null)
    {
        particleSystem.Stop();
        Destroy(particleInstance, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
    }

    if (BulletPenetrationConfig != null && BulletPenetrationConfig.MaxObjectsToPenetrate > Iteration)
    {
        yield return null;
        Vector3 direction = (EndPoint - StartPoint).normalized;
        Vector3 backCastOrigin = Hit.point + direction * BulletPenetrationConfig.MaxPenetrationDepth;

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
        return Instantiate(ShootConfig.BulletPrefab, BulletPoolParent);
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

        config.animatorOverrideController = animatorOverrideController;
        config.AutoShootAnimationEnable = AutoShootAnimationEnable;

        return config;
    }
}