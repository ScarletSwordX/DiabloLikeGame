namespace Gameplay.Character.View
{
    /// <summary>
    /// 角色死亡动画播放接口（敌人/玩家 View 可实现）。
    /// </summary>
    public interface ICharacterDeathAnimationView
    {
        void PlayDie();
    }
}
