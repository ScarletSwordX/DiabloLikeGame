# Player MVP

## MVP-0 — 已实现

- [x] `PlayerInput` + `PlayerInputBridge` → `GameplayInputBus`
- [x] `PlayerView` 订阅 Move，Root Motion + `CharacterController`
- [x] `PlayerPresenter`：Attack / Skill1-3 / Item1-3
- [x] 玩家预制体 / Main 场景放置 + `GameBootstrap` 接线

## MVP-1 / P2 — 部分完成

- [x] 基础 **3C**：Root Motion 移动 + Cinemachine 跟随（`Main.unity` Virtual Camera）
- [x] `Walk` BlendTree locomotion（线速度 + 相机相对转向分离）
- [x] `Attack1` 普攻动画 + 对敌人伤害（`PlayAnimation` + `OnFinish` 回 Walk）
- [x] 输入去重（Bridge 同帧、`Presenter._inputBound`）
- [ ] 无敌人时仍播攻击动画（当前 `TryAttack` 需 `FindEnemy`）
- [ ] 连招（`Attack2`/`Attack3`）与攻击锁细化
- [ ] 玩家血条 UI 绑定（HUD 已订阅 `HealthChanged`）
- [ ] 拾取后移速 Buff 可见（dash_boots）

## Post-MVP

- 冲刺独立动作、鼠标精确瞄准
- `PlayerMovementConfig.MoveSpeed` 驱动 Model（替代硬编码）
