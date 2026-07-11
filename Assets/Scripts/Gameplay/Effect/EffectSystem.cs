using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.EventBus;
using Gameplay.Feedback;
using UnityEngine;

namespace Gameplay.Effect
{
    /// <summary>
    /// 玩法效果执行器：伤害/治疗/状态（查 StatusCatalog）。视听 VFX 见 IGameplayFeedback。
    /// </summary>
    public class EffectSystem : MonoBehaviour
    {
        GameplayFeedbackProvider _feedbackProvider;
        StatusCatalog _statusCatalog;

        readonly Dictionary<EffectType, System.Func<EffectRequest, EffectResult>> _appliers =
            new Dictionary<EffectType, System.Func<EffectRequest, EffectResult>>();

        public StatusCatalog StatusCatalog => _statusCatalog;

        public void Initialize(StatusCatalog statusCatalog)
        {
            EnsureAppliers();
            _statusCatalog = statusCatalog;
            foreach (var entity in FindObjectsOfType<CharacterEntity>())
                entity.SetStatusCatalog(statusCatalog);
        }

        void Awake()
        {
            _feedbackProvider = FindObjectOfType<GameplayFeedbackProvider>();
            EnsureAppliers();
        }

        void EnsureAppliers()
        {
            if (_appliers.Count > 0)
                return;

            _appliers[EffectType.Damage] = ApplyDamage;
            _appliers[EffectType.Heal] = ApplyHeal;
            _appliers[EffectType.ApplyStatus] = ApplyStatus;
            _appliers[EffectType.ApplyShield] = ApplyShieldBuff;
            _appliers[EffectType.ApplySlow] = ApplySlowBuff;
            _appliers[EffectType.ApplySpeedBuff] = ApplySpeedBuff;
        }

        public EffectResult Apply(EffectRequest request)
        {
            if (!_appliers.TryGetValue(request.EffectType, out var fn))
                return new EffectResult { Status = EffectResultStatus.Resisted };

            var result = fn(request);
            GameEventBus.Instance.Publish(new EffectAppliedEvent
            {
                EffectType = request.EffectType,
                SourceId = request.SourceId,
                TargetsAffected = result.TargetsAffected
            });
            return result;
        }

        public EffectResult ApplyMany(IReadOnlyList<EffectRequest> requests)
        {
            var total = 0;
            var status = EffectResultStatus.NoValidTarget;
            foreach (var r in requests)
            {
                var res = Apply(r);
                if (res.Status == EffectResultStatus.Applied)
                {
                    status = EffectResultStatus.Applied;
                    total += res.TargetsAffected;
                }
            }
            return new EffectResult { Status = status, TargetsAffected = total };
        }

        public EffectResult ApplyProfile(ActionEffectProfile profile, ActionEffectContext ctx)
        {
            var requests = ActionEffectApplier.BuildRequests(profile, ctx);
            return ApplyMany(requests);
        }

        EffectResult ApplyDamage(EffectRequest request)
        {
            if (request.Radius > 0f)
                return ApplyDamageInArea(request);

            var target = ResolveTarget(request);
            if (target == null)
                return new EffectResult { Status = EffectResultStatus.NoValidTarget };

            return ApplyDamageToTarget(target, request);
        }

        EffectResult ApplyDamageInArea(EffectRequest request)
        {
            var targets = ResolveTargetsInRadius(request);
            if (targets.Count == 0)
                return new EffectResult { Status = EffectResultStatus.NoValidTarget };

            var count = 0;
            foreach (var target in targets)
            {
                ApplyDamageToTarget(target, request);
                count++;
            }

            return new EffectResult { Status = EffectResultStatus.Applied, TargetsAffected = count };
        }

        EffectResult ApplyDamageToTarget(CharacterEntity target, EffectRequest request)
        {
            var dr = target.ProcessDamage(new DamageRequest
            {
                SourceId = request.SourceId,
                TargetId = target.EntityId,
                RawDamage = request.Magnitude
            });
            GameEventBus.Instance.Publish(new CombatLogEvent
            {
                Message = $"伤害 {dr.FinalDamage:0.#} → 实体 {target.EntityId}"
            });
            return new EffectResult { Status = EffectResultStatus.Applied, TargetsAffected = 1 };
        }

