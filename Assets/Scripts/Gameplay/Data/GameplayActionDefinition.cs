using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 历史共用基类（阶段时间）。技能请使用 <see cref="SkillData"/>；道具请使用 <see cref="ItemDefinition"/>（瞬时效果，无阶段时间）。
    /// </summary>
    public abstract class GameplayActionDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("阶段时间（秒）")]
        [Tooltip("使用前摇：效果生效前的准备时间")]
        public float PreCastSeconds;

        [Tooltip("持续时间：引导或效果维持窗口（0 表示瞬时）")]
        public float DurationSeconds;

        [Tooltip("使用后摇：效果后的恢复时间")]
        public float PostCastSeconds;

        [Header("效果（伤害 / 恢复 / 状态 可同时存在）")]
        public ActionEffectProfile EffectProfile = new ActionEffectProfile();
    }
}
