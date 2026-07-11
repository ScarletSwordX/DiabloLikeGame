using System.Collections.Generic;
using Gameplay.Data;

namespace Gameplay.Item.Model
{
    public class ItemRuntimeModel
    {
        public readonly Dictionary<string, ItemDefinition> Definitions = new Dictionary<string, ItemDefinition>();
        public readonly HashSet<int> ConsumedInstances = new HashSet<int>();
        readonly Dictionary<int, Dictionary<string, int>> _chargesByUser = new Dictionary<int, Dictionary<string, int>>();

        public void Register(ItemDefinition def)
        {
            if (def != null && !string.IsNullOrEmpty(def.Id))
                Definitions[def.Id] = def;
        }

        public bool IsConsumed(int instanceId) => ConsumedInstances.Contains(instanceId);

        public int GetCharges(int userId, string itemDefinitionId)
        {
            if (!_chargesByUser.TryGetValue(userId, out var bag))
                return 0;
            return bag.TryGetValue(itemDefinitionId, out var count) ? count : 0;
        }

        public void AddCharges(int userId, string itemDefinitionId, int amount)
        {
            if (amount <= 0) return;
            if (!_chargesByUser.TryGetValue(userId, out var bag))
            {
                bag = new Dictionary<string, int>();
                _chargesByUser[userId] = bag;
            }
            bag.TryGetValue(itemDefinitionId, out var current);
            bag[itemDefinitionId] = current + amount;
        }

        public bool TryConsumeCharge(int userId, string itemDefinitionId)
        {
            if (!_chargesByUser.TryGetValue(userId, out var bag))
                return false;
            if (!bag.TryGetValue(itemDefinitionId, out var count) || count <= 0)
                return false;
            bag[itemDefinitionId] = count - 1;
            return true;
        }
    }
}
