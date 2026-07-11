using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 道具表：集中存放可选 ItemDefinition，供 Loadout 槽位下拉解析。
    /// </summary>
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "Gameplay/Item Catalog")]
    public class ItemCatalog : ScriptableObject
    {
        public ItemDefinition[] Entries = System.Array.Empty<ItemDefinition>();

        readonly Dictionary<string, ItemDefinition> _byId = new Dictionary<string, ItemDefinition>();

        void OnEnable() => RebuildIndex();

        public void RebuildIndex()
        {
            _byId.Clear();
            if (Entries == null) return;
            foreach (var entry in Entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.Id))
                    _byId[entry.Id] = entry;
            }
        }

        public bool TryGet(string itemId, out ItemDefinition definition)
        {
            if (_byId.Count == 0)
                RebuildIndex();
            return _byId.TryGetValue(itemId, out definition);
        }

        public ItemDefinition GetOrNull(string itemId) =>
            TryGet(itemId, out var def) ? def : null;
    }
}
