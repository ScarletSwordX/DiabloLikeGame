namespace Gameplay.Data
{
    /// <summary>
    /// 技能阶段模型：决定 PreCast / Duration / PostCast 哪些字段生效。
    /// </summary>
    public enum SkillKind
    {
        /// <summary>投射物：前摇 + 后摇，无 Duration。</summary>
        Projectile = 0,

        /// <summary>持续型：前摇 + 持续 + 后摇。</summary>
        Channeled = 1,

        /// <summary>状态赋予：仅前摇，效果在前摇结束后结算。</summary>
        StatusApply = 2
    }
}
