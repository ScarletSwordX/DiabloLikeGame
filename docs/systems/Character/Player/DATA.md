# Player 数据基线（3C）

## PlayerModel（运行时）

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `AttackDamage` | 8 | 可由 `PlayerMovementConfig` 在 `Initialize` 时覆盖 |
| `MoveSpeed` | 6（只读） | 行走目标线速度 m/s，暂硬编码 |
| `RunSpeed` | 9（只读） | 奔跑目标线速度 m/s，暂硬编码 |
| `State` | `Idle` | `PlayerActivityState` 枚举 |
| `AllowsLocomotion` | — | 死亡/受击/攻击/技能/道具期间为 false |

**不含**瞬时移动输入向量；输入由 `PlayerView` 本地 `_moveInput` 持有。

### PlayerActivityState

`Idle` | `Moving` | `Attacking` | `UsingSkill` | `UsingItem` | `Hit` | `Dizzy` | `Dead`

## PlayerView（Inspector 调参）

| 字段 | 默认 | 说明 |
|------|------|------|
| `_camera` | — | 相机相对转向；**Main 场景未绑**，走 `Camera.main`（Cinemachine Brain 输出） |
| `_linearAcceleration` | 24 | 线速度趋近速率 |
| `_runInputThreshold` | 0.95 | 摇杆幅度 ≥ 此值用 `RunSpeed` |
| `_turnSpeedScale` | 1 | 转向 Blend 缩放 |
| `_verticalSpeedBlendMax` | 1.2（Prefab）；Main 实例覆盖 **6** | `VerticalSpeed` 上限映射 |
| `_locomotionDamp` | 0.12 | Blend 参数平滑 |
| `_fallbackActionLockSeconds` | 0.5 | 技能/道具无独立 Clip 时的占位锁 |

## Prefab 组件（Player）

| 组件 | 说明 |
|------|------|
| `PlayerInput` | Input System，`Default Action Map = Gameplay`，Send Messages |
| `PlayerInputBridge` | Send Messages → `GameplayInputBus`（**已含于 Prefab**） |
| `PlayerView` | Animator + Bus.Move |
| `PlayerPresenter` | 战斗输入 |
| `PlayerRootMotionMotor` | Root Motion → CC |
| `CharacterController` | 高 **1.8**，半径 **0.44**，`center.y ≈ 0.79` |
| `Animator` | `Apply Root Motion = true`，Controller = `PlayerAnimator.controller` |
| `CharacterEntity` / `CharacterPresenter` / `CharacterView` | 共用 Character 战斗与闪色 |
| `PlayerController` | `GameBootstrap` 调用 `Initialize` 接线 |

**不再**使用 Rigidbody / CapsuleCollider 做玩家位移。

## Main.unity 场景（3C）

`Assets/Scenes/Main.unity` 根节点与 3C 相关对象：

| 场景对象 | 组件 / 作用 |
|----------|-------------|
| **Player** | `Player.prefab` 实例；Cinemachine **Follow** 目标 = 根 `Transform` |
| **Virtual Camera** | `CinemachineVirtualCamera`，Priority 10 |
| **Main Camera** | `Camera` + **`CinemachineBrain`** + URP Additional Camera Data；Tag `MainCamera` |
| **GameBootstrap** | 同物体挂载 Effect/Item/Skill；引用 Player / Enemy / Pickup |
| **Plane** | 地面碰撞（MeshCollider） |
| Directional Light / EventSystem / Canvas(HUD) | 环境与 UI |

### Cinemachine 配置（Virtual Camera）

| 项 | Main 场景值 | 说明 |
|----|-------------|------|
| **Body** | `Cinemachine3rdPersonFollow` | 第三人称跟随（非旧版 `GameplayCameraRig` 顶视锁定） |
| **Follow** | Player 根 `Transform` | 与 Prefab 实例 `Player` 同级节点 |
| **Look At** | （空） | 由 3rd Person Follow + VCam 旋转决定朝向 |
| **Camera Distance** | **14** | 与角色水平距离 |
| **Tracked Object Offset** | **(0, 0.93, 0)** | 瞄准点约在角色胸口高度 |
| **VCam 旋转（初始）** | 约 **(45°, -55°, 0°)** | 斜俯第三人称视角 |
| **Lens FOV** | 45 | 与 Main Camera 一致 |
| **Damping X/Z** | 1 | 水平跟随阻尼 |

包版本：`com.unity.cinemachine` **2.10.7**。

### 数据流（与场景一致）

```text
PlayerInput（Player 上）
  → PlayerInputBridge
  → GameplayInputBus.Move → PlayerView
  → Animator（Walk BlendTree + 动作态）
  → PlayerRootMotionMotor → CharacterController

Cinemachine Virtual Camera（Follow Player）
  → CinemachineBrain（Main Camera）
  → PlayerView 用 Camera.main 做相机相对移动方向
```

`GameBootstrap` **不**创建或绑定相机；VCam Follow 在 Editor 中拖 Player 根节点即可。

## 输入绑定

| 动作 | 默认键 | 说明 |
|------|--------|------|
| Move | WASD | → `PlayerView` locomotion |
| Attack | 空格 / LMB | → `TryPlayAttack` + 敌人伤害 |
| Skill1–3 | 1–3 | 技能 SO |
| Item1–3 | 4–6 | 快捷栏 |

## 实现资产

| 资产 | 路径 |
|------|------|
| 玩家 Prefab | `Assets/Prefab/Player.prefab` |
| Animator | `Assets/Animation/PlayerAnimator.controller` |
| 主场景 | `Assets/Scenes/Main.unity` |
| Cinemachine | 场景内 `Virtual Camera` + `Main Camera`（非独立 Prefab） |
| 旧相机工具 | `GameplayCameraRig.cs` / Editor `GameplayCamera.prefab` — **Main 未使用** |
