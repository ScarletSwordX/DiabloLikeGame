using System;
using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 单个道具槽绑定：空 ItemId 表示空槽。槽位 index 对应 Item1/2/3。
    /// </summary>
    [Serializable]
    public struct ItemSlotBinding
    {
        public string ItemId;

        public bool IsEmpty => string.IsNullOrEmpty(ItemId);
    }
}
