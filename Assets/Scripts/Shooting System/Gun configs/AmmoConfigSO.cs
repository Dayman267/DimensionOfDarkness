using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 3)]
public class AmmoConfigSO : ScriptableObject,ICloneable
{
    //public int MaxAmmo = 120;
    public int ClipSize = 30;

    //public int CurrentAmmo = 120;
    public int CurrentClipAmmo = 30;

    public bool SingleBulletLoad = false;
    

    public void Reload()
    {
        int availableBulletsInCurrentClip = ClipSize - CurrentClipAmmo;
        if (SingleBulletLoad)
        {
            CurrentClipAmmo++;
        }
        else
        {
            //int maxReloadAmount = Mathf.Min(ClipSize, CurrentAmmo);
            //int reloadAmount = Mathf.Min(maxReloadAmount,availableBulletsInCurrentClip);
            int reloadAmount = Mathf.Min(ClipSize,availableBulletsInCurrentClip);

            CurrentClipAmmo += reloadAmount;
            //CurrentAmmo -= reloadAmount;
        }
    }
    

    public bool CanReload()
    {
        //return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
        return CurrentClipAmmo < ClipSize;
    }

    public object Clone()
    {
        AmmoConfigSO config = CreateInstance<AmmoConfigSO>();
        
        Utilities.CopyValues(this, config);

        return config;
    }
}
