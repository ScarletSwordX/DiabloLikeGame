namespace Gameplay.Character.Enemy.View
{
    /// <summary>
    /// 敌人动画解析：将统一行为意图映射为 Animator 状态名与过渡时长。
    /// 不同敌人类型可实现此接口，在 Inspector 挂到 <see cref="EnemyView"/> 的自定义驱动上。
    /// </summary>
    public interface IEnemyAnimationPlayback
    {
        bool TryResolve(EnemyActivityIntent intent, out EnemyAnimationCue cue);
    }

    public struct EnemyAnimationCue
    {
        public string StateName;
        public float CrossFadeDuration;
    }

    /// <summary>
    /// 默认映射：与共用 Controller 状态名一致（Hit / Dizzy / Die01 等）。
    /// </summary>
    public sealed class DefaultEnemyAnimationPlayback : IEnemyAnimationPlayback
    {
        public bool TryResolve(EnemyActivityIntent intent, out EnemyAnimationCue cue)
        {
            cue = default;
            switch (intent)
            {
                case EnemyActivityIntent.Walk:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateWalk,
                        CrossFadeDuration = 0.2f
                    };
                    return true;
                case EnemyActivityIntent.Hit:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateHit,
                        CrossFadeDuration = 0.05f
                    };
                    return true;
                case EnemyActivityIntent.Dizzy:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateDizzy,
                        CrossFadeDuration = 0.1f
                    };
                    return true;
                case EnemyActivityIntent.Die:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateDie01,
                        CrossFadeDuration = 0.05f
                    };
                    return true;
                case EnemyActivityIntent.DieStay:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateDie01Stay,
                        CrossFadeDuration = 0.05f
                    };
                    return true;
                case EnemyActivityIntent.Attack:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateAttack1,
                        CrossFadeDuration = 0.2f
                    };
                    return true;
                case EnemyActivityIntent.Idle:
                    cue = new EnemyAnimationCue
                    {
                        StateName = EnemyView.AnimatorContract.StateWalk,
                        CrossFadeDuration = 0.2f
                    };
                    return true;
                default:
                    return false;
            }
        }
    }
}
