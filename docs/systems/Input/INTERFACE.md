# Input 接口

```csharp
interface IInputReader {
    RawInput Sample();
}

interface IInputMapper {
    MoveCommand MapMove(RawInput raw);
    IEnumerable<CastIntent> MapSkills(RawInput raw);
}
```

**扩展**：手柄支持时新增 `IInputReader` 实现，Mapper 不变。
