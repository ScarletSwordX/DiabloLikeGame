using Gameplay.Character;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 技能投递方式：InstantArea 用作用区域；Projectile 用飞行速度与最大飞行距离。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDelivery", menuName = "Gameplay/Skill Delivery")]
    public class SkillDeliveryData : ScriptableObject
    {
        public string Id;
        [TextArea] public string Description;

        public SkillDeliveryKind Kind = SkillDeliveryKind.InstantArea;

        [Header("作用区域（InstantArea）")]
        public SkillAreaSettings Area = new SkillAreaSettings();

        [Header("投射物（Projectile）")]
        [Tooltip("投射物 Prefab；未配置速度/距离 ≤0 时回退 Prefab 上 CombatProjectile 默认值")]
        public GameObject ProjectilePrefab;
        [Tooltip("true：由技能动画 Event 在 PlayerView 上发射")]
        public bool SpawnOnAnimationEvent;
        public SkillProjectileSettings Projectile = new SkillProjectileSettings();

        public bool IsProjectile => Kind == SkillDeliveryKind.Projectile;

        public SkillAreaSettings EffectArea =>
            Kind == SkillDeliveryKind.InstantArea ? Area : null;

        public float ResolveFlightSpeed(float prefabFallback)
        {
            if (!IsProjectile)
                return prefabFallback;

            return Projectile.FlightSpeed > 0f ? Projectile.FlightSpeed : prefabFallback;
        }

        public float ResolveMaxFlightDistance()
        {
            if (!IsProjectile)
                return 0f;

            return Projectile.MaxFlightDistance > 0f ? Projectile.MaxFlightDistance : 0f;
        }

        public Vector3 ResolveProjectileAimPoint(CharacterEntity caster)
        {
            if (caster == null)
                return Vector3.zero;

            var forward = caster.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            var distance = ResolveMaxFlightDistance();
            if (distance <= 0f)
                distance = 24f;

            return caster.transform.position + forward.normalized * distance;
        }
    }
}
