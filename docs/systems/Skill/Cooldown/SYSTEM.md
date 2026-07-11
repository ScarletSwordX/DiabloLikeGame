# Skill / Cooldown 子模块

> 隶属 Skill 系统，**非**顶层独立系统。

## 体验目标

技能冷却**准确、可查询**；UI 能显示剩余时间；冷却中施法返回明确失败。

## 系统边界

### 负责

- 按 `skillId + casterId` 维护冷却结束时间
- `CooldownQuery` / `CooldownQueryResult`
- 施法成功后启动冷却
- 发布 `CooldownStateChanged`（Started / Tick / Ended）

### 不负责

- 道具次数（Item）
- UI 渲染（FeedbackUI 仅订阅）

## 依赖关系

| 方向 | 系统 |
|------|------|
| 上游 | Skill 核心（施法成功/查询） |
| 下游 | EventBus → FeedbackUI |

## 统一输入输出

| 端口名 | 方向 | 载体 | 生产者 | 消费者 | 失败语义 |
|--------|------|------|--------|--------|----------|
| `CooldownQuery` | In | struct | Skill 核心 | Cooldown | — |
| `CooldownQueryResult` | Out | struct | Cooldown | Skill 核心 | `canCast=false` |
| `CooldownStart` | In | struct | Skill 核心 | Cooldown | — |
| `CooldownStateChanged` | Out | Event | Cooldown | EventBus | phase: Started/Tick/Ended |

## 相关文档

- [DATA.md](./DATA.md)
