# 版本路线图（Diablo-like Demo）

**用途**：记录 v0.1 已交付基线与后续版本规划。状态以本表为准。

- 愿景：[GAME_CORE.md](./GAME_CORE.md)  
- 系统索引：[SYSTEM_DESIGN.md](./SYSTEM_DESIGN.md)  
- 程序架构：[ARCHITECTURE.md](./ARCHITECTURE.md)  

**任务 ID**：`BASE` 已交付基线 | `P1`/`P2`/`P3` 后续规划 | `DLV` 工程交付  

**分类结构**：第一层 **版本/阶段** → 第二层 **系统**（对应 `docs/systems/<SystemId>/`）。

---

## v0.1 已交付（当前版本）

| 类别 | 内容 |
|------|------|
| 战斗 | 移动、普攻、3 技能槽、道具拾取/使用、冷却、状态、投射物 |
| 表现 | 玩家/敌人动画、Cinemachine 3C、伤害飘字、HUD、DPS |
| 数据 | SO + Catalog + `GameplaySessionConfig` Loadout |
| 测试 | EditMode 17 条 + Play 自检 4 条 |
| 文档 | `docs/` 架构与系统说明、README、第三方资源清单 |

---

## 分阶段总览

| 阶段 | 目标 | 包含 | 刻意不做 |
|------|------|------|----------|
| **一** | 打假人 → 掉血 → 用恢复 | 移动；1 个**非指向**伤害技能；地面拾取**即时治疗**；可死亡无动画假人；HUD/自测 | 经验、背包、死亡掉落、配装 UI、敌人动画 |
| **二** | 内容 + 表现 + 拾取链路 | ≥3 技能/道具 SO；状态表扩展；击杀掉落 + 靠近拾取 + **简化库存**；敌人/玩家动画；核心验证用例 | 经验、解锁、完整背包 UI、自由配装 UI |
| **三 a** | 成长 | 击杀经验、等级、`SkillUnlockTable`、解锁通知 | — |
| **三 b** | 收纳 | 背包 UI、消耗品从包使用、`SkillLoadout` 配装、`LootTable` | — |

**阶段策略**：先战斗闭环 → 再内容与拾取链路 → 最后元系统；阶段一须发 `EnemyDied`（可无掉落）；阶段二须做简化库存。

---

## v0.1 与后续规划对照

| v0.1 已有 | 后续版本目标 |
|------|------|
| Main 场景 Player/Enemy Prefab + 系统/HUD/拾取 Prefab | 敌人可死亡、掉落与背包 |
| fireball + lightingball + shield 三技能 | 非指向 AOE、自由配装 UI |
| 场景药水 + 快捷栏次数 | 完整背包与消耗品管理 |
| 站桩假人 | `EnemyDiedEvent`、Loot 表驱动 |
| 固定 SessionConfig Loadout | `SkillLoadout` 运行时配置 |

---

## 已完成基线（BASE）

### Character — `docs/systems/Character/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-CHR-01 | 战斗/移动 MVP | `CharacterEntity` 门面 | 完成 |
| BASE-CHR-02 | `HealthChanged` 事件 | EventBus | 完成 |

### Character / Player — `docs/systems/Character/Player/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-PLY-01 | Input → Bus；Move 进 View；战斗进 Presenter；**Main 场景 Cinemachine 3C** | `GameplayInputBus` + Root Motion + VCam | 完成 |

### Character / Enemy — `docs/systems/Character/Enemy/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-ENM-01 | 站桩假人 MVP | `TrainingDummyMarker` | 完成 |

### Skill — `docs/systems/Skill/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-SKL-01 | 施法管道 + 冷却 | `SkillPresenter` / `CooldownService` | 完成 |

### Skill / Cooldown — `docs/systems/Skill/Cooldown/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-SKL-02 | 冷却子模块 | `CooldownService` in Model | 完成 |

### Item — `docs/systems/Item/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-ITM-01 | 拾取/使用管道 MVP | `ItemPresenter` | 完成 |

