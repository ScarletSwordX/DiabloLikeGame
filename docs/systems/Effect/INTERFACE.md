# Effect 接口

```csharp
interface IEffectApplier {
    bool CanApply(EffectRequest request);
    EffectResult Apply(EffectRequest request);
}

interface IEffectApplierRegistry {
    void Register(EffectType type, IEffectApplier applier);
}
```

**新增效果类型步骤**：

1. 实现 `IEffectApplier`
2. 在 Registry 注册
3. 在 Skill/Item 配置的 `effects` 中引用 `effectType`
