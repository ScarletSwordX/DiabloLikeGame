# Character 接口预留

## IMovementPort

```csharp
MoveResult ProcessMove(MoveIntent intent);
```

- **调用方**：PlayerController、EnemyController（若有移动）
- **职责**：应用移速倍率（Buff）、碰撞阻挡

## ICombatPort

```csharp
DamageResult ProcessDamage(DamageRequest request);
HealResult ProcessHeal(HealRequest request);
```

- **调用方**：EffectSystem、PlayerPresenter（普攻，须对**目标** `CharacterEntity` 调用）
- **职责**：对**挂载实体**扣血/治疗；`ProcessDamage` 触发本实体 View 受击表现（`PlayHit`）

## IBuffPort

```csharp
BuffApplyResult ApplyBuff(BuffApplyRequest request);
void RemoveBuff(EntityId target, BuffInstanceId instance);
```

- **调用方**：EffectSystem
- **扩展**：毒性 DoT、眩晕等新增 buffId + DATA 行即可

## ITargetable

```csharp
Faction Faction { get; }
Transform AimPoint { get; }
bool IsAlive { get; }
```

- **调用方**：Skill 瞄准、Effect 目标解析

## 组合方式

Player / Enemy 的 MonoBehaviour **组合**上述接口实现类（`CharacterCombat`, `CharacterMovement`），避免 `Player : Enemy` 继承。

## 扩展预留

| 场景 | 接口扩展 |
|------|----------|
| 多段伤害 | `DamageRequest.hitIndex` |
| 治疗抑制 | `HealResult.rejectedReason` |
| 阵营友好火 | `DamageRequest.friendlyFirePolicy` |
