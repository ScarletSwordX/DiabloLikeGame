using System.Collections;
using Gameplay.Character;
using Gameplay.Character.Enemy.Model;
using Gameplay.Character.View;
using Gameplay.Combat;
using Gameplay.Core;
using UnityEngine;

namespace Gameplay.Character.Enemy.View
{
    /// <summary>
    /// 敌人 View：与 <see cref="Player.View.PlayerView"/> 对齐的动画门控与 <see cref="PlayAnimation"/> 入口。
    /// 默认由本类 + <see cref="DefaultEnemyAnimationPlayback"/> 驱动；可挂自定义 <see cref="IEnemyAnimationPlayback"/> 映射不同 Controller。
    /// </summary>
    [RequireComponent(typeof(CharacterView))]
    public class EnemyView : MonoBehaviour, ICharacterAnimationView, ICharacterDeathAnimationView
    {
        public static class AnimatorContract
        {
            public const string StateWalk = "Walk";
            public const string StateAttack1 = "Attack1";
            public const string StateAttack2 = "Attack2";
            public const string StateAttack3 = "Attack3";
            public const string StateDie01 = "Die01";
            public const string StateDie01Stay = "Die01_Stay";
            public const string StateHit = "Hit";
            public const string StateDizzy = "Dizzy";
        }

        [SerializeField] Animator _animator;
        [SerializeField] float _fallbackActionLockSeconds = 0.5f;
        [Tooltip("可选：实现 IEnemyAnimationPlayback 的组件，用于按敌人类型映射意图→状态名。")]
        [SerializeField] MonoBehaviour _customAnimationDriver;

        EnemyModel _model;
        CharacterView _characterView;
        CharacterEntity _entity;
        IEnemyAnimationPlayback _animationPlayback;
        float _actionLockTimer;
        string _currentAnimationState;

        int _stateWalk;
        int _stateAttack1;
        int _stateAttack2;
        int _stateAttack3;
        int _stateDie01;
        int _stateDie01Stay;
        int _stateHit;
        int _stateDizzy;

        public EnemyModel Model => _model;
        public CharacterView CharacterView => _characterView;
        public bool HasAnimator => _animator != null;
        public IEnemyAnimationPlayback AnimationPlayback => _animationPlayback;

        public bool IsActionBlocked =>
            _model == null
            || _model.State == EnemyActivityState.Dead
            || _actionLockTimer > 0f
            || IsInSingleShotAnimatorState;

        public void Bind(EnemyModel model, CharacterView characterView)
        {
            _model = model;
            _characterView = characterView;
            _entity = GetComponent<CharacterEntity>();
            ResolveAnimationPlayback();
            ResolveAnimator();
            CacheAnimatorHashes();
        }

        void ResolveAnimationPlayback()
        {
            if (_customAnimationDriver is IEnemyAnimationPlayback custom)
                _animationPlayback = custom;
            else
                _animationPlayback = new DefaultEnemyAnimationPlayback();
        }

        /// <summary>
        /// 运行时替换动画驱动（例如切换敌人类型配置）。
        /// </summary>
        public void SetAnimationPlayback(IEnemyAnimationPlayback playback)
        {
            _animationPlayback = playback ?? new DefaultEnemyAnimationPlayback();
        }

        void ResolveAnimator()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        void CacheAnimatorHashes()
        {
            if (_animator == null) return;
            _stateWalk = Animator.StringToHash(AnimatorContract.StateWalk);
            _stateAttack1 = Animator.StringToHash(AnimatorContract.StateAttack1);
            _stateAttack2 = Animator.StringToHash(AnimatorContract.StateAttack2);
            _stateAttack3 = Animator.StringToHash(AnimatorContract.StateAttack3);
            _stateDie01 = Animator.StringToHash(AnimatorContract.StateDie01);
            _stateDie01Stay = Animator.StringToHash(AnimatorContract.StateDie01Stay);
            _stateHit = Animator.StringToHash(AnimatorContract.StateHit);
            _stateDizzy = Animator.StringToHash(AnimatorContract.StateDizzy);
        }

