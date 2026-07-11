# Character / Player 子模块

## 体验目标

玩家获得**跟手的第三人称移动**（相机相对输入 + Root Motion + CharacterController），移动与转向分离驱动 Animator BlendTree，战斗输入转化为 Skill / Item / Attack。镜头由 **Cinemachine** 跟随，不在代码里手写相机位移。

## 3C 分工

| C | 实现 | 场景 / 代码 |
|---|------|-------------|
| **Character** | Root Motion + `CharacterController` | `Player.prefab`：`Animator`、`PlayerRootMotionMotor`、CC（高 1.8 / 半径 0.44） |
| **Camera** | Cinemachine 第三人称跟随 | `Main.unity`：`Main Camera` + `Virtual Camera`（见 [DATA.md § Main 场景](./DATA.md#main-unity-场景-3c)） |
| **Control** | Unity Input System → Bus | `PlayerInput` + `PlayerInputBridge` → `PlayerView` / `PlayerPresenter` |

```text
WASD / 摇杆
  → PlayerInputBridge.OnMove
  → PlayerView：相机 forward/right 投影 → VerticalSpeed / TurnSpeed
  → Animator Root Motion
  → PlayerRootMotionMotor → CharacterController

Virtual Camera（Follow = Player 根节点）
  → CinemachineBrain → Main Camera
  → PlayerView.ResolveCamera() 使用同一视口做转向基准
```

## 系统边界

### 负责

- `PlayerInput` + `PlayerInputBridge`：Action → `GameplayInputBus`
- `PlayerView`：订阅 `Bus.Move`；线速度 / 转向 → `VerticalSpeed` / `TurnSpeed`；动作动画与门控
- `PlayerModel`：移速目标值、活动状态（`Idle` / `Moving` / `Attacking` / …）
- `PlayerRootMotionMotor`：Animator 根运动 → `CharacterController`
- `PlayerPresenter`：Attack / Skill / Item；普攻对 **敌人** `ProcessDamage`

### 不负责

- 相机轨道、阻尼、距离（**Cinemachine** Inspector 配置，非 `PlayerView`）
- HP 结算规则（Character 父模块 `CharacterModel`）
- 技能冷却（Skill/Cooldown）
- UI（FeedbackUI）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | Input（`PlayerInput`）、Camera（Cinemachine → `Camera.main`；可选绑 `PlayerView._camera`） |
| 下游 | Character（伤害/治疗）、Skill（Cast）、Animator |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `Move` | In | Vector2 | PlayerInputBridge | **PlayerView**（非 Presenter） | — |
| `Attack` / `Skill*` / `Item*` | In | event | PlayerInputBridge | PlayerPresenter | `IsActionBlocked` 时忽略 |
| Root motion delta | In | Vector3 | Animator | PlayerRootMotionMotor → CC | CC 阻挡 |

## 移动实现要点

1. **线速度**：`_currentLinearSpeed` 向 `PlayerModel.MoveSpeed` / `RunSpeed` 趋近 → 归一化写入 `VerticalSpeed`  
2. **转向**：**当前 Main Camera** 的 forward/right 投影地面 → 输入映射世界方向 → 转角色本地 → `Atan2` 得弧度角差 → `√|θ|` → `TurnSpeed`  
3. **位移**：Animator **Root Motion**（非 CC 直接改 Transform 位置）  
4. **状态**：`PlayerModel.State` 由 View（locomotion / 动画）与 Presenter（战斗）共同维护；`AllowsLocomotion` 控制是否写 BlendTree  

## 相关文档

- [DATA.md](./DATA.md)（含 Main 场景 Cinemachine 接线）| [MVP.md](./MVP.md) | [../ANIMATION.md](../ANIMATION.md)
