using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.EventBus;
using UnityEngine;

namespace Gameplay.Skill
{
    public class CooldownService
    {
        readonly Dictionary<(int caster, string skill), float> _endTimes = new Dictionary<(int, string), float>();
        float _tickInterval = 0.1f;
        float _lastTick;

        static string ResolveCooldownKey(CooldownQuery query) =>
            !string.IsNullOrEmpty(query.CooldownInstanceId)
                ? query.CooldownInstanceId
                : query.SkillId;

        public CooldownQueryResult Query(CooldownQuery query)
        {
            var key = (query.CasterId, ResolveCooldownKey(query));
            if (!_endTimes.TryGetValue(key, out var end) || Time.time >= end)
                return new CooldownQueryResult { CanCast = true, RemainingSeconds = 0f };

            return new CooldownQueryResult
            {
                CanCast = false,
                RemainingSeconds = end - Time.time
            };
        }

        public void StartCooldown(int casterId, string cooldownKey, float duration, string skillIdForEvent = null)
        {
            _endTimes[(casterId, cooldownKey)] = Time.time + duration;
            Publish(casterId, cooldownKey, duration, CooldownPhase.Started, skillIdForEvent);
        }

        public void Tick(int casterId, string cooldownKey, string skillIdForEvent = null)
        {
            var q = Query(new CooldownQuery { CasterId = casterId, CooldownInstanceId = cooldownKey });
            if (!q.CanCast)
                Publish(casterId, cooldownKey, q.RemainingSeconds, CooldownPhase.Tick, skillIdForEvent);
            else if (_endTimes.ContainsKey((casterId, cooldownKey)))
            {
                _endTimes.Remove((casterId, cooldownKey));
                Publish(casterId, cooldownKey, 0f, CooldownPhase.Ended, skillIdForEvent);
            }
        }

        public void UpdateAll(IEnumerable<string> cooldownKeys, int casterId, System.Func<string, string> skillIdLookup = null)
        {
            if (Time.time - _lastTick < _tickInterval) return;
            _lastTick = Time.time;
            foreach (var id in cooldownKeys)
                Tick(casterId, id, skillIdLookup?.Invoke(id));
        }

        void Publish(int casterId, string cooldownKey, float remaining, CooldownPhase phase, string skillIdForEvent)
        {
            GameEventBus.Instance.Publish(new CooldownStateChangedEvent
            {
                CasterId = casterId,
                SkillId = skillIdForEvent ?? cooldownKey,
                CooldownInstanceId = cooldownKey,
                RemainingSeconds = remaining,
                Phase = phase
            });
        }
    }
}
