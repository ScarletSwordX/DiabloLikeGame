using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Item;
using Gameplay.Item.View;
using UnityEngine;

namespace Gameplay.WorldInteraction
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(ItemPickupView))]
    public class PickupItem : MonoBehaviour
    {
        static int _nextInstanceId = 1000;

        [SerializeField] ItemSystem _itemSystem;
        ItemPickupView _view;
        int _instanceId;

        public void AssignItemSystem(ItemSystem system)
        {
            _itemSystem = system;
            if (_view == null)
                _view = GetComponent<ItemPickupView>();
            if (_view != null && _itemSystem != null)
                _view.Bind(_itemSystem.Presenter.Model);
        }

        void Awake()
        {
            _instanceId = _nextInstanceId++;
            _view = GetComponent<ItemPickupView>();
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            if (_itemSystem != null)
                _view.Bind(_itemSystem.Presenter.Model);
        }

        void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;
            if (_itemSystem == null)
                _itemSystem = FindObjectOfType<ItemSystem>();

            _view.Bind(_itemSystem.Presenter.Model);
            var result = _itemSystem.TryPickup(new PickupRequest
            {
                PickerId = player.EntityId,
                ItemInstanceId = _instanceId,
                ItemDefinitionId = _view.ItemDefinitionId
            }, _view);

            if (result.Status == PickupResultStatus.Success)
                _instanceId = _nextInstanceId;
        }
    }
}
