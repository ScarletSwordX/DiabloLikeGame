using System;
using Gameplay.Bootstrap;
using Gameplay.Data;

namespace Gameplay.Skill.Model
{
    public class SkillRuntimeModel
    {
        public readonly CooldownService Cooldown = new CooldownService();
        public readonly System.Collections.Generic.Dictionary<string, SkillData> Definitions =
            new System.Collections.Generic.Dictionary<string, SkillData>();

        public SkillSlotInstance[] Slots { get; private set; } = new SkillSlotInstance[GameplaySessionConfig.SkillSlotCount];

        public void SetLoadout(SkillData[] skills, SkillSlotInstance[] previousSlots = null)
        {
            Definitions.Clear();
            Slots = new SkillSlotInstance[GameplaySessionConfig.SkillSlotCount];

            if (skills == null)
                return;

            for (var i = 0; i < Slots.Length; i++)
            {
                var skill = i < skills.Length ? skills[i] : null;
                if (skill == null || string.IsNullOrEmpty(skill.Id))
                    continue;

                if (!Definitions.ContainsKey(skill.Id))
                    Definitions[skill.Id] = skill;

                var prev = previousSlots != null && i < previousSlots.Length ? previousSlots[i] : default;
                var reuseUuid = prev.Skill != null
                    && prev.Skill.Id == skill.Id
                    && !string.IsNullOrEmpty(prev.InstanceUuid);

                Slots[i] = new SkillSlotInstance
                {
                    SlotIndex = i,
                    Skill = skill,
                    InstanceUuid = reuseUuid ? prev.InstanceUuid : Guid.NewGuid().ToString("N")
                };
            }
        }

        public void SetEquipped(SkillData[] skills) => SetLoadout(skills, Slots);

        public bool TryGet(string skillId, out SkillData def) =>
            Definitions.TryGetValue(skillId, out def);

        public SkillSlotInstance GetSlot(int index) =>
            index >= 0 && index < Slots.Length ? Slots[index] : default;

        public string ResolveSkillIdForInstance(string instanceUuid)
        {
            if (string.IsNullOrEmpty(instanceUuid))
                return null;

            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.InstanceUuid == instanceUuid)
                    return slot.SkillId;
            }

            return null;
        }
    }
}