        void Update()
        {
            if (_actionLockTimer > 0f)
                _actionLockTimer -= Time.deltaTime;
        }

        void LateUpdate()
        {
            SyncActivityStateFromAnimator();
        }

        /// <summary>
        /// 统一切换 Animator 状态入口（与 PlayerView 一致）。
        /// </summary>
        public void PlayAnimation(string stateName, float crossFadeDuration = 0.2f, float time = 0f)
        {
            if (_animator == null || string.IsNullOrEmpty(stateName))
                return;

            if (_model != null
                && _model.State == EnemyActivityState.Dead
                && stateName != AnimatorContract.StateDie01
                && stateName != AnimatorContract.StateDie01Stay)
                return;

            if (time > 0f)
                StartCoroutine(PlayAfterDelay(stateName, crossFadeDuration, time));
            else
                CrossFadeTo(stateName, crossFadeDuration);
        }

        IEnumerator PlayAfterDelay(string stateName, float crossFadeDuration, float time)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, time - crossFadeDuration));
            CrossFadeTo(stateName, crossFadeDuration);
        }

        void CrossFadeTo(string stateName, float crossFadeDuration)
        {
            if (_currentAnimationState == stateName)
                return;

            _currentAnimationState = stateName;
            _animator.CrossFade(Animator.StringToHash(stateName), crossFadeDuration);
            ApplyModelStateForAnimation(stateName);
        }

        /// <summary>
        /// 按行为意图播放动画；状态名由 <see cref="IEnemyAnimationPlayback"/> 解析。
        /// </summary>
        public bool TryPlayIntent(EnemyActivityIntent intent)
        {
            if (_animator == null || _model == null || _animationPlayback == null)
                return false;

            if (!_animationPlayback.TryResolve(intent, out var cue) || string.IsNullOrEmpty(cue.StateName))
                return false;

            PlayAnimation(cue.StateName, cue.CrossFadeDuration);
            return true;
        }

        void ApplyModelStateForAnimation(string stateName)
        {
            if (_model == null) return;

            switch (stateName)
            {
                case AnimatorContract.StateAttack1:
                case AnimatorContract.StateAttack2:
                case AnimatorContract.StateAttack3:
                    _model.State = EnemyActivityState.Attacking;
                    break;
                case AnimatorContract.StateHit:
                    _model.State = EnemyActivityState.Hit;
                    break;
                case AnimatorContract.StateDizzy:
                    _model.State = EnemyActivityState.Dizzy;
                    break;
                case AnimatorContract.StateDie01:
                case AnimatorContract.StateDie01Stay:
                    _model.State = EnemyActivityState.Dead;
                    break;
                case AnimatorContract.StateWalk:
                    if (_model.State != EnemyActivityState.Dead)
                        _model.State = EnemyActivityState.Idle;
                    break;
            }
        }

        void SyncActivityStateFromAnimator()
        {
            if (_model == null || _animator == null) return;
            if (IsInLocomotionAnimatorState(out _)) return;
            if (TryGetDominantSingleShotState(out var activityState))
                _model.State = activityState;
        }

        public void PlayHit()
        {
            if (_animator == null || _model == null)
            {
                GameplayCombatLog.HitReaction(Faction.Enemy, _entity?.EntityId ?? 0, "PlayHit skipped: no Animator/Model");
                return;
            }

            if (_model.State == EnemyActivityState.Dead)
            {
                GameplayCombatLog.HitReaction(Faction.Enemy, _entity.EntityId, "PlayHit skipped: Dead");
                return;
            }

            if (IsSingleShotPlaying(_stateHit))
            {
                GameplayCombatLog.HitReaction(Faction.Enemy, _entity.EntityId, "PlayHit skipped: Hit already playing");
                return;
            }

            TryPlayIntent(EnemyActivityIntent.Hit);
            GameplayCombatLog.HitReaction(Faction.Enemy, _entity.EntityId, "PlayHit → Hit");
        }

        public void PlayDizzy()
        {
            if (_animator == null || _model == null) return;
            if (_model.State == EnemyActivityState.Dead) return;
            if (_model.State == EnemyActivityState.Dizzy) return;

            TryPlayIntent(EnemyActivityIntent.Dizzy);
        }

        /// <summary>进入眩晕：循环播放 Dizzy，直到 <see cref="EndDizzy"/>。</summary>
        public void BeginDizzy()
        {
            if (_animator == null || _model == null) return;
            if (_model.State == EnemyActivityState.Dead) return;

            TryPlayIntent(EnemyActivityIntent.Dizzy);
        }

        /// <summary>眩晕结束，回到 Walk/Idle。</summary>
        public void EndDizzy()
        {
            if (_animator == null || _model == null) return;
            if (_model.State == EnemyActivityState.Dead) return;
            if (_model.State != EnemyActivityState.Dizzy) return;

            TryPlayIntent(EnemyActivityIntent.Idle);
            _currentAnimationState = null;
        }

        public void PlayDie()
        {
            if (_animator == null || _model == null) return;
            if (_model.State == EnemyActivityState.Dead) return;

            TryPlayIntent(EnemyActivityIntent.Die);
            GameplayCombatLog.HitReaction(Faction.Enemy, _entity?.EntityId ?? 0, "PlayDie → Die01");
        }

        public void PlayDieStay()
        {
            if (_animator == null || _model == null) return;

            TryPlayIntent(EnemyActivityIntent.DieStay);
        }

        public bool TryPlayAttack()
        {
            if (IsActionBlocked)
                return false;
            if (_model.State == EnemyActivityState.Attacking)
                return false;

            return TryPlayIntent(EnemyActivityIntent.Attack);
        }

        bool TryGetDominantSingleShotState(out EnemyActivityState activityState)
        {
            activityState = EnemyActivityState.Idle;
            if (_animator == null) return false;

            var info = _animator.GetCurrentAnimatorStateInfo(0);
            var hash = info.shortNameHash;

            if (hash == _stateDie01 || hash == _stateDie01Stay)
            {
                activityState = EnemyActivityState.Dead;
                return true;
            }

            if (hash == _stateHit)
            {
                activityState = EnemyActivityState.Hit;
                return true;
            }

            if (hash == _stateDizzy)
            {
                activityState = EnemyActivityState.Dizzy;
                return true;
            }

            if (hash == _stateAttack1 || hash == _stateAttack2 || hash == _stateAttack3)
            {
                activityState = EnemyActivityState.Attacking;
                return true;
            }

            return false;
        }

        bool IsInSingleShotAnimatorState
        {
            get
            {
                if (_animator == null) return false;
                if (IsInLocomotionAnimatorState(out _)) return false;
                return TryGetDominantSingleShotState(out _);
            }
        }

        bool IsInLocomotionAnimatorState(out string detail)
        {
            detail = null;
            if (_animator == null) return false;

            var current = _animator.GetCurrentAnimatorStateInfo(0);
            if (current.shortNameHash == _stateWalk)
            {
                detail = AnimatorContract.StateWalk;
                return true;
            }

            if (_animator.IsInTransition(0))
            {
                var next = _animator.GetNextAnimatorStateInfo(0);
                if (next.shortNameHash == _stateWalk)
                {
                    detail = $"→{AnimatorContract.StateWalk}";
                    return true;
                }
            }

            return false;
        }

        bool IsSingleShotPlaying(int stateHash)
        {
            if (_animator == null) return false;
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            return info.shortNameHash == stateHash && info.normalizedTime < 1f;
        }
    }
}
