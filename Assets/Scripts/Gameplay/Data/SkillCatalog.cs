using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 技能表：集中存放可选 SkillData，供 Loadout 槽位下拉解析。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillCatalog", menuName = "Gameplay/Skill Catalog")]
    public class SkillCatalog : ScriptableObject
    {
        public SkillData[] Entries = System.Array.Empty<SkillData>();

        readonly Dictionary<string, SkillData> _byId = new Dictionary<string, SkillData>();

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

        public bool TryGet(string skillId, out SkillData definition)
        {
            if (_byId.Count == 0)
                RebuildIndex();
            return _byId.TryGetValue(skillId, out definition);
        }

        public SkillData GetOrNull(string skillId) =>
            TryGet(skillId, out var def) ? def : null;
    }
}
