using Gameplay.Character;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay.Skill
{
    public static class SkillAreaResolver
    {
        public static Vector3 ResolveWorldCenter(CharacterEntity caster, SkillAreaSettings area)
        {
            if (caster == null || area == null)
                return Vector3.zero;
            return caster.transform.TransformPoint(area.LocalOffset);
        }

        public static bool ContainsWorldPoint(CharacterEntity caster, SkillAreaSettings area, Vector3 worldPoint)
        {
            if (caster == null || area == null)
                return false;

            var local = caster.transform.InverseTransformPoint(worldPoint);
            var offset = area.LocalOffset;

            if (area.Shape == SkillAreaShape.Box)
            {
                var half = area.BoxSize * 0.5f;
                var delta = local - offset;
                return Mathf.Abs(delta.x) <= half.x
                    && Mathf.Abs(delta.y) <= half.y
                    && Mathf.Abs(delta.z) <= half.z;
            }

            var flatDelta = local - offset;
            flatDelta.y = 0f;
            return flatDelta.sqrMagnitude <= area.Radius * area.Radius;
        }
    }
}
