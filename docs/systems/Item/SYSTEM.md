# Item 系统

## 体验目标

玩家拾取道具后**效果立即生效**；一次性道具不会无限触发；次数变化在 UI 可见。

## 系统边界

### 负责

- `PickupRequest` / `UseRequest` 处理
- 充能次数、场景实例消耗状态
- 成功后向 Effect 发送 `EffectRequest[]`
- 发布 `ItemPickedUp`、`ItemConsumed`

### 不负责

- 碰撞检测（WorldInteraction 产生 PickupRequest）
- 技能冷却（Skill/Cooldown）
- 效果数值结算（Effect）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | WorldInteraction |
| 下游 | Effect, EventBus |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `PickupRequest` | In | struct | WorldInteraction | Item | — |
| `PickupResult` | Out | struct | Item | World / UI | AlreadyConsumed, Invalid |
| `UseRequest` | In | struct | Input（可选） | Item | OutOfCharges |
| `UseResult` | Out | struct | Item | UI | — |
| `EffectRequest[]` | Out | struct[] | Item | Effect | — |
| `ItemConsumed` | Out | Event | Item | EventBus, World | — |

## 相关文档

- [DATA.md](./DATA.md) | [MVP.md](./MVP.md) | [INTERFACE.md](./INTERFACE.md)
