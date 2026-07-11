using UnityEngine;

namespace Gameplay.Data
{
    public enum ItemTriggerType
    {
        OnPickup,
        OnUse
    }

    /// <summary>
    /// 道具配置：瞬时一次性效果，无施法前摇/持续/后摇阶段。
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Gameplay/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("身份")]
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("效果（瞬时生效）")]
        public ActionEffectProfile EffectProfile = new ActionEffectProfile();

        [Header("UI")]
        public Sprite Icon;

        [Header("道具专属")]
        public ItemTriggerType TriggerType = ItemTriggerType.OnPickup;
        [Tooltip("OnUse：每次拾取增加的次数；OnPickup 忽略")]
        public int Charges = 1;

        public bool AppliesEffectOnPickup => TriggerType == ItemTriggerType.OnPickup;
        public bool AddsChargesOnPickup => TriggerType == ItemTriggerType.OnUse;
        public bool RequiresHotbarUse => TriggerType == ItemTriggerType.OnUse;

        public void SetHealPotionDefaults()
        {
            Id = "heal_potion";
            DisplayName = "治疗药水";
            TriggerType = ItemTriggerType.OnPickup;
            Charges = 1;
            Description = "恢复生命";
            EffectProfile = new ActionEffectProfile
            {
                Damage = new DamageEffectPart { Enabled = false },
                Heal = new HealEffectPart { Enabled = true, Amount = 30f },
                StatusEffects = System.Array.Empty<StatusEffectPart>()
            };
        }
    }
}
