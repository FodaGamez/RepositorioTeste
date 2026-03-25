# 📋 PLANEJAMENTO: PlayerController para Roll a Ball

## 🎯 Objetivo
Implementar um **PlayerController robusto, simples de configurar** que gerencie:
- Movimento da bola via física
- Input de múltiplos dispositivos
- Estados (Idle, Moving, Jumping)
- Câmera acompanhando o player

---

## 📊 Fases de Implementação

### **FASE 1: INPUT CENTRALIZADO** (1-2 horas)

#### 1.1 InputManager.cs (Singleton)
```csharp
public class InputManager : MonoBehaviour
{
    // Callbacks de input
    public event System.Action<Vector2> OnMove;
    public event System.Action OnJumpPressed;
    public event System.Action OnJumpReleased;
    public event System.Action<Vector2> OnLook;
    public event System.Action OnSprintPressed;
    
    // Singleton
    private static InputManager instance;
    public static InputManager Instance => instance;
    
    // Public properties
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; set; }
    
    // Private
    private InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;
}
```

**Benefícios:**
- ✅ Input centralizado
- ✅ Fácil debug
- ✅ Remapeável em runtime
- ✅ Listeners independentes

---

### **FASE 2: MOVIMENTO FÍSICO** (2-3 horas)

#### 2.1 MovementController.cs
```csharp
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveForce = 50f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 1f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float groundDrag = 0.5f;
    [SerializeField] private float airDrag = 0.1f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    private Rigidbody rb;
    private InputManager inputManager;
    private bool canJump = true;
    private Vector3 movementDirection;
    
    public void Initialize(Rigidbody rigidbody)
    {
        rb = rigidbody;
        inputManager = InputManager.Instance;
        
        // Subscribe
        inputManager.OnMove += HandleMove;
        inputManager.OnJumpPressed += HandleJump;
    }
    
    private void HandleMove(Vector2 input)
    {
        movementDirection = new Vector3(input.x, 0f, input.y).normalized;
    }
    
    private void HandleJump()
    {
        if (canJump && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }
    }
    
    private void FixedUpdate()
    {
        ApplyMovement();
        UpdateDrag();
        LimitSpeed();
    }
    
    private void ApplyMovement()
    {
        if (movementDirection.magnitude > 0)
        {
            rb.AddForce(movementDirection * moveForce, ForceMode.Force);
        }
    }
    
    private void UpdateDrag()
    {
        rb.linearDamping = IsGrounded() ? groundDrag : airDrag;
    }
    
    private void LimitSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }
    
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    public float GetCurrentSpeed()
    {
        return new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
    }
}
```

**Benefícios:**
- ✅ Separação de responsabilidades
- ✅ Fácil de debugar
- ✅ Configurable via Inspector
- ✅ Ground detection automático

---

### **FASE 3: CONTROLLER PRINCIPAL** (1-2 horas)

#### 3.1 PlayerController.cs (Façade)
```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputManager inputManager;
    private MovementController movementController;
    private Rigidbody rb;
    
    private void Awake()
    {
        // Setup InputManager
        inputManager = gameObject.AddComponent<InputManager>();
        inputManager.Initialize(inputActionAsset);
        
        // Setup MovementController
        rb = GetComponent<Rigidbody>();
        movementController = gameObject.AddComponent<MovementController>();
        movementController.Initialize(rb);
    }
    
    private void OnDestroy()
    {
        inputManager?.Cleanup();
    }
    
    // Public methods
    public void ResetPosition(Vector3 newPosition)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = newPosition;
    }
    
    public float GetCurrentSpeed() => movementController.GetCurrentSpeed();
}
```

---

### **FASE 4: CÂMERA** (Opcional - 1-2 horas)

#### 4.1 CameraController.cs
```csharp
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float sensitivity = 2f;
    
    private InputManager inputManager;
    private float rotationX = 0f;
    
    private void Start()
    {
        inputManager = InputManager.Instance;
    }
    
    private void LateUpdate()
    {
        FollowPlayer();
        RotateWithInput();
    }
    
    private void FollowPlayer()
    {
        Vector3 desiredPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
    
    private void RotateWithInput()
    {
        Vector2 look = inputManager.LookInput;
        rotationX -= look.y * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        
        // Atualizar câmera e player
    }
}
```

---

## 🗂️ Estrutura de Pastas

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs          ← Main Controller
│   │   ├── MovementController.cs        ← Physics
│   │   └── BallController.cs            ← (Legacy, pode ser removido)
│   ├── Input/
│   │   ├── InputManager.cs              ← Input Singleton
│   │   └── InputSystemActions.cs        ← Wrapper
│   ├── Camera/
│   │   └── CameraController.cs          ← Optional
│   └── Utils/
│       ├── Constants.cs
│       └── Extensions.cs
├── Prefabs/
│   └── Player.prefab                    ← Ball com componentes
├── Scenes/
│   └── GameScene.unity
└── Input/
    └── InputSystem_Actions.inputactions