### Effect — `docs/systems/Effect/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-EFT-01 | 玩法结算 | `EffectSystem` / `ActionEffectApplier` | 完成 |

### Status — `docs/systems/Status/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-STA-01 | `StatusCatalog` + `StatusDefinition` | 运行时/代码 | 完成 |

### EventBus — `docs/systems/EventBus/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-EVT-01 | 事件总线 + 战斗/技能/道具事件 | `GameEventBus` | 完成 |

### Input — `docs/systems/Input/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-INP-01 | Input System + `GameplayInputBus` | 8 种行动 | 完成 |

### WorldInteraction — `docs/systems/WorldInteraction/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-WLD-01 | 场景 `PickupItem` 触发 | `PickupItem` Prefab | 完成 |

### FeedbackUI — `docs/systems/FeedbackUI/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-UI-01 | HUD：HP、技能 CD、日志 | `GameplayHud` Prefab | 完成 |

### Feedback — `docs/systems/Feedback/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-FDB-01 | `IGameplayFeedback` 空实现 | `NullGameplayFeedback` | 完成 |

### DebugTest — `docs/systems/DebugTest/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-DBG-01 | T01–T04 + Run Self Test | `GameplayConfigTests` | 完成 |

### 工程交付 — 仓库与文档

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| DLV-01 | Main.unity + GameBootstrap | 可 Play | 完成 |
| DLV-PREF-01 | 场景对象 Prefab 化 | `Assets/Prefab/` + 无运行时生成 | 完成 |
| DLV-02 | README 与 docs 交付版 | 运行、测试、第三方资源说明 | 完成 |
| DLV-03 | 第三方资源不入库 | `Packages/THIRD_PARTY_ASSETS.md` + `.gitignore` | 完成 |

### Docs

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| BASE-DOC-01 | 策划体系 + ARCHITECTURE | `docs/` | 完成 |

---

## 阶段一 — 战斗 MVP（规划中）

### Character / Enemy — `docs/systems/Character/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-CHR-01 | 假人 HP 归零可死亡 | 发 `EnemyDiedEvent` | 规划中 |
| P1-CHR-02 | 假人无动画 | 不依赖 Animator Controller | 完成 |

### Skill — `docs/systems/Skill/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-SKL-01 | 默认技能改为非指向 AOE（`nova`） | `Targeting.NonTargeted` + `AreaRadius` | 规划中 |
| P1-SKL-02 | 收敛为 1 个战斗技能键 | 移除/禁用 shield 等 | 规划中 |
| P1-SKL-03 | 占位第三技能 SO | 仅配置存在，可不绑键 | 规划中 |

### SkillLoadout — `docs/systems/SkillLoadout/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-LD-01 | **预留**装备槽数据结构 | `equippedSkillIds[]` 写死 Bootstrap | 规划中 |

### Item — `docs/systems/Item/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-ITM-01 | 地面拾取 **即时治疗** | 拾取即 `Heal`；无背包语义 | 部分完成 |
| P1-ITM-02 | 移除阶段一对快捷栏次数的依赖 | 不用 Item1–3 当背包 | 规划中 |

### Effect — `docs/systems/Effect/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-EFT-01 | 非指向伤害对范围内敌生效 | `EffectProfile.AreaRadius` | 待检 |

### EventBus — `docs/systems/EventBus/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-EVT-01 | 新增 `EnemyDiedEvent` | Progression/Loot 可订阅 | 规划中 |

### Input — `docs/systems/Input/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-INP-01 | 收敛阶段一输入 | 1 技能 + 移动 + 拾取 | 规划中 |

### DebugTest — `docs/systems/DebugTest/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-DBG-02 | 自测覆盖死亡 / 非指向伤害 | 扩展用例 | 规划中 |

### Docs

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P1-DOC-01 | GAME_CORE / 本表 / SYSTEM_DESIGN 对齐 | 文档与代码一致 | 完成 |

---

## 阶段二 — 内容 + 表现 + 拾取链路（规划中）

