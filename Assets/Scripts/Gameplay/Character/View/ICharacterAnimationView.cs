namespace Gameplay.Character.View
{
    /// <summary>
    /// 角色动画播放共用接口：由具体 View（Player/Enemy）实现。
    /// </summary>
    public interface ICharacterAnimationView
    {
        void PlayAnimation(string stateName, float crossFadeDuration = 0.2f, float time = 0f);
    }
}
