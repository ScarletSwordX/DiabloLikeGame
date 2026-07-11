# EventBus 接口

```csharp
interface IEventBus {
    void Publish<T>(T gameEvent) where T : struct;
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
}
```

**约定**：事件为 `readonly struct`，避免 GC；载荷仅含 id 与数值，不含 UnityEngine.Object 引用（可用 instanceId）。
