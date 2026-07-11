using Gameplay.Skill.Model;
using UnityEngine;

namespace Gameplay.Skill.View
{
    /// <summary>
    /// 技能视觉占位（P0 无特效，后续 Feel）。
    /// </summary>
    public class SkillCastView : MonoBehaviour
    {
        SkillRuntimeModel _model;

        public SkillRuntimeModel Model => _model;

        public void Bind(SkillRuntimeModel model) => _model = model;

        public void OnCastSucceeded(string skillId, Vector3 position)
        {
            // P1: 接 IGameplayFeedback / Feel
        }
    }
}
