# Unity 可配置技能道具原型

Task spec: [CHALLENGE.md](./CHALLENGE.md)

| 文档 | 说明 |
|------|------|
| [游戏操作说明书](./docs/GAME_MANUAL.md) | 键位、场景玩法、HUD 与反馈 |
| [技能/道具添加指南](./docs/GUIDE_ADD_SKILL_ITEM.md) | 新建 SO、注册 Catalog、绑定 Loadout |
| [系统策划总纲](./docs/SYSTEM_DESIGN.md) | 系统索引与阶段规划 |
| [程序架构](./docs/ARCHITECTURE.md) | 模块划分与 UML |
| [IO 约定](./docs/SYSTEM_IO_CONVENTION.md) | Request/Result 与 EventBus |
| [任务表](./docs/TASK_BACKLOG.md) | 排期与 DoD |

---

## 1. 运行方式

1. 使用 **Unity 2022.3 LTS** 打开**本仓库根目录**（含 `Assets/`、`Packages/`、`ProjectSettings/`）。
2. 首次打开建议依次执行菜单：
   - **Gameplay → Create P0 Default Assets**（生成默认 SO）
   - **Gameplay → Build Prefab Assets**（可选，刷新 Prefab）
   - **Gameplay → Ensure Scene Bootstrap**（为 Main 场景补齐系统组件）
3. 打开 `Assets/Scenes/Main.unity`，点击 **Play**。

场景已包含玩家、敌人、地面、三个拾取物、HUD、DPS 面板与 `GameBootstrap` 运行时接线。

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

## 5. 自测方式

本项目提供两套互补验证：**EditMode 单元测试**（不进入 Play）与 **Play Mode 运行时自检**（最小闭环冒烟）。

### 方式 A — Unity Test Runner（`Assets/Tests/`）

**如何运行：**

1. Unity 顶部菜单打开 Test Runner（**不是** `Window → Test Runner`，该菜单不存在）：
   - 英文界面：`Window → General → Test Runner`
   - 中文界面：`窗口 → 常规 → 测试运行器`
   - 找不到时：按 **Ctrl+Shift+T**（macOS：**Cmd+Shift+T**）直接打开
2. 在 Test Runner 窗口顶部切到 **EditMode** 标签（不是 PlayMode）
3. 等待右下角脚本编译完成（Console 无红色报错）
4. 列表中应出现 **GameplayConfigTests**（17 条）；点击 **Run All**

**Test Runner 里看不到测试？**

| 现象 | 处理 |
|------|------|
| 菜单里没有 Test Runner | 用 `Window → General → Test Runner`，或快捷键 Ctrl+Shift+T |
| 窗口打开了但是空的 | 确认在 **EditMode** 标签；点窗口右上角 **Refresh**（刷新） |
| 一直空 / 有编译错误 | 打开 **Console**，修掉所有红色 C# 报错后再刷新 Test Runner |
| 包未安装 | `Packages/manifest.json` 应含 `com.unity.test-framework`；首次打开项目等 Package Manager 下载完成 |

**测试文件：** `Assets/Tests/GameplayConfigTests.cs`（NUnit + Unity Test Framework）

**覆盖范围（17 条）：**

| 编号 | 测试名 | 验证内容 |
|------|--------|----------|
| T01 | `T01_ConfigLoad_FireballDefaults` | **代码工厂** `SetFireballDefaults()`（非磁盘 Fireball.asset） |
| T01b | `T01b_GameInput_HasEightGameplayActions` | Input Actions 含 Attack + Skill1–3 + Item1–3（Move 单独绑定） |
| T02 | `T02_Cooldown_BlocksSecondCast` | `CooldownService` 开始后 `CanCast == false` |
| T03 | `T03_Damage_ReducesHp` | `CharacterEntity.ProcessDamage` 扣血 |
| T04 | `T04_Pickup_ConsumedOnce` | OnPickup 治疗道具：首次 Success、二次 AlreadyConsumed、无热键栏次数 |
| T04b | `T04b_OnUsePickup_AddsChargesWithoutImmediateEffect` | OnUse 拾取只加次数、按键使用才治疗 |
| T05 | `T05_SkillCatalog_TryGet` | Catalog Id 查表 |
| T06 | `T06_SessionConfig_ResolveSkillSlots` | SessionConfig 解析技能槽 |
| T07 | `T07_SessionConfig_InvalidItemId_ReturnsNull` | 无效道具 Id 返回 null |
| T08–T09 | 槽位 InstanceUuid | 同槽 Reload 保留 UUID；换技能生成新 UUID |
| T10–T11 | 眩晕 / 灼伤 Status | StatusCatalog 注册与 Model 状态 |
| T12–T13 | 资产断言 | `LightingballEffect` 含 dizzy；`FirePotion` 含 burn + 范围 |
| T14–T15 | 施法动画时序 | T14：**磁盘** `Fireball.asset` Event + PreCast/PostCast；T15：合成 Clip 速度计算 |
| T16–T17 | SkillKind | Shield（StatusApply 仅前摇）、Channeled 三阶段 |