### Character — `docs/systems/Character/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-CHR-01 | 敌人受击/死亡表现 | `EnemyView`：`Hit` / `Die01` / `Die01_Stay` | 部分完成 |

### Character / Player — `docs/systems/Character/Player/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-CHR-02 | 玩家 locomotion + 战斗动画 | `Walk` BlendTree + `Attack1` + `PlayAnimation`/`OnFinish` + **Cinemachine 3C** | 部分完成 |

### Skill — `docs/systems/Skill/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-SKL-01 | ≥3 种技能 SO | 指向/非指向/状态混合 | 部分完成 |
| P2-CHL-01 | 复制 `HeavyFireball` 接入验证 | 配置扩展性用例 | v0.1 已完成 |

### SkillLoadout — `docs/systems/SkillLoadout/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-DOC-LD | 新建 `systems/SkillLoadout/SYSTEM.md` | 接口约定 | 规划中 |

### Item — `docs/systems/Item/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-ITM-01 | ≥2 种道具 SO | 治疗、移速 Buff 等 | 规划中 |

### Inventory — `docs/systems/Inventory/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-INV-01 | **简化库存**（列表/计数） | 拾取入列表，无格子 UI | 规划中 |
| P2-DOC-INV | 新建 `systems/Inventory/SYSTEM.md` | 与 Loot 拾取约定 | 规划中 |

### Loot — `docs/systems/Loot/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-Loot-01 | 敌人死亡生成掉落物 | 订阅 `EnemyDiedEvent` | 规划中 |
| P2-Loot-02 | 靠近自动拾取 | 对接 `P2-INV-01` | 规划中 |
| P2-DOC-Loot | 新建 `systems/Loot/SYSTEM.md` | 掉落物 Prefab 约定 | 规划中 |

### Status — `docs/systems/Status/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-STA-01 | 扩展状态表与 `ActionEffectProfile` | 护盾/减速等 SO 条目 | 规划中 |
| P2-DOC-STA | 补全 `Status/DATA.md` | 与任务对齐 | 规划中 |

### Progression — `docs/systems/Progression/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-DOC-PRG | 新建 `systems/Progression/SYSTEM.md` | 经验/解锁设计 | 规划中 |

### FeedbackUI — `docs/systems/FeedbackUI/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-UI-01 | Buff/掉落/简化库存提示（可选） | 扩展 HUD | 规划中 |

### Feedback — `docs/systems/Feedback/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-FDB-01 | Feel 实现 `IGameplayFeedback` | 命中/治疗/拾取 VFX | 规划中 |

### DebugTest — `docs/systems/DebugTest/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P2-CHL-02 | 核心验证用例扩展（死亡、非指向等） | 配置/冷却/拾取/扩展/自测 | 规划中 |

---

## 阶段三 a — 成长

### Progression — `docs/systems/Progression/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-PRG-01 | 击杀获得经验 | `XpGainedEvent`；订阅 `EnemyDied` | 规划中 |
| P3-PRG-02 | 等级与 `SkillUnlockTable` SO | `Assets/Data/Progression/` | 规划中 |
| P3-PRG-03 | 解锁通知 UI | HUD 或弹条 | 规划中 |

### EventBus — `docs/systems/EventBus/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-EVT-01 | 新增 `SkillUnlockedEvent` 等 | UI 解锁通知 | 规划中 |

---

## 阶段三 b — 收纳

### Inventory — `docs/systems/Inventory/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-INV-01 | 背包数据 + UI | 完整背包 | 规划中 |

### Item — `docs/systems/Item/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-INV-02 | 从背包使用消耗品 | 替代快捷栏 | 规划中 |

### SkillLoadout — `docs/systems/SkillLoadout/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-LD-01 | 已解锁技能 → 技能栏自由配置 | UI + Presenter | 规划中 |

### Loot — `docs/systems/Loot/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-Loot-01 | `LootTable` SO 驱动掉落 | 按敌人配置 | 规划中 |

### FeedbackUI — `docs/systems/FeedbackUI/`

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| P3-UI-01 | 背包 UI + 技能配置 UI | 与 Inventory/Loadout 联动 | 规划中 |

