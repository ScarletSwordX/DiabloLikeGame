# Skill 系统

## 体验目标

玩家通过按键**主动释放技能**；冷却期间无法重复生效；新增技能仅需配置 + 键位绑定。

## 系统边界

### 负责

- 解析 `CastIntent` → `CastRequest`
- 施法前调用 **Cooldown 子模块**（见 [Cooldown/SYSTEM.md](./Cooldown/SYSTEM.md)）
- 成功后组装 `EffectRequest[]` 交给 Effect
- 发布 `SkillCastAttempted` / `Succeeded` / `Failed`

### 不负责

- 伤害/治疗结算（Effect → Character）
- 道具拾取（Item）
- UI 绘制（FeedbackUI）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | Input, SkillDefinition 配置 |
| 内部 | Skill/Cooldown |
| 下游 | Effect, EventBus |

## 统一输入输出

| 端口名 | 方向 | 所属 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|------|--------|--------|----------|
| `CastIntent` | In | 核心 | struct | Input | Skill | — |
| `CastRequest` | In | 核心 | struct | Skill 内部 | Skill 核心 | — |
| `CastResult` | Out | 核心 | struct | Skill | Input/UI | OnCooldown, NoTarget, InvalidState |
| `EffectRequest[]` | Out | 核心 | struct[] | Skill | Effect | — |
| `CooldownQuery` | In | 子模块 | struct | Skill 核心 | Cooldown | — |
| `CooldownQueryResult` | Out | 子模块 | struct | Cooldown | Skill 核心 | canCast=false |

## 子模块

- [Cooldown](./Cooldown/SYSTEM.md)：冷却计时，**非独立顶层系统**

## 相关文档

- [DATA.md](./DATA.md) | [MVP.md](./MVP.md) | [INTERFACE.md](./INTERFACE.md)
