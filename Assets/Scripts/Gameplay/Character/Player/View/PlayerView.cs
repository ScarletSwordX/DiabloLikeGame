using Gameplay.Bootstrap;
using Gameplay.Character;
using Gameplay.Character.Player.Model;
using Gameplay.Character.View;
using Gameplay.Combat;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.Input;
using System.Collections;
using UnityEngine;

namespace Gameplay.Character.Player.View
{
    /// <summary>
    /// 玩家 View：locomotion（Walk BlendTree）与单次动作态门控；规则见 docs/systems/Character/ANIMATION.md。
    /// 动画切换统一经 <see cref="PlayAnimation"/>；单次动作结束可由 Animator 上 <see cref="OnFinish"/> 回 Walk。
    /// </summary>
    [RequireComponent(typeof(CharacterView))]
    public class PlayerView : MonoBehaviour, ICharacterAnimationView
    {
        public static class AnimatorContract
        {
            public const string ParamTurnSpeed = "TurnSpeed";
            public const string ParamVerticalSpeed = "VerticalSpeed";
            public const string StateWalk = "Walk";
            public const string StateAttack1 = "Attack1";
            public const string StateAttack2 = "Attack2";
            public const string StateAttack3 = "Attack3";
            public const string StateDie01 = "Die01";
            public const string StateDie01Stay = "Die01_Stay";
            public const string StateHit = "Hit";
            public const string StateDizzy = "Dizzy";
            public const string StateSkill1 = "Skill1";
            public const string StateSkill2 = "Skill2";
            public const string StateSkill3 = "Skill3";
            public const string StateItemUse = "ItemUse";
            public const int AttackSlotCount = 3;
            public const int SkillSlotCount = 3;
        }

        [SerializeField] Animator _animator;
        [SerializeField] Camera _camera;
        [SerializeField] float _locomotionDamp = 0.12f;
        [SerializeField] float _linearAcceleration = 24f;
        [SerializeField] float _runInputThreshold = 0.95f;
        [SerializeField] float _turnSpeedScale = 1f;
        [SerializeField] float _verticalSpeedBlendMax = 1.2f;
        [SerializeField] float _fallbackActionLockSeconds = 0.5f;
        [SerializeField] Transform _projectileSpawnHandle;

        PlayerModel _model;
        CharacterView _characterView;
        CharacterEntity _entity;
        PendingSkillProjectileCast? _pendingSkillProjectile;
        SkillCastClipTiming.ResolvedTiming? _activeCastTiming;
        float _defaultAnimatorSpeed = 1f;
        bool _castSpeedScalingActive;
        Vector2 _moveInput;
        float _actionLockTimer;
        float _currentLinearSpeed;
        float _smoothedTurnSpeed;
        float _smoothedVerticalSpeed;

        int _turnSpeedParam;
        int _verticalSpeedParam;
        int _stateWalk;
        int _stateAttack1;
        int _stateAttack2;
        int _stateAttack3;
        int _stateDie01;
        int _stateDie01Stay;
        int _stateHit;
        int _stateDizzy;
        int _stateSkill1;
        int _stateSkill2;
        int _stateSkill3;
        int _stateItemUse;

        private string currentAnimationState;

        struct PendingSkillProjectileCast
        {
            public SkillData Definition;
            public CastRequest Request;
            public EffectSystem EffectSystem;
        }

        public CharacterView CharacterView => _characterView;
        public PlayerModel Model => _model;
        public bool HasAnimator => _animator != null;

        public bool IsActionBlocked =>
            _model == null
            || _model.State == PlayerActivityState.Dead
            || _actionLockTimer > 0f
            || IsInSingleShotAnimatorState;

        /// <summary>供 Presenter 日志：说明 <see cref="IsActionBlocked"/> 为 true 的原因。</summary>
        public string DescribeActionBlockReason()
        {
            if (_model == null) return "model=null";
            if (_model.State == PlayerActivityState.Dead) return "dead";
            if (_actionLockTimer > 0f) return $"actionLock={_actionLockTimer:0.##}s";

            if (_animator == null) return "animator=null";

            if (TryGetDominantSingleShotState(out var shot))
            {
                var info = _animator.GetCurrentAnimatorStateInfo(0);
                return $"singleShot={DescribeAnimatorState(info)} activity={shot} nt={info.normalizedTime:0.##} trans={_animator.IsInTransition(0)} model={_model.State}";
            }

            return $"anim={DescribeAnimatorState(_animator.GetCurrentAnimatorStateInfo(0))} model={_model.State} trans={_animator.IsInTransition(0)}";
        }

