# 第三方资源说明

本仓库**不包含**下列第三方资源的文件内容。克隆后请在 Unity Asset Store 或对应来源自行导入到相同路径，再打开工程。

> 说明列「可否公开上传」指在遵守许可的前提下，是否适合放入公开 Git 仓库；**本仓库默认均不上传**，仅作协作参考。

| 资源 | 本地路径 | 获取方式 | 可否公开上传 | 备注 |
|------|----------|----------|--------------|------|
| Damage Numbers Pro | `Assets/DamageNumbersPro/` | [Unity Asset Store](https://assetstore.unity.com/packages/tools/gui/damage-numbers-pro-186447)（付费） | **否** | 标准 Asset Store EULA，禁止再分发源码与资源文件；`Gameplay.asmdef` 依赖其程序集 |
| RPG Tiny Hero Duo | `Assets/RPG Tiny Hero Duo/` | [Unity Asset Store](https://assetstore.unity.com/packages/3d/characters/humanoids/rpg-tiny-hero-duo-pbr-polyart-225148) | **否** | 角色模型、材质、剑盾动画；`Player.prefab`、敌人 Prefab 引用此包 |
| DoubleL — Magic Animations | `Assets/DoubleL/` | [Unity Asset Store](https://assetstore.unity.com/packages/3d/animations/magic-animations-free-163259)（免费） | **否** | 法术攻击 FBX；`PlayerAnimator` 与技能 CastClip 引用 |
| Source Han Sans CN（思源黑体） | `Assets/TextFont/` | [Adobe Fonts / 思源系列](https://github.com/adobe-fonts/source-han-sans) | **可以** | SIL OFL 1.1，可随项目分发但须保留 `LICENSE.txt`；本仓库为精简体积仍不纳入 |
| Mixamo — Y Bot Hit Reaction | `Assets/Animation/HitReaction/` | [Adobe Mixamo](https://www.mixamo.com/) | **否** | Mixamo 服务条款禁止再分发动画文件 |
| TextMesh Pro Essentials | `Assets/TextMesh Pro/` | Unity Package Manager：`com.unity.textmeshpro` | **可以** | Unity 官方包，本仓库已纳入 `Assets/TextMesh Pro/` |
| Humanoid Locomotion（Walk） | `Assets/Animation/Walk/` | Unity 示例人形动画（常见教程资源） | **视来源而定** | 本仓库已纳入；若你自行替换为 Asset Store 资源，请按该资源许可处理 |

## 导入后检查

1. 按上表路径导入缺失资源。
2. 打开 `Assets/Scenes/Main.unity`，确认 Prefab 与 Animator 无 Missing 引用。
3. 菜单 **Gameplay → Ensure Scene Bootstrap**（可选）重新校验场景接线。
4. 运行 **EditMode** 测试：`GameplayConfigTests`（部分用例读取磁盘 `.asset`，不依赖第三方美术）。

## 与本仓库代码的耦合

- **伤害飘字**：`DamageNumbersGameplayFeedback` → Damage Numbers Pro
- **角色外观与动画**：`Player.prefab` / 敌人 → RPG Tiny Hero Duo + DoubleL
- **HUD 字体**：`GameplaySceneSetup` 引用 `Assets/TextFont/SourceHanSans SDF.asset`
