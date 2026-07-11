using Gameplay.Character;

using Gameplay.Combat;

using Gameplay.Core;

using Gameplay.Data;

using Gameplay.Effect;

using Gameplay.EventBus;

using Gameplay.Feedback;

using Gameplay.Skill;

using Gameplay.Skill.Model;

using Gameplay.Skill.View;

using UnityEngine;



namespace Gameplay.Skill.Presenter

{

    public class SkillPresenter

    {

        readonly SkillRuntimeModel _model = new SkillRuntimeModel();

        SkillCastView _view;

        EffectSystem _effectSystem;

        GameplayFeedbackProvider _feedbackProvider;



        public SkillRuntimeModel Model => _model;



        public void BindView(SkillCastView view) => _view = view;



        public void Initialize(EffectSystem effectSystem, SkillData[] equipped)

        {

            _effectSystem = effectSystem;

            SetEquipped(equipped);

            _view?.Bind(_model);

            _feedbackProvider = Object.FindObjectOfType<GameplayFeedbackProvider>();

        }



        public void SetEquipped(SkillData[] equipped) => _model.SetLoadout(equipped, _model.Slots);



        public void TickCooldowns(int casterId)
        {
            var keys = new System.Collections.Generic.List<string>();
            var skillByInstance = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var slot in _model.Slots)
            {
                if (slot.IsEmpty)
                    continue;
                keys.Add(slot.InstanceUuid);
                skillByInstance[slot.InstanceUuid] = slot.SkillId;
            }

            if (keys.Count == 0)
                return;

            _model.Cooldown.UpdateAll(keys, casterId, id =>
                skillByInstance.TryGetValue(id, out var skillId) ? skillId : null);
        }

        public CastResult ValidateCast(CastRequest request, out SkillData definition)

        {

            definition = null;

            GameEventBus.Instance.Publish(new SkillCastAttemptedEvent

            {

                SkillId = request.SkillId,

                CasterId = request.CasterId

            });



            if (!_model.TryGet(request.SkillId, out definition))

                return Fail(request, "InvalidState");



            if (!definition.IsActiveSkill)

                return Fail(request, "InvalidState");



            var cd = QueryCooldown(request.CasterId, request.CooldownInstanceId, request.SkillId);

            if (!cd.CanCast)

                return Fail(request, "OnCooldown");



            if (FindCaster(request.CasterId) == null)

                return Fail(request, "InvalidState");



            return new CastResult { Status = CastResultStatus.Success };

        }



        public CastResult ApplyCast(CastRequest request)

        {

            if (!_model.TryGet(request.SkillId, out var def))

                return Fail(request, "InvalidState");



            var caster = FindCaster(request.CasterId);

            if (caster == null)

                return Fail(request, "InvalidState");



            var ctx = BuildEffectContext(def, request, caster);

            if (def.UsesProjectile)

            {

                if (!def.ProjectileSpawnOnAnimationEvent)

                    CombatProjectileSpawner.Spawn(def, caster, _effectSystem, request);

            }

            else

            {

                _effectSystem.ApplyProfile(def.EffectProfile, ctx);

            }



            var cdKey = ResolveCooldownKey(request);

            _model.Cooldown.StartCooldown(request.CasterId, cdKey, def.CooldownSeconds, request.SkillId);



            var pos = def.UsesProjectile

                ? caster.transform.position + caster.transform.forward

                : ctx.WorldPosition;

            Feedback.OnSkillCast(request.CasterId, request.SkillId, pos);

            _view?.OnCastSucceeded(request.SkillId, pos);



            GameEventBus.Instance.Publish(new SkillCastSucceededEvent

            {

                SkillId = request.SkillId,

                CasterId = request.CasterId

            });

            GameEventBus.Instance.Publish(new CombatLogEvent { Message = $"释放 {def.DisplayName}" });



            return new CastResult { Status = CastResultStatus.Success };

        }



        public CooldownQueryResult QueryCooldown(int casterId, string cooldownInstanceId, string skillIdFallback = null) =>

            _model.Cooldown.Query(new CooldownQuery

            {

                CasterId = casterId,

                CooldownInstanceId = cooldownInstanceId,

                SkillId = skillIdFallback

            });



        static string ResolveCooldownKey(CastRequest request) =>

            !string.IsNullOrEmpty(request.CooldownInstanceId)

                ? request.CooldownInstanceId

                : request.SkillId;



        CastResult Fail(CastRequest request, string reason)

        {

            var status = reason == "OnCooldown" ? CastResultStatus.OnCooldown :

                reason == "NoTarget" ? CastResultStatus.NoTarget : CastResultStatus.InvalidState;



            GameEventBus.Instance.Publish(new SkillCastFailedEvent

            {

                SkillId = request.SkillId,

                CasterId = request.CasterId,

                Reason = reason

            });

            GameEventBus.Instance.Publish(new CombatLogEvent { Message = $"技能失败: {reason}" });

            return new CastResult { Status = status };

        }



        static ActionEffectContext BuildEffectContext(SkillData def, CastRequest cast, CharacterEntity caster)

        {

            if (def.UsesProjectile && def.Delivery != null)

            {

                return new ActionEffectContext

                {

                    SourceId = cast.CasterId,

                    PrimaryTargetId = cast.CasterId,

                    WorldPosition = cast.AimPoint,

                    Radius = 0f

                };

            }



            var area = def.Area ?? new SkillAreaSettings();

            var worldPos = SkillAreaResolver.ResolveWorldCenter(caster, area);

            var radius = area.ResolveEffectRadius();



            return new ActionEffectContext

            {

                SourceId = cast.CasterId,

                PrimaryTargetId = cast.CasterId,

                WorldPosition = worldPos,

                Radius = radius

            };

        }



        static CharacterEntity FindCaster(int id)

        {

            foreach (var c in Object.FindObjectsOfType<CharacterEntity>())

                if (c.EntityId == id) return c;

            return null;

        }



        IGameplayFeedback Feedback =>

            _feedbackProvider != null ? _feedbackProvider.Feedback : new NullGameplayFeedback();

    }

}


