# Character 系统

## 体验目标

为玩家与敌人提供**统一的生命、受击、Buff、移动与战斗结算**体验；技能与道具通过标准端口作用到角色，HUD 与日志可清晰呈现 HP 与状态变化。

## 系统边界

### 负责

- 生命值、治疗、伤害结算（含护盾减伤）
- Buff 挂接、时长、叠层（规则见 DATA）
- 阵营（Faction）与可否被选为目标
- `MoveIntent` / `DamageRequest` 等端口的同步处理
- 状态变化事件的生产（`HealthChanged`、`BuffAdded` 等）

### 不负责

- 技能冷却、道具拾取判定（Skill / Item / WorldInteraction）
- 原始输入读取（Input）
- 配置表解析（Skill/Item 各自 DATA）
- UI 绘制（FeedbackUI）

## 依赖关系

| 方向 | 系统 | 说明 |
|------|------|------|
| 上游 | Effect | 传入 Damage/Heal/Buff 请求 |
| 上游 | Player Presenter | 普攻：`enemy.ProcessDamage` |
| 下游 | EventBus | 广播状态变化 |
| 下游 | FeedbackUI | 订阅事件展示血条 |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `MoveIntent` | In | struct | Player/Enemy 驱动 | Character 移动模块 | — |
| `MoveResult` | Out | struct | Character 移动模块 | 驱动层 | `blocked=true` |
| `DamageRequest` | In | struct | Effect, 环境陷阱 | Character 战斗模块 | — |
| `DamageResult` | Out | struct | Character 战斗模块 | Effect | `finalDamage=0` 当免疫 |
| `HealRequest` | In | struct | Effect | Character 战斗模块 | — |
| `HealResult` | Out | struct | Character 战斗模块 | Effect | 满血溢出 |
| `BuffApplyRequest` | In | struct | Effect | Character Buff 模块 | `Rejected: SlotFull` |
| `BuffApplyResult` | Out | struct | Character Buff 模块 | Effect | 见失败语义 |
| `HealthChanged` | Out | Event | Character | EventBus → UI | — |
| `BuffAdded` / `BuffRemoved` | Out | Event | Character | EventBus → UI | — |

## 子模块

- [Player](./Player/SYSTEM.md)：Root Motion 3C + Cinemachine 镜头 + Animator；**不**经 `MoveIntent` 驱动表现层移动
- [Enemy](./Enemy/SYSTEM.md)：训练假人，MVP-0 站桩

## 相关文档

- [DATA.md](./DATA.md) | [MVP.md](./MVP.md) | [INTERFACE.md](./INTERFACE.md)
