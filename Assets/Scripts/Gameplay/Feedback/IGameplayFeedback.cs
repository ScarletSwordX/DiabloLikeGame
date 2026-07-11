using Gameplay.Core;
using UnityEngine;

namespace Gameplay.Feedback
{
    /// <summary>
    /// 玩法反馈接口。P0 使用 NullGameplayFeedback；后续由 Feel MMFeedbacks 实现。
    /// </summary>
    public interface IGameplayFeedback
    {
        void OnSkillCast(int casterId, string skillId, Vector3 position);
        void OnDamage(int targetId, float amount, Vector3 position);
        void OnHeal(int targetId, float amount, Vector3 position);
        void OnItemPickup(int pickerId, string itemId, Vector3 position);
        void OnBuffApplied(int targetId, string buffId, Vector3 position);
    }
}
