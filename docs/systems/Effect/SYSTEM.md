# Effect 系统

## 体验目标

所有技能与道具的**效果施加路径唯一**；新增效果类型只需注册处理器，不复制伤害代码。

## 系统边界

### 负责

- 接收 `EffectRequest`（来自 Skill / Item / 环境）
- 解析目标列表（半径、单体、自身）
- 转换为 Character 的 `DamageRequest` / `HealRequest` / `BuffApplyRequest`
- 返回 `EffectResult`；发布 `EffectApplied`

### 不负责

- 施法合法性、冷却（Skill）
- 拾取逻辑（Item）
- HP 最终存储（Character）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | Skill, Item, WorldInteraction（陷阱） |
| 下游 | Character, EventBus |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `EffectRequest` | In | struct | Skill, Item | Effect | — |
| `EffectResult` | Out | struct | Effect | 调用方 | NoValidTarget, Resisted |
| `DamageRequest` | Out | struct | Effect | Character | — |
| `HealRequest` | Out | struct | Effect | Character | — |
| `BuffApplyRequest` | Out | struct | Effect | Character | — |

## 相关文档

- [DATA.md](./DATA.md) | [INTERFACE.md](./INTERFACE.md)
