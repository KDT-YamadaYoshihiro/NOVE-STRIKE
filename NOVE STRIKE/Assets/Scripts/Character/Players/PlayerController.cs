using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharaBase
{
    [Header("Input Action References")]
    [SerializeField] private InputActionReference m_moveAction;
    [SerializeField] private InputActionReference m_aimAction;
    [SerializeField] private InputActionReference m_shootAction;

    // プレイヤーの戦闘プレゼンターへの参照
    private PlayerCombatPresenter m_combatPresenter;
    // メインカメラへの参照
    private Camera m_mainCamera;
    // 入力デバイスがキーボードとマウスかどうかを判定するフラグ
    private bool m_isKeyboardMouse;

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
        m_mainCamera = Camera.main;
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
        DetectControlScheme();

        // 3D空間での回転（Y軸回転）
        if (m_aimInput.sqrMagnitude > 0.1f)
        {
            RotateTowardsMouseCursor();
        }
        else if (m_moveInput.sqrMagnitude > 0.1f)
        {
            RotateTowardsStickDirection();
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

    private void DetectControlScheme()
    {
        // 最初に入力があったデバイスを確認
        var lastControl = m_aimAction.action.activeControl?.device;
        if (lastControl != null)
        {
            m_isKeyboardMouse = lastControl is Keyboard || lastControl is Mouse;
        }
    }

    private void MovePlayer()
    {
        // 2DのVector2入力を、3D空間の水平面（X, Z）の動きに変換
        Vector3 movement = new Vector3(m_moveInput.x, 0f, m_moveInput.y);

        // Unity 6推奨の3D用 linearVelocity を使用
        CachedRigidbody.linearVelocity = movement * Status.BaseMoveSpeed;
    }

    // 【コントローラー用】スティックの入力方向を向く
    private void RotateTowardsStickDirection()
    {
        if (m_moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 direction = new Vector3(m_moveInput.x, 0f, m_moveInput.y);
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // 【キーマウ用】マウスカーソルのワールド位置を向く
    private void RotateTowardsMouseCursor()
    {
        if (m_mainCamera == null) return;

        Ray ray = m_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float hitDistance))
        {
            Vector3 targetPoint = ray.GetPoint(hitDistance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
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