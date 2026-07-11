# DebugTest 系统

## 体验目标

开发者能**一键验证**配置读取、冷却、效果、拾取，无需手动猜测。

## 系统边界

### 负责

- PlayMode Test 或 Debug Panel 按钮
- 注入标准 Request，断言 Result / 事件
- 输出可读日志

### 不负责

- 替代完整玩法体验

## 依赖关系

| 方向 | 系统 |
|------|------|
| 被测 | Character, Skill, Item, Effect, 配置加载 |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `RunSelfTest` | In | 命令 | UI/TestRunner | DebugTest | — |
| `TestReport` | Out | struct | DebugTest | 日志/断言 | `failedCases[]` |

## 相关文档

- [MVP.md](./MVP.md)
- [README §5 自测方式](../../README.md#5-自测方式) — EditMode 17 条 + Play Mode 4 条明细
