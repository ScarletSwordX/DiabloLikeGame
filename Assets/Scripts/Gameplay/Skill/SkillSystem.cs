using System.Collections;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.Input;
using Gameplay.Skill.Presenter;
using Gameplay.Skill.View;
using UnityEngine;

namespace Gameplay.Skill
{
    public class SkillSystem : MonoBehaviour
    {
        readonly SkillPresenter _presenter = new SkillPresenter();
        [SerializeField] SkillData[] _equippedSkills;

        SkillCastView _view;
        Coroutine _castRoutine;

        public bool IsCasting => _castRoutine != null;

        void Awake()
        {
            _view = GetComponent<SkillCastView>();
            if (_view == null)
                _view = gameObject.AddComponent<SkillCastView>();
            _presenter.BindView(_view);
        }

        public void Initialize(EffectSystem effectSystem, SkillData[] equipped)
        {
            _presenter.Initialize(effectSystem, equipped);
            SetEquipped(equipped);
        }

        public void SetEquipped(SkillData[] equipped)
        {
            _equippedSkills = equipped;
            _presenter.SetEquipped(equipped);
        }

        void Update()
        {
            var player = Object.FindObjectOfType<Character.PlayerController>();
            if (player == null) return;
            _presenter.TickCooldowns(player.EntityId);
        }

        public CastResult TryCast(CastRequest request)
        {
            GameplayInputLog.Skill(request.SkillId, $"TryCast caster={request.CasterId} instance={request.CooldownInstanceId}");

            if (IsCasting)
            {
                GameplayInputLog.Skill(request.SkillId, "rejected: already casting");
                return new CastResult { Status = CastResultStatus.InvalidState };
            }

            var validation = _presenter.ValidateCast(request, out var def);
            if (validation.Status != CastResultStatus.Success)
            {
                GameplayInputLog.Skill(request.SkillId, $"rejected: {validation.Status}");
                return validation;
            }

            _castRoutine = StartCoroutine(CastPhaseRoutine(request, def));
            GameplayInputLog.Skill(request.SkillId, $"accepted: InProgress kind={def.Kind}");
            return new CastResult { Status = CastResultStatus.InProgress };
        }

        IEnumerator CastPhaseRoutine(CastRequest request, SkillData def)
        {
            switch (def.Kind)
            {
                case SkillKind.Projectile:
                    yield return RunProjectilePhases(request, def);
                    break;
                case SkillKind.Channeled:
                    yield return RunChanneledPhases(request, def);
                    break;
                case SkillKind.StatusApply:
                    yield return RunStatusApplyPhases(request, def);
                    break;
                default:
                    yield return RunLegacyPhases(request, def);
                    break;
            }

            _castRoutine = null;
        }

        IEnumerator RunProjectilePhases(CastRequest request, SkillData def)
        {
            if (def.UsesAnimationEventProjectileLaunch)
            {
                var apply = _presenter.ApplyCast(request);
                if (apply.Status != CastResultStatus.Success)
                {
                    GameplayInputLog.Skill(request.SkillId, $"ApplyCast failed: {apply.Status}");
                    yield break;
                }

                GameplayInputLog.Skill(request.SkillId, "ApplyCast success (projectile anim-event)");

                var lockSeconds = def.GetLogicCastLockSeconds();
                if (lockSeconds > 0f)
                    yield return new WaitForSeconds(lockSeconds);
                yield break;
            }

            if (def.EffectivePreCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePreCastSeconds);

            var instantApply = _presenter.ApplyCast(request);
            if (instantApply.Status != CastResultStatus.Success)
            {
                GameplayInputLog.Skill(request.SkillId, $"ApplyCast failed: {instantApply.Status}");
                yield break;
            }

            GameplayInputLog.Skill(request.SkillId, "ApplyCast success (projectile instant)");

            if (def.EffectivePostCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePostCastSeconds);
        }

        IEnumerator RunChanneledPhases(CastRequest request, SkillData def)
        {
            if (def.EffectivePreCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePreCastSeconds);

            var apply = _presenter.ApplyCast(request);
            if (apply.Status != CastResultStatus.Success)
            {
                GameplayInputLog.Skill(request.SkillId, $"ApplyCast failed: {apply.Status}");
                yield break;
            }

            GameplayInputLog.Skill(request.SkillId, "ApplyCast success (channeled)");

            if (def.EffectiveDurationSeconds > 0f)
                yield return new WaitForSeconds(def.EffectiveDurationSeconds);

            if (def.EffectivePostCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePostCastSeconds);
        }

        IEnumerator RunStatusApplyPhases(CastRequest request, SkillData def)
        {
            if (def.EffectivePreCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePreCastSeconds);

            var apply = _presenter.ApplyCast(request);
            if (apply.Status != CastResultStatus.Success)
            {
                GameplayInputLog.Skill(request.SkillId, $"ApplyCast failed: {apply.Status}");
                yield break;
            }

            GameplayInputLog.Skill(request.SkillId, "ApplyCast success (status apply)");
        }

        IEnumerator RunLegacyPhases(CastRequest request, SkillData def)
        {
            if (def.EffectivePreCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePreCastSeconds);

            var apply = _presenter.ApplyCast(request);
            if (apply.Status != CastResultStatus.Success)
                yield break;

            if (def.EffectiveDurationSeconds > 0f)
                yield return new WaitForSeconds(def.EffectiveDurationSeconds);

            if (def.EffectivePostCastSeconds > 0f)
                yield return new WaitForSeconds(def.EffectivePostCastSeconds);
        }

        public CooldownQueryResult QueryCooldown(int casterId, string cooldownInstanceId, string skillIdFallback = null) =>
            _presenter.QueryCooldown(casterId, cooldownInstanceId, skillIdFallback);

        public CooldownQueryResult QueryCooldownForSlot(int casterId, int slotIndex)
        {
            var slot = _presenter.Model.GetSlot(slotIndex);
            if (slot.IsEmpty)
                return new CooldownQueryResult { CanCast = true, RemainingSeconds = 0f };
            return QueryCooldown(casterId, slot.InstanceUuid, slot.SkillId);
        }

        public SkillData GetDefinition(string skillId) =>
            _presenter.Model.TryGet(skillId, out var d) ? d : null;

        public SkillSlotInstance GetSlotInstance(int slotIndex) => _presenter.Model.GetSlot(slotIndex);

        public SkillSlotInstance[] SlotInstances => _presenter.Model.Slots;

        public SkillData[] Equipped => _equippedSkills;
    }
}
