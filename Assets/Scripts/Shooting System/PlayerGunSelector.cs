using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    public Camera Camera;
    [SerializeField] private GunType GunType;
    [SerializeField] private Transform GunParent;

    [SerializeField] private List<GunSO> Guns;
    // video 1
    //  [SerializeField] private PlayerIK InversKinematics;

    [Space] [Header("Runtime Filled")] public GunSO ActiveGun;
    [SerializeField] private GunSO ActiveBaseGun;

    private void Awake()
    {
        GunSO gun = Guns[0];

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
            SwapGun(-1);
            isQKeyDown = true;
        }
        else if (!PlayerController.IsQKeyDown() && isQKeyDown)
        {
            isQKeyDown = false;
        }

        if (PlayerController.IsEKeyDown() && !isEKeyDown)
        {
            SwapGun(1);
            isEKeyDown = true;
        }
        else if (!PlayerController.IsEKeyDown() && isEKeyDown)
        {
            isEKeyDown = false;
        }
    }

    public void DespawnActiveGun()
    {
        ActiveGun.Despawn();
        Destroy(ActiveGun);
    }

    private void SwapGun(int swapDirection)
    {
        GunSO NewActiveGun;
        int ActiveGunPosition = Guns.FindIndex(gun => gun.Type == ActiveGun.Type);

        if (ActiveGunPosition + swapDirection < 0)
            NewActiveGun = Guns[Guns.Count - 1];
        else if (ActiveGunPosition + swapDirection >= Guns.Count)
            NewActiveGun = Guns[0];
        else
            NewActiveGun = Guns[ActiveGunPosition + swapDirection];

        Debug.Log(NewActiveGun.Name);
        //Debug.Log($"Swap Direction: {swapDirection}");
        //Debug.Log( $"Active Gun Pos: {ActiveGunPosition}");
        //Debug.Log($"Sum: { ActiveGunPosition + swapDirection}");
        DespawnActiveGun();
        SetupGun(NewActiveGun);
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
            ActiveGun.Spawn(GunParent, this, Camera);
    }

}