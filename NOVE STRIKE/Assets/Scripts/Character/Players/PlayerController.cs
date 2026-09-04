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
    private Quaternion m_targetRotation = Quaternion.identity;

    private Vector2 m_moveInput;
    private bool m_isShooting;

    public PlayerStatus PlayerStatus => Status as PlayerStatus;

    protected override void SetupStatusModel()
    {
        Status = new PlayerStatus(DefaultMaxHealth, DefaultBaseMoveSpeed, DefaultAttackPower, DefaultDefensePower);
    }

    public override void InitializeCharacter()
    {
        base.InitializeCharacter();

        m_combatPresenter = GetComponent<PlayerCombatPresenter>();
        m_mainCamera = Camera.main;

        if (m_combatPresenter != null)
        {
            m_combatPresenter = GetComponent<PlayerCombatPresenter>();
        }
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
        if (m_isKeyboardMouse)
        {
            CalculateMouseCursorRotation();
        }
        else if (m_moveInput.sqrMagnitude > 0.1f)
        {
            CalculateStickDirectionRotation();
        }
    }

    public void TickFixedUpdate()
    {
        ApplyRotationPhysics();
        MovePlayer();
    }

    private void GatherInputValues()
    {
        m_moveInput = m_moveAction.action.ReadValue<Vector2>();
        m_isShooting = m_shootAction.action.IsPressed();
    }

    private void DetectControlScheme()
    {
        InputControl activeControl = null;

        if(m_moveAction.action.IsPressed())
        {
            activeControl = m_moveAction.action.activeControl;
        }
        else if(m_shootAction.action.IsPressed())
        {
            activeControl = m_shootAction.action.activeControl;
        }
        else if(m_aimAction.action.IsPressed())
        {
            activeControl = m_aimAction.action.activeControl;
        }

        if (activeControl != null)
        {
            m_isKeyboardMouse = activeControl.device is Keyboard || activeControl.device is Mouse;
        }
    }

    private void MovePlayer()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (forward * m_moveInput.y) + (right * m_moveInput.x);

        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        // Unity 6推奨の3D用 linearVelocity を使用
        CachedRigidbody.linearVelocity = movement * Status.BaseMoveSpeed;
    }

    // 【コントローラー用】スティックの入力方向を向く
    private void CalculateStickDirectionRotation()
    {
        if (m_moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 direction = new Vector3(m_moveInput.x, 0f, m_moveInput.y);
            m_targetRotation = Quaternion.LookRotation(direction);
        }
    }

    // 【キーマウ用】マウスカーソルのワールド位置を向く
    private void CalculateMouseCursorRotation()
    {
        if (m_mainCamera == null || Mouse.current == null) { return; }

        Ray ray = m_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float hitDistance))
        {
            Vector3 targetPoint = ray.GetPoint(hitDistance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                m_targetRotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void ApplyRotationPhysics()
    {
        if(CachedRigidbody != null && m_targetRotation != Quaternion.identity)
        {
            CachedRigidbody.MoveRotation(m_targetRotation);
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