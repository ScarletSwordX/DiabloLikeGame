using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 技能主配置：身份、阶段时间、冷却；快捷键由 Loadout 槽位 index 决定。投递与效果由独立 SO 引用。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "Gameplay/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("身份")]
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("技能类型")]
        [Tooltip("投射物：前摇+后摇；持续型：前摇+持续+后摇；状态赋予：仅前摇")]
        public SkillKind Kind = SkillKind.Projectile;

        [Header("动画")]
        [Tooltip("施法时写入对应槽位 Skill1/2/3 状态的 Override Clip")]
        public AnimationClip CastClip;

        [Header("阶段时间（秒）")]
        [Tooltip("前摇。投射/持续/状态赋予均有效；投射物+动画 Event 时为目标 wall-clock 前段时长")]
        public float PreCastSeconds;

        [Tooltip("持续窗口。仅 SkillKind.Channeled 生效")]
        public float DurationSeconds;

        [Tooltip("后摇。投射物与持续型生效；投射物+动画 Event 时为目标 wall-clock 后段时长")]
        public float PostCastSeconds;

        [Header("施法规则")]
        public float CooldownSeconds = 1f;
        public bool IsActiveSkill = true;

        [Header("组成")]
        public SkillDeliveryData Delivery;
        public SkillEffectData Effect;

        public ActionEffectProfile EffectProfile => Effect != null ? Effect.Profile : null;
        public SkillAreaSettings Area => Delivery != null ? Delivery.EffectArea : null;
        public SkillProjectileSettings ProjectileSettings =>
            Delivery != null && Delivery.IsProjectile ? Delivery.Projectile : null;
        public bool UsesProjectile => Delivery != null && Delivery.IsProjectile;
        public bool ProjectileSpawnOnAnimationEvent =>
            Delivery != null && Delivery.SpawnOnAnimationEvent;
        public bool UsesAnimationEventProjectileLaunch =>
            Kind == SkillKind.Projectile && UsesProjectile && ProjectileSpawnOnAnimationEvent;
        public GameObject ProjectilePrefab => Delivery != null ? Delivery.ProjectilePrefab : null;

        public bool UsesPreCastPhase => true;

        public bool UsesDurationPhase => Kind == SkillKind.Channeled;

        public bool UsesPostCastPhase =>
            Kind == SkillKind.Projectile || Kind == SkillKind.Channeled;

        public float EffectivePreCastSeconds => PreCastSeconds;

        public float EffectiveDurationSeconds => UsesDurationPhase ? DurationSeconds : 0f;

        public float EffectivePostCastSeconds => UsesPostCastPhase ? PostCastSeconds : 0f;

        /// <summary>逻辑层施法占用总时长（与 SkillSystem 协程等待一致）。</summary>
        public float GetLogicCastLockSeconds()
        {
            if (UsesAnimationEventProjectileLaunch)
            {
                var timing = SkillCastClipTiming.Resolve(this);
                if (timing.Valid && timing.TotalWallClockSeconds > 0f)
                    return timing.TotalWallClockSeconds;
                return EffectivePostCastSeconds;
            }

            return EffectivePreCastSeconds + EffectiveDurationSeconds + EffectivePostCastSeconds;
        }

        public void SetFireballDefaults()
        {
            Id = "fireball";
            DisplayName = "火球";
            Kind = SkillKind.Projectile;
            CooldownSeconds = 1f;
            PreCastSeconds = 0.35f;
            DurationSeconds = 0f;
            PostCastSeconds = 0.4f;
            IsActiveSkill = true;
            Description = "发射火球投射物";

            EnsureDelivery();
            Delivery.Id = "fireball_projectile";
            Delivery.Kind = SkillDeliveryKind.Projectile;
            Delivery.SpawnOnAnimationEvent = true;
            Delivery.Projectile = new SkillProjectileSettings
            {
                FlightSpeed = 12f,
                MaxFlightDistance = 24f
            };

            EnsureEffect();
            Effect.Id = "fireball_damage";
            Effect.Profile = new ActionEffectProfile
            {
                Damage = new DamageEffectPart { Enabled = true, Amount = 10f },
                Heal = new HealEffectPart { Enabled = false },
                StatusEffects = System.Array.Empty<StatusEffectPart>()
            };
        }

        public void SetShieldDefaults()
        {
            Id = "shield";
            DisplayName = "护盾";
            Kind = SkillKind.StatusApply;
            CooldownSeconds = 8f;
            PreCastSeconds = 0.5f;
            DurationSeconds = 0f;
            PostCastSeconds = 0f;
            IsActiveSkill = true;
            Description = "自身周围护盾";

            EnsureDelivery();
            Delivery.Id = "shield_self_area";
            Delivery.Kind = SkillDeliveryKind.InstantArea;
            Delivery.SpawnOnAnimationEvent = false;
            Delivery.Area = new SkillAreaSettings
            {
                Shape = SkillAreaShape.Circle,
                LocalOffset = Vector3.zero,
                Radius = 0.5f
            };

            EnsureEffect();
            Effect.Id = "shield_buff";
            Effect.Profile = new ActionEffectProfile
            {
                Damage = new DamageEffectPart { Enabled = false },
                Heal = new HealEffectPart { Enabled = false },
                StatusEffects = new[]
                {
                    new StatusEffectPart
                    {
                        Enabled = true,
                        StatusId = "shield",
                        Magnitude = 0.5f,
                        DurationSeconds = 5f
                    }
                }
            };
        }

        void EnsureDelivery()
        {
            if (Delivery == null)
                Delivery = CreateInstance<SkillDeliveryData>();
        }

        void EnsureEffect()
        {
            if (Effect == null)
                Effect = CreateInstance<SkillEffectData>();
        }
    }
}
