using System;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 技能作用区域：相对施法者局部坐标（+Z 为面向前方）。
    /// </summary>
    [Serializable]
    public class SkillAreaSettings
    {
        public SkillAreaShape Shape = SkillAreaShape.Circle;

        [Tooltip("相对施法者 Transform 的局部偏移（forward = +Z）")]
        public Vector3 LocalOffset = new Vector3(0f, 0f, 2f);

        [Tooltip("圆形半径（Circle）")]
        public float Radius = 2f;

        [Tooltip("盒体尺寸（Box，局部 XYZ 全尺寸）")]
        public Vector3 BoxSize = new Vector3(2f, 1f, 2f);

        public float ResolveEffectRadius()
        {
            if (Shape == SkillAreaShape.Box)
                return Mathf.Max(BoxSize.x, BoxSize.z) * 0.5f;
            return Radius;
        }
    }
}
