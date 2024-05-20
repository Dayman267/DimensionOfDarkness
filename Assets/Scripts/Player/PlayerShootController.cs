using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShootController : MonoBehaviour
{
    public PlayerGunSelector GunSelector;

    [SerializeField] private bool AutoReload = false;

    //private static bool isReloading = false;
    private bool isStopReloading = false;

    public static event Action OnReloadAnimation;

    void Update()
    {
        if (GunSelector.ActiveGun != null && 
            PlayerController.IsPlayerHasIdleState() && 
            PlayerController.GetPlayerMoveState() != PlayerMoveStates.dashing)
        {
            GunSelector.ActiveGun.CallTick(PlayerController.IsLeftClickDown());
        }

        if (PlayerController.IsLeftClickDown() && PlayerController.GetPlayerState() == PlayerStates.reloading && GunSelector.ActiveGun.AmmoConfig.SingleBulletLoad)
        {
            isStopReloading = true;
        }

        if (ShouldManualReload() || ShouldAutoReload())
        {
            Reload();
        }
    }

    private void Reload()
    {
        GunSelector.ActiveGun.StartReloading();
        PlayerController.SetPlayerState(PlayerStates.reloading);
        OnReloadAnimation?.Invoke();
    }

    
    /// <summary>
    ///  EndReload is switch animation method
    /// </summary>
    private void EndReload()
    {
        if (isStopReloading)
        {
            isStopReloading = false;
            PlayerController.SetPlayerState(PlayerStates.idle);
            return;
        }

        GunSelector.ActiveGun.EndReload();
        PlayerController.SetPlayerState(PlayerStates.idle);
        if (GunSelector.ActiveGun.CanReload())
        {
            Reload();
        }
    }


    private bool ShouldManualReload()
    {
        return PlayerController.IsPlayerHasIdleState() && PlayerController.IsRKeyDown() &&
               GunSelector.ActiveGun.CanReload();
    }

    private bool ShouldAutoReload()
    {
        return PlayerController.IsPlayerHasIdleState()
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