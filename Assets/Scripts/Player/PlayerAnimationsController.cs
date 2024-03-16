using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private float shootingSpeedAcceleration = 0.5f;
    
    // Start is called before the first frame update
    private int moveSpeedAnimHash;
    private int aimAnimHash;
    private int shootingSpeedAnimHash;
    
    float shootingSpeed = 0.0f;
    
    void Start()
    {
        Debug.Log(_animator == null ? "null" : "great");
        moveSpeedAnimHash = Animator.StringToHash("MovementSpeed");
        aimAnimHash = Animator.StringToHash("isAim");
        shootingSpeedAnimHash = Animator.StringToHash("ShootingSpeed");
    }
    
    private void OnEnable()
    {
        PlayerController.OnMoveAnimation += MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable += EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble += DisableAimAnimationHandler;
        PlayerController.OnShootAnimationEnable += EnableShootAnimationHandler;
        PlayerController.OnShootAnimationDiasble += DisableShootAnimationHandler;
        PlayerController.OnSendAnimator += GetAnimator;
    }

    private void OnDisable()
    {
        PlayerController.OnMoveAnimation -= MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable -= EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble -= DisableAimAnimationHandler;
        PlayerController.OnShootAnimationEnable -= EnableShootAnimationHandler;
        PlayerController.OnShootAnimationDiasble -= DisableShootAnimationHandler;
        PlayerController.OnSendAnimator -= GetAnimator;
    }

    private void EnableAimAnimationHandler()
    {
        if (!IsAnimActive(aimAnimHash))
        {
            _animator.SetBool(aimAnimHash, true);
        }
    }

    private void DisableAimAnimationHandler()
    {
        if (IsAnimActive(aimAnimHash))
        {
            _animator.SetBool(aimAnimHash, false);
        }
    }
    
    private void EnableShootAnimationHandler()
    {
        if (shootingSpeed <= 1f)
            shootingSpeed += Time.deltaTime * shootingSpeedAcceleration;
        _animator.SetFloat(shootingSpeedAnimHash, shootingSpeed);
    }

    private void DisableShootAnimationHandler()
    {
        if (shootingSpeed >= 0f)
            shootingSpeed = 0;
        _animator.SetFloat(shootingSpeedAnimHash, shootingSpeed);
    }


    private void MoveAnimationSwitcherHandler(float movementSpeed)
    {
        _animator.SetFloat(moveSpeedAnimHash, movementSpeed);
    }

    private bool IsAnimActive(int parameterHash)
    {
        return _animator.GetBool(parameterHash);
    }

    private void GetAnimator(Animator animator)
    {
        _animator = animator;
    }
}