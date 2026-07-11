# Input 系统

## 体验目标

操作**跟手、可配置**；通过 **Unity Input System** 的 `InputActionAsset` 统一映射；**玩法代码不硬编码 KeyCode**。

## 系统边界

### 负责

- Player 上 `PlayerInput` 读取设备
- `PlayerInputBridge` 转发为 `GameplayInputBus` 事件
- 与 `GameInputActions` 常量对齐的 8 种行动

### 不负责

- 移动物理与 Animator（`PlayerView` / `PlayerRootMotionMotor`）
- 相机跟随与构图（**Cinemachine**，见 [Character/Player/DATA.md](../Character/Player/DATA.md#main-unity-场景-3c)）
- 技能逻辑与冷却（Skill）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 下游 | `GameplayInputBus` → `PlayerView`（Move）、`PlayerPresenter`（战斗）→ `SkillSystem` / `ItemSystem` |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `Move` | Out | `Action<Vector2>` | PlayerInputBridge | PlayerView | — |
| `Attack` 等 | Out | `Action` | PlayerInputBridge | PlayerPresenter | Presenter 内 early return |
| `TryCast` | Out | 经 PlayerPresenter | PlayerPresenter | SkillSystem | 冷却/目标失败 |

## 相关文档

- [DATA.md](./DATA.md) | [INTERFACE.md](./INTERFACE.md)
