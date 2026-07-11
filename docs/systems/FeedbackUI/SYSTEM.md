# FeedbackUI 系统

## 体验目标

试玩时玩家能看懂：**当前技能、冷却剩余、血量、命中/治疗/拾取、DPS、伤害飘字**。

## 系统边界

### 负责

- 技能/道具热键栏（`GameplayHudHotbarView`）
- 冷却与次数展示（订阅 `CooldownStateChanged`、读 Item charges）
- 玩家/敌人 HP 文本
- 战斗日志（TMP）
- **DPS 面板**（`GameplayDpsBoardView` + `GameplayDpsTracker`）
- 协调 MVP 就绪后 `GameplayHud.Bind`

### 不负责

- 修改玩法状态
- 冷却计时（Skill/Cooldown）
- 伤害飘字渲染（由 `IGameplayFeedback` / Damage Numbers Pro 实现，见 `GameplayFeedbackProvider`）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | EventBus, Skill（只读）, Item（只读）, `GameplayMvpSession` |
| 并行 | Feedback（飘字）、Combat（`DamageDealtEvent`） |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `GameEvent` | In | Event | EventBus | FeedbackUI | — |
| `DamageDealtEvent` | In | Event | Combat | DpsTracker | — |

**无修改型 Output**；纯展示。

## 相关文档

- [DATA.md](./DATA.md)
- [GAME_MANUAL.md](../../GAME_MANUAL.md)
