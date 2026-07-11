using DamageNumbersPro;
using UnityEngine;

namespace Gameplay.Feedback
{
    /// <summary>
    /// 使用 Damage Numbers Pro 在世界空间弹出伤害数字。
    /// </summary>
    public sealed class DamageNumbersGameplayFeedback : IGameplayFeedback
    {
        readonly DamageNumber _prefab;
        readonly float _heightOffset;

        public DamageNumbersGameplayFeedback(DamageNumber prefab, float heightOffset = 1.5f)
        {
            _prefab = prefab;
            _heightOffset = heightOffset;
        }

        public void OnSkillCast(int casterId, string skillId, Vector3 position) { }

        public void OnDamage(int targetId, float amount, Vector3 position)
        {
            if (_prefab == null || amount <= 0f)
                return;

            var spawnPos = position + Vector3.up * _heightOffset;
            _prefab.Spawn(spawnPos, amount);
        }

        public void OnHeal(int targetId, float amount, Vector3 position) { }
        public void OnItemPickup(int pickerId, string itemId, Vector3 position) { }
        public void OnBuffApplied(int targetId, string buffId, Vector3 position) { }
    }

}