**特点：** 在 Editor 内秒级跑完，**不依赖 Main 场景**；多数用 `ScriptableObject.CreateInstance` 构造内存数据，T12–T14 会读取磁盘上的 `.asset` 验证策划配置。

### 方式 B — 游戏内 Run Self Test（Play Mode）

**如何运行：**

1. 打开 `Main.unity`，点击 **Play**
2. 点击 HUD 右上角 **Run Self Test** 按钮  
   或：Hierarchy 选中 `GameBootstrap` 下 `DebugTest` → Inspector 右键 **Run Skill System Self Test**

**实现：** `Assets/Scripts/Gameplay/DebugTest/GameplaySelfTestRunner.cs`

**覆盖范围（4 条，CHALLENGE 核心四项）：**

| 编号 | 名称 | 验证内容 |
|------|------|----------|
| T01 | 配置读取 | 内存创建 `SkillData` + `SetFireballDefaults()`，断言 CD=1、伤害=10 |
| T02 | 冷却限制 | 新建 `CooldownService`，StartCooldown 后连续 Query 均不可施法 |
| T03 | 伤害生效 | 临时创建假人 `CharacterEntity`，ProcessDamage 10 → HP 90 |
| T04 | 拾取消耗 | 临时 `ItemSystem` + OnPickup 治疗，首次 Success、二次 AlreadyConsumed |

**结果查看：** Console 窗口过滤 `[SelfTest]`，末尾汇总 `N 通过, M 失败`；失败项为红色 `FAIL`。

**与方式 A 的区别：** 运行时自检是 **Play Mode 冒烟**，只跑 4 条最核心规则；完整回归请用 Test Runner。

---

## 6. AI 工具使用

### 工具与会话记录

| 项 | 说明 |
|----|------|
| 工具 | **Cursor**（Agent 模式） |
| 会话记录 | [`.cursor/cursor_.md`](./.cursor/cursor_.md)（完整对话与任务分解，**强制交付物**，见 [CLAUDE.md](./CLAUDE.md)） |
| 协作文档 | `docs/` 架构与系统说明、本 README |

提交前确认：`.cursor/` 已 `git add`，且 **未** 被 `.gitignore` 忽略。

### AI 主要生成的部分

| 类别 | 内容 |
|------|------|
| 架构与文档 | `docs/ARCHITECTURE.md`、`docs/SYSTEM_IO_CONVENTION.md`、各 `docs/systems/*/SYSTEM.md` 与 DATA |
| Gameplay 脚本 | Skill / Item / Effect / Character 管道、`GameEventBus`、冷却、投射物、HUD、DPS |
| 数据管线 | `SkillData` / `ItemDefinition` / Catalog / `GameplaySessionConfig`、Editor 菜单与 Rebuild |
| 测试 | `Assets/Tests/GameplayConfigTests.cs`、`GameplaySelfTestRunner` |
| 场景/Prefab 辅助 | `GameplaySceneSetup` Editor 脚本、Bootstrap 接线逻辑 |

AI 负责快速搭骨架、补测试与文档；**数值、Prefab 引用、边界行为由人工在 Unity 中验收后调整**。

### 人工审查方式（Unity 实际运行 + 读代码 + Debug）

以下为提交前实际采用的验收流程：

1. **Play Mode 试玩**  
   打开 `Main.unity` → Play，按 [GAME_MANUAL.md](./docs/GAME_MANUAL.md) 流程操作：移动、普攻、三技能、三道具、拾取、观察 HUD / 飘字 / DPS。

2. **读 Console 日志**  
   - 自测：过滤 `[SelfTest]` 确认 4/4 通过  
   - 运行时：`SkillCastFailed`（冷却中）、`EffectApplied`、拾取/伤害相关 `CombatLogEvent` 写入 HUD 日志  
   - 改 SO 数值（如 Fireball 伤害 10→50）后重进 Play，确认表现变化（CHALLENGE 用例 1）

