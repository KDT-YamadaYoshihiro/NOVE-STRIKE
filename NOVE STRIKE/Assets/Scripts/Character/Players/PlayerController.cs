// PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharaBase
{
    [Header("Input Action References")]
    [SerializeField] private InputActionReference m_moveAction;
    [SerializeField] private InputActionReference m_aimAction;
    [SerializeField] private InputActionReference m_shootAction;

    private PlayerCombatPresenter m_combatPresenter;

    private Vector2 m_moveInput;
    private Vector2 m_aimInput;
    private bool m_isShooting;

    public PlayerStatus PlayerStatus => Status as PlayerStatus;

    protected override void SetupStatusModel()
    {
        Status = new PlayerStatus(DefaultMaxHealth, DefaultBaseMoveSpeed);
    }

    public override void InitializeCharacter()
    {
        base.InitializeCharacter();
        m_combatPresenter = GetComponent<PlayerCombatPresenter>();
        EnableInputActions();
    }

    public override void ReleaseCharacter()
    {
        DisableInputActions();
        base.ReleaseCharacter();
    }

    public void TickUpdate()
    {
        GatherInputValues();

        // 3D空間での回転（Y軸回転）
        if (m_aimInput.sqrMagnitude > 0.1f)
        {
            RotatePlayer(m_aimInput);
        }
        else if (m_moveInput.sqrMagnitude > 0.1f)
        {
            RotatePlayer(m_moveInput);
        }

        if (m_isShooting)
        {
            TriggerWeaponFire();
        }
    }

    public void TickFixedUpdate()
    {
        MovePlayer();
    }

    private void GatherInputValues()
    {
        m_moveInput = m_moveAction.action.ReadValue<Vector2>();
        m_aimInput = m_aimAction.action.ReadValue<Vector2>();
        m_isShooting = m_shootAction.action.IsPressed();
    }

    private void MovePlayer()
    {
        // 2DのVector2入力を、3D空間の水平面（X, Z）の動きに変換
        Vector3 movement = new Vector3(m_moveInput.x, 0f, m_moveInput.y);

        // Unity 6推奨の3D用 linearVelocity を使用
        CachedRigidbody.linearVelocity = movement * Status.BaseMoveSpeed;
    }

    private void RotatePlayer(Vector2 arg_direction)
    {
        // 3D空間のY軸回転用の角度を計算（XとYの入力を、3DのXとZに対応させる）
        float targetAngle = Mathf.Atan2(arg_direction.x, arg_direction.y) * Mathf.Rad2Deg;

        // Y軸を中心に回転させる
        transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
    }

    private void TriggerWeaponFire()
    {
        if (m_combatPresenter != null)
        {
            m_combatPresenter.OnShootInputPressed();
        }
    }

    private void EnableInputActions()
    {
        m_moveAction.action.Enable();
        m_aimAction.action.Enable();
        m_shootAction.action.Enable();
    }

    private void DisableInputActions()
    {
        m_moveAction.action.Disable();
        m_aimAction.action.Disable();
        m_shootAction.action.Disable();
    }
}