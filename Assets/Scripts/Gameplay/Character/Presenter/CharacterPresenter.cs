using System.Collections;
using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Character.Enemy.View;
using Gameplay.Character.Model;
using Gameplay.Character.Player.View;
using Gameplay.Character.View;
using Gameplay.Combat;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.EventBus;
using UnityEngine;

namespace Gameplay.Character.Presenter
{
    [RequireComponent(typeof(CharacterView))]
    public class CharacterPresenter : MonoBehaviour, IDamageable
    {
        readonly CharacterModel _model = new CharacterModel();
        readonly Dictionary<string, Coroutine> _statusCoroutines = new Dictionary<string, Coroutine>();
        CharacterView _view;

        [SerializeField] Faction _defaultFaction = Faction.Player;
        [SerializeField] float _defaultMaxHp = 100f;
        [SerializeField] float _defaultMoveSpeed = 6f;
        [Tooltip("初始当前生命值；-1 表示等于 MaxHp")]
        [SerializeField] float _startCurrentHp = -1f;

        public CharacterModel Model => _model;
        public CharacterView View => _view;

        public int EntityId => _model.EntityId;
        public bool IsAlive => _model.IsAlive;
        public float CurrentHp => _model.CurrentHp;
        public float MaxHp => _model.MaxHp;

        void Awake() => EnsureModelReady();

        void EnsureModelReady()
        {
            if (_model.EntityId > 0)
                return;

            _view = GetComponent<CharacterView>();
            _model.Initialize(_defaultFaction, _defaultMaxHp, _defaultMoveSpeed);
            ApplyStartCurrentHp();
            if (_view != null)
                _view.Bind(_model);
        }

        void ApplyStartCurrentHp()
        {
            if (_startCurrentHp >= 0f)
                SetCurrentHp(_startCurrentHp, publishHealth: false);
        }

        /// <summary>直接设置当前生命值（Clamp 到 [0, MaxHp]）并广播血量变化。</summary>
        public void SetCurrentHp(float currentHp, bool publishHealth = true)
        {
            var wasAlive = _model.IsAlive;
            _model.SetCurrentHp(currentHp);
            if (wasAlive && !_model.IsAlive)
                TryPlayDeathAnimation();
            if (publishHealth)
                PublishHealth();
        }

        public void ConfigureAsEnemy(float maxHp, float moveSpeed)
        {
            EnsureModelReady();
            _model.ConfigureAsEnemy(maxHp, moveSpeed);
        }

        public void ConfigureMovement(float moveSpeed)
        {
            EnsureModelReady();
            _model.ConfigureMovement(moveSpeed);
        }

        public void SetStatusCatalog(StatusCatalog catalog) => _model.SetStatusCatalog(catalog);

        public MoveResult ProcessMove(MoveIntent intent)
        {
            var result = _model.ComputeMove(intent);
            _view.ApplyDisplacement(result.Displacement);
            return result;
        }

        public DamageResult ProcessDamage(DamageRequest request)
        {
            var hpBefore = _model.CurrentHp;
            var result = _model.ApplyDamage(request);
            GameplayCombatLog.DamageReceived(
                _model.Faction,
                _model.EntityId,
                request,
                result,
                hpBefore,
                _model.CurrentHp,
                _model.MaxHp);

            if (result.Killed)
            {
                TryPlayDeathAnimation();
            }
            else if (!request.SuppressHitReaction)
            {
                _view.PlayHitFlash();
                if (TryGetComponent<EnemyView>(out var enemyView))
                    enemyView.PlayHit();
                else if (TryGetComponent<PlayerView>(out var playerView))
                    playerView.PlayHit();
            }

            if (result.FinalDamage > 0f)
            {
                GameplayCombatFeedback.NotifyDamageDealt(
                    request.SourceId,
                    _model.EntityId,
                    result.FinalDamage,
                    transform.position);
            }

            PublishHealth();
            return result;
        }

        void TryPlayDeathAnimation()
        {
            if (_model.Faction != Faction.Enemy)
                return;

            if (TryGetComponent<ICharacterDeathAnimationView>(out var deathView))
                deathView.PlayDie();
        }

