using System;

namespace Gameplay.Data
{
    /// <summary>
    /// 运行时技能槽实例：槽位 index 固定对应 Skill1/2/3，冷却按 InstanceUuid 独立计算。
    /// </summary>
    [Serializable]
    public struct SkillSlotInstance
    {
        public int SlotIndex;
        public SkillData Skill;
        public string InstanceUuid;

        public bool IsEmpty => Skill == null || string.IsNullOrEmpty(InstanceUuid);
        public string SkillId => Skill != null ? Skill.Id : null;
    }
}
