# Skill 接口

```csharp
interface ISkillCaster {
    CastResult TryCast(CastRequest request);
}

interface ISkillRegistry {
    SkillDefinition Get(string skillId);
    IReadOnlyList<SkillDefinition> GetEquipped(EntityId caster);
}
```

**扩展**：充能技能（多段充能）在 `SkillDefinition` 增加 `chargeMax`，Cooldown 子模块扩展为 Charge 模式 — Post-MVP。

**注册步骤（README 用）**：

1. 复制 `Assets/Data/Skills/Fireball.asset`
2. 改 `id`、数值、`CastClip` 等
3. 在 `GameplaySessionConfig` 槽位下拉中选择该技能，或写入 `SkillCatalog`
