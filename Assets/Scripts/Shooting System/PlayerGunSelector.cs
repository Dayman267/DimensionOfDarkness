using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField] private GunType Gun;
    [SerializeField] private Transform GunParent;

    [SerializeField] private List<GunSO> Guns;
    // video 1
    //  [SerializeField] private PlayerIK InversKinematics;

    [Space] [Header("Runtime Filled")] public GunSO ActiveGun;

    private void Start()
    {
        GunSO gun = Guns.Find(gun => gun.Type == Gun);

        if (gun == null)
        {
            Debug.LogError($"No GunSO found for GunType: {gun}");
        }

        ActiveGun = gun;
        gun.Spawn(GunParent,this);
        
        //Some magic for IK
        // video 1 - 14.18
        /*Transform[] allChildren = GunParent.GetComponentsInChildren<Transform>();
        InversKinematics.LeftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        InversKinematics.RightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        InversKinematics.LeftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
        InversKinematics.RightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");*/
    }
}