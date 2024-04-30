using System;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    private Camera Camera;
    [SerializeField] private GunType GunType;
    [SerializeField] private Transform GunParent;

    [SerializeField] private List<GunSO> Guns;

    private List<int> runtimeCurrentAmmoClips;
    private int ActiveGunPosition;
    private int NewActiveGunPosition;

    public static event Action OnGunChanged;
    // video 1
    //  [SerializeField] private PlayerIK InversKinematics;

    [Space] [Header("Runtime Filled")] public GunSO ActiveGun;
    [SerializeField] private GunSO ActiveBaseGun;

    private void Awake()
    {
        GunSO gun = Guns[0];
        Camera = Camera.main;

        runtimeCurrentAmmoClips = new List<int>(Guns.Count);
        
        for (int i = 0; i < Guns.Count; i++)
        {
            runtimeCurrentAmmoClips.Add(Guns[i].AmmoConfig.CurrentClipAmmo);
        }

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


    private bool isQKeyDown = false;
    private bool isEKeyDown = false;

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

    public void DespawnActiveGun()
    {
        runtimeCurrentAmmoClips[ActiveGunPosition] = ActiveGun.AmmoConfig.CurrentClipAmmo;
        ActiveGun.Despawn();
        Destroy(ActiveGun);
    }

    private void SwitchWeapon(int switchDirection)
    {
        GunSO NewActiveGun;
        ActiveGunPosition = Guns.FindIndex(gun => gun.Name == ActiveGun.Name);

        if (ActiveGunPosition + switchDirection < 0)
        {
            NewActiveGun = Guns[Guns.Count - 1];
            NewActiveGunPosition = Guns.Count - 1;
        }
        else if (ActiveGunPosition + switchDirection >= Guns.Count)
        {
            NewActiveGun = Guns[0];
            NewActiveGunPosition = 0;
        }
        else
        {
            NewActiveGun = Guns[ActiveGunPosition + switchDirection];
            NewActiveGunPosition = ActiveGunPosition + switchDirection;
        }

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

        foreach (IModifier modifier in Modifiers)
        {
            modifier.Apply(ActiveGun);
        }
    }

    private void SetupGun(GunSO gun)
    {
        ActiveBaseGun = gun;
        ActiveGun = gun.Clone() as GunSO;
        if (ActiveGun != null)
        {
            ActiveGun.Spawn(GunParent, this, Camera);
            ActiveGun.AmmoConfig.CurrentClipAmmo = runtimeCurrentAmmoClips[NewActiveGunPosition];
        }

        
        
    }
}