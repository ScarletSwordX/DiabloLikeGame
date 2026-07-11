# 系统统一输入/输出约定

全项目跨系统通信遵循本约定。各系统详情见 `docs/systems/<SystemId>/SYSTEM.md` 中的「## 统一输入输出」章节。

## 原则

1. **Request/Result 对**：同步逻辑用命名稳定的输入结构 + 输出结构，携带失败原因码。
2. **领域事件**：状态变化（生命、冷却、拾取）经 [EventBus](./systems/EventBus/SYSTEM.md) 广播，供 UI 订阅。
3. **禁止越权写状态**：Skill/Item/Effect 不得直接修改 `Character` 内部 HP 字段，必须走 `DamageRequest` 等端口。
4. **配置只读**：运行时读取 `SkillData` / `ItemDefinition`（ScriptableObject）；道具为瞬时效果，无施法阶段字段。不在玩法逻辑中硬编码伤害常数。
5. **输入中枢**：`PlayerInput` → `PlayerInputBridge` → `GameplayInputBus` → `PlayerView`（Move）/ `PlayerPresenter`（Attack/Skill/Item）；测试可用 `GameInputReader`。禁止 `SkillInputBridge` 直连技能。

## 端口表模板

每个系统 `SYSTEM.md` 须包含：

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| （示例） | In/Out | struct / Event | | | |

## 全局共享结构（概念）

### 身份与目标

```text
Faction: Player | Enemy | Neutral
EntityId: 场景内唯一标识
```

### Character 域

| 结构 | 主要字段 |
|------|----------|
| `MoveIntent` | `entityId`, `direction`, `speedMultiplier`, `deltaTime` |
| `MoveResult` | `displacement`, `actualSpeed`, `blocked` |
| `DamageRequest` | `sourceId`, `targetId`, `rawDamage`, `damageType` |
| `DamageResult` | `finalDamage`, `absorbedByShield`, `killed` |
| `HealRequest` | `sourceId`, `targetId`, `amount` |
| `HealResult` | `actualHeal`, `overheal` |
| `BuffApplyRequest` | `targetId`, `buffId`, `duration`, `stacks`, `magnitude` |
| `BuffApplyResult` | `instanceId`, `rejectedReason` |

### Skill 域

| 结构 | 主要字段 |
|------|----------|
| `CastIntent` | `casterId`, `skillSlotOrKey`, `timestamp` |
| `CastRequest` | `skillId`, `casterId`, `aimPoint`, `targetId?` |
| `CastResult` | `status`: Success \| InProgress \| OnCooldown \| NoTarget \| InvalidState |
| `CooldownQuery` | `skillId`, `casterId` |
| `CooldownQueryResult` | `canCast`, `remainingSeconds` |

### Item 域

| 结构 | 主要字段 |
|------|----------|
| `PickupRequest` | `pickerId`, `itemInstanceId`, `itemDefinitionId` |
| `PickupResult` | `status`: Success \| AlreadyConsumed \| Invalid |
| `UseRequest` | `userId`, `itemDefinitionId` |
| `UseResult` | `status`: Success \| OutOfCharges |

### Effect 域

| 结构 | 主要字段 |
|------|----------|
| `EffectRequest` | `effectType`, `magnitude`, `duration`, `radius`, `sourceId`, `targetIds[]` |
| `EffectResult` | `status`, `perTargetResults[]` |

### EffectType 枚举（基线）

`Damage` | `Heal` | `ApplySlow` | `ApplyShield` | `ApplySpeedBuff` | `DelayedDamage`

## 事件命名（EventBus）

| 事件名 | 生产者 | 典型载荷 |
|--------|--------|----------|
| `SkillCastAttempted` | Skill | skillId, casterId |
| `SkillCastSucceeded` | Skill | skillId, casterId |
| `SkillCastFailed` | Skill | skillId, reason |
| `EffectApplied` | Effect | effectType, sourceId, targetsAffected |
| `DamageDealt` | Combat/Character | sourceId, targetId, amount, worldPosition |
| `ItemPickedUp` | Item | itemId, pickerId |
| `ItemConsumed` | Item | itemId |
| `CooldownStateChanged` | Skill/Cooldown | skillId, remaining, phase |
| `HealthChanged` | Character | entityId, current, max |
| `BuffAdded` / `BuffRemoved` | Character | entityId, buffId |

## 实现阶段代码路径（建议）

与文档文件夹对齐：

```text
Assets/Scripts/Gameplay/
  Character/ Input/ Skill/ Item/ Effect/ Combat/
  EventBus/ Bootstrap/ Feedback/ FeedbackUI/ DebugTest/
Assets/Input/GameInput.inputactions
Assets/Data/Skills/ Items/ Statuses/
Assets/Prefab/DPSBoard.prefab  Assets/Prefab/Red Glow.prefab
```

## 玩家文档

- [GAME_MANUAL.md](./GAME_MANUAL.md) — 操作说明
- [GUIDE_ADD_SKILL_ITEM.md](./GUIDE_ADD_SKILL_ITEM.md) — 新增技能/道具
