using UnityEngine;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "StatusDefinition", menuName = "Gameplay/Status Definition")]
    public class StatusDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public StatusKind Kind = StatusKind.DamageReduction;
        [Header("周期效果")]
        [Tooltip("DamageOverTime：每次结算间隔（秒）")]
        public float TickIntervalSeconds = 1f;
        [TextArea] public string Description;

        public void SetShieldDefaults()
        {
            Id = "shield";
            DisplayName = "护盾";
            Kind = StatusKind.DamageReduction;
            Description = "按 Magnitude 比例减伤（0.5 = 50%）";
        }

        public void SetSlowDefaults()
        {
            Id = "slow";
            DisplayName = "减速";
            Kind = StatusKind.MoveSpeedMultiplier;
            Description = "移速倍率（小于 1 为减速）";
        }

        public void SetSpeedBoostDefaults()
        {
            Id = "speed_boost";
            DisplayName = "加速";
            Kind = StatusKind.MoveSpeedMultiplier;
            Description = "移速倍率（大于 1 为加速）";
        }

        public void SetDizzyDefaults()
        {
            Id = "dizzy";
            DisplayName = "眩晕";
            Kind = StatusKind.Dizzy;
            TickIntervalSeconds = 0f;
            Description = "无法行动，循环播放 Dizzy 动画";
        }

        public void SetBurnDefaults()
        {
            Id = "burn";
            DisplayName = "灼伤";
            Kind = StatusKind.DamageOverTime;
            TickIntervalSeconds = 1f;
            Description = "周期受到 Magnitude 点伤害";
        }
    }
}
