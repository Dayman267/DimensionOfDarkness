using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Config", menuName = "Guns/Ammo Config", order = 3)]
public class AmmoConfigSO : ScriptableObject
{
    //public int MaxAmmo = 120;
    public int ClipSize = 30;

    //public int CurrentAmmo = 120;
    public int CurrentClipAmmo = 30;

    public void Reload()
    {
        //int maxReloadAmount = Mathf.Min(ClipSize, CurrentAmmo);
        int availableBulletsInCurrentClip = ClipSize - CurrentClipAmmo;
        //int reloadAmount = Mathf.Min(maxReloadAmount,availableBulletsInCurrentClip);
        int reloadAmount = Mathf.Min(ClipSize,availableBulletsInCurrentClip);

        CurrentClipAmmo += reloadAmount;
        //CurrentAmmo -= reloadAmount;
    }

    public bool CanReload()
    {
        //return CurrentClipAmmo < ClipSize && CurrentAmmo > 0;
        return CurrentClipAmmo < ClipSize;
    }
}
