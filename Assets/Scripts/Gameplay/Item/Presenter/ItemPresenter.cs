using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.EventBus;
using Gameplay.Feedback;
using Gameplay.Item.Model;
using Gameplay.Item.View;
using UnityEngine;

namespace Gameplay.Item.Presenter
{
    public class ItemPresenter
    {
        readonly ItemRuntimeModel _model = new ItemRuntimeModel();
        EffectSystem _effectSystem;
        GameplayFeedbackProvider _feedbackProvider;

        public ItemRuntimeModel Model => _model;

        public void Initialize(EffectSystem effectSystem, ItemDefinition[] definitions)
        {
            _effectSystem = effectSystem;
            _model.Definitions.Clear();
            if (definitions != null)
            {
                foreach (var def in definitions)
                    _model.Register(def);
            }
            _feedbackProvider = Object.FindObjectOfType<GameplayFeedbackProvider>();
        }

        public PickupResult TryPickup(PickupRequest request, ItemPickupView pickupView)
        {
            if (_model.ConsumedInstances.Contains(request.ItemInstanceId))
                return new PickupResult { Status = PickupResultStatus.AlreadyConsumed };

            if (!_model.Definitions.TryGetValue(request.ItemDefinitionId, out var def))
                return new PickupResult { Status = PickupResultStatus.Invalid };

            var picker = FindEntity(request.PickerId);
            if (picker == null)
                return new PickupResult { Status = PickupResultStatus.Invalid };

            string logMessage;
            if (def.AppliesEffectOnPickup)
            {
                ApplyItemEffects(def, request.PickerId, picker);
                logMessage = $"拾取 {def.DisplayName}（即时生效）";
            }
            else if (def.AddsChargesOnPickup)
            {
                _model.AddCharges(request.PickerId, def.Id, def.Charges);
                logMessage = $"拾取 {def.DisplayName}（+{def.Charges} 次，快捷键栏可用）";
            }
            else
            {
                return new PickupResult { Status = PickupResultStatus.Invalid };
            }

            _model.ConsumedInstances.Add(request.ItemInstanceId);

            var pos = picker.transform.position;
            Feedback.OnItemPickup(request.PickerId, request.ItemDefinitionId, pos);
            pickupView?.OnConsumed();

            GameEventBus.Instance.Publish(new ItemPickedUpEvent
            {
                ItemDefinitionId = request.ItemDefinitionId,
                PickerId = request.PickerId
            });
            GameEventBus.Instance.Publish(new ItemConsumedEvent
            {
                ItemDefinitionId = request.ItemDefinitionId,
                ItemInstanceId = request.ItemInstanceId
            });
            GameEventBus.Instance.Publish(new CombatLogEvent { Message = logMessage });

            return new PickupResult { Status = PickupResultStatus.Success };
        }

        public UseResult TryUse(UseRequest request)
        {
            if (!_model.Definitions.TryGetValue(request.ItemDefinitionId, out var def))
                return new UseResult { Status = UseResultStatus.Invalid };

            if (!def.RequiresHotbarUse)
                return new UseResult { Status = UseResultStatus.Invalid };

            var user = FindEntity(request.UserId);
            if (user == null)
                return new UseResult { Status = UseResultStatus.Invalid };

            if (_model.GetCharges(request.UserId, request.ItemDefinitionId) <= 0)
                return new UseResult { Status = UseResultStatus.NoCharges };

            if (!_model.TryConsumeCharge(request.UserId, request.ItemDefinitionId))
                return new UseResult { Status = UseResultStatus.NoCharges };

            ApplyItemEffects(def, request.UserId, user);

            GameEventBus.Instance.Publish(new CombatLogEvent { Message = $"使用 {def.DisplayName}" });
            return new UseResult { Status = UseResultStatus.Success };
        }

        void ApplyItemEffects(ItemDefinition def, int sourceId, CharacterEntity target)
        {
            var ctx = new ActionEffectContext
            {
                SourceId = sourceId,
                PrimaryTargetId = target.EntityId,
                WorldPosition = target.transform.position,
                Radius = def.EffectProfile != null ? def.EffectProfile.AreaRadius : 0f
            };
            _effectSystem.ApplyProfile(def.EffectProfile, ctx);
        }

        static CharacterEntity FindEntity(int id)
        {
            foreach (var c in Object.FindObjectsOfType<CharacterEntity>())
                if (c.EntityId == id) return c;
            return null;
        }

        IGameplayFeedback Feedback =>
            _feedbackProvider != null ? _feedbackProvider.Feedback : new NullGameplayFeedback();
    }
}
