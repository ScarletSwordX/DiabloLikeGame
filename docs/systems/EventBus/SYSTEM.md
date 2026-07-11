# EventBus 系统

## 体验目标

技能释放、冷却、拾取、生命变化等流程**可追踪**；UI 与调试工具可订阅同一事件源。

## 系统边界

### 负责

- `Publish(GameEvent)` / `Subscribe<T>(handler)`
- 事件名与载荷类型登记（见 SYSTEM_IO_CONVENTION）

### 不负责

- 业务规则判断
- 持久化事件日志（Debug 可选）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | Skill, Item, Effect, Character |
| 下游 | FeedbackUI, DebugTest |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `Publish` | In | 方法 | 各玩法系统 | EventBus | — |
| `Subscribe` | In | 委托注册 | UI/Debug | EventBus | — |
| `GameEvent` | Out | 广播 | EventBus | 订阅者 | — |

**无独立 DATA.md**；事件列表见 [SYSTEM_IO_CONVENTION.md](../../SYSTEM_IO_CONVENTION.md)。

## 相关文档

- [MVP.md](./MVP.md) | [INTERFACE.md](./INTERFACE.md)
