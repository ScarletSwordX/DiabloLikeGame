# Character 数据基线

## 共性数值

| 字段 | 默认值 | 调参范围 | 说明 |
|------|--------|----------|------|
| `defaultMaxHp` | 100 | 50–500 | 玩家与假人初始上限 |
| `buffSlotMax` | 8 | 4–16 | Buff 实例上限 |
| `invincibleDurationOnHit` | 0 | 0–0.5 | 原型无受击无敌 |
| `defaultFaction` | 按实体 | — | Player / Enemy |

## Buff 叠层规则（基线）

| buffId | 默认 duration | 叠层 | magnitude 含义 |
|--------|---------------|------|----------------|
| `shield` | 5s | 刷新时长 | 减伤比例 0.5（50%） |
| `slow` | 3s | 不叠层，刷新 | 移速倍率 0.5 |
| `speed_boost` | 5s | 不叠层，刷新 | 移速倍率 1.5 |

## 伤害结算（基线公式）

```text
finalDamage = rawDamage * (1 - shieldReduction) * targetDamageTakenMultiplier
```

- `shieldReduction`：来自 `shield` Buff，0 表示无护盾
- 结果下限 0，上限不超过当前 HP

## 配置资产（实现阶段）

- 角色基线：`Assets/Data/Character/CharacterBaseline.asset`（可选 ScriptableObject）
- 玩家/敌人覆盖：见 [Player/DATA.md](./Player/DATA.md)、[Enemy/DATA.md](./Enemy/DATA.md)
