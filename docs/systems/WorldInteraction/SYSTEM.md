# WorldInteraction 系统

## 体验目标

场景中**拾取物与陷阱**触发可信；碰撞后正确转发给 Item 或 Skill/Effect，不产生重复拾取。

## 系统边界

### 负责

- Trigger/Collision 检测
- 生成 `PickupRequest`（道具）
- 陷阱激活：范围检测后向 Effect 或 Skill 发请求
- 监听 `ItemConsumed` 销毁或禁用场景物体

### 不负责

- 道具效果与次数（Item）
- 技能冷却（Skill）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 下游 | Item, Effect, Skill（陷阱布置） |
| 上游 | EventBus（ItemConsumed） |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `TriggerOverlap` | In | 物理回调 | Unity | WorldInteraction | — |
| `PickupRequest` | Out | struct | WorldInteraction | Item | — |
| `TrapArmRequest` | Out | struct | WorldInteraction | Effect/Skill | — |

## 相关文档

- [DATA.md](./DATA.md)
