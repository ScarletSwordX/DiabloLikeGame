using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 状态表：集中存放角色可能出现的 StatusDefinition。
    /// </summary>
    [CreateAssetMenu(fileName = "StatusCatalog", menuName = "Gameplay/Status Catalog")]
    public class StatusCatalog : ScriptableObject
    {
        public StatusDefinition[] Entries = System.Array.Empty<StatusDefinition>();

        readonly Dictionary<string, StatusDefinition> _byId = new Dictionary<string, StatusDefinition>();

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

        public bool TryGet(string statusId, out StatusDefinition definition)
        {
            if (_byId.Count == 0)
                RebuildIndex();
            return _byId.TryGetValue(statusId, out definition);
        }

        public StatusDefinition GetOrNull(string statusId) =>
            TryGet(statusId, out var def) ? def : null;

        public void RegisterRuntime(StatusDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.Id)) return;
            _byId[definition.Id] = definition;
        }
    }
}
