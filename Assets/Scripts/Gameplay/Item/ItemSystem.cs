using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using Gameplay.Input;
using Gameplay.Item.Presenter;
using UnityEngine;

namespace Gameplay.Item
{
    public class ItemSystem : MonoBehaviour
    {
        readonly ItemPresenter _presenter = new ItemPresenter();

        public void Initialize(EffectSystem effectSystem, ItemDefinition[] definitions) =>
            _presenter.Initialize(effectSystem, definitions);

        public PickupResult TryPickup(PickupRequest request, View.ItemPickupView view = null) =>
            _presenter.TryPickup(request, view);

        public UseResult TryUse(UseRequest request)
        {
            GameplayInputLog.Item(request.ItemDefinitionId, $"TryUse user={request.UserId}");
            var result = _presenter.TryUse(request);
            GameplayInputLog.Item(
                request.ItemDefinitionId,
                result.Status == UseResultStatus.Success ? "success" : $"rejected: {result.Status}");
            return result;
        }

        public int GetCharges(int userId, string itemDefinitionId) =>
            _presenter.Model.GetCharges(userId, itemDefinitionId);

        public bool IsConsumed(int instanceId) => _presenter.Model.IsConsumed(instanceId);

        public ItemPresenter Presenter => _presenter;
    }
}
