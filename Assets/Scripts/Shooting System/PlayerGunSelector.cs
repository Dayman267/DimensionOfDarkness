using System;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    private Camera Camera;
    [SerializeField] private GunType GunType;
    
    [SerializeField] private Transform GunParent;
    [SerializeField] private Transform BulletPoolParent;
    [SerializeField] private Transform BulletCasesPoolParent;
    [SerializeField] private Transform ShootingStartPoint;
  

    [SerializeField] private List<GunSO> Guns;

    private List<int> runtimeCurrentAmmoClips;
    private int ActiveGunPosition;
    private int NewActiveGunPosition;
    
    [Space] [Header("Runtime Filled")] public GunSO ActiveGun;
    [SerializeField] private GunSO ActiveBaseGun;

    public static event Action OnGunChanged;
    // video 1
    //  [SerializeField] private PlayerIK InversKinematics;

    

    public static event Action OnSwitchWeapon;

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
        if ((PlayerController.GetPlayerMoveState() != PlayerMoveStates.dashing && PlayerController.IsPlayerHasIdleState()) 
            || PlayerController.GetPlayerState() == PlayerStates.switchingWeapon)
        {
            if (PlayerController.IsQKeyDown() && !isQKeyDown)
            {
                InvokeSwitchWeaponAnimation();
                SwitchWeapon(-1);
                isQKeyDown = true;
            }
            else if (!PlayerController.IsQKeyDown() && isQKeyDown)
            {
                isQKeyDown = false;
            }


            if (PlayerController.IsEKeyDown() && !isEKeyDown)
            {
                InvokeSwitchWeaponAnimation();
                SwitchWeapon(1);
                isEKeyDown = true;
            }
            else if (!PlayerController.IsEKeyDown() && isEKeyDown)
            {
                isEKeyDown = false;
            }
        }
    }

    private void InvokeSwitchWeaponAnimation()
    {
        if (PlayerController.IsPlayerHasIdleState())
            OnSwitchWeapon?.Invoke();
    }

    public void DespawnActiveGun()
    {
        runtimeCurrentAmmoClips[ActiveGunPosition] = ActiveGun.AmmoConfig.CurrentClipAmmo;
        ActiveGun.Despawn();
        Destroy(ActiveGun);
    }

    private void SwitchWeapon(int switchDirection)
    {
        PlayerController.SetPlayerState(PlayerStates.switchingWeapon);
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

    /// <summary>
    ///  SwitchingEnd is switch animation method
    /// </summary>
    public void SwitchingEnd()
    {
        PlayerController.SetPlayerState(PlayerStates.idle);
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
            ActiveGun.Spawn(GunParent, this,BulletPoolParent,BulletCasesPoolParent,ShootingStartPoint, Camera);
            ActiveGun.AmmoConfig.CurrentClipAmmo = runtimeCurrentAmmoClips[NewActiveGunPosition];
        }
    }
}