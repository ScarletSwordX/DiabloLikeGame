# Character MVP 切片

> 版本规划见 [TASK_BACKLOG.md](../../TASK_BACKLOG.md)（Character / Enemy 节）。

## v0.1 已交付

- [x] 战斗 MVP：`CharacterEntity`、Presenter/Model/View 管道
- [x] 玩家 locomotion + `Attack1`、技能动画 Override（见 [ANIMATION.md](ANIMATION.md)）
- [x] 敌人受击表现（`EnemyView.CrossFade`）
- [x] Cinemachine 第三人称相机（Main 场景）
- [x] HUD 订阅 `HealthChanged`

## 后续规划

- [ ] 假人 **可死亡** + `EnemyDiedEvent`
- [ ] 攻击锁 / 连招；敌人死亡流程与 `Die01_Stay` 接线
- [ ] 护盾吸收在 DamageResult 中可观测
- [ ] 敌人简易巡逻（移速 > 0）

## 子模块状态

| 子模块 | 代码路径 | v0.1 |
|--------|----------|------|
| Player | `Character/Player/{Model,View,Presenter}` + Input/RootMotion | ✅ |
| Enemy | `Character/Enemy/{Model,View,Presenter}` | ✅ 站桩假人 |
