# DebugTest MVP

## 用例表（对齐 CHALLENGE 自测四项 + 用例 5）

| caseId | 描述 | 步骤 | 期望 |
|--------|------|------|------|
| T01 | 配置读取 | 加载 fireball SO | damage=10, cooldown=1 |
| T02 | 冷却 | 连续 TryCast 5 次 | 仅 1 次 Success |
| T03 | 伤害生效 | Cast fireball @ dummy | dummy HP -10 |
| T04 | 拾取消耗 | Pickup heal_potion ×2 | 第二次 AlreadyConsumed |
| T05 | 配置驱动 | 改 SO damage=50 重载 | T03 期望 -50 |

## 入口（实现二选一）

1. `Assets/Tests/GameplayConfigTests.cs` — Unity Test Runner
2. Main 场景 Debug Panel — `Run Skill System Self Test`

## MVP-0

- [ ] T01–T04 通过
- [ ] README 说明运行方式

## MVP-1

- [ ] T05 或 Editor 改 SO 后 PlayMode 验证文档
