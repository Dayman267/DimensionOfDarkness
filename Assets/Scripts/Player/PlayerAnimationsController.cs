using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private float shootingSpeedAcceleration = 0.5f;
    
    // Start is called before the first frame update
    private int moveSpeedAnimHash;
    private int aimAnimHash;
    private int shootingSpeedAnimHash;
    private int horizontalAnimHash;
    private int verticalAnimHash;
    
    float shootingSpeed = 0.0f;
    
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        moveSpeedAnimHash = Animator.StringToHash("MovementSpeed");
        aimAnimHash = Animator.StringToHash("isAim");
        shootingSpeedAnimHash = Animator.StringToHash("ShootingSpeed");
        horizontalAnimHash = Animator.StringToHash("Horizontal");
        verticalAnimHash = Animator.StringToHash("Vertical");
        Debug.Log(_animator == null ? "null" : "isAnimator");
    }
    
    private void OnEnable()
    {
        PlayerController.OnMoveAnimation += MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable += EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble += DisableAimAnimationHandler;
        PlayerController.OnShootAnimationEnable += EnableShootAnimationHandler;
        PlayerController.OnShootAnimationDiasble += DisableShootAnimationHandler;
        PlayerController.OnSend_X_Z_Pos += Get_X_Z_PosHandler;
    }

    private void OnDisable()
    {
        PlayerController.OnMoveAnimation -= MoveAnimationSwitcherHandler;
        PlayerController.OnAimAnimationEnable -= EnableAimAnimationHandler;
        PlayerController.OnAimAnimationDiasble -= DisableAimAnimationHandler;
        PlayerController.OnShootAnimationEnable -= EnableShootAnimationHandler;
        PlayerController.OnShootAnimationDiasble -= DisableShootAnimationHandler;
        PlayerController.OnSend_X_Z_Pos -= Get_X_Z_PosHandler;
    }
    
    private void Get_X_Z_PosHandler(float x, float z)
    {
        _animator.SetFloat(horizontalAnimHash, x);
        _animator.SetFloat(verticalAnimHash, z);
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
}