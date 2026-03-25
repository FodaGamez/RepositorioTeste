# Documentação do Projeto Roll a Ball - Locust

## 📋 Resumo Executivo
Este é um projeto **Roll a Ball** em Unity que implementa um controlador de bola física com sistema de input moderno (New Input System). O projeto utiliza Rigidbody para física realista e o InputActionAsset para capturar inputs de múltiplos dispositivos (Teclado, Gamepad, Joystick).

---

## 🎮 Funcionamento Atual do Projeto

### Arquitetura:
```
Assets/
├── InputSystem_Actions.inputactions  ← Arquivo de configuração de inputs
├── InputSystemActions.cs             ← Wrapper C# para type-safe access aos actions
├── BallController.cs                 ← Controlador principal da bola
└── Scenes/                           ← Cenas do projeto
```

### Componentes Principais:

#### 1. **InputSystem_Actions.inputactions**
- Arquivo JSON que define todos os inputs suportados
- **Action Maps:**
  - **Player:** Move, Look, Attack, Interact, Crouch, Jump, Previous, Next, Sprint
  - **UI:** Navigate, Submit, Cancel, Point, Click, etc.
- **Control Schemes:** Keyboard&Mouse, Gamepad, Touch, Joystick, XR
- **Bindings:** 
  - WASD/Setas para movimento (Keyboard)
  - Left Stick para movimento (Gamepad)
  - Joystick/XR controllers para movimento

#### 2. **BallController.cs**
Responsável pelo controle de movimento da bola:
- **Input Handling:** Captura o input de movimento via InputActionMap
- **Physics:** Aplica força à Rigidbody baseada no input normalizado
- **Limitações:** Controla velocidade máxima horizontal mantendo queda vertical
- **Features:**
  - Carregar InputActionAsset automaticamente ou via Inspector
  - Callbacks de input com `performed` e `canceled`
  - Métodos públicos: `ResetPosition()`, `GetCurrentSpeed()`
  - Damping configurável para movimento suave

#### 3. **InputSystemActions.cs** (Wrapper)
Classe gerada que fornece acesso type-safe aos actions:
- Implementa `IInputActionCollection2`
- Propriedades para cada action: `Player.Move`, `Player.Jump`, etc.
- Struct `PlayerActions` e `UIActions` para organização
- Interfaces `IPlayerActions` e `IUIActions` para callbacks

---

## 🔧 Configuração Atual

### Propriedades do BallController:
```csharp
[Header("Input")]
[SerializeField] private InputActionAsset inputActionAsset;

[Header("Movement Settings")]
[SerializeField] private float moveForce = 500f;           // Força aplicada ao movimento
[SerializeField] private float maxSpeed = 20f;            // Velocidade máxima horizontal
[SerializeField] private float linearDrag = 0.5f;         // Amortecimento linear
[SerializeField] private float angularDrag = 0.5f;        // Amortecimento angular
```

### Requisitos do GameObject (Ball):
- ✅ **Rigidbody** (com gravidade habilitada)
- ✅ **Collider** (Sphere, Box, ou Capsule)
- ✅ **Transform** posicionado na cena
- ✅ **Script BallController** anexado

---

## 📊 Fluxo de Funcionamento

```
OnEnable()
  ↓
Carrega InputActionAsset (Resources ou atribuído no Inspector)
  ↓
Encontra Action Map "Player"
  ↓
Conecta callbacks: moveAction.performed += OnMovePerformed
  ↓
playerActionMap.Enable()
  ↓
[FixedUpdate] Loop
  ├─ Lê moveInput do callback
  ├─ ApplyMovementForce() se moveInput > 0
  ├─ LimitSpeed() para manter maxSpeed
  └─ rb.AddForce(direction * moveForce)
```

---

## ⚠️ Problema Resolvido

### Erro: "InputSystemActions type or namespace name could not be found"

**Causa:** O arquivo `InputSystemActions.cs` não estava presente no projeto.

**Solução:** 
✅ Criado arquivo `InputSystemActions.cs` com classe wrapper que:
- Implementa `IInputActionCollection2`
- Fornece acesso type-safe aos actions do InputActionAsset
- Carrega o asset automaticamente de Resources ou recebe via constructor

**Como usar:**
```csharp
// Opção 1: Referenciar o InputActionAsset no Inspector (Recomendado)
[SerializeField] private InputActionAsset inputActionAsset;

// Opção 2: Carregar de Resources (o arquivo será movido para Assets/Resources/)
var actions = Resources.Load<InputActionAsset>("InputSystem_Actions");
```

---

## 🎯 Planejamento de Implementação - PlayerController

### Fase 1: Setup Básico ✅ (CONCLUÍDO)
- [x] Criar InputSystemActions.cs wrapper
- [x] Configurar BallController com New Input System
- [x] Testar callbacks de input

### Fase 2: Controle de Movimento (PRÓ-XIMA)
**Objetivo:** Implementar um PlayerController robusto e simples de configurar

#### Componentes a implementar:
1. **PlayerController.cs** (Script Principal)
   - Manager central do jogador
   - Herança/Composição com BallController
   - Gerenciamento de estados (Idle, Moving, Jumping, etc.)

2. **InputManager.cs** (Camada de Input)
   - Centralizar captura de inputs
   - Permitir remapeamento em runtime
   - Suportar multiple devices simultâneos
   - Events para listeners (observers pattern)

3. **MovementController.cs** (Física)
   - Lógica de movimento separada
   - Apply forces baseado em input
   - Controlar velocidade máxima
   - Ground detection para jump

4. **CameraController.cs** (Câmera) - Opcional
   - Follow player com offset
   - Rotação com Look input
   - Suavização (lerp/smooth damp)

### Melhores Práticas de Implementação (SIMPLES & CONFIGURÁVEL):

#### ✨ Design Patterns:
```
PlayerController (Façade)
    ├── InputManager (Singleton)
    ├── MovementController (Composição)
    └── CameraController (Composição)
```

#### 🎛️ Configurabilidade via Inspector:
```csharp
[SerializeField] private float moveForce = 50f;
[SerializeField] private float maxSpeed = 20f;
[SerializeField] private float jumpForce = 5f;
[SerializeField] private float groundDrag = 0.5f;
[SerializeField] private float airDrag = 0.1f;
[SerializeField] private LayerMask groundLayer;
```

#### 🔄 Event-Driven Architecture:
```csharp
public class InputManager
{
    public event System.Action<Vector2> OnMove;
    public event System.Action OnJump;
    public event System.Action OnSprint;
}

// Subscribers ouvem eventos
movementController.OnInputMove += HandleMovement;
```

#### 📦 Prefab-Ready:
- Scripts devem funcionar em qualquer GameObject com Rigidbody
- Configurações pré-definidas para diferentes gameplay styles
- Herança para especialização (EnemyController extends PlayerController)

---

## 🚀 Próximos Passos

### Imediato:
1. ✅ Resolver erro de InputSystemActions
2. ⬜ Testar BallController em Play Mode
3. ⬜ Verificar inputs de múltiplos dispositivos

### Curto Prazo (Próxima Sprint):
1. Criar PlayerController mais robusto
2. Implementar Jump com Ground Detection
3. Criar InputManager centralizado
4. Adicionar CameraFollow opcional

### Médio Prazo:
1. Sistema de Pickups/Powerups
2. UI de Score/Interface
3. Níveis/Checkpoints
4. Efeitos Sonoros

---

## 📚 Estrutura de Pastas Recomendada

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── MovementController.cs
│   │   └── BallController.cs
│   ├── Input/
│   │   ├── InputManager.cs
│   │   └── InputSystemActions.cs
│   ├── Camera/
│   │   └── CameraController.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   └── UIManager.cs
│   └── Utils/
│       └── Constants.cs
├── Prefabs/
│   ├── Player.prefab
│   ├── Ball.prefab
│   └── Enemy.prefab
├── Scenes/
│   ├── MainMenu.unity
│   ├── Level_1.unity
│   └── Level_2.unity
├── Input/
│   └── InputSystem_Actions.inputactions
├── Materials/
├── Sounds/
└── UI/
```

---

## 🔗 Referências Importantes

### Arquivos Chave:
- `BallController.cs` - Implementação atual
- `InputSystem_Actions.inputactions` - Configuração de inputs
- `InputSystemActions.cs` - Wrapper type-safe

### Métodos Públicos Importantes:
```csharp
// BallController
public void ResetPosition(Vector3 newPosition);
public float GetCurrentSpeed();

// InputActionMap
playerActionMap.FindAction("Move");
playerActionMap.Enable();
playerActionMap.Disable();
```

---

## 📝 Notas Técnicas

### Por que InputSystemActions.cs é necessário?
- O New Input System não gera automaticamente wrappers C# como o antigo
- O wrapper fornece IntelliSense/autocomplete melhorado
- Permite refactoring seguro com rename
- Implementa interfaces para type-safety

### Performance Considerations:
- Callbacks de input são chamados apenas quando há mudança
- Rigidbody.linearVelocity é mais eficiente que rb.velocity (deprecated)
- Usar `FixedUpdate` para física ao invés de `Update`
- Limitar número de raycasts para ground detection

---

**Última Atualização:** 25/03/2025
**Status:** ✅ InputSystemActions corrigido, pronto para desenvolvimento
**Próximo Milestone:** Implementação de PlayerController robusto