        EffectResult ApplyHeal(EffectRequest request)
        {
            if (request.Radius > 0f)
            {
                var targets = ResolveTargetsInRadius(request, includeSelf: true);
                if (targets.Count == 0)
                    return new EffectResult { Status = EffectResultStatus.NoValidTarget };

                var count = 0;
                foreach (var target in targets)
                {
                    ApplyHealToTarget(target, request);
                    count++;
                }

                return new EffectResult { Status = EffectResultStatus.Applied, TargetsAffected = count };
            }

            var single = ResolveTarget(request, allowSelf: true);
            if (single == null)
                return new EffectResult { Status = EffectResultStatus.NoValidTarget };

            ApplyHealToTarget(single, request);
            return new EffectResult { Status = EffectResultStatus.Applied, TargetsAffected = 1 };
        }

        EffectResult ApplyHealToTarget(CharacterEntity target, EffectRequest request)
        {
            var hr = target.ProcessHeal(new HealRequest
            {
                SourceId = request.SourceId,
                TargetId = target.EntityId,
                Amount = request.Magnitude
            });
            Feedback.OnHeal(target.EntityId, hr.ActualHeal, target.transform.position);
            GameEventBus.Instance.Publish(new CombatLogEvent
            {
                Message = $"治疗 {hr.ActualHeal:0.#} → 实体 {target.EntityId}"
            });
            return new EffectResult { Status = EffectResultStatus.Applied, TargetsAffected = 1 };
        }

        EffectResult ApplyStatus(EffectRequest request)
        {
            if (string.IsNullOrEmpty(request.StatusId))
                return new EffectResult { Status = EffectResultStatus.Resisted };

            if (request.Radius > 0f)
            {
                var targets = ResolveTargetsInRadius(request, includeSelf: true);
                foreach (var t in targets)
                    ApplyStatusToTarget(t, request);
                return new EffectResult
                {
                    Status = targets.Count > 0 ? EffectResultStatus.Applied : EffectResultStatus.NoValidTarget,
                    TargetsAffected = targets.Count
                };
            }

            var target = ResolveTarget(request, allowSelf: true);
            if (target == null)
                return new EffectResult { Status = EffectResultStatus.NoValidTarget };

            ApplyStatusToTarget(target, request);
            return new EffectResult { Status = EffectResultStatus.Applied, TargetsAffected = 1 };
        }

        void ApplyStatusToTarget(CharacterEntity target, EffectRequest request)
        {
            target.ApplyBuff(new BuffApplyRequest
            {
                TargetId = target.EntityId,
                SourceId = request.SourceId,
                BuffId = request.StatusId,
                Duration = request.Duration,
                Magnitude = request.Magnitude,
                Stacks = request.Stacks
            });
            Feedback.OnBuffApplied(target.EntityId, request.StatusId, target.transform.position);
        }

        EffectResult ApplyShieldBuff(EffectRequest request)
        {
            request.StatusId = "shield";
            request.EffectType = EffectType.ApplyStatus;
            return ApplyStatus(request);
        }

        EffectResult ApplySlowBuff(EffectRequest request)
        {
            request.StatusId = "slow";
            request.EffectType = EffectType.ApplyStatus;
            return ApplyStatus(request);
        }

        EffectResult ApplySpeedBuff(EffectRequest request)
        {
            request.StatusId = "speed_boost";
            request.EffectType = EffectType.ApplyStatus;
            return ApplyStatus(request);
        }

        static CharacterEntity ResolveTarget(EffectRequest request, bool allowSelf = false)
        {
            if (request.PrimaryTargetId > 0)
            {
                foreach (var c in Object.FindObjectsOfType<CharacterEntity>())
                    if (c.EntityId == request.PrimaryTargetId)
                        return c;
            }

            if (allowSelf && request.SourceId > 0)
            {
                foreach (var c in Object.FindObjectsOfType<CharacterEntity>())
                    if (c.EntityId == request.SourceId)
                        return c;
            }

            foreach (var c in Object.FindObjectsOfType<CharacterEntity>())
                if (c.Faction == Faction.Enemy)
                    return c;

            return null;
        }

        IGameplayFeedback Feedback =>
            _feedbackProvider != null ? _feedbackProvider.Feedback : new NullGameplayFeedback();

        static List<CharacterEntity> ResolveTargetsInRadius(EffectRequest request, bool includeSelf = false)
        {
            var list = new List<CharacterEntity>();
            foreach (var c in Object.FindObjectsOfType<CharacterEntity>())
            {
                if (c.Faction == Faction.Enemy)
                {
                    if (Vector3.Distance(c.transform.position, request.WorldPosition) <= request.Radius)
                        list.Add(c);
                    continue;
                }

                if (includeSelf && c.EntityId == request.SourceId)
                {
                    if (Vector3.Distance(c.transform.position, request.WorldPosition) <= request.Radius)
                        list.Add(c);
                }
            }
            return list;
        }
    }
}