        public HealResult ProcessHeal(HealRequest request)
        {
            var result = _model.ApplyHeal(request);
            PublishHealth();
            return result;
        }

        public BuffApplyResult ApplyBuff(BuffApplyRequest request)
        {
            StopStatusCoroutine(request.BuffId);
            var result = _model.ApplyBuff(request);
            BeginStatusPresentation(request);

            if (request.Duration > 0f)
            {
                _statusCoroutines[request.BuffId] = StartCoroutine(StatusDurationRoutine(request));
                if (TryResolveStatusDefinition(request.BuffId, out var def) &&
                    def.Kind == StatusKind.DamageOverTime)
                {
                    _statusCoroutines[StatusTickKey(request.BuffId)] =
                        StartCoroutine(DamageOverTimeRoutine(request, def));
                }
            }

            return result;
        }

        void BeginStatusPresentation(BuffApplyRequest request)
        {
            if (!TryResolveStatusDefinition(request.BuffId, out var def))
                return;

            switch (def.Kind)
            {
                case StatusKind.Dizzy:
                    if (TryGetComponent<EnemyView>(out var enemyView))
                        enemyView.BeginDizzy();
                    else if (TryGetComponent<PlayerView>(out var playerView))
                        playerView.PlayDizzy();
                    break;
            }
        }

        void EndStatusPresentation(string buffId)
        {
            if (!TryResolveStatusDefinition(buffId, out var def))
                return;

            switch (def.Kind)
            {
                case StatusKind.Dizzy:
                    if (TryGetComponent<EnemyView>(out var enemyView))
                        enemyView.EndDizzy();
                    break;
            }
        }

        IEnumerator StatusDurationRoutine(BuffApplyRequest request)
        {
            yield return new WaitForSeconds(request.Duration);
            _model.ClearBuff(request.BuffId);
            EndStatusPresentation(request.BuffId);
            _statusCoroutines.Remove(request.BuffId);
        }

        IEnumerator DamageOverTimeRoutine(BuffApplyRequest request, StatusDefinition def)
        {
            var tickKey = StatusTickKey(request.BuffId);
            var interval = def.TickIntervalSeconds > 0f ? def.TickIntervalSeconds : 1f;
            var endTime = Time.time + request.Duration;

            while (Time.time < endTime && _model.IsAlive && _model.HasActiveStatus(request.BuffId))
            {
                yield return new WaitForSeconds(interval);
                if (!_model.IsAlive || !_model.HasActiveStatus(request.BuffId))
                    break;

                ProcessDamage(new DamageRequest
                {
                    SourceId = request.SourceId,
                    TargetId = _model.EntityId,
                    RawDamage = request.Magnitude,
                    SuppressHitReaction = true
                });

                GameEventBus.Instance.Publish(new CombatLogEvent
                {
                    Message = $"灼伤 {request.Magnitude:0.#} → 实体 {_model.EntityId}"
                });
            }

            _statusCoroutines.Remove(tickKey);
        }

        bool TryResolveStatusDefinition(string buffId, out StatusDefinition definition) =>
            _model.TryGetStatusDefinition(buffId, out definition);

        void StopStatusCoroutine(string buffId)
        {
            if (_statusCoroutines.TryGetValue(buffId, out var durationRoutine))
            {
                if (durationRoutine != null)
                    StopCoroutine(durationRoutine);
                _statusCoroutines.Remove(buffId);
            }

            var tickKey = StatusTickKey(buffId);
            if (_statusCoroutines.TryGetValue(tickKey, out var tickRoutine))
            {
                if (tickRoutine != null)
                    StopCoroutine(tickRoutine);
                _statusCoroutines.Remove(tickKey);
            }
        }

        static string StatusTickKey(string buffId) => $"{buffId}__tick";

        void PublishHealth()
        {
            GameEventBus.Instance.Publish(new HealthChangedEvent
            {
                EntityId = _model.EntityId,
                Current = _model.CurrentHp,
                Max = _model.MaxHp
            });
        }
    }
}
