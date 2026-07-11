# WorldInteraction 数据基线

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `pickupRadius` | 1.0 | 拾取触发体半径 |
| `pickupLayer` | Player | 仅玩家层触发 |
| `trapRadius` | 3.0 | frost_trap 范围 |
| `trapTriggerOnce` | true | 陷阱单次触发 |

## 场景物体组件

| 组件 | 字段 |
|------|------|
| `PickupItem` | `itemDefinitionId`, `itemInstanceId` |
| `DamageTrap` | `skillId` 或内嵌 EffectSpec |