```

---

## 📈 Timeline de Implementação

| Fase | Componente | Duração | Status |
|------|-----------|---------|--------|
| 1 | InputManager | 1-2h | ⏳ Próximo |
| 2 | MovementController | 2-3h | ⏳ Próximo |
| 3 | PlayerController | 1-2h | ⏳ Próximo |
| 4 | CameraController | 1-2h | 🔲 Optional |

**Total Estimado:** 5-9 horas (Incluindo testes)

---

## 🎮 Configuração no Inspector

### Após Implementação Completa:

**PlayerController (Inspector):**
```
Input Action Asset: InputSystem_Actions
```

**Rigidbody (Pré-configurado):**
```
Mass: 1
Drag: 0.5 (será sobrescrito)
Angular Drag: 0.5
Use Gravity: ✓
Constraints: Freeze Rotation X, Y, Z
```

**Collider:**
```
Type: Sphere
Radius: 0.5
Convex: ✓ (se for usar como trigger)
```

---

## 💡 Melhores Práticas Implementadas

### 1. **Separação de Responsabilidades**
- ✅ InputManager = Input
- ✅ MovementController = Física
- ✅ PlayerController = Orquestração
- ✅ CameraController = Câmera

### 2. **Escalabilidade**
- ✅ Fácil adicionar novos inputs
- ✅ Fácil criar EnemyController
- ✅ Suporta múltiplos players
- ✅ Plugável

### 3. **Configurabilidade**
- ✅ Tudo exposé no Inspector
- ✅ Valores default sensatos
- ✅ Presets para diferentes estilos
- ✅ Sem hardcodes

### 4. **Performance**
- ✅ FixedUpdate para física
- ✅ Cache de componentes
- ✅ Lazy initialization
- ✅ Object pooling ready

### 5. **Testabilidade**
- ✅ Métodos públicos para testes
- ✅ Events para mock
- ✅ Sem singletons hard-coded (usáveis em testes)

---

## 🧪 Checklist de Testes

- [ ] Movimento em 4 direções (WASD)
- [ ] Movimento com Gamepad
- [ ] Velocidade máxima funciona
- [ ] Jump funciona no ground
- [ ] Não pula no ar
- [ ] Câmera acompanha jogador
- [ ] Câmera rotaciona com Look input
- [ ] Reset position funciona
- [ ] Speed aumenta com Sprint (opcional)
- [ ] Múltiplos inputs simultâneos

---

## 🚀 Próximas Features Após Base

1. **Sprint** - Aumentar velocidade com Shift
2. **Crouch** - Diminuir altura e velocidade
3. **Knockback** - Quando bate em inimigos
4. **Audio** - Footsteps, jump, impact
5. **Particles** - Movimento, poeira, etc.
6. **Animation** - Idle, moving, jumping states
7. **Pickups** - Coletar itens
8. **UI** - Score, health, compass

---

## 📖 Referências & Recursos

### Unity Docs:
- https://docs.unity3d.com/Manual/InputSystem.html
- https://docs.unity3d.com/ScriptReference/Rigidbody.html
- https://docs.unity3d.com/Manual/PhysicsHowTos.html

### Padrões de Design:
- **Singleton:** InputManager
- **Façade:** PlayerController
- **Observer:** Events
- **Component:** ECS-like architecture

---

## 📝 Notas Importantes

1. **Ground Detection:**
   - Use Raycast downward do center da bola
   - Distance = radius + small offset
   - Importante para jump e landing feedback

2. **Physics Timing:**
   - Sempre usar FixedUpdate para Rigidbody
   - Input em Update, apply em FixedUpdate
   - Cuidado com Time.deltaTime vs Time.fixedDeltaTime

3. **Input Buffering:**
   - Considere buffering de jump input
   - Permite "presionar jump 1 frame antes de tocar chão"
   - Melhora feel do jogo

4. **Câmera Suave:**
   - Use Vector3.Lerp ao invés de assignment direto
   - Ou SmoothDamp para mais controle
   - Evita camera jitter

5. **Multiplayer (Futuro):**
   - Architecture permite múltiplos players
   - Apenas crie multiple PlayerController instances
   - InputManager pode ser global ou por player

---

**Status:** 📋 **PLANEJADO**
**Próximo:** ⏳ Aguardando aprovação para começar implementação

Deseja que eu comece a implementar alguma destas fases?

