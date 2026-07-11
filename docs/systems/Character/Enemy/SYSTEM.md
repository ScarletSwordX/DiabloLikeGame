# Character / Enemy 子模块

## 体验目标

提供**可承受技能效果的训练假人**；评审能观察 HP 下降与 Buff 状态，无需复杂 AI。

## 系统边界

### 负责

- 敌人/假人实体、Faction = Enemy
- MVP-0 站桩（moveSpeed = 0）
- 可选：极简巡逻（Post-MVP）

### 不负责

- 主动施法
- 掉落与战利品

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | Effect（伤害、Buff） |
| 下游 | Character 战斗/移动端口、EventBus |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `MoveIntent` | In | struct | Enemy 驱动（若有移动） | Character | — |
| `DamageRequest` | In | struct | Effect | Character（敌人实体） | — |
| `DamageResult` | Out | struct | Character | Effect / UI | — |

敌人**不产出** CastIntent。

## 相关文档

- [DATA.md](./DATA.md) | [MVP.md](./MVP.md)
