# Item 数据

## ItemCatalog

集中注册可选 `ItemDefinition`，供 `GameplaySessionConfig` 道具槽下拉与运行时 Id 解析。Editor 可用 **Rebuild From Folder** 扫描 `Assets/Data/Items/*.asset`。

当前注册示例：`fire_potion`、`heal_potion`、`heal_potion_pickup`、`heal_ball_pickupUse`（以 Catalog 为准）。

## Loadout 槽位

| Index | 输入 | 说明 |
|-------|------|------|
| 0 | Item1（默认键 4） | `_itemSlot0.ItemId` |
| 1 | Item2（默认键 5） | `_itemSlot1.ItemId` |
| 2 | Item3（默认键 6） | `_itemSlot2.ItemId` |

换槽后 Play Mode 下 **Apply Loadout Now** / `GameplayLoadoutService.Reload()`。

## ItemDefinition

独立 `ScriptableObject`，**瞬时效果**，无施法前摇/持续/后摇。效果结构与技能共用 `ActionEffectProfile`（见 [Skill/DATA.md](../Skill/DATA.md)）。

| 字段 | 说明 |
|------|------|
| `Id` | Catalog 查表键 |
| `DisplayName` / `Description` | UI / 日志 |
| `Icon` | HUD 与拾取物 Billboard |
| `EffectProfile` | 伤害 / 治疗 / 状态 / 范围 |
| `TriggerType` | `OnPickup` 或 `OnUse` |
| `Charges` | **仅 OnUse**：每次拾取增加的次数 |

## 拾取 vs 使用

| TriggerType | `TryPickup` | `TryUse` |
|-------------|-------------|----------|
| **OnPickup** | 立即 `ApplyProfile`；**不**增加热键栏次数 | 返回 Invalid |
| **OnUse** | 仅 `AddCharges`；**不**立即生效 | 消耗次数后 `ApplyProfile` |

- **拾取**：世界 `PickupItem` → `ItemPresenter.TryPickup`
- **使用**：`PlayerPresenter` → `ItemSystem.TryUse`

操作步骤见 [GUIDE_ADD_SKILL_ITEM.md](../../GUIDE_ADD_SKILL_ITEM.md)。

## 场景拾取 Prefab

`Assets/Prefab/Item/`：`HealPotionTouchApplyPickup`（OnPickup）、`HealPotionPickup` / `HealPotionTouchPickup`（OnUse 治疗）、`FirePotionTouchPickup`（OnUse 火焰）。

子物体 `ItemIcon`：`SpriteRenderer` + `PickupItemBillboard`（Prefab 内配置，非运行时生成）。
