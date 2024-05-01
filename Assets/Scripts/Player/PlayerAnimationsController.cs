using System;
using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private float shootingSpeedAcceleration = 0.5f;
    
    private int moveSpeedAnimHash;
    private int aimAnimHash;
    private int shootingSpeedAnimHash;
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
        shootingSpeedAnimHash = Animator.StringToHash("ShootingSpeed");
        horizontalAnimHash = Animator.StringToHash("Horizontal");
        verticalAnimHash = Animator.StringToHash("Vertical");
        reloadAnimHash = Animator.StringToHash("Reload");
        switchAnimHash = Animator.StringToHash("SwitchingWeapon");
    }

    private void OnEnable()
    {
        PlayerController.OnMoveAnimation += MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable += EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble += DisableAimAnimationHandler;
        PlayerController.OnShootAnimationEnable += EnableShootAnimationHandler;
        PlayerController.OnShootAnimationDiasble += DisableShootAnimationHandler;
        PlayerController.OnSend_X_Z_Pos += Get_X_Z_PosHandler;
        
        PlayerShootController.OnReloadAnimation += ReloadAnimation;
        
        PlayerGunSelector.OnSwitchWeapon += SwitchWeaponAnimation;       
        
    }

    private void OnDisable()
    {
        PlayerController.OnMoveAnimation -= MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable -= EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble -= DisableAimAnimationHandler;
        PlayerController.OnShootAnimationEnable -= EnableShootAnimationHandler;
        PlayerController.OnShootAnimationDiasble -= DisableShootAnimationHandler;
        PlayerController.OnSend_X_Z_Pos -= Get_X_Z_PosHandler;
        
        PlayerShootController.OnReloadAnimation -= ReloadAnimation;
        
        PlayerGunSelector.OnSwitchWeapon -= SwitchWeaponAnimation;     
    }

    private void ReloadAnimation()
    {
        DisableShootAnimationHandler();
        animator.SetTrigger(reloadAnimHash);
    }
    
    private void SwitchWeaponAnimation()
    {
        DisableShootAnimationHandler();
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
    
    private void EnableShootAnimationHandler()
    {
        if (shootingSpeed <= 1f)
            shootingSpeed += Time.deltaTime * shootingSpeedAcceleration;
        animator.SetFloat(shootingSpeedAnimHash, shootingSpeed);
    }

    private void DisableShootAnimationHandler()
    {
        if (shootingSpeed > 0f)
            shootingSpeed = 0;
        animator.SetFloat(shootingSpeedAnimHash, shootingSpeed);
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