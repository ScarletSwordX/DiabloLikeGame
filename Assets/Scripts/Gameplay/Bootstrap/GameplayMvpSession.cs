using System;
using Gameplay.Character;
using Gameplay.Data;
using Gameplay.DebugTest;
using Gameplay.Effect;
using Gameplay.Item;
using Gameplay.Skill;

namespace Gameplay.Bootstrap
{
    /// <summary>
    /// MVP 初始化完成后的只读会话，供 UI / 相机等模块订阅。
    /// </summary>
    public static class GameplayMvpSession
    {
        public static bool IsReady { get; private set; }
        public static event Action Ready;

        public static EffectSystem EffectSystem { get; private set; }
        public static ItemSystem ItemSystem { get; private set; }
        public static SkillSystem SkillSystem { get; private set; }
        public static SkillData[] EquippedSkills { get; private set; }
        public static ItemDefinition[] HotbarItems { get; private set; }
        public static event Action LoadoutChanged;
        public static PlayerController Player { get; private set; }
        public static CharacterEntity Enemy { get; private set; }
        public static GameplaySelfTestRunner SelfTestRunner { get; private set; }

        public static void Publish(
            EffectSystem effectSystem,
            ItemSystem itemSystem,
            SkillSystem skillSystem,
            SkillData[] equippedSkills,
            ItemDefinition[] hotbarItems,
            PlayerController player,
            CharacterEntity enemy,
            GameplaySelfTestRunner selfTestRunner)
        {
            EffectSystem = effectSystem;
            ItemSystem = itemSystem;
            SkillSystem = skillSystem;
            EquippedSkills = equippedSkills;
            HotbarItems = hotbarItems;
            Player = player;
            Enemy = enemy;
            SelfTestRunner = selfTestRunner;
            IsReady = true;
            Ready?.Invoke();
        }

        public static void UpdateLoadout(SkillData[] equippedSkills, ItemDefinition[] hotbarItems)
        {
            EquippedSkills = equippedSkills;
            HotbarItems = hotbarItems;
            LoadoutChanged?.Invoke();
        }

        public static void Clear()
        {
            IsReady = false;
            Ready = null;
            EffectSystem = null;
            ItemSystem = null;
            SkillSystem = null;
            EquippedSkills = null;
            HotbarItems = null;
            Player = null;
            Enemy = null;
            SelfTestRunner = null;
            LoadoutChanged = null;
        }
    }
}
