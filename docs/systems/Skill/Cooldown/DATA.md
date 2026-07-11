# Cooldown 数据基线

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `tickInterval` | 0.1s | 向 UI 发送 Tick 事件间隔 |
| `uiSyncThreshold` | 0.05s | 剩余时间变化小于此值可不发 Tick |
| `useUnscaledTime` | false | 是否受 timeScale 影响 |

冷却时长**来自** `SkillDefinition.cooldownSeconds`，本子模块不重复存储设计值。
