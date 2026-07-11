# Enemy 数据基线

## 训练假人（MVP-0 默认）

| 字段 | 默认值 | 调参范围 | 说明 |
|------|--------|----------|------|
| `maxHp` | 100 | 50–1000 | 与玩家同值便于对比火球伤害 |
| `moveSpeed` | 0 | 0–5 | **0 = 站桩**（训练假人默认）
| `faction` | Enemy | — | — |
| `showHealthBar` | true | — | 世界空间或屏幕 UI |
| `canDie` | false | — | 假人 HP 可扣至 1 或循环重置 |

## 受击反馈（UI 用）

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `hitFlashDuration` | 0.1s | 材质变色 |
| `damageNumberEnabled` | true | 飘字伤害 |

## 实现资产

`Assets/Prefabs/TrainingDummy.prefab`
