using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.Data;

namespace Gameplay.Effect
{
    /// <summary>
    /// 单次技能/道具结算的上下文（来源、目标、位置）。非 VFX。
    /// </summary>
    public struct ActionEffectContext
    {
        public int SourceId;
        public int PrimaryTargetId;
        public UnityEngine.Vector3 WorldPosition;
        public float Radius;
    }

    /// <summary>
    /// 将 ActionEffectProfile 展开为 EffectSystem 可执行的玩法请求（伤害/治疗/状态）。
    /// 与 IGameplayFeedback（VFX/SFX 占位）无关。
    /// </summary>
    public static class ActionEffectApplier
    {
        public static List<EffectRequest> BuildRequests(ActionEffectProfile profile, ActionEffectContext ctx)
        {
            var list = new List<EffectRequest>();
            if (profile == null) return list;

            var radius = ctx.Radius > 0f ? ctx.Radius : profile.AreaRadius;

            if (profile.HasDamage)
            {
                list.Add(new EffectRequest
                {
                    EffectType = EffectType.Damage,
                    Magnitude = profile.Damage.Amount,
                    SourceId = ctx.SourceId,
                    PrimaryTargetId = ctx.PrimaryTargetId,
                    WorldPosition = ctx.WorldPosition,
                    Radius = radius
                });
            }

            if (profile.HasHeal)
            {
                list.Add(new EffectRequest
                {
                    EffectType = EffectType.Heal,
                    Magnitude = profile.Heal.Amount,
                    SourceId = ctx.SourceId,
                    PrimaryTargetId = ctx.PrimaryTargetId,
                    WorldPosition = ctx.WorldPosition,
                    Radius = radius
                });
            }

            if (profile.StatusEffects != null)
            {
                foreach (var status in profile.StatusEffects)
                {
                    if (status == null || !status.Enabled || string.IsNullOrEmpty(status.StatusId))
                        continue;
                    list.Add(new EffectRequest
                    {
                        EffectType = EffectType.ApplyStatus,
                        Magnitude = status.Magnitude,
                        Duration = status.DurationSeconds,
                        Radius = radius,
                        SourceId = ctx.SourceId,
                        PrimaryTargetId = ctx.PrimaryTargetId,
                        WorldPosition = ctx.WorldPosition,
                        StatusId = status.StatusId,
                        Stacks = status.Stacks
                    });
                }
            }

            return list;
        }
    }
}
