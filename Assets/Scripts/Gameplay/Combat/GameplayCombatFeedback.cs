using Gameplay.EventBus;
using Gameplay.Feedback;
using UnityEngine;

namespace Gameplay.Combat
{
    /// <summary>
    /// 统一发布伤害事件与飘字反馈，避免各伤害入口重复接线。
    /// </summary>
    public static class GameplayCombatFeedback
    {
        static GameplayFeedbackProvider _provider;

        public static void NotifyDamageDealt(int sourceId, int targetId, float amount, Vector3 worldPosition)
        {
            if (amount <= 0f)
                return;

            ResolveProvider()?.Feedback.OnDamage(targetId, amount, worldPosition);

            GameEventBus.Instance.Publish(new DamageDealtEvent
            {
                SourceId = sourceId,
                TargetId = targetId,
                Amount = amount,
                WorldPosition = worldPosition
            });
        }

        static GameplayFeedbackProvider ResolveProvider()
        {
            if (_provider == null)
                _provider = Object.FindObjectOfType<GameplayFeedbackProvider>();
            return _provider;
        }
    }
}
