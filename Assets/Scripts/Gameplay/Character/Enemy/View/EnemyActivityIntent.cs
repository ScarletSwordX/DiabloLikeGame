namespace Gameplay.Character.Enemy.View
{
    /// <summary>
    /// 敌人行为意图：View 层按意图请求动画，具体状态名由 <see cref="IEnemyAnimationPlayback"/> 解析。
    /// </summary>
    public enum EnemyActivityIntent
    {
        Idle,
        Walk,
        Hit,
        Dizzy,
        Die,
        DieStay,
        Attack
    }
}
