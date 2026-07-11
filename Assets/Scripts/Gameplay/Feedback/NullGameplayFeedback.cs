using UnityEngine;

namespace Gameplay.Feedback
{
    public sealed class NullGameplayFeedback : IGameplayFeedback
    {
        public void OnSkillCast(int casterId, string skillId, Vector3 position) { }
        public void OnDamage(int targetId, float amount, Vector3 position) { }
        public void OnHeal(int targetId, float amount, Vector3 position) { }
        public void OnItemPickup(int pickerId, string itemId, Vector3 position) { }
        public void OnBuffApplied(int targetId, string buffId, Vector3 position) { }
    }
}
