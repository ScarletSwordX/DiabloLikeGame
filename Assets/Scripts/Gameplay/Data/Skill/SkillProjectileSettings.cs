using System;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 投射物投递参数：仅 SkillDeliveryKind.Projectile 使用。
    /// </summary>
    [Serializable]
    public class SkillProjectileSettings
    {
        [Tooltip("飞行速度（米/秒）；≤0 时使用 Prefab 上 CombatProjectile 默认值")]
        public float FlightSpeed = 12f;

        [Tooltip("最大飞行距离（米）；超出后销毁投射物")]
        public float MaxFlightDistance = 24f;
    }

}
