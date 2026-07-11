namespace Gameplay.Character.Player.Model
{
    public enum PlayerActivityState
    {
        Idle,
        Moving,
        Attacking,
        UsingSkill,
        UsingItem,
        Hit,
        Dizzy,
        Dead
    }

    /// <summary>
    /// 玩家运行时数据：数值、活动状态（不含瞬时输入向量）。
    /// </summary>
    public class PlayerModel
    {
        public float AttackDamage { get; set; } = 8f;

        /// <summary>行走目标线速度（m/s，暂硬编码）。</summary>
        public float MoveSpeed { get; } = 6f;

        /// <summary>奔跑目标线速度（m/s，暂硬编码）。</summary>
        public float RunSpeed { get; } = 9f;

        public PlayerActivityState State { get; set; } = PlayerActivityState.Idle;

        public bool AllowsLocomotion =>
            State != PlayerActivityState.Dead
            && State != PlayerActivityState.Hit
            && State != PlayerActivityState.Dizzy
            && State != PlayerActivityState.Attacking
            && State != PlayerActivityState.UsingSkill
            && State != PlayerActivityState.UsingItem;
    }
}
