using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 3)]
public class AmmoConfigSO : ScriptableObject, ICloneable
{
    //public int MaxAmmo = 120;
    public int ClipSize = 30;

    //public int CurrentAmmo = 120;
    public int CurrentClipAmmo = 30;

    public bool SingleBulletLoad;

    public object Clone()
    {
        var config = CreateInstance<AmmoConfigSO>();

        Utilities.CopyValues(this, config);

        return config;
    }


    public void Reload()
    {
        var availableBulletsInCurrentClip = ClipSize - CurrentClipAmmo;
        if (SingleBulletLoad)
        {
            CurrentClipAmmo++;
        }
        else
        {
            //int maxReloadAmount = Mathf.Min(ClipSize, CurrentAmmo);
            //int reloadAmount = Mathf.Min(maxReloadAmount,availableBulletsInCurrentClip);
            var reloadAmount = Mathf.Min(ClipSize, availableBulletsInCurrentClip);

            CurrentClipAmmo += reloadAmount;
            //CurrentAmmo -= reloadAmount;
        }
    }


    public bool CanReload()
    {
        //return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
        return CurrentClipAmmo < ClipSize;
    }
}