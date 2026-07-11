# 角色动画（View 层）

## 约定

**玩家**：`PlayerView` 内嵌 `AnimatorContract` 常量，须与 `Assets/Animation/PlayerAnimator.controller` **状态名、参数名**一致。  
**敌人**：共用同一 Controller 状态名时，`EnemyView` 以 `CrossFade` 触发；无 locomotion 纯代码驱动。

### 全局规则

| 规则 | 说明 |
|------|------|
| **默认状态** | Base Layer 默认状态为 `Walk` |
| **统一切换入口** | 所有玩家动作态经 `PlayerView.PlayAnimation(stateName, crossFadeDuration = 0.2f, time = 0f)` |
| **不可打断** | 单次动作态在 Controller 中 Transition 的 `Interruption Source = None`；播放过程中**不被其他动作态 CrossFade 覆盖** |
| **Any State 覆盖** | `Hit`、`Dizzy`、`Die01` 配置 **Any State → 目标态**（无 Exit Time），可在 locomotion 或部分动作中**强制切入** |
| **代码门控** | `IsInSingleShotAnimatorState` / `IsActionBlocked` 阻止重复发起 Attack / Skill / Item；`PlayerModel.AllowsLocomotion` 在动作态期间停止写 BlendTree |
| **回到 Walk** | 单次动作 Clip **播完**后，由状态上挂载的 `OnFinish` 行为脚本延迟调用 `PlayAnimation(Walk)`；死亡链 `Die01` → `Die01_Stay` 同理 |

---

## `PlayAnimation`（统一入口）

```csharp
void PlayAnimation(string stateName, float crossFadeDuration = 0.2f, float time = 0f)
```

| 参数 | 含义 |
|------|------|
| `stateName` | Animator 状态名（与 Controller 一致） |
| `crossFadeDuration` | `CrossFade` 过渡时长（秒），默认 `0.2` |
| `time` | 延迟切换：大于 0 时等待 `time - crossFadeDuration` 秒后再 `CrossFade`；`OnFinish` 传入 `stateInfo.length` 实现「播完再切」 |

附加逻辑：

- 死亡态仅允许切 `Die01` / `Die01_Stay`
- `currentAnimationState` 与目标相同则跳过（避免同帧重复 CrossFade）
- `ApplyModelStateForAnimation` 同步 `PlayerModel.State`

---

## `OnFinish`（StateMachineBehaviour）

路径：`Assets/Scripts/Gameplay/Character/Player/View/OnFinish.cs`  
挂在单次动作态（如 `Attack1`、`Hit`）的 Animator State 上。

| 序列化字段 | 默认 | 说明 |
|------------|------|------|
| `_nextStateName` | `Walk` | 播完后切换到的状态 |
| `_crossFadeDuration` | `0.2` | 传给 `PlayAnimation` |

**`OnStateEnter`**：调用 `PlayAnimation(_nextStateName, _crossFadeDuration, stateInfo.length)`，由 `time` 参数在 Clip 时长结束后切回 locomotion。

`PlayerAnimator.controller` 中 `Attack1` 已挂载 `OnFinish`（`Die01` 等可按需配置 `_nextStateName = Die01_Stay`）。

---

## 各状态：进入 / 离开 / 循环 / 打断

### `Walk`（2D BlendTree · locomotion）

| 项 | 说明 |
|----|------|
| **进入** | 场景默认状态；单次动作经 `OnFinish` 结束后回到此状态 |
| **离开** | `PlayAnimation` 进入 Attack；Any State 切入 Hit / Dizzy / Die01 |
| **循环** | **是** — BlendTree 内 locomotion Clip 循环播放 |
| **可被打断** | **是** |

**参数写入**（`PlayerView`，仅 `AllowsLocomotion` 且非 `IsInSingleShotAnimatorState` 时）：

| 参数 | 含义 |
|------|------|
| `VerticalSpeed` | 线速度趋近 `PlayerModel.MoveSpeed` / `RunSpeed` 后归一化 |
| `TurnSpeed` | 相机相对输入 → 本地 yaw 弧度 → `sign(θ)·√|θ|` |

位移：**Root Motion** → `PlayerRootMotionMotor` → `CharacterController`。

---

### `Attack1`

| 项 | 说明 |
|----|------|
| **进入** | `PlayerPresenter.TryAttack` 成功 → `TryPlayAttack` → `PlayAnimation(Attack1)` |
| **离开** | `OnFinish` 在 Clip 时长结束后 → `PlayAnimation(Walk)` |
| **循环** | **否** |
| **可被打断** | **否**（代码门控）；Any State **`Hit`** / **`Die01`** 仍可强制切入 |

**触发链**：

```text
PlayerInputBridge.OnAttack
  → GameplayInputBus.Attack
  → PlayerPresenter.TryAttack（需场景有敌人 + 未 IsActionBlocked）
  → PlayerView.TryPlayAttack
  → PlayAnimation(Attack1)
  → enemy.ProcessDamage（对敌人，非玩家自身）
```

Clip 来源：`DoubleL/Magic_Attack_1`（Controller 状态 `Magic_Attack_1` 子状态映射为 `Attack1`）。

---

### `Attack2` / `Attack3`（预留）

