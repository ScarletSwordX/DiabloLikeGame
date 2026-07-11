# 技能 / 道具添加操作指南

面向策划与程序：如何以**数据驱动**方式新增或复制技能、道具，并接入 Main 场景试玩。

---

## 1. 总览

```text
创建 / 复制 SO  →  注册 Catalog  →  写入 GameplaySessionConfig 槽位  →  （可选）场景拾取 Prefab  →  Play 验证
```

| 层级 | 技能 | 道具 |
|------|------|------|
| 定义 SO | `SkillData` + `Delivery` + `Effect` | `ItemDefinition` |
| 注册表 | `SkillCatalog.asset` | `ItemCatalog.asset` |
| 装备/热键 | `_skillSlot0..2` | `_itemSlot0..2` |
| 世界拾取 | — | `PickupItem` Prefab + `ItemPickupView` |

核心代码**无需修改**即可接入新 Id（只要效果类型已在 `EffectSystem` / `StatusCatalog` 支持范围内）。

---

## 2. 新增技能

### 2.1 复制现有技能（推荐）

以 **HeavyFireball** 为例（CHALLENGE 扩展性用例；仓库已含现成资产 `Assets/Data/Skills/HeavyFireball.asset`）：

1. 在 Project 窗口复制 `Assets/Data/Skills/Fireball.asset` → 重命名为 `HeavyFireball.asset`（或直接使用已有 `HeavyFireball.asset` 作参考）。
2. 修改 **SkillData** 字段：
   - `Id`：`heavy_fireball`（全局唯一，小写+下划线）
   - `DisplayName`：重火球
   - `CooldownSeconds`：例如 `5`（仓库内 `HeavyFireball.asset` 默认 5s）
   - `Kind`：保持 `Projectile` / `StatusApply` / `Channeled` 之一
3. 打开引用的 **Effect** 子资产（如 `FireballEffect.asset` 的副本），提高 `Damage.Amount`。
4. 若需不同投射物行为，复制并修改 **Delivery** 子资产：
   - `Kind` = `Projectile`
   - `ProjectilePrefab`：投射物 Prefab
   - `Projectile.FlightSpeed`：飞行速度（米/秒）
   - `Projectile.MaxFlightDistance`：最大飞行距离（米）
   - 即时范围技能则设 `Kind` = `InstantArea`，配置 `Area` 半径/形状

### 2.2 从零创建

1. **Create → Gameplay → Skill Data**（或 Duplicate 模板）。
2. **Create → Gameplay → Skill Delivery Data** / **Skill Effect Data**，配置投递方式与 `ActionEffectProfile`。
3. 在 SkillData 上链接 `Delivery`、`Effect`、`CastClip`（Animator Override 用）。

### 2.3 SkillKind 与阶段时间

| Kind | 前摇 | 持续 | 后摇 | 典型 |
|------|------|------|------|------|
| `Projectile` | ✓ | — | ✓ | 火球、闪电球 |
| `Channeled` | ✓ | ✓ | ✓ | 引导型 |
| `StatusApply` | ✓ | — | — | 护盾 |

`PreCastSeconds` / `DurationSeconds` / `PostCastSeconds` 与 `CastClip` 动画 Event（`OnAnimationLaunchProjectile`）配合控制投射物生成时机，见 `SkillCastClipTiming`。

### 2.4 注册 SkillCatalog

1. 选中 `Assets/Data/Skills/SkillCatalog.asset`。
2. Inspector 点击 **Rebuild From Folder**（扫描 `Assets/Data/Skills/*.asset`，不含子目录），或手动把新 SO 拖入 `Entries`。
3. 确认 `TryGet("heavy_fireball")` 能解析（Play 前可在 SessionConfig Inspector 预览）。

### 2.5 绑定到按键

1. 打开 `Assets/Data/GameplaySessionConfig.asset`。
2. 设置槽位，例如：
   - `_skillSlot0.SkillId` = `fireball`
   - `_skillSlot1.SkillId` = `heavy_fireball`
   - `_skillSlot2.SkillId` = `shield`
3. **Play Mode** 中修改后，在 Inspector 点 **Apply Loadout Now**（需场景有 `GameplayLoadoutService`）。

槽位与输入对应关系：

| 槽位 | Input Action | 默认键 |
|------|--------------|--------|
| 0 | Skill1 | 1 |
| 1 | Skill2 | 2 |
| 2 | Skill3 | 3 |

冷却按槽位 **InstanceUuid** 独立计算，同一 SkillId 占多槽不共享 CD。

---

## 3. 新增道具

### 3.1 ItemDefinition 字段

| 字段 | 说明 |
|------|------|
| `Id` | Catalog 查表键，唯一 |
| `DisplayName` / `Description` | UI 与日志 |
| `Icon` | HUD 与拾取物 Billboard |
| `EffectProfile` | 伤害 / 治疗 / 状态（与技能共用结构） |
| `TriggerType` | 见下表 |
| `Charges` | **仅 OnUse**：每次拾取增加的次数 |

道具为**瞬时效果**，无 `PreCastSeconds` 等施法阶段字段。

### 3.2 TriggerType 行为

| TriggerType | 拾取时 | 热键使用 |
|-------------|--------|----------|
| **OnPickup** | 立即 `ApplyProfile`，消耗拾取实例 | 不可用 |
| **OnUse** | 仅 `AddCharges`，不立即生效 | 消耗 1 次后 `ApplyProfile` |

现有示例：

| Id | TriggerType | 说明 |
|----|-------------|------|
| `heal_potion` | OnPickup | 绿球，触碰即治疗 |
| `heal_potion_pickup` | OnUse | 拾取 +1 次，按 Item1 使用 |
| `fire_potion` | OnUse | 范围灼伤 |

### 3.3 注册 ItemCatalog

1. 选中 `Assets/Data/Items/ItemCatalog.asset`。
2. **Rebuild From Folder** 或手动加入 `Entries`。

### 3.4 绑定热键栏

在 `GameplaySessionConfig.asset`：

- `_itemSlot0.ItemId` = `heal_potion_pickup`
- `_itemSlot1.ItemId` = `fire_potion`

| 槽位 | Input | 默认键 |
|------|-------|--------|
| 0 | Item1 | 4 |
| 1 | Item2 | 5 |
| 2 | Item3 | 6 |

### 3.5 场景拾取物（可选）

1. 复制 Prefab：`Assets/Prefab/Item/` 下现有拾取物（如 `HealPotionTouchApplyPickup`）。
2. 修改 `ItemPickupView._itemDefinitionId` 为新 Id。
3. 子物体 **ItemIcon**：`SpriteRenderer` 指定 Icon，`PickupItemBillboard` 负责朝向摄像机。
4. 将 Prefab 拖入 `Main.unity` 场景。

---

## 4. 状态效果（Buff / DOT / 眩晕）

1. **Create → Gameplay → Status Definition**（或复制 `Dizzy` / `Burn`）。
2. 加入 `Assets/Data/Statuses/StatusCatalog.asset`。
3. 在技能/道具的 `EffectProfile.StatusEffects` 中填写 `StatusId`、 `DurationSeconds`、 `Magnitude`。
4. 范围效果设置 `EffectProfile.AreaRadius` > 0。

---

## 5. 验证清单

- [ ] Catalog 含新 Id，SessionConfig 槽位已填 Id
- [ ] Play：对应键位能释放/使用
- [ ] 改 SO 数值后表现变化（伤害、CD、治疗量）
- [ ] OnPickup 拾取一次后不可重复；OnUse 次数正确增减
- [ ] HUD 日志、飘字、DPS（若造成伤害）正常
- [ ] 可选：在 `GameplayConfigTests` 增加一条配置断言

---

## 6. 常用菜单与路径

| 菜单 | 作用 |
|------|------|
| Gameplay → Create P0 Default Assets | 生成默认 SO |
| Gameplay → Build Prefab Assets | 重建 Prefab |
| Gameplay → Ensure Scene Bootstrap | 补齐 GameBootstrap 组件 |
| SkillCatalog / ItemCatalog → Rebuild From Folder | 刷新注册表 |

| 路径 | 内容 |
|------|------|
| `Assets/Data/GameplaySessionConfig.asset` | Loadout 总配置 |
| `Assets/Data/Skills/` | 技能与 Delivery/Effects |
| `Assets/Data/Items/` | 道具 |
| `Assets/Data/Statuses/` | 状态 |
| `Assets/Input/GameInput.inputactions` | 键位 |

---

## 7. 相关文档

- [GAME_MANUAL.md](./GAME_MANUAL.md) — 试玩操作
- [systems/Skill/DATA.md](./systems/Skill/DATA.md) — 技能字段详解
- [systems/Item/DATA.md](./systems/Item/DATA.md) — 道具字段详解
- [systems/Status/DATA.md](./systems/Status/DATA.md) — 状态表
