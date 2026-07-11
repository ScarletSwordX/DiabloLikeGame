# Input 数据基线

## 运行时链路（玩家）

```text
PlayerInput（Send Messages，m_NotificationBehavior = 0）
  → PlayerInputBridge.OnMove / OnAttack / OnSkill* / OnItem*
  → GameplayInputBus.Notify*
  ├─ Move  → PlayerView.HandleMove（locomotion，不经 Presenter）
  └─ Attack / Skill1-3 / Item1-3 → PlayerPresenter.Try*
Animator Root Motion → PlayerRootMotionMotor → CharacterController.Move
```

镜头：**Cinemachine**（`Main.unity` 内 `Virtual Camera` 跟随 Player），不由 Input 或 `PlayerView` 驱动相机 Transform。

| 组件 | 挂载 | 职责 |
|------|------|------|
| `PlayerInput` | Player（Prefab 或 Main 场景实例） | Input System 设备与 Action Map |
| `PlayerInputBridge` | Player Prefab | Send Messages 入口 → `GameplayInputBus` |
| `PlayerView` | Player | 订阅 `Bus.Move`；驱动 Animator |
| `PlayerRootMotionMotor` | Player | `OnAnimatorMove` + CC 碰撞 |
| `CharacterController` | Player | 位移与碰撞体 |

`GameBootstrap` → `PlayerController.Initialize` 将 `InputActionAsset` 赋给 `PlayerInput.actions`（可与 Prefab 默认引用一致）。

### 与文档/资产差异（实现现状）

| 项 | 说明 |
|----|------|
| 相机 | **Main 场景**用 Cinemachine `Virtual Camera` + `CinemachineBrain`；见 [Player/DATA.md](../Character/Player/DATA.md#main-unity-场景-3c) |
| `PlayerInputBridge` | 已含于 `Player.prefab` |
| `GameInputReader` | 仍在 `GameplaySystems.prefab`，无 `_inputActions` 时不向 Bus 写入 |
| 订阅时机 | `PlayerPresenter` 用 `_inputBound` 防重复；`Initialize` 先 `UnbindInput` 再 `BindInput` |
| Button 去重 | `PlayerInputBridge.TryConsumeButton` 同帧只消费一次 Attack/Skill/Item |

## Input Actions（`Assets/Input/GameInput.inputactions`）

| Action 名 | 默认绑定 | 消费者 |
|-----------|----------|--------|
| `Move` | WASD | `PlayerView` → BlendTree |
| `Attack` | 空格 / 鼠标左键 | `PlayerPresenter.TryAttack` → 扇形 `PlayerAttackHitbox` |
| `Skill1` | 1 | `PlayerPresenter` → `SkillSystem.TryCast` |
| `Skill2` | 2 | 同上 |
| `Skill3` | 3 | 同上 |
| `Item1` | 4 | 快捷栏槽位 0 |
| `Item2` | 5 | 快捷栏槽位 1 |
| `Item3` | 6 | 快捷栏槽位 2 |

代码常量见 `GameInputActions.cs`，须与上表 Action 名一致。改键请编辑 **Input Actions 资产**，不要改 C# 键位硬编码。

## 灵敏度（Post-MVP）

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `moveDeadzone` | 0.1 | 轴死区 |
| `castBufferTime` | 0.1s | 输入缓冲 |

## 玩家战斗配置

| 来源 | 字段 | 说明 |
|------|------|------|
| `PlayerMovementConfig` | `AttackDamage` | 初始化时写入 `PlayerModel`（默认 8） |
| `PlayerMovementConfig` | `MoveSpeed` | SO 预留；**当前** `PlayerModel.MoveSpeed` 硬编码 6 |
| `PlayerModel` | `RunSpeed` | 硬编码 9；满摇杆阈值见 `PlayerView._runInputThreshold` |

## 测试用适配器

`GameInputReader`：可选，直接向 `GameplayInputBus` 灌输入（EditMode / 自动化测试）。**勿**与 Player 上 `PlayerInput` 同时启用同一 Action Map。
