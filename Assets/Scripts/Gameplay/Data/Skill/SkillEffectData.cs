using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 技能效果：命中后造成的伤害、治疗、状态等结果配置。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillEffect", menuName = "Gameplay/Skill Effect")]
    public class SkillEffectData : ScriptableObject
    {
        public string Id;
        [TextArea] public string Description;

        public ActionEffectProfile Profile = new ActionEffectProfile();
    }
}
