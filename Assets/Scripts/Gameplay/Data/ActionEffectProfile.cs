using System;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 技能/道具效果配置：伤害、恢复、状态赋予三类可同时启用（不互斥）。
    /// </summary>
    [Serializable]
    public class ActionEffectProfile
    {
        public DamageEffectPart Damage = new DamageEffectPart();
        public HealEffectPart Heal = new HealEffectPart();
        public StatusEffectPart[] StatusEffects = Array.Empty<StatusEffectPart>();

        [Tooltip("范围状态（如减速陷阱）半径，0 表示仅主目标")]
        public float AreaRadius;

        public bool HasDamage => Damage != null && Damage.Enabled;
        public bool HasHeal => Heal != null && Heal.Enabled;
        public bool HasAnyStatus => StatusEffects != null && StatusEffects.Length > 0;
    }

    [Serializable]
    public class DamageEffectPart
    {
        public bool Enabled;
        public float Amount;
    }

    [Serializable]
    public class HealEffectPart
    {
        public bool Enabled;
        public float Amount;
    }

    [Serializable]
    public class StatusEffectPart
    {
        public bool Enabled;
        [Tooltip("对应 StatusDefinition.Id")]
        public string StatusId;
        public float Magnitude;
        public float DurationSeconds = 5f;
        public int Stacks = 1;
    }
}
