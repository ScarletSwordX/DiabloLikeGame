# DiabloLikeGame

俯视角类暗黑（Diablo-like）战斗原型：数据驱动的技能/道具系统，可在 Main 场景中直接试玩与扩展。

| 文档 | 说明 |
|------|------|
| [游戏操作说明书](./docs/GAME_MANUAL.md) | 键位、场景玩法、HUD 与反馈 |
| [技能/道具添加指南](./docs/GUIDE_ADD_SKILL_ITEM.md) | 新建 SO、注册 Catalog、绑定 Loadout |
| [游戏核心策划](./docs/GAME_CORE.md) | 产品定位、核心循环与版本范围 |
| [系统策划总纲](./docs/SYSTEM_DESIGN.md) | 系统索引与模块关系 |
| [程序架构](./docs/ARCHITECTURE.md) | 模块划分与 UML |
| [IO 约定](./docs/SYSTEM_IO_CONVENTION.md) | Request/Result 与 EventBus |
| [版本路线图](./docs/TASK_BACKLOG.md) | 已实现基线与后续规划 |
| [第三方资源说明](./Packages/THIRD_PARTY_ASSETS.md) | 需自行导入的 Asset Store 资源 |

---

## 1. 环境要求与首次打开

1. 使用 **Unity 2022.3 LTS**（项目版本见 `ProjectSettings/ProjectVersion.txt`）打开**本仓库根目录**。
2. 按 [Packages/THIRD_PARTY_ASSETS.md](./Packages/THIRD_PARTY_ASSETS.md) 导入缺失的第三方资源（Damage Numbers Pro、角色与动画包、字体等）。
3. 等待 Package Manager 下载完成，确认 Console 无红色编译错误。
4. 首次打开建议依次执行菜单：
   - **Gameplay → Create P0 Default Assets**（生成默认 SO，若已存在可跳过）
   - **Gameplay → Build Prefab Assets**（可选，刷新 Prefab）
   - **Gameplay → Ensure Scene Bootstrap**（为 Main 场景补齐系统组件）
5. 打开 `Assets/Scenes/Main.unity`，点击 **Play**。

场景已包含玩家、敌人、地面、拾取物、HUD、DPS 面板与 `GameBootstrap` 运行时接线。

---

## 2. 操作方式（摘要）

输入由 **Input System**（`Assets/Input/GameInput.inputactions`）配置，代码通过 `GameInputActions` 常量引用动作名。

| Action | 默认键 | 作用 |
|--------|--------|------|
| Move | WASD | 移动 |
| Attack | 空格 / 鼠标左键 | 近战普攻 |
| Skill1 | 1 | 技能槽 0 |
| Skill2 | 2 | 技能槽 1 |
| Skill3 | 3 | 技能槽 2 |
| Item1 | 4 | 道具槽 0 |
| Item2 | 5 | 道具槽 1 |
| Item3 | 6 | 道具槽 2 |

完整说明见 **[docs/GAME_MANUAL.md](./docs/GAME_MANUAL.md)**。

---

## 3. 配置文件位置

| 类型 | 路径 | 说明 |
|------|------|------|
| 会话/loadout | `Assets/Data/GameplaySessionConfig.asset` | 技能槽 0–2、道具槽 0–2、Catalog 引用 |
| 技能 | `Assets/Data/Skills/*.asset` | `SkillData` + Delivery/Effect 子 SO |
| 技能表 | `Assets/Data/Skills/SkillCatalog.asset` | Id → SkillData |
| 道具 | `Assets/Data/Items/*.asset` | `ItemDefinition`（瞬时效果，无施法阶段） |
| 道具表 | `Assets/Data/Items/ItemCatalog.asset` | Id → ItemDefinition |
| 状态 | `Assets/Data/Statuses/` | `StatusDefinition` + `StatusCatalog` |
| 移动/普攻 | `Assets/Data/PlayerMovement.asset` | `moveSpeed`、`attackDamage` |
| 输入 | `Assets/Input/GameInput.inputactions` | Action Map `Gameplay` |

新增或复制技能/道具的步骤见 **[docs/GUIDE_ADD_SKILL_ITEM.md](./docs/GUIDE_ADD_SKILL_ITEM.md)**。

---

## 4. 事件流简述

