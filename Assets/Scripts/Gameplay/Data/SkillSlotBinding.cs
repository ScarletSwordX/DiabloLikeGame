using System;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 单个技能槽绑定：空 SkillId 表示空槽。槽位 index 对应 Skill1/2/3。
    /// </summary>
    [Serializable]
    public struct SkillSlotBinding
    {
        public string SkillId;

        public bool IsEmpty => string.IsNullOrEmpty(SkillId);
    }
}
