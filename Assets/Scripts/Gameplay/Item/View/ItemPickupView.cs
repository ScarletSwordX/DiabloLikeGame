using Gameplay.Item.Model;
using UnityEngine;

namespace Gameplay.Item.View
{
    public class ItemPickupView : MonoBehaviour
    {
        ItemRuntimeModel _model;
        [SerializeField] string _itemDefinitionId = "heal_potion";

        public ItemRuntimeModel Model => _model;
        public string ItemDefinitionId => _itemDefinitionId;

        public void Bind(ItemRuntimeModel model) => _model = model;

        public void OnConsumed()
        {
            gameObject.SetActive(false);
        }
    }
}
