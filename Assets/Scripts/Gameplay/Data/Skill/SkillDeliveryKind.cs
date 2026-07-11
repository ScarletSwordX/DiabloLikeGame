namespace Gameplay.Data
{
    /// <summary>技能如何命中目标。</summary>
    public enum SkillDeliveryKind
    {
        /// <summary>施法时立即在作用区域内结算效果。</summary>
        InstantArea = 0,

        /// <summary>生成投射物，碰撞目标时结算效果。</summary>
        Projectile = 1
    }
}