1. **输入** → `GameplayInputBus` → `PlayerPresenter`（Attack / Skill / Item）或 `PlayerView`（Move）。
2. **技能** → `SkillSystem.TryCast` → 冷却检查 → 施法阶段（`SkillKind` 决定前摇/持续/后摇）→ `EffectSystem.ApplyProfile`。
3. **道具** → 拾取：`ItemSystem.TryPickup`（`OnPickup` 即时生效 / `OnUse` 仅加次数）；使用：`ItemSystem.TryUse`（消耗次数后生效）。
4. **伤害** → `CharacterPresenter.ProcessDamage` → `GameplayCombatFeedback` 发布 `DamageDealtEvent` + 伤害飘字（Damage Numbers Pro）。
5. **展示** → `GameEventBus` → `GameplayHud`（HP、冷却、日志）、`GameplayDpsTracker`（DPS 统计）。

详见 [docs/SYSTEM_IO_CONVENTION.md](./docs/SYSTEM_IO_CONVENTION.md)。

---

## 5. 测试与验证

本项目提供两套互补验证：**EditMode 单元测试**（不进入 Play）与 **Play Mode 运行时自检**（最小闭环冒烟）。

### 方式 A — Unity Test Runner（`Assets/Tests/`）

1. 打开 Test Runner：`窗口 → 常规 → 测试运行器`（或 **Ctrl+Shift+T** / macOS **Cmd+Shift+T**）。
2. 切换到 **EditMode** 标签，等待脚本编译完成。
3. 运行 **GameplayConfigTests**（17 条）→ **Run All**。

| 现象 | 处理 |
|------|------|
| 列表为空 | 确认在 EditMode；点 Refresh；检查 Console 编译错误 |
| 包未安装 | `Packages/manifest.json` 应含 `com.unity.test-framework` |

**测试文件：** `Assets/Tests/GameplayConfigTests.cs`

### 方式 B — 游戏内 Run Self Test（Play Mode）

1. 打开 `Main.unity`，点击 **Play**。
2. 点击 HUD 右上角 **Run Self Test**，或在 `GameBootstrap` → `DebugTest` 上右键 **Run Skill System Self Test**。

**实现：** `Assets/Scripts/Gameplay/DebugTest/GameplaySelfTestRunner.cs`（4 条核心冒烟：配置读取、冷却、伤害、拾取消耗）。

**结果查看：** Console 过滤 `[SelfTest]`，末尾汇总通过/失败数。

### 配置驱动验证

修改 `Assets/Data/Skills/Fireball.asset` 的伤害或冷却后重进 Play，火球表现应随之变化。更多步骤见 [docs/GAME_MANUAL.md §7](./docs/GAME_MANUAL.md#7-修改配置后如何验证)。

---

## 6. 架构要点

### 数据驱动

- 新技能/道具通过 SO + Catalog 接入，避免在 Presenter 中硬编码 Id。
- EditMode 测试覆盖结构与边界；Play 自检覆盖运行时闭环；手感与表现以 Main 场景试玩为准。

### 动画：代码驱动 Animator + SO 关联技能

| 层级 | 做法 | 作用 |
|------|------|------|
| **Animator 骨架** | `PlayerAnimator.controller` 固定 Skill1/2/3 等状态 | 状态名由 `PlayerView.AnimatorContract` 约定，新增技能不必复制 Controller |
| **运行时换片** | `PlayerSkillAnimationOverride` + `AnimatorOverrideController` | Loadout 变更时按槽位 Override `SkillData.CastClip` |
| **SO 关联** | `SkillData.Id` ↔ `CastClip` ↔ `GameplaySessionConfig` 槽位 | 一条 SO 同时承载玩法与动画配置 |
| **时序** | `SkillCastClipTiming` + Clip Event + `PreCastSeconds` / `PostCastSeconds` | 逻辑与表现解耦又可配置 |

详见 [docs/GUIDE_ADD_SKILL_ITEM.md](./docs/GUIDE_ADD_SKILL_ITEM.md) 与 [docs/systems/Character/ANIMATION.md](./docs/systems/Character/ANIMATION.md)。

---

## 7. 技术栈摘要

- 引擎：**Unity 2022.3 LTS**，URP
- 输入：**Unity Input System**
- 数据：**ScriptableObject** + Catalog 注册
- 道具：**瞬时效果**（`ItemDefinition` 无前摇/持续/后摇）
- 反馈：**IGameplayFeedback**（Damage Numbers Pro 飘字）+ HUD 日志 + DPS 面板
- 相机：**Cinemachine** 第三人称跟随（Main 场景）

---

## 8. 当前版本（v0.1）能力边界

**已包含：** 3 技能 + 多道具 SO、冷却与状态、投射物、HUD/DPS、EditMode 与 Play 自测、玩家/敌人动画与 Cinemachine 3C、数据驱动扩展（如 `heavy_fireball`）。

**尚未包含（见 [TASK_BACKLOG.md](./docs/TASK_BACKLOG.md)）：** 经验与技能解锁、完整背包 UI、击杀掉落表、自由配装 UI、存档与关卡。