        public void Bind(PlayerModel model, CharacterView characterView)
        {
            _model = model;
            _characterView = characterView;
            _entity = GetComponent<CharacterEntity>();
            ResolveAnimator();
            CacheAnimatorHashes();
        }

        /// <summary>
        /// 统一切换 Animator 状态入口。
        /// </summary>
        /// <param name="stateName">Animator 状态名（须与 Controller 一致）</param>
        /// <param name="crossFadeDuration">过渡时长（秒）；0 表示硬切（Animator.Play）</param>
        /// <param name="time">动画播放时长，默认 0</param>
        public void PlayAnimation(string stateName, float crossFadeDuration = 0.2f, float time = 0f)
        {
            if (_animator == null || string.IsNullOrEmpty(stateName))
                return;

            if (_model != null
                && _model.State == PlayerActivityState.Dead
                && stateName != AnimatorContract.StateDie01
                && stateName != AnimatorContract.StateDie01Stay)
                return;

            if (time > 0f)
            {
                StartCoroutine(Wait());
            }
            else            {
                Validate();
            }

            IEnumerator Wait()
            {
                yield return new WaitForSeconds(Mathf.Max(0f, time - crossFadeDuration));
                Validate();
            }

            void Validate()
            {
                if (currentAnimationState == stateName)
                    return;
                currentAnimationState = stateName;

                var stateHash = Animator.StringToHash(stateName);
                if (crossFadeDuration <= 0f)
                    _animator.Play(stateHash, 0, 0f);
                else
                    _animator.CrossFade(stateHash, crossFadeDuration);

                ApplyModelStateForAnimation(stateName);
            }

        }

        void ResolveAnimator()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        void CacheAnimatorHashes()
        {
            if (_animator == null) return;
            _turnSpeedParam = Animator.StringToHash(AnimatorContract.ParamTurnSpeed);
            _verticalSpeedParam = Animator.StringToHash(AnimatorContract.ParamVerticalSpeed);
            _stateWalk = Animator.StringToHash(AnimatorContract.StateWalk);
            _stateAttack1 = Animator.StringToHash(AnimatorContract.StateAttack1);
            _stateAttack2 = Animator.StringToHash(AnimatorContract.StateAttack2);
            _stateAttack3 = Animator.StringToHash(AnimatorContract.StateAttack3);
            _stateDie01 = Animator.StringToHash(AnimatorContract.StateDie01);
            _stateDie01Stay = Animator.StringToHash(AnimatorContract.StateDie01Stay);
            _stateHit = Animator.StringToHash(AnimatorContract.StateHit);
            _stateDizzy = Animator.StringToHash(AnimatorContract.StateDizzy);
            _stateSkill1 = Animator.StringToHash(AnimatorContract.StateSkill1);
            _stateSkill2 = Animator.StringToHash(AnimatorContract.StateSkill2);
            _stateSkill3 = Animator.StringToHash(AnimatorContract.StateSkill3);
            _stateItemUse = Animator.StringToHash(AnimatorContract.StateItemUse);
        }

        void OnEnable() => GameplayInputBus.Move += HandleMove;

        void OnDisable() => GameplayInputBus.Move -= HandleMove;

        void HandleMove(Vector2 v)
        {
            var wasMoving = _moveInput.sqrMagnitude > 0.01f;
            var isMoving = v.sqrMagnitude > 0.01f;
            _moveInput = v;

            if (wasMoving == isMoving)
                return;

            GameplayInputLog.View(isMoving
                ? $"start ({v.x:0.##}, {v.y:0.##})"
                : "stop");
        }

        void Update()
        {
            if (_actionLockTimer > 0f)
                _actionLockTimer -= Time.deltaTime;
        }

        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            SyncActivityStateFromAnimator();
            if (_animator != null && _model != null)
            {
                if (CanWriteLocomotionParameters)
                    ApplyLocomotion(deltaTime);
                else
                    DecayLocomotion(deltaTime);
            }

            SyncLocomotionActivityState();
        }

        bool CanWriteLocomotionParameters =>
            _model != null
            && _model.AllowsLocomotion
            && !IsInSingleShotAnimatorState;

