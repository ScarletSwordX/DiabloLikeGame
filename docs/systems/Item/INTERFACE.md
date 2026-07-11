# Item 接口

```csharp
interface IItemService {
    PickupResult TryPickup(PickupRequest request);
    UseResult TryUse(UseRequest request);
}

interface IItemRegistry {
    ItemDefinition Get(string itemId);
}
```

**扩展**：持久化库存 — 新增 `IInventory` 端口，Pickup 写入库存而非立即消耗 charges。
