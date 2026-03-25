using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// BallController - Controla o movimento de uma bola em um jogo tipo Roll a Ball
/// Usa o novo Input System para capturar input de movimento (WASD/Gamepad/Joystick)
/// </summary>
public class BallController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private Vector2 moveInput;
    private InputActionMap playerActionMap;

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 500f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float linearDrag = 0.5f;
    [SerializeField] private float angularDrag = 0.5f;

    private Rigidbody rb;

    private void OnEnable()
    {
        // Carrega o asset de input se não foi atribuído no inspector
        if (inputActionAsset == null)
        {
            inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActionAsset == null)
            {
                Debug.LogError("BallController: InputActionAsset não encontrado! Atribua 'InputSystem_Actions' no Inspector ou coloque em Assets/Resources/");
                enabled = false;
                return;
            }
        }

        // Obtém o action map do Player
        playerActionMap = inputActionAsset.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("BallController: Action Map 'Player' não encontrado no InputActionAsset!");
            enabled = false;
            return;
        }

        playerActionMap.Enable();

        // Conecta callbacks para o input de movimento
        var moveAction = playerActionMap.FindAction("Move");
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }
    }

    private void OnDisable()
    {
        if (playerActionMap != null)
        {
            var moveAction = playerActionMap.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }
            playerActionMap.Disable();
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("BallController: Rigidbody não encontrado! Adicione um Rigidbody ao GameObject.");
            enabled = false;
            return;
        }

        // Configurar propriedades do Rigidbody
        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;
    }

    private void FixedUpdate()
    {
        // Aplicar força baseada no input
        if (moveInput.magnitude > 0)
        {
            ApplyMovementForce();
        }

        // Limitar velocidade máxima
        LimitSpeed();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void ApplyMovementForce()
    {
        // Direção do movimento em WorldSpace (X e Z)
        Vector3 forceDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        // Aplicar força
        rb.AddForce(forceDirection * moveForce, ForceMode.Force);
    }

    private void LimitSpeed()
    {
        Vector3 velocity = rb.linearVelocity;
        float currentSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        if (currentSpeed > maxSpeed)
        {
            // Manter a componente Y (queda) e limitar apenas movimento horizontal
            float normalizedSpeed = maxSpeed / currentSpeed;
            Vector3 limitedVelocity = new Vector3(velocity.x * normalizedSpeed, velocity.y, velocity.z * normalizedSpeed);
            rb.linearVelocity = limitedVelocity;
        }
    }

    /// <summary>
    /// Método público para resetar a posição da bola (útil para respawn/restart)
    /// </summary>
    public void ResetPosition(Vector3 newPosition)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = newPosition;
    }

    /// <summary>
    /// Método público para obter a velocidade atual da bola
    /// </summary>
    public float GetCurrentSpeed()
    {
        Vector3 velocity = rb.linearVelocity;
        return new Vector3(velocity.x, 0f, velocity.z).magnitude;
    }
}