3. **读代码核对数据流**  
   重点路径：`PlayerInputBridge` → `GameplayInputBus` → `PlayerPresenter.TryCast` / `SkillPresenter` → `CooldownService` → `EffectSystem.ApplyProfile` → `CharacterEntity.ProcessDamage`；道具 `PickupItem` → `ItemPresenter.TryPickup`。对照 `docs/SYSTEM_IO_CONVENTION.md` 事件名与 Request/Result 结构。

4. **Debug 模式断点**  
   Visual Studio / Rider 附加 Unity Editor，在以下位置设断点抽查：  
   - `SkillPresenter.TryCast`：冷却拒绝 vs 成功施法分支  
   - `CooldownService.StartCooldown` / `Query`：剩余秒数  
   - `ItemPresenter.TryPickup`：OnPickup 即时治疗 vs OnUse 只加次数  
   - `EffectSystem.ApplyProfile`：伤害/治疗/状态分支  
   单步观察 `CastResult.Status`、`PickupResultStatus`、HP 变化是否符合预期。

5. **EditMode 测试回归**  
   Test Runner 跑全量 `GameplayConfigTests`，确保重构后工厂默认值、Catalog、SessionConfig、Status 资产未被改坏。

6. **人工修改示例**  
   - 调整 `Fireball.asset` / `GameplaySessionConfig.asset` 策划数值与 Loadout  
   - 修正 HUD Prefab 引用、Main 场景拾取物 `_itemDefinitionId`  
   - 根据 Play 表现微调 CD 显示格式、投射物速度/距离、眩晕/灼伤 Duration  

### 取舍说明

- **优先数据驱动**：新技能/道具走 SO + Catalog，避免在 Presenter 里硬编码 Id。  
- **测试分层**：EditMode 覆盖结构与边界；Play 自检覆盖「能跑起来」；完整手感仍靠 Main 场景试玩。  
- **未做加分项**：JSON 运行时热更、试玩记录模板（见 CHALLENGE Bonus）。

#### 动画：代码驱动 Animator + SO 关联技能

为便于**后续扩展更多技能动画**（换 Clip、加槽位、调前摇/后摇），没有为每个技能单独做 Animator Controller，而是采用 **代码驱动的 Animator 管线**：

| 层级 | 做法 | 作用 |
|------|------|------|
| **Animator 骨架** | `PlayerAnimator.controller` 固定 **Skill1 / Skill2 / Skill3**（及 Walk、Attack 等）状态机 | 状态名、过渡、BlendTree 由代码约定（`PlayerView.AnimatorContract`），新增技能**不必**复制 Controller |
| **运行时换片** | `PlayerSkillAnimationOverride` + `AnimatorOverrideController` | Loadout 变更时，按槽位 index 把各 `SkillData.CastClip` **Override** 到对应 Skill 状态的占位 Clip 上 |
| **SO 双向关联** | `SkillData.Id` ↔ `CastClip` ↔ `GameplaySessionConfig` 槽位 | 策划在技能 SO 上填 **Id** 与 **CastClip**；SessionConfig 槽位填 **SkillId** → 运行时既解析玩法（冷却、Delivery、Effect），又解析动画（哪条 Clip 挂到 Skill1/2/3） |
| **时序与逻辑** | `SkillCastClipTiming` 读 Clip 内 `OnAnimationLaunchProjectile` Event + SO 的 `PreCastSeconds` / `PostCastSeconds` | 动画 Event 决定投射物发射帧；SO 数值决定 Animator 播放速度与实际 wall-clock 前摇/后摇，**逻辑与表现解耦又可配置** |

**为何这样取舍：**

- **扩展成本低**：复制 `Fireball.asset` → 改 Id、换 `CastClip`、注册 Catalog、绑槽位即可试新技能，无需改 C# 状态机。  
- **技能与动画可追溯**：一条技能 SO 同时承载「我是谁（Id）」「播什么（CastClip）」「造成什么（Effect）」，评审改配置时能一眼看到动画与玩法是否匹配。  
- **代价**：首次需理解 Override + Event 时序；Clip 须含约定 Event 名（见 `SkillCastClipTiming.LaunchProjectileEventName`）；槽位 index 与 Input `Skill1–3`、Animator 状态名三者需保持一致（见 [GUIDE_ADD_SKILL_ITEM.md](./docs/GUIDE_ADD_SKILL_ITEM.md)）。

---

## 实现约束摘要

- 输入：**Unity Input System**
- 数据：**ScriptableObject** + Catalog 注册
- 道具：**瞬时效果**（`ItemDefinition` 无前摇/持续/后摇）
- 反馈：**IGameplayFeedback**（Damage Numbers Pro 飘字）+ HUD 日志 + DPS 面板
