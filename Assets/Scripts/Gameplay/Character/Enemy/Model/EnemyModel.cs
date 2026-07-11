namespace Gameplay.Character.Enemy.Model
{
    public enum EnemyActivityState
    {
        Idle,
        Moving,
        Attacking,
        Hit,
        Dizzy,
        Dead
    }

    public class EnemyModel
    {
        public float MaxHp { get; set; } = 100f;
        public float MoveSpeed { get; set; }
        public bool ShowHealthBar { get; set; } = true;

        public EnemyActivityState State { get; set; } = EnemyActivityState.Idle;

        public bool AllowsLocomotion =>
            State != EnemyActivityState.Dead
            && State != EnemyActivityState.Hit
            && State != EnemyActivityState.Dizzy
            && State != EnemyActivityState.Attacking;
    }
}
