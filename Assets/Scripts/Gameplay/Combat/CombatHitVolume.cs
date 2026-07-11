using System;
using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.EventBus;
using UnityEngine;

namespace Gameplay.Combat
{
    /// <summary>
    /// Trigger 伤害/效果体积：每次激活窗口内，对每个目标 Collider 的 Enter 只结算一次。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CombatHitVolume : MonoBehaviour
    {
        Collider _collider;
        int _sourceEntityId;
        Faction _sourceFaction;
        float _damage;
        EffectSystem _effectSystem;
        ActionEffectProfile _effectProfile;

        readonly HashSet<int> _hitEntityIds = new HashSet<int>();
        bool _active;

        public event Action<CharacterEntity> TargetHit;

        void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            SetVolumeActive(false);
        }

        public void ConfigurePhysicalHit(int sourceEntityId, Faction sourceFaction, float damage)
        {
            _sourceEntityId = sourceEntityId;
            _sourceFaction = sourceFaction;
            _damage = damage;
            _effectSystem = null;
            _effectProfile = null;
        }

        public void ConfigureEffectHit(
            int sourceEntityId,
            Faction sourceFaction,
            EffectSystem effectSystem,
            ActionEffectProfile profile)
        {
            _sourceEntityId = sourceEntityId;
            _sourceFaction = sourceFaction;
            _damage = 0f;
            _effectSystem = effectSystem;
            _effectProfile = profile;
        }

        public void BeginHitWindow()
        {
            _hitEntityIds.Clear();
            SetVolumeActive(true);
        }

        public void EndHitWindow() => SetVolumeActive(false);

        void SetVolumeActive(bool active)
        {
            _active = active;
            if (_collider != null)
                _collider.enabled = active;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_active) return;
            if (!TryResolveTarget(other, out var target)) return;
            if (!_hitEntityIds.Add(target.EntityId)) return;

            ApplyHit(target, other.ClosestPoint(transform.position));
        }

        bool TryResolveTarget(Collider other, out CharacterEntity target)
        {
            target = other.GetComponentInParent<CharacterEntity>();
            if (target == null || !target.IsAlive) return false;
            if (target.EntityId == _sourceEntityId) return false;
            if (target.Faction == _sourceFaction) return false;
            return true;
        }

        void ApplyHit(CharacterEntity target, Vector3 hitPoint)
        {
            if (_effectSystem != null && _effectProfile != null)
            {
                _effectSystem.ApplyProfile(_effectProfile, new ActionEffectContext
                {
                    SourceId = _sourceEntityId,
                    PrimaryTargetId = target.EntityId,
                    WorldPosition = hitPoint,
                    Radius = 0f
                });
                TargetHit?.Invoke(target);
                return;
            }

            if (_damage <= 0f) return;

            var result = target.ProcessDamage(new DamageRequest
            {
                SourceId = _sourceEntityId,
                TargetId = target.EntityId,
                RawDamage = _damage
            });
            GameEventBus.Instance.Publish(new CombatLogEvent
            {
                Message = $"伤害 {result.FinalDamage:0.#} → 实体 {target.EntityId}"
            });

            TargetHit?.Invoke(target);
        }
    }
}
