using DamageNumbersPro;
using UnityEngine;

namespace Gameplay.Feedback
{
    /// <summary>
    /// 场景中挂载；提供伤害数字等玩法反馈实现。
    /// </summary>
    public class GameplayFeedbackProvider : MonoBehaviour
    {
        [Header("Damage Numbers Pro")]
        [SerializeField] DamageNumber _damageNumberPrefab;
        [SerializeField] float _damageNumberHeightOffset = 1.5f;

        IGameplayFeedback _impl;

        public IGameplayFeedback Feedback => _impl ??= BuildFeedback();

        IGameplayFeedback BuildFeedback()
        {
            if (_damageNumberPrefab != null)
                return new DamageNumbersGameplayFeedback(_damageNumberPrefab, _damageNumberHeightOffset);

            return new NullGameplayFeedback();
        }

        void OnValidate() => _impl = null;
    }
}
