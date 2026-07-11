using Gameplay.Data;
using UnityEngine;

namespace Gameplay.EventBus
{
    public struct SkillCastAttemptedEvent
    {
        public string SkillId;
        public int CasterId;
    }

    public struct SkillCastSucceededEvent
    {
        public string SkillId;
        public int CasterId;
    }

    public struct SkillCastFailedEvent
    {
        public string SkillId;
        public int CasterId;
        public string Reason;
    }

    public struct CooldownStateChangedEvent
    {
        public string SkillId;
        public string CooldownInstanceId;
        public int CasterId;
        public float RemainingSeconds;
        public Core.CooldownPhase Phase;
    }

    public struct HealthChangedEvent
    {
        public int EntityId;
        public float Current;
        public float Max;
    }

    public struct ItemPickedUpEvent
    {
        public string ItemDefinitionId;
        public int PickerId;
    }

    public struct ItemConsumedEvent
    {
        public string ItemDefinitionId;
        public int ItemInstanceId;
    }

    public struct EffectAppliedEvent
    {
        public Core.EffectType EffectType;
        public int SourceId;
        public int TargetsAffected;
    }

    public struct DamageDealtEvent
    {
        public int SourceId;
        public int TargetId;
        public float Amount;
        public Vector3 WorldPosition;
    }

    public struct CombatLogEvent
    {
        public string Message;
    }

    public struct LoadoutChangedEvent
    {
        public SkillData[] Skills;
        public ItemDefinition[] Items;
    }
}
