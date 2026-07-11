namespace Gameplay.Core
{
    public enum Faction
    {
        Player,
        Enemy,
        Neutral
    }

    public enum EffectType
    {
        Damage,
        Heal,
        ApplyStatus,
        ApplySlow,
        ApplyShield,
        ApplySpeedBuff
    }

    public enum CastResultStatus
    {
        Success,
        InProgress,
        OnCooldown,
        NoTarget,
        InvalidState
    }

    public enum PickupResultStatus
    {
        Success,
        AlreadyConsumed,
        Invalid
    }

    public enum UseResultStatus
    {
        Success,
        NoCharges,
        Invalid
    }

    public enum EffectResultStatus
    {
        Applied,
        NoValidTarget,
        Resisted
    }

    public enum CooldownPhase
    {
        Started,
        Tick,
        Ended
    }
}
