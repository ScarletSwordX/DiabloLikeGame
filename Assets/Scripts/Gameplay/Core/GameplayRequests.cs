using UnityEngine;
using Gameplay.Core;

namespace Gameplay.Core
{
    public struct MoveIntent
    {
        public int EntityId;
        public Vector3 Direction;
        public float SpeedMultiplier;
        public float DeltaTime;
    }

    public struct MoveResult
    {
        public Vector3 Displacement;
        public float ActualSpeed;
        public bool Blocked;
    }

    public struct DamageRequest
    {
        public int SourceId;
        public int TargetId;
        public float RawDamage;
        public bool SuppressHitReaction;
    }

    public struct DamageResult
    {
        public float FinalDamage;
        public bool AbsorbedByShield;
        public bool Killed;
    }

    public struct HealRequest
    {
        public int SourceId;
        public int TargetId;
        public float Amount;
    }

    public struct HealResult
    {
        public float ActualHeal;
        public float Overheal;
    }

    public struct BuffApplyRequest
    {
        public int TargetId;
        public int SourceId;
        public string BuffId;
        public float Duration;
        public int Stacks;
        public float Magnitude;
    }

    public struct BuffApplyResult
    {
        public int InstanceId;
        public bool Success;
        public string RejectedReason;
    }

    public struct CastIntent
    {
        public int CasterId;
        public string SkillId;
        public float Timestamp;
    }

    public struct CastRequest
    {
        public string SkillId;
        /// <summary>槽位实例 UUID，用于冷却查表（与 SkillId 解耦，同技能占多槽互不共享 CD）。</summary>
        public string CooldownInstanceId;
        public int CasterId;
        public Vector3 AimPoint;
        public int TargetId;
    }

    public struct CastResult
    {
        public CastResultStatus Status;
    }

    public struct CooldownQuery
    {
        /// <summary>优先使用；为空时回退 SkillId（兼容旧测试）。</summary>
        public string CooldownInstanceId;
        public string SkillId;
        public int CasterId;
    }

    public struct CooldownQueryResult
    {
        public bool CanCast;
        public float RemainingSeconds;
    }

    public struct PickupRequest
    {
        public int PickerId;
        public int ItemInstanceId;
        public string ItemDefinitionId;
    }

    public struct PickupResult
    {
        public PickupResultStatus Status;
    }

    public struct UseRequest
    {
        public int UserId;
        public string ItemDefinitionId;
    }

    public struct UseResult
    {
        public UseResultStatus Status;
    }

    public struct EffectRequest
    {
        public EffectType EffectType;
        public float Magnitude;
        public float Duration;
        public float Radius;
        public int SourceId;
        public int PrimaryTargetId;
        public Vector3 WorldPosition;
        public string StatusId;
        public int Stacks;
    }

    public struct EffectResult
    {
        public EffectResultStatus Status;
        public int TargetsAffected;
    }
}
