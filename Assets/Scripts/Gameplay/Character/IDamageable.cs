using Gameplay.Core;

namespace Gameplay.Character
{
    /// <summary>
    /// 可受伤实体：扣血入口，由 <see cref="Presenter.CharacterPresenter"/> 实现。
    /// </summary>
    public interface IDamageable
    {
        int EntityId { get; }
        bool IsAlive { get; }
        DamageResult ProcessDamage(DamageRequest request);
    }
}