| 项 | 说明 |
|----|------|
| **进入** | Controller 内连招 Transition（Exit Time）；**代码固定只进 Attack1** |
| **离开** | 可挂 `OnFinish` → `Walk` |
| **循环** | **否** |
| **可被打断** | **否** |

Clip 来源：`RPG Tiny Hero Duo` 剑盾攻击片段。

---

### `Hit` / `Dizzy` / `Die01` / `Die01_Stay`

与先前规范一致；`PlayHit` / `PlayDizzy` / `PlayDie` 均经 `PlayAnimation`。受击须对**目标** `CharacterEntity.ProcessDamage` 调用。

---

## 状态一览表

| 状态 | 类型 | 进入条件 | 离开条件 | 循环 | 可被打断 |
|------|------|----------|----------|------|----------|
| `Walk` | BlendTree | 默认 / OnFinish | PlayAnimation / Any State | 是 | 是 |
| `Attack1` | Clip | Attack 成功 | OnFinish → Walk | 否 | 否（Hit/Die 除外） |
| `Attack2` / `Attack3` | Clip | 连招预留 | OnFinish → Walk | 否 | 否 |
| `Hit` | Clip | `PlayHit` | OnFinish → Walk | 否 | 否 |
| `Dizzy` | Clip | `PlayDizzy` | OnFinish → Walk | 否 | 否 |
| `Die01` | Clip | `PlayDie` | → Die01_Stay | 否 | 否 |
| `Die01_Stay` | Clip | Die01 / `PlayDieStay` | 终态 | 否 | 否 |

---

## 转向计算（Walk 专用）

1. 取 **Main Camera**（Cinemachine Brain 输出）的 `forward` / `right`，投影到地面平面  
2. `worldDir = camForward * input.y + camRight * input.x`  
3. `localDir = InverseTransformDirection(worldDir)`（XZ）  
4. `θ = Atan2(localDir.x, localDir.z)` → 平滑后写入 `TurnSpeed`  

`PlayerView._camera` 可选手动绑定；**Main 场景未绑**，使用 `Camera.main`（与 `Virtual Camera` → `CinemachineBrain` 一致）。  
`PlayerView` 订阅 `GameplayInputBus.Move`；Presenter **不**处理 Move。

---

## 代码与 View 职责

| 类型 | 职责 |
|------|------|
| `PlayerView` | locomotion 参数、`PlayAnimation`、动作门控、`PlayerModel.State` |
| `OnFinish` | 单次动作结束 → 调用 `PlayAnimation` 回 locomotion |
| `EnemyView` | `PlayHit` / `PlayDie` 等（规则同上，无 locomotion 驱动） |
| `CharacterView` | 受击闪色 |
| `CharacterPresenter` | 对**本实体** `ProcessDamage` → 调 View `PlayHit()` |

### 技能 / 道具（暂无独立 Clip）

| 行为 | 进入 | 动画 | 离开 |
|------|------|------|------|
| 技能 | `TryPlaySkill` 成功 | 仍处 `Walk`（占位 `_fallbackActionLockSeconds`） | 计时结束 → locomotion |
| 道具 | `TryPlayItem` 成功 | 同上 | 同上 |

---

## Controller 与动画资产

| 资产 | 路径 |
|------|------|
| Animator Controller | `Assets/Animation/PlayerAnimator.controller` |
| Locomotion FBX | `Assets/Animation/Walk/`（Humanoid 行走/奔跑/转向） |
| 普攻 Clip | `Assets/DoubleL/FBX_Animations/Magic/Magic_Attack_*.fbx` |
| 剑盾动作 | `Assets/RPG Tiny Hero Duo/Animation/SwordAndShield/`（Attack2/3、Hit、Die、Dizzy） |
| 角色模型 | `Assets/RPG Tiny Hero Duo/` + `Assets/Prefab/Player.prefab` |

维护 Transition 时保持：**单次动作不循环、Interruption Source = None**；回 Walk 优先由 `OnFinish` 驱动，避免与 Exit Time 过渡重复。

---

## 攻击输入与已知问题

### 已缓解

| 问题 | 处理 |
|------|------|
| Presenter 重复订阅 `Attack` | `_inputBound` + `Initialize` 时先 `UnbindInput` |
| 同帧多次 Button 回调 | `PlayerInputBridge.TryConsumeButton` 按 `Time.frameCount` 去重 |
| 普攻误伤玩家自身 Hit | `TryAttack` 对 `enemy.ProcessDamage`，非玩家 `CharacterPresenter` |
| 动画切换分散 | 统一 `PlayAnimation` + `OnFinish` 回 Walk |

### 仍待完善（攻击触发）

| 问题 | 说明 |
|------|------|
| **无敌人不播攻击** | `TryAttack` 在 `FindEnemy()` 为 null 时直接 return，**不调用** `TryPlayAttack` |
| **OnFinish 查找 View** | `animator.GetComponent<PlayerView>()`；View 若在父节点需改为 `GetComponentInParent` |
| **连招未接** | 仅 `Attack1`；`Attack2`/`Attack3` Controller 过渡未与输入衔接 |
| **双重回 Walk** | Controller Exit Time 过渡未禁用时，可能与 `OnFinish` 重复 CrossFade |

调试：开启 `GameplayInputLog`（`[PlayerInput]` 分层日志：Bridge → Presenter → View）。
