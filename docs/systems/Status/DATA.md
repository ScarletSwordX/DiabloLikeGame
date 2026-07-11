# Status 状态表

## StatusDefinition（单条状态）

路径：`Assets/Data/Statuses/*.asset`

| 字段 | 说明 |
|------|------|
| `Id` | 全局唯一，与 `StatusEffectPart.StatusId` 对应 |
| `DisplayName` | 显示名 |
| `Kind` | `DamageReduction` / `MoveSpeedMultiplier` |
| `Description` | 策划说明 |

| Kind | Magnitude 含义 |
|------|----------------|
| `DamageReduction` | 减伤比例（0.5 = 50%） |
| `MoveSpeedMultiplier` | 移速倍率（0.5 减速，1.5 加速） |

## StatusCatalog（状态表）

路径：`Assets/Data/Statuses/StatusCatalog.asset`

- `Entries[]`：所有可用的 `StatusDefinition`
- 运行时由 `EffectSystem.Initialize(catalog)` 注入角色模型
- 菜单：**Gameplay → Create P0 Default Assets** 生成 `shield` / `slow` / `speed_boost`

## 与技能/道具的关系

技能或道具的 `EffectProfile.StatusEffects` 只引用 `StatusId`；具体行为由本表定义，不在技能 SO 里重复写规则逻辑。
