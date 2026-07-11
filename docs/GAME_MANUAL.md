# 游戏操作说明书

面向玩家与开发者：如何在 Main 场景中操作角色、理解 UI 与反馈。

相关配置说明见 [GUIDE_ADD_SKILL_ITEM.md](./GUIDE_ADD_SKILL_ITEM.md)；技术事件流见 [SYSTEM_IO_CONVENTION.md](./SYSTEM_IO_CONVENTION.md)。

---

## 1. 启动游戏

1. Unity 2022.3 LTS 打开项目根目录。
2. 按 [Packages/THIRD_PARTY_ASSETS.md](../Packages/THIRD_PARTY_ASSETS.md) 导入第三方资源（角色、动画、飘字、字体等）。
3. 打开场景 `Assets/Scenes/Main.unity`。
4. 点击 **Play**。

首次克隆若缺少默认数据，可执行菜单 **Gameplay → Create P0 Default Assets** 与 **Gameplay → Ensure Scene Bootstrap**。

---

## 2. 键位一览

键位定义于 `Assets/Input/GameInput.inputactions`，可在 Unity Input Actions 窗口中修改绑定。

| 操作 | 默认键 | 说明 |
|------|--------|------|
| 移动 | W / A / S / D | 俯视角八向移动 |
| 普攻 | 空格 或 鼠标左键 | 前方扇形近战，对敌人造成伤害 |
| 技能 1 | 1 | 技能槽 0（默认：**火球** `fireball`） |
| 技能 2 | 2 | 技能槽 1（默认：**闪电球** `lightingball`，附带眩晕） |
| 技能 3 | 3 | 技能槽 2（默认：**护盾** `shield`） |
| 道具 1 | 4 | 热键栏道具槽 0（默认：治疗药水·可重复使用） |
| 道具 2 | 5 | 道具槽 1（默认：火焰药水） |
| 道具 3 | 6 | 道具槽 2（默认可为空） |

---

## 3. 场景内容

| 对象 | 说明 |
|------|------|
| 玩家 | 可移动、普攻、施法、使用热键栏道具 |
| 敌人 / 训练假人 | 承受伤害；HP 归零播放死亡动画 |
| 拾取物 ×3 | 触碰拾取；上方有图标提示种类（面向摄像机） |

### 三个拾取物行为

| 拾取物 | 道具 Id | 类型 | 效果 |
|--------|---------|------|------|
| 绿色（即时治疗） | `heal_ball_pickupUse` | **OnPickup** | 触碰立即恢复 30 生命，**不**增加热键栏次数 |
| 治疗球（入包） | `heal_potion_pickup` | **OnUse** | 拾取后热键栏 **Item1** 次数 +1，按 **4** 使用才治疗 |
| 火焰球 | `fire_potion` | **OnUse** | 拾取后 **Item2** 次数 +1，按 **5** 使用，对周围敌人施加灼伤 |

同一拾取实例只会生效一次（再次触碰无效果）。

---

## 4. HUD 与面板

### 战斗 HUD（GameplayHud）

- **左上/热键栏**：技能 1–3、道具 1–3 图标、冷却遮罩、道具剩余次数。
- **文本区**：玩家/敌人 HP、技能冷却状态、战斗日志（伤害、治疗、拾取、施法失败等）。

### DPS 面板（DPSBoard）

- 位于画面上方，显示当前 **DPS**（玩家造成的伤害 / 自上次重置以来的时间）。
- 统计包含：**普攻、技能、道具、持续伤害（如灼伤 tick）** 等所有经 `ProcessDamage` 结算的伤害。
- 点击 **Reset** 清零统计并重新开始计时。

### 伤害飘字

- 对敌人造成伤害时，目标上方弹出 **Red Glow** 飘字（Damage Numbers Pro）。
- 普攻与技能伤害均会显示。

---

## 5. 推荐试玩流程

1. **普攻**：靠近敌人，按空格，观察 HP 下降、飘字与 DPS 上升。
2. **火球**：按 **1** 施法，注意冷却期间无法重复释放；HUD 显示剩余 CD。
3. **闪电球 / 护盾**：按 **2** / **3** 分别施法，观察眩晕与护盾效果。
4. **即时治疗**：触碰绿色 OnPickup 球，HP 立即增加，热键栏次数不变。
5. **可重复使用治疗**：拾取另一治疗球 → 按 **4** 使用 → 次数减 1。
6. **火焰药水**：拾取火焰球 → 按 **5** 对范围内敌人挂灼伤。
7. **DPS**：持续输出后查看 DPS 数值，点 Reset 验证归零。

---

## 6. 自测与调试

| 方式 | 操作 |
|------|------|
| 单元测试 | `窗口 → 常规 → 测试运行器`（或 **Ctrl+Shift+T**）→ **EditMode** → `GameplayConfigTests`（详见 [README §5](../README.md#5-自测方式)） |
| 运行时自检 | Play 后点击 **Run Self Test**（4 条核心冒烟，详见 [README §5](../README.md#5-自测方式)） |

---

## 7. 修改配置后如何验证

- 在 `Assets/Data/Skills/Fireball.asset` 中把伤害改为 **50** 或冷却改为 **5**，Play 后火球表现应变化。
- 在 `Assets/Data/GameplaySessionConfig.asset` 中把 `_skillSlot1` 改为 `shield`，Reload 后 **2** 键应为护盾。
- 复制 Fireball 为 `HeavyFireball` 并绑到新槽位的步骤见 [GUIDE_ADD_SKILL_ITEM.md](./GUIDE_ADD_SKILL_ITEM.md)。

---

## 8. 默认 Loadout 与已知限制

当前 `Assets/Data/GameplaySessionConfig.asset` 默认配置：

| 槽位 | 键 | Id | 说明 |
|------|-----|-----|------|
| 技能 0 | 1 | `fireball` | 投射物火球 |
| 技能 1 | 2 | `lightingball` | 投射物 + 眩晕 |
| 技能 2 | 3 | `shield` | 自身护盾 |
| 道具 0 | 4 | `heal_potion_pickup` | 拾取加次数，按键使用 |
| 道具 1 | 5 | `fire_potion` | 范围灼伤 |
| 道具 2 | 6 | （空） | — |

扩展技能 `heavy_fireball` 已在 Catalog 注册，可按 [GUIDE_ADD_SKILL_ITEM.md](./GUIDE_ADD_SKILL_ITEM.md) 绑到任意技能槽试玩。

**已知限制：**

- 道具 **OnUse** 类型不会在拾取瞬间生效，必须按对应 Item 键。
- 运行时 Loadout 热重载：Play Mode 下改 `GameplaySessionConfig` 后点 **Apply Loadout Now**（Inspector 按钮）。
