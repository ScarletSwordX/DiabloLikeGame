# DebugTest MVP

## 用例表（核心自测项）

| caseId | 描述 | 步骤 | 期望 |
|--------|------|------|------|
| T01 | 配置读取 | 加载 fireball SO | damage=10, cooldown=1 |
| T02 | 冷却 | 连续 TryCast 5 次 | 仅 1 次 Success |
| T03 | 伤害生效 | Cast fireball @ dummy | dummy HP -10 |
| T04 | 拾取消耗 | Pickup heal_potion ×2 | 第二次 AlreadyConsumed |
| T05 | 配置驱动 | 改 SO damage=50 重载 | T03 期望 -50 |

## 入口

1. `Assets/Tests/GameplayConfigTests.cs` — Unity Test Runner（EditMode，17 条）
2. Main 场景 — HUD **Run Self Test** 或 `GameplaySelfTestRunner`（Play Mode，4 条冒烟）

## v0.1 已交付

- [x] T01–T04 通过（EditMode + Play 自检）
- [x] README 说明运行方式
- [x] T05 可通过改 `Fireball.asset` 后 Play 验证（见 GAME_MANUAL §7）

## 后续规划

- [ ] 敌人死亡、非指向伤害等扩展用例
