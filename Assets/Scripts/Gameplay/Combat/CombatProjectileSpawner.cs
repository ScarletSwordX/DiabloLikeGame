using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using UnityEngine;

namespace Gameplay.Combat
{
    public static class CombatProjectileSpawner
    {
        const string DefaultPrefabPath = "Assets/Prefab/SkillProjectile/FireballProjectile.prefab";

        public static CombatProjectile Spawn(
            SkillData skill,
            CharacterEntity caster,
            EffectSystem effectSystem,
            CastRequest castRequest)
        {
            if (skill?.Delivery == null || skill.EffectProfile == null || caster == null || effectSystem == null)
                return null;

            var delivery = skill.Delivery;
            if (!delivery.IsProjectile || delivery.SpawnOnAnimationEvent)
                return null;

            var spawnPos = caster.transform.position + Vector3.up * 1.1f + caster.transform.forward * 0.6f;
            var direction = ResolveDirection(spawnPos, castRequest, caster);
            var prefab = ResolvePrefab(delivery);
            return SpawnAt(spawnPos, Quaternion.LookRotation(direction, Vector3.up), prefab, caster, effectSystem,
                skill.EffectProfile, direction, delivery);
        }

        public static CombatProjectile SpawnAtHandle(
            Transform spawnHandle,
            GameObject prefab,
            CharacterEntity caster,
            EffectSystem effectSystem,
            ActionEffectProfile profile,
            Vector3 aimPoint,
            SkillDeliveryData delivery = null)
        {
            if (spawnHandle == null || caster == null || effectSystem == null || profile == null)
                return null;

            var spawnPos = spawnHandle.position;
            var direction = ResolveDirection(spawnPos, aimPoint, caster);
            var rotation = direction.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(direction, Vector3.up)
                : spawnHandle.rotation;
            return SpawnAt(spawnPos, rotation, prefab, caster, effectSystem, profile, direction, delivery);
        }

        public static CombatProjectile SpawnAt(
            Vector3 position,
            Quaternion rotation,
            GameObject prefab,
            CharacterEntity caster,
            EffectSystem effectSystem,
            ActionEffectProfile profile,
            Vector3 direction,
            SkillDeliveryData delivery = null)
        {
            if (caster == null || effectSystem == null || profile == null)
                return null;

            var instanceGo = InstantiateProjectilePrefab(prefab, position, rotation);
            var projectile = instanceGo.GetComponent<CombatProjectile>();
            if (projectile == null)
                projectile = instanceGo.AddComponent<CombatProjectile>();

            var speed = delivery != null
                ? delivery.ResolveFlightSpeed(projectile.ConfiguredSpeed)
                : 0f;
            var maxDistance = delivery != null ? delivery.ResolveMaxFlightDistance() : 0f;
            projectile.Launch(caster, effectSystem, profile, direction, speed, maxDistance);
            return projectile;
        }

        static GameObject InstantiateProjectilePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab != null)
                return Object.Instantiate(prefab, position, rotation);

#if UNITY_EDITOR
            var editorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabPath);
            if (editorPrefab != null)
                return Object.Instantiate(editorPrefab, position, rotation);
#endif
            var resources = Resources.Load<GameObject>("FireballProjectile");
            if (resources != null)
                return Object.Instantiate(resources, position, rotation);

            return BuildRuntimeProjectile(position, rotation);
        }

        static GameObject BuildRuntimeProjectile(Vector3 position, Quaternion rotation)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "CombatProjectile";
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.localScale = Vector3.one * 0.35f;
            go.GetComponent<SphereCollider>().isTrigger = true;
            if (go.GetComponent<CombatHitVolume>() == null)
                go.AddComponent<CombatHitVolume>();
            if (go.GetComponent<CombatProjectile>() == null)
                go.AddComponent<CombatProjectile>();
            return go;
        }

        static GameObject ResolvePrefab(SkillDeliveryData delivery)
        {
            if (delivery?.ProjectilePrefab != null)
                return delivery.ProjectilePrefab;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabPath);
#else
            return Resources.Load<GameObject>("FireballProjectile");
#endif
        }

        static Vector3 ResolveDirection(Vector3 spawnPos, CastRequest castRequest, CharacterEntity caster)
        {
            return ResolveDirection(spawnPos, castRequest.AimPoint, caster);
        }

        static Vector3 ResolveDirection(Vector3 spawnPos, Vector3 aimPoint, CharacterEntity caster)
        {
            var direction = aimPoint - spawnPos;
            if (direction.sqrMagnitude < 0.01f)
                direction = caster.transform.forward;
            return FlattenToXZ(direction, caster.transform.forward);
        }

        /// <summary>投射物仅在 XZ 平面飞行，剔除 Y 分量。</summary>
        static Vector3 FlattenToXZ(Vector3 direction, Vector3 fallbackForward)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude >= 0.0001f)
                return direction.normalized;

            fallbackForward.y = 0f;
            return fallbackForward.sqrMagnitude >= 0.0001f
                ? fallbackForward.normalized
                : Vector3.forward;
        }
    }
}