---

## 交付 / 工程（跨阶段）

### Docs

| ID | 任务 | DoD | 状态 |
|----|------|-----|------|
| DOC-ARCH | [ARCHITECTURE.md](./ARCHITECTURE.md) | 程序架构 + UML | 完成 |

---

## LEGACY — 原训练场任务处置

| 原 ID | 处置 | 对应本表 |
|-------|------|----------|
| SKL-P0 fireball+shield | nova + 阶段二 shield | P1-SKL-01/02，P2-SKL-01 |
| ITM-P0 heal + 快捷栏 | 即时拾取 + 阶段三背包 | P1-ITM-01/02，P3-INV-* |
| SKL-P1-03 HeavyFireball | 阶段二 | P2-CHL-01 |
| ITM-P1-01 dash_boots | 阶段二道具 | P2-ITM-01 |

---

## 核心验证用例

| 用例 | 说明 | 对应任务 | 版本 |
|------|------|----------|------|
| 1 | 改配置改表现 | BASE-EFT；改 `Fireball.asset` | v0.1 ✅ |
| 2 | 冷却连按 + UI | BASE-SKL-02，BASE-UI | v0.1 ✅ |
| 3 | 拾取/消耗 | BASE-ITM；`P3-INV-*` | v0.1 部分 / 三 b |
| 4 | 复制 HeavyFireball | `heavy_fireball` 已注册 | v0.1 ✅ |
| 5 | 自测路径 | BASE-DBG-01 | v0.1 ✅ |
| ≥3 配置项 | 3 技能 + 多道具 SO | 已满足 | v0.1 ✅ |

---

## Prefab 清单（Main 场景）

| Prefab | 路径 | 用途 |
|--------|------|------|
| Player | `Assets/Prefab/Player.prefab` | 玩家角色 + MVP 组件 |
| Enemy | `Assets/Prefab/Enemy.prefab` | 训练假人 + MVP 组件 |
| GameplaySystems | `Assets/Prefab/GameplaySystems.prefab` | Effect/Item/Skill/Feedback/DebugTest（含未激活的 `GameInputReader`） |
| GameplayHud | `Assets/Prefab/GameplayHud.prefab` | HUD Canvas |
| HealPotionPickup | `Assets/Prefab/HealPotionPickup.prefab` | 地面治疗拾取物 |

场景维护菜单：**Tools → Gameplay → Build Prefab Assets**（生成 Prefab 资产；**不**自动写入 GameBootstrap 引用）。

---

## 系统 ↔ 任务 ID 速查

| 系统文件夹 | 任务 ID |
|------------|---------|
| `Character/` | BASE-CHR, P1/P2-CHR |
| `Character/Player/` | BASE-PLY, P2-CHR-02 |
| `Character/Enemy/` | BASE-ENM, P1-CHR-* |
| `Skill/` | BASE-SKL, P1/P2-SKL, P2-CHL |
| `Skill/Cooldown/` | BASE-SKL-02 |
| `SkillLoadout/` | P1-LD, P2-DOC-LD, P3-LD |
| `Item/` | BASE-ITM, P1/P2-ITM, P3-INV-02 |
| `Inventory/` | P2-INV, P2-DOC-INV, P3-INV |
| `Loot/` | P2-Loot, P2-DOC-Loot, P3-Loot |
| `Effect/` | BASE-EFT, P1-EFT |
| `Status/` | BASE-STA, P2-STA, P2-DOC-STA |
| `EventBus/` | BASE-EVT, P1-EVT, P3-EVT |
| `Input/` | BASE-INP, P1-INP |
| `WorldInteraction/` | BASE-WLD, P2-Loot-02 |
| `Progression/` | P2-DOC-PRG, P3-PRG |
| `FeedbackUI/` | BASE-UI, P2/P3-UI |
| `Feedback/` | BASE-FDB, P2-FDB |
| `DebugTest/` | BASE-DBG, P1-DBG-02, P2-CHL, DLV-* |
