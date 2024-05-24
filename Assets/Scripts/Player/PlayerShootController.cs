using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShootController : MonoBehaviour
{
    private static bool isReloading;
    public PlayerGunSelector GunSelector;
    [SerializeField] private bool AutoReload;
    private bool isStopReloading;

    private void Update()
    {
        if (GunSelector.ActiveGun != null && !isReloading)
            GunSelector.ActiveGun.Tick(PlayerController.IsLeftClickDown());

        if (PlayerController.IsLeftClickDown() && isReloading) isStopReloading = true;

        if (ShouldManualReload() || ShouldAutoReload()) Reload();
    }

    public static event Action OnReloadAnimation;

    private void Reload()
    {
        GunSelector.ActiveGun.StartReloading();
        isReloading = true;
        OnReloadAnimation?.Invoke();
    }

    public static bool IsReloading()
    {
        return isReloading;
    }

    private void EndReload()
    {
        if (isStopReloading)
        {
            isStopReloading = false;
            isReloading = false;
            return;
        }

        GunSelector.ActiveGun.EndReload();
        isReloading = false;
        if (GunSelector.ActiveGun.CanReload()) Reload();
    }


    private bool ShouldManualReload()
    {
        return !isReloading && PlayerController.IsRKeyDown() && GunSelector.ActiveGun.CanReload();
    }

    private bool ShouldAutoReload()
    {
        return !isReloading
               && AutoReload
               && GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo == 0
               && GunSelector.ActiveGun.CanReload();
    }

    /*private void UpdateCrossHair()
   {
       if (GunSelector.ActiveGun.ShootConfig.ShootType == ShootType.FromGun)
       {
           Vector3 gunTipPoint = GunSelector.ActiveGun.GetRaycastOrigin();
           Vector3 gunForward = GunSelector.ActiveGun.GetGunForward();
           Vector3 hitPoint = gunTipPoint + gunForward * 10;

           if (Physics.Raycast(
                   gunTipPoint,
                   gunForward,
                   out RaycastHit hit,
                   float.MaxValue,
                   GunSelector.ActiveGun.ShootConfig.HitMask
               ))
           {
               hitPoint = hit.point;
           }

           Vector3 screenSpaceLocation = GunSelector.Camera.WorldToScreenPoint(hitPoint);
           if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                   (RectTransform)Crosshair.transform.parent,
                   screenSpaceLocation,
                   null,
                   out Vector2 localPosition))
           {
               Crosshair.rectTransform.anchoredPosition = localPosition;
           }
           else
           {
               Crosshair.rectTransform.anchoredPosition = Vector2.zero;
           }
       }
   }*/
}