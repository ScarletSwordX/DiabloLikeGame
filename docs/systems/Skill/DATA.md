# Skill 数据

## SkillCatalog

集中注册可选 `SkillData`，供 `GameplaySessionConfig` 槽位下拉与运行时 Id 解析。

| API | 说明 |
|-----|------|
| `Entries[]` | 全部技能 SO |
| `TryGet(skillId)` | Id → `SkillData` |
| Editor **Rebuild From Folder** | 扫描 `Assets/Data/Skills/*.asset`（不含 Delivery/Effects 子目录） |

## Loadout 槽位（GameplaySessionConfig）

固定 3 槽，**index 即快捷键**：

| Index | 输入 | Inspector 字段 |
|-------|------|----------------|
| 0 | Skill1 | `_skillSlot0` |
| 1 | Skill2 | `_skillSlot1` |
| 2 | Skill3 | `_skillSlot2` |

槽位存 `SkillId` 字符串；空 Id = 空槽。Play Mode 改槽位后点 **Apply Loadout Now** 或调 `GameplayLoadoutService.Reload()`。

**槽位实例 UUID**：Loadout 应用到槽位时为每个非空槽生成 `InstanceUuid`（同槽同技能 Reload 时保留，换技能则新 UUID）。**冷却按 UUID 计**，同 `SkillId` 占多槽互不共享 CD。

**热重载边界**：改 `SkillData` SO 字段（冷却、伤害、`CastClip` 等）即时生效；换槽位/换 Id 需 Reload。

## SkillData（ScriptableObject）

身份、阶段时间、冷却；投递与效果由独立 SO 引用。

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | string | Catalog 与运行时查表键 |
| `DisplayName` | string | UI / 下拉显示 |
| `Icon` | Sprite | HUD 图标 |
| `CastClip` | AnimationClip | 写入对应槽位 Skill1/2/3 的 Override 动画 |
| `cooldownSeconds` | float | 冷却 |
| `Kind` | SkillKind | `Projectile` / `Channeled` / `StatusApply` |
| `PreCastSeconds` / `DurationSeconds` / `PostCastSeconds` | float | 阶段时间（Kind 决定哪些生效） |
| `isActiveSkill` | bool | 是否可施放 |
| `Delivery` / `Effect` | SO | 投递与效果子资产（见下） |

### SkillDeliveryData（子 SO）

`Assets/Data/Skills/Delivery/*.asset`，由 `SkillData.Delivery` 引用。

| 字段 | 说明 |
|------|------|
| `Kind` | `InstantArea`（即时范围）或 `Projectile`（投射物） |
| `Area` | **仅 InstantArea**：作用区域形状与半径 |
| `ProjectilePrefab` | **仅 Projectile**：投射物 Prefab |
| `SpawnOnAnimationEvent` | **仅 Projectile**：是否在动画 Event 时生成 |
| `Projectile` | **仅 Projectile**：`SkillProjectileSettings` |

#### SkillAreaSettings（InstantArea）

| 字段 | 类型 | 说明 |
|------|------|------|
| `shape` | SkillAreaShape | `Circle` / `Box` |
| `localOffset` | Vector3 | 相对施法者局部偏移（+Z 为面向前方） |
| `radius` | float | 圆形半径 |
| `boxSize` | Vector3 | 盒体全尺寸（Box） |

效果半径由 `ResolveEffectRadius()` 推导，供 `EffectSystem` AOE 结算。

#### SkillProjectileSettings（Projectile）

| 字段 | 类型 | 说明 |
|------|------|------|
| `FlightSpeed` | float | 飞行速度（米/秒）；≤0 时回退 Prefab 默认值 |
| `MaxFlightDistance` | float | 最大飞行距离（米）；超出后销毁投射物 |

`SkillData.Area` 仅在 Delivery 为 `InstantArea` 时有值；投射物技能通过 `ProjectileSettings` 读取速度/距离。

新增技能步骤见 [GUIDE_ADD_SKILL_ITEM.md](../../GUIDE_ADD_SKILL_ITEM.md)。

## EffectProfile（与道具共用）

三类效果 **不互斥**，可同时勾选：

| 分区 | 字段 | 说明 |
|------|------|------|
| **伤害** | `Damage.Enabled`, `Damage.Amount` | 对目标造成伤害 |
| **恢复** | `Heal.Enabled`, `Heal.Amount` | 治疗数值 |
| **状态** | `StatusEffects[]` | 每项：`StatusId`, `Magnitude`, `DurationSeconds`, `Stacks` |
| 范围 | `AreaRadius` | &gt;0 时对范围内敌人施加（状态/伤害） |

`StatusId` 必须在 [Status 表](../Status/DATA.md) 的 `StatusCatalog` 中注册。
