using System;
using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private float shootingSpeedAcceleration = 1f;
    
    private int moveSpeedAnimHash;
    private int aimAnimHash;
    private int autoShootingSpeedAnimHash;
    private int singleShootingSpeedAnimHash;
    private int horizontalAnimHash;
    private int verticalAnimHash;
    private int reloadAnimHash;
    private int switchAnimHash;

    float shootingSpeed = 0.0f;
    
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        moveSpeedAnimHash = Animator.StringToHash("MovementSpeed");
        aimAnimHash = Animator.StringToHash("isAim");
        autoShootingSpeedAnimHash = Animator.StringToHash("ShootingSpeed");
        horizontalAnimHash = Animator.StringToHash("Horizontal");
        verticalAnimHash = Animator.StringToHash("Vertical");
        reloadAnimHash = Animator.StringToHash("Reload");
        switchAnimHash = Animator.StringToHash("SwitchingWeapon");
        singleShootingSpeedAnimHash = Animator.StringToHash("SingleShoot");
    }

    private void OnEnable()
    {
        PlayerController.OnMoveAnimation += MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable += EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble += DisableAimAnimationHandler;
        PlayerController.OnSend_X_Z_Pos += Get_X_Z_PosHandler;
        
        PlayerShootController.OnReloadAnimation += ReloadAnimation;
        
        PlayerGunSelector.OnSwitchWeapon += SwitchWeaponAnimation;    
        
        GunSO.OnAutoShootAnimationEnable += EnableAutoShootAnimationHandler;
        GunSO.OnAutoShootAnimationDiasble += DisableAutoShootAnimationHandler;
        GunSO.OnSingleShootAnimationEnable += SingleShootAnimation;
    }

    private void OnDisable()
    {
        PlayerController.OnMoveAnimation -= MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable -= EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble -= DisableAimAnimationHandler;
        PlayerController.OnSend_X_Z_Pos -= Get_X_Z_PosHandler;
        
        PlayerShootController.OnReloadAnimation -= ReloadAnimation;
        
        PlayerGunSelector.OnSwitchWeapon -= SwitchWeaponAnimation;     
        
        GunSO.OnAutoShootAnimationEnable -= EnableAutoShootAnimationHandler;
        GunSO.OnAutoShootAnimationDiasble -= DisableAutoShootAnimationHandler;
        GunSO.OnSingleShootAnimationEnable -= SingleShootAnimation;
    }

    private void ReloadAnimation()
    {
        DisableAutoShootAnimationHandler();
        animator.SetTrigger(reloadAnimHash);
    }
    
    private void SingleShootAnimation()
    {
        animator.SetTrigger(singleShootingSpeedAnimHash);
    }
    
    private void SwitchWeaponAnimation()
    {
        DisableAutoShootAnimationHandler();
        animator.SetTrigger(switchAnimHash);
    }
    
    private void Get_X_Z_PosHandler(float x, float z)
    {
        animator.SetFloat(horizontalAnimHash, x);
        animator.SetFloat(verticalAnimHash, z);
    }

    private void EnableAimAnimationHandler()
    {
        if (!IsAnimActive(aimAnimHash))
        {
            animator.SetBool(aimAnimHash, true);
        }
    }

    private void DisableAimAnimationHandler()
    {
        if (IsAnimActive(aimAnimHash))
        {
            animator.SetBool(aimAnimHash, false);
        }
    }
    
    private void EnableAutoShootAnimationHandler()
    {
        if (shootingSpeed <= 1f)
            shootingSpeed += Time.deltaTime * shootingSpeedAcceleration;
        animator.SetFloat(autoShootingSpeedAnimHash, shootingSpeed);
    }

    private void DisableAutoShootAnimationHandler()
    {
        if (shootingSpeed > 0f)
            shootingSpeed = 0;
        animator.SetFloat(autoShootingSpeedAnimHash, shootingSpeed);
    }


    private void MoveAnimationSwitcherHandler(float movementSpeed)
    {
        animator.SetFloat(moveSpeedAnimHash, movementSpeed);
    }

    private bool IsAnimActive(int parameterHash)
    {
        return animator.GetBool(parameterHash);
    }
}