using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Input;
using Gameplay.Item;
using Gameplay.Skill;
using UnityEngine;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 装备栏图标区：3 技能槽（冷却进度）+ 3 道具槽（剩余次数）。布局由 Prefab 预置。
    /// </summary>
    public class GameplayHudHotbarView : MonoBehaviour
    {
        const int SlotCount = 3;
        const string SkillRowName = "SkillRow";
        const string ItemRowName = "ItemRow";

        static readonly string[] SkillHotkeys =
        {
            GameInputActions.Skill1,
            GameInputActions.Skill2,
            GameInputActions.Skill3
        };

        static readonly string[] ItemHotkeys =
        {
            GameInputActions.Item1,
            GameInputActions.Item2,
            GameInputActions.Item3
        };

        [SerializeField] GameplayHudSlotView[] _skillSlots = new GameplayHudSlotView[SlotCount];
        [SerializeField] GameplayHudSlotView[] _itemSlots = new GameplayHudSlotView[SlotCount];

        void Awake() => EnsureSlotsBound();

        public void EnsureSlotsBound()
        {
            if (!HasValidSlots(_skillSlots))
                _skillSlots = ResolveSlotsForRow(SkillRowName);
            if (!HasValidSlots(_itemSlots))
                _itemSlots = ResolveSlotsForRow(ItemRowName);
        }

        public void Refresh(
            SkillSystem skills,
            ItemSystem items,
            SkillData[] equippedSkills,
            ItemDefinition[] hotbarItems,
            int playerEntityId)
        {
            if (!gameObject.scene.IsValid())
                return;

            EnsureSlotsBound();

            for (var i = 0; i < SlotCount; i++)
            {
                var slot = GetSlot(_skillSlots, i);
                if (slot == null)
                    continue;

                var skill = ResolveSkillAt(equippedSkills, i);
                if (skill == null || skills == null || playerEntityId <= 0)
                    slot.SetEmpty(SkillHotkeys[i]);
                else
                {
                    var cd = skills.QueryCooldownForSlot(playerEntityId, i);
                    slot.SetSkill(skill, cd, skills.IsCasting, SkillHotkeys[i]);
                }
            }

            for (var i = 0; i < SlotCount; i++)
            {
                var slot = GetSlot(_itemSlots, i);
                if (slot == null)
                    continue;

                var item = hotbarItems != null && i < hotbarItems.Length ? hotbarItems[i] : null;
                if (item == null || items == null || playerEntityId <= 0)
                    slot.SetEmpty(ItemHotkeys[i]);
                else
                {
                    var charges = items.GetCharges(playerEntityId, item.Id);
                    slot.SetItem(item, charges, ItemHotkeys[i]);
                }
            }
        }

        static GameplayHudSlotView GetSlot(GameplayHudSlotView[] slots, int index) =>
            slots != null && index >= 0 && index < slots.Length ? slots[index] : null;

        static bool HasValidSlots(GameplayHudSlotView[] slots)
        {
            if (slots == null || slots.Length < SlotCount)
                return false;

            for (var i = 0; i < SlotCount; i++)
            {
                if (slots[i] == null)
                    return false;
            }

            return true;
        }

        GameplayHudSlotView[] ResolveSlotsForRow(string rowName)
        {
            var slots = new GameplayHudSlotView[SlotCount];
            var row = FindRow(rowName);
            if (row == null)
                return slots;

            var index = 0;
            for (var i = 0; i < row.childCount && index < SlotCount; i++)
            {
                var slot = row.GetChild(i).GetComponent<GameplayHudSlotView>();
                if (slot == null)
                    continue;

                slot.EnsureReferencesBound();
                slots[index++] = slot;
            }

            return slots;
        }

        Transform FindRow(string rowName)
        {
            foreach (Transform child in transform)
            {
                if (child.name == rowName)
                    return child;
            }

            return transform.Find(rowName);
        }

        static SkillData ResolveSkillAt(SkillData[] equipped, int slotIndex)
        {
            if (equipped == null || slotIndex < 0 || slotIndex >= equipped.Length)
                return null;
            return equipped[slotIndex];
        }
    }
}

