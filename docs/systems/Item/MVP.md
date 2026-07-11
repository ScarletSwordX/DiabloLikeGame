# Item MVP

> 任务见 [TASK_BACKLOG.md](../../TASK_BACKLOG.md)（Item / Inventory / Loot 节）。

## 阶段一

- [x] 恢复效果（`EffectProfile.Heal`）
- [x] **地面拾取即时治疗**（`heal_potion`，OnPickup）
- [x] OnUse 道具拾取加次数、热键使用（与 OnPickup 区分）

## 阶段二

- [x] 多种道具 SO（即时治疗、可重复使用治疗、火焰范围灼伤）
- [x] 场景拾取 Prefab + 图标 Billboard
- [ ] 击杀掉落物 + 简化库存

## 阶段三

- [ ] 背包 `Inventory`：拾取进包、从包使用
- [ ] 敌人 `LootTable` 驱动掉落

## 已实现

- [x] `ItemPresenter` / 场景 `PickupItem`
- [x] 快捷栏 Item1–3（键 4/5/6）
- [x] `ItemCatalog` + SessionConfig 槽位

操作与字段见 [GUIDE_ADD_SKILL_ITEM.md](../../GUIDE_ADD_SKILL_ITEM.md)、[DATA.md](./DATA.md)。
