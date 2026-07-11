# FeedbackUI 数据基线

## 血条 / HP 文本

| 字段 | 说明 |
|------|------|
| 玩家 HP | `GameplayHud` `_playerHpText` |
| 敌人 HP | `GameplayHud` `_enemyHpText` |
| 事件源 | `HealthChangedEvent` |

## 冷却与热键栏

| 字段 | 说明 |
|------|------|
| 技能槽 | 3 槽 → Skill1/2/3 |
| 道具槽 | 3 槽 → Item1/2/3，显示剩余次数 |
| 事件源 | `CooldownStateChangedEvent`、`LoadoutChangedEvent` |

## 战斗日志

| 类型 | 来源事件 / 说明 |
|------|-----------------|
| 伤害/治疗 | `CombatLogEvent` |
| 施法失败 | `SkillCastFailedEvent` |
| 拾取 | `ItemPickedUpEvent` |

## DPS 面板（`Assets/Prefab/DPSBoard.prefab`）

| 组件 | 说明 |
|------|------|
| `GameplayDpsBoardView` | 刷新 `DPS: {值}` 文本；Reset 按钮 |
| `GameplayDpsTracker` | 挂于 `GameBootstrap`；统计玩家 `SourceId` 的 `DamageDealtEvent` |

## 伤害飘字

| 配置 | 路径 |
|------|------|
| Prefab | `Assets/Prefab/Red Glow.prefab` |
| Provider | `GameBootstrap` → `GameplayFeedbackProvider._damageNumberPrefab` |
| 触发 | `CharacterPresenter.ProcessDamage` → `GameplayCombatFeedback.NotifyDamageDealt` |

## 布局（Main 场景）

- 热键栏 + 日志 + HP：`GameplayHud` Prefab 实例
- DPS：Canvas 下 `DPSBoard` 实例（画面上方）
- 自测按钮：HUD 右上角 **Run Self Test**

试玩说明见 [GAME_MANUAL.md](../../GAME_MANUAL.md)。