        void DecayLocomotion(float deltaTime)
        {
            _currentLinearSpeed = Mathf.MoveTowards(_currentLinearSpeed, 0f, _linearAcceleration * deltaTime);
            var damp = _locomotionDamp <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / _locomotionDamp);
            _smoothedVerticalSpeed = Mathf.Lerp(_smoothedVerticalSpeed, 0f, damp);
            _smoothedTurnSpeed = Mathf.Lerp(_smoothedTurnSpeed, 0f, damp);
            _animator.SetFloat(_verticalSpeedParam, _smoothedVerticalSpeed);
            _animator.SetFloat(_turnSpeedParam, _smoothedTurnSpeed);
        }

        void ApplyLocomotion(float deltaTime)
        {
            UpdateMovement(deltaTime);
            UpdateTurning(deltaTime);
        }

        void UpdateMovement(float deltaTime)
        {
            var inputMag = _moveInput.magnitude;
            var targetLinearSpeed = ResolveTargetLinearSpeed(inputMag);
            _currentLinearSpeed = Mathf.MoveTowards(
                _currentLinearSpeed,
                targetLinearSpeed,
                _linearAcceleration * deltaTime);

            var verticalBlend = _model.RunSpeed > 0f
                ? _currentLinearSpeed / _model.RunSpeed * _verticalSpeedBlendMax
                : 0f;
            verticalBlend = Mathf.Clamp(verticalBlend, 0f, _verticalSpeedBlendMax);

            var damp = _locomotionDamp <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / _locomotionDamp);
            _smoothedVerticalSpeed = Mathf.Lerp(_smoothedVerticalSpeed, verticalBlend, damp);
            _animator.SetFloat(_verticalSpeedParam, _smoothedVerticalSpeed);
        }

        float ResolveTargetLinearSpeed(float inputMag)
        {
            if (inputMag < 0.01f)
                return 0f;

            var speedCap = inputMag >= _runInputThreshold ? _model.RunSpeed : _model.MoveSpeed;
            return speedCap * inputMag;
        }

        void UpdateTurning(float deltaTime)
        {
            var targetTurnSpeed = ComputeTargetTurnSpeed(_moveInput);
            var damp = _locomotionDamp <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / _locomotionDamp);
            _smoothedTurnSpeed = Mathf.Lerp(_smoothedTurnSpeed, targetTurnSpeed, damp);
            _animator.SetFloat(_turnSpeedParam, _smoothedTurnSpeed);
        }

        float ComputeTargetTurnSpeed(Vector2 moveInput)
        {
            var worldDir = ResolveWorldMoveDirection(moveInput);
            if (worldDir.sqrMagnitude < 0.0001f)
                return 0f;

            var localDir = transform.InverseTransformDirection(worldDir);
            localDir.y = 0f;
            if (localDir.sqrMagnitude < 0.0001f)
                return 0f;

            localDir.Normalize();
            var angleRad = Mathf.Atan2(localDir.x, localDir.z);
            if (Mathf.Approximately(angleRad, 0f))
                return 0f;

            return Mathf.Sign(angleRad) * Mathf.Sqrt(Mathf.Abs(angleRad)) * _turnSpeedScale;
        }

        Vector3 ResolveWorldMoveDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            var camera = ResolveCamera();
            if (camera == null)
                return new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            var camForward = ProjectToGroundPlane(camera.transform.forward);
            var camRight = ProjectToGroundPlane(camera.transform.right);
            if (camForward.sqrMagnitude < 0.0001f || camRight.sqrMagnitude < 0.0001f)
                return new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            camForward.Normalize();
            camRight.Normalize();
            return (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }

        Camera ResolveCamera() => _camera != null ? _camera : Camera.main;

        static Vector3 ProjectToGroundPlane(Vector3 direction) =>
            Vector3.ProjectOnPlane(direction, Vector3.up);

        void ApplyModelStateForAnimation(string stateName)
        {
            if (_model == null) return;

            switch (stateName)
            {
                case AnimatorContract.StateAttack1:
                case AnimatorContract.StateAttack2:
                case AnimatorContract.StateAttack3:
                    _model.State = PlayerActivityState.Attacking;
                    break;
                case AnimatorContract.StateHit:
                    _model.State = PlayerActivityState.Hit;
                    break;
                case AnimatorContract.StateDizzy:
                    _model.State = PlayerActivityState.Dizzy;
                    break;
                case AnimatorContract.StateDie01:
                case AnimatorContract.StateDie01Stay:
                    _model.State = PlayerActivityState.Dead;
                    break;
                case AnimatorContract.StateSkill1:
                case AnimatorContract.StateSkill2:
                case AnimatorContract.StateSkill3:
                    _model.State = PlayerActivityState.UsingSkill;
                    break;
                case AnimatorContract.StateItemUse:
                    _model.State = PlayerActivityState.UsingItem;
                    break;
                case AnimatorContract.StateWalk:
                    ApplyLocomotionModelState();
                    break;
            }
        }

        void ApplyLocomotionModelState()
        {
            if (_model == null || _model.State == PlayerActivityState.Dead)
                return;

            var hasMotion = _moveInput.sqrMagnitude > 0.01f || _currentLinearSpeed > 0.05f;
            _model.State = hasMotion ? PlayerActivityState.Moving : PlayerActivityState.Idle;
        }

        void SyncActivityStateFromAnimator()
        {
            if (_model == null || _animator == null) return;

            if (IsInLocomotionAnimatorState(out _))
                return;

            if (TryGetDominantSingleShotState(out var activityState))
                _model.State = activityState;
        }

        void SyncLocomotionActivityState()
        {
            if (_model == null) return;
            if (_model.State == PlayerActivityState.Dead) return;
            if (IsInSingleShotAnimatorState || _actionLockTimer > 0f) return;

            ApplyLocomotionModelState();

            if (_moveInput.sqrMagnitude <= 0.01f && _currentLinearSpeed <= 0.05f)
                _currentLinearSpeed = Mathf.MoveTowards(_currentLinearSpeed, 0f, _linearAcceleration * Time.deltaTime);
        }

        public bool TryPlayAttack()
        {
            if (_animator == null || _model == null) return false;
            if (IsActionBlocked)
            {
                GameplayInputLog.View("TryPlayAttack rejected: blocked");
                return false;
            }

            if (_model.State == PlayerActivityState.Attacking)
            {
                GameplayInputLog.View("TryPlayAttack rejected: already attacking");
                return false;
            }

            PlayAnimation(AnimatorContract.StateAttack1, crossFadeDuration: 0f);
            GameplayInputLog.View("TryPlayAttack → Attack1 (hard cut)");
            return true;
        }

        public bool TryPlaySkill(int skillSlot)
        {
            if (_animator == null || _model == null) return false;
            if (IsActionBlocked)
            {
                GameplayInputLog.ViewAction($"TryPlaySkill({skillSlot}) rejected: {DescribeActionBlockReason()}");
                return false;
            }

            if (!TryResolveSkillStateName(skillSlot, out var stateName))
            {
                GameplayInputLog.ViewAction($"TryPlaySkill({skillSlot}) rejected: invalid slot");
                return false;
            }

            PlayAnimation(stateName);
            GameplayInputLog.ViewAction($"TryPlaySkill → {stateName}");
            return true;
        }

        /// <summary>
        /// 技能动画 Event 调用：在 Handle 上生成并发射待释放的投射物。
        /// </summary>
        public void OnAnimationLaunchProjectile()
        {
            if (_pendingSkillProjectile.HasValue)
            {
                var pending = _pendingSkillProjectile.Value;
                _pendingSkillProjectile = null;

                if (pending.Definition != null && pending.EffectSystem != null && _entity != null)
                {
                    var handle = ResolveProjectileSpawnHandle();
                    if (handle != null)
                    {
                        var projectile = CombatProjectileSpawner.SpawnAtHandle(
                            handle,
                            pending.Definition.ProjectilePrefab,
                            _entity,
                            pending.EffectSystem,
                            pending.Definition.EffectProfile,
                            pending.Request.AimPoint,
                            pending.Definition.Delivery);

                        if (projectile != null)
                        {
                            GameplayInputLog.ViewAction(
                                $"OnAnimationLaunchProjectile → {pending.Definition.Id} speed={projectile.ConfiguredSpeed:0.##} from={handle.name}");
                        }
                        else
                        {
                            GameplayInputLog.ViewAction("OnAnimationLaunchProjectile failed: spawn returned null");
                        }
                    }
                    else
                    {
                        GameplayInputLog.ViewAction("OnAnimationLaunchProjectile failed: Handle not found");
                    }
                }
                else
                {
                    GameplayInputLog.ViewAction("OnAnimationLaunchProjectile failed: missing definition/effect/caster");
                }
            }
            else
            {
                GameplayInputLog.ViewAction("OnAnimationLaunchProjectile: no pending projectile");
            }

            ApplyPostEventCastSpeed();
        }

        /// <summary>
        /// 施法开始时：读 CastClip Event 时刻，按 PreCastSeconds 设置前段 Animator.speed。
        /// </summary>
        public void BeginCastClipSpeedScaling(SkillData definition)
        {
            ResetCastClipSpeedScaling();

            if (_animator == null || definition == null)
                return;

            var timing = SkillCastClipTiming.Resolve(definition);
            if (!timing.Valid)
                return;

            _defaultAnimatorSpeed = _animator.speed;
            _activeCastTiming = timing;
            _castSpeedScalingActive = true;
            _animator.speed = timing.PrePlaybackSpeed;

            GameplayInputLog.ViewAction(
                $"BeginCastClipSpeedScaling skill={definition.Id} preSpeed={timing.PrePlaybackSpeed:0.##} " +
                $"postSpeed={timing.PostPlaybackSpeed:0.##} launch@{timing.LaunchTime:0.###}s " +
                $"targets pre={timing.PreTargetSeconds:0.##} post={timing.PostTargetSeconds:0.##}");
        }

        void ApplyPostEventCastSpeed()
        {
            if (!_castSpeedScalingActive || _animator == null || !_activeCastTiming.HasValue)
                return;

            var timing = _activeCastTiming.Value;
            if (timing.Kind != SkillKind.Projectile || !timing.UsesPostSegment)
                return;

            _animator.speed = timing.PostPlaybackSpeed;
            GameplayInputLog.ViewAction($"ApplyPostEventCastSpeed → {timing.PostPlaybackSpeed:0.##}");
        }

        /// <summary>技能动画结束或被打断时恢复 Animator 默认速度。</summary>
        public void ResetCastClipSpeedScaling()
        {
            if (_animator != null && _castSpeedScalingActive)
                _animator.speed = _defaultAnimatorSpeed;

            _activeCastTiming = null;
            _castSpeedScalingActive = false;
            _pendingSkillProjectile = null;
        }

        /// <summary>施法成功后登记待由动画 Event 发射的投射物。</summary>
        public void PrepareSkillProjectileLaunch(
            SkillData definition,
            CastRequest request,
            EffectSystem effectSystem)
        {
            if (definition == null || !definition.UsesProjectile || !definition.ProjectileSpawnOnAnimationEvent)
                return;

            _pendingSkillProjectile = new PendingSkillProjectileCast
            {
                Definition = definition,
                Request = request,
                EffectSystem = effectSystem
            };
            GameplayInputLog.ViewAction($"PrepareSkillProjectileLaunch skill={definition.Id}");
        }

        Transform ResolveProjectileSpawnHandle()
        {
            if (_projectileSpawnHandle != null)
                return _projectileSpawnHandle;

            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "Handle")
                {
                    _projectileSpawnHandle = child;
                    return _projectileSpawnHandle;
                }
            }

            return null;
        }

        /// <summary>按 Input Action 名（Skill1–Skill3）播放对应技能动画。</summary>
        public bool TryPlaySkillForAction(string actionName)
        {
            if (!TryResolveSkillSlot(actionName, out var skillSlot))
                return false;

            return TryPlaySkill(skillSlot);
        }

        public bool TryPlayItem()
        {
            if (_animator == null || _model == null) return false;
            if (IsActionBlocked)
            {
                GameplayInputLog.ViewAction($"TryPlayItem rejected: {DescribeActionBlockReason()}");
                return false;
            }

            PlayAnimation(AnimatorContract.StateItemUse);
            GameplayInputLog.ViewAction("TryPlayItem → ItemUse");
            return true;
        }

        static bool TryResolveSkillSlot(string actionName, out int skillSlot)
        {
            skillSlot = 0;
            if (actionName == GameInputActions.Skill1) skillSlot = 1;
            else if (actionName == GameInputActions.Skill2) skillSlot = 2;
            else if (actionName == GameInputActions.Skill3) skillSlot = 3;
            return skillSlot > 0;
        }

        static bool TryResolveSkillStateName(int skillSlot, out string stateName)
        {
            switch (skillSlot)
            {
                case 1:
                    stateName = AnimatorContract.StateSkill1;
                    return true;
                case 2:
                    stateName = AnimatorContract.StateSkill2;
                    return true;
                case 3:
                    stateName = AnimatorContract.StateSkill3;
                    return true;
                default:
                    stateName = null;
                    return false;
            }
        }

        public void PlayHit()
        {
            if (_animator == null || _model == null)
            {
                GameplayCombatLog.HitReaction(Faction.Player, _entity?.EntityId ?? 0, "PlayHit skipped: no Animator/Model");
                return;
            }

            if (_model.State == PlayerActivityState.Dead)
            {
                GameplayCombatLog.HitReaction(Faction.Player, _entity.EntityId, "PlayHit skipped: Dead");
                return;
            }

            if (IsSingleShotPlaying(_stateHit))
            {
                GameplayCombatLog.HitReaction(Faction.Player, _entity.EntityId, "PlayHit skipped: Hit already playing");
                return;
            }

            ResetCastClipSpeedScaling();
            PlayAnimation(AnimatorContract.StateHit);
            GameplayCombatLog.HitReaction(Faction.Player, _entity.EntityId, "PlayHit → Hit");
        }

        public void PlayDizzy()
        {
            if (_animator == null || _model == null) return;
            if (_model.State == PlayerActivityState.Dead) return;
            if (IsSingleShotPlaying(_stateDizzy)) return;

            PlayAnimation(AnimatorContract.StateDizzy, 0.1f);
        }

        public void PlayDie()
        {
            if (_animator == null || _model == null) return;
            if (_model.State == PlayerActivityState.Dead) return;

            PlayAnimation(AnimatorContract.StateDie01);
        }

        public void PlayDieStay()
        {
            if (_animator == null || _model == null) return;

            PlayAnimation(AnimatorContract.StateDie01Stay);
        }

        bool TryGetDominantSingleShotState(out PlayerActivityState activityState)
        {
            activityState = PlayerActivityState.Idle;
            if (_animator == null) return false;

            var info = _animator.GetCurrentAnimatorStateInfo(0);
            var hash = info.shortNameHash;

            if (hash == _stateDie01 || hash == _stateDie01Stay)
            {
                activityState = PlayerActivityState.Dead;
                return true;
            }

            if (hash == _stateHit)
            {
                activityState = PlayerActivityState.Hit;
                return true;
            }

            if (hash == _stateDizzy)
            {
                activityState = PlayerActivityState.Dizzy;
                return true;
            }

            if (hash == _stateAttack1 || hash == _stateAttack2 || hash == _stateAttack3)
            {
                activityState = PlayerActivityState.Attacking;
                return true;
            }

            if (hash == _stateSkill1 || hash == _stateSkill2 || hash == _stateSkill3)
            {
                activityState = PlayerActivityState.UsingSkill;
                return true;
            }

            if (hash == _stateItemUse)
            {
                activityState = PlayerActivityState.UsingItem;
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
                detail = DescribeAnimatorState(current);
                return true;
            }

            if (_animator.IsInTransition(0))
            {
                var next = _animator.GetNextAnimatorStateInfo(0);
                if (next.shortNameHash == _stateWalk)
                {
                    detail = $"→{DescribeAnimatorState(next)}";
                    return true;
                }
            }

            return false;
        }

        string DescribeAnimatorState(AnimatorStateInfo info)
        {
            if (info.shortNameHash == _stateWalk) return AnimatorContract.StateWalk;
            if (info.shortNameHash == _stateAttack1) return AnimatorContract.StateAttack1;
            if (info.shortNameHash == _stateAttack2) return AnimatorContract.StateAttack2;
            if (info.shortNameHash == _stateAttack3) return AnimatorContract.StateAttack3;
            if (info.shortNameHash == _stateHit) return AnimatorContract.StateHit;
            if (info.shortNameHash == _stateDizzy) return AnimatorContract.StateDizzy;
            if (info.shortNameHash == _stateSkill1) return AnimatorContract.StateSkill1;
            if (info.shortNameHash == _stateSkill2) return AnimatorContract.StateSkill2;
            if (info.shortNameHash == _stateSkill3) return AnimatorContract.StateSkill3;
            if (info.shortNameHash == _stateItemUse) return AnimatorContract.StateItemUse;
            if (info.shortNameHash == _stateDie01) return AnimatorContract.StateDie01;
            if (info.shortNameHash == _stateDie01Stay) return AnimatorContract.StateDie01Stay;
            return $"hash={info.shortNameHash}";
        }

        bool IsSingleShotPlaying(int stateHash)
        {
            if (_animator == null) return false;
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            return info.shortNameHash == stateHash && info.normalizedTime < 1f;
        }
    }
}
