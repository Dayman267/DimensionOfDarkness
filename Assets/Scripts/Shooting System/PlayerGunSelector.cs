using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField] private GunType GunType;
    [SerializeField] private Transform GunParent;

    [SerializeField] private List<GunSO> Guns;
    // video 1
    //  [SerializeField] private PlayerIK InversKinematics;

    [Space] [Header("Runtime Filled")] public GunSO ActiveGun;
    [SerializeField] private GunSO ActiveBaseGun;
    private Camera Camera;
    private bool isEKeyDown;


    private bool isQKeyDown;

    private void Awake()
    {
        var gun = Guns[0];
        Camera = Camera.main;

        if (gun == null)
        {
            Debug.LogError($"No GunSO found for GunType: {gun}");
            return;
        }

        SetupGun(gun);

        //Some magic for IK
        // video 1 - 14.18
        /*Transform[] allChildren = GunParent.GetComponentsInChildren<Transform>();
        InversKinematics.LeftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        InversKinematics.RightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        InversKinematics.LeftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
        InversKinematics.RightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");*/
    }

    private void Update()
    {
        if (PlayerController.IsQKeyDown() && !isQKeyDown)
        {
            SwitchWeapon(-1);
            isQKeyDown = true;
        }
        else if (!PlayerController.IsQKeyDown() && isQKeyDown)
        {
            isQKeyDown = false;
        }

        if (PlayerController.IsEKeyDown() && !isEKeyDown)
        {
            SwitchWeapon(1);
            isEKeyDown = true;
        }
        else if (!PlayerController.IsEKeyDown() && isEKeyDown)
        {
            isEKeyDown = false;
        }
    }

    public static event Action OnGunChanged;

    public void DespawnActiveGun()
    {
        ActiveGun.Despawn();
        Destroy(ActiveGun);
    }

    private void SwitchWeapon(int switchDirection)
    {
        GunSO NewActiveGun;
        var ActiveGunPosition = Guns.FindIndex(gun => gun.Name == ActiveGun.Name);

        if (ActiveGunPosition + switchDirection < 0)
            NewActiveGun = Guns[Guns.Count - 1];
        else if (ActiveGunPosition + switchDirection >= Guns.Count)
            NewActiveGun = Guns[0];
        else
            NewActiveGun = Guns[ActiveGunPosition + switchDirection];

        DespawnActiveGun();
        SetupGun(NewActiveGun);

        OnGunChanged?.Invoke();
    }

    public void PickupGun(GunSO gun)
    {
        DespawnActiveGun();
        SetupGun(gun);
    }

    public void ApplyModifiers(IModifier[] Modifiers)
    {
        DespawnActiveGun();
        SetupGun(ActiveBaseGun);

        foreach (var modifier in Modifiers) modifier.Apply(ActiveGun);
    }

    private void SetupGun(GunSO gun)
    {
        ActiveBaseGun = gun;
        ActiveGun = gun.Clone() as GunSO;
        if (ActiveGun != null)
            ActiveGun.Spawn(GunParent, this, Camera);
    }
}