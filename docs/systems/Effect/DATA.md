# Effect 数据基线

## 效果类型默认系数

| effectType | 默认 magnitude | 默认 duration | 说明 |
|------------|----------------|---------------|------|
| `Damage` | 10 | — | 直伤 |
| `Heal` | 30 | — | 治疗 |
| `ApplySlow` | 0.5 | 3s | 移速×0.5 |
| `ApplyShield` | 0.5 | 5s | 减伤 50% |
| `ApplySpeedBuff` | 1.5 | 5s | 移速×1.5 |
| `DelayedDamage` | 10 | 2s | 陷阱延迟伤害（Post-MVP） |

配置中的 `magnitude` **覆盖**上表默认。

## 目标解析规则

| 模式 | 规则 |
|------|------|
| 单体 | `targetId` 来自 Cast |
| 自身 | `sourceId == targetId` |
| 范围 | `Physics.OverlapSphere`，Faction=Enemy |

## 实现

`Assets/Scripts/Effect/EffectApplierRegistry.cs` — 效果类型 → `IEffectApplier`
