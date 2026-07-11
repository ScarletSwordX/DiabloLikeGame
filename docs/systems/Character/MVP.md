# Character MVP 切片

> 阶段任务见 [TASK_BACKLOG.md](../../TASK_BACKLOG.md)（Character / Enemy 节）。

## 阶段一

- [x] 战斗 MVP 代码
- [ ] 假人 **可死亡** + `EnemyDiedEvent`
- [x] 假人动画 API（`EnemyView.CrossFade`）

## 阶段二

- [x] 玩家 locomotion + `Attack1`（`PlayerView` BlendTree，见 [ANIMATION.md](ANIMATION.md)）
- [ ] 攻击锁 / 连招；敌人死亡流程与 `Die01_Stay` 接线

## MVP-0（P0）— 已实现

- [x] `CharacterModel`：Damage / Heal / Buff 数据与规则
- [x] `CharacterPresenter`：协调 Model + View，发布事件
- [x] `CharacterView`：受击闪色、AimPoint
- [x] `CharacterEntity` Facade：对外 I/O 端口
- [x] Faction 与 EntityId

## MVP-1（P1）

- [ ] 敌人血条数据源稳定（HUD 已订阅 HealthChanged）
- [ ] 护盾吸收在 DamageResult 中可观测
- [ ] 与评审用例 1–3 联调

## Post-MVP

- 敌人简易巡逻（移速 > 0）
- 死亡：禁用碰撞、状态机过渡完善

## 子模块 MVP

| 子模块 | 代码路径 | 状态 |
|--------|----------|------|
| Player | `Character/Player/{Model,View,Presenter}` + Input/RootMotion | P0 + 动画部分 |
| Enemy | `Character/Enemy/{Model,View,Presenter}` | P0 完成 |
