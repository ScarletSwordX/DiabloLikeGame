using Gameplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Bootstrap
{
    [CreateAssetMenu(fileName = "GameplaySessionConfig", menuName = "Gameplay/Session Config")]
    public class GameplaySessionConfig : ScriptableObject
    {
        public const int SkillSlotCount = 3;
        public const int ItemSlotCount = 3;

        [Header("Catalog")]
        [SerializeField] SkillCatalog _skillCatalog;
        [SerializeField] ItemCatalog _itemCatalog;

        [Header("Skill Slots (0=Skill1, 1=Skill2, 2=Skill3)")]
        [SerializeField] SkillSlotBinding _skillSlot0;
        [SerializeField] SkillSlotBinding _skillSlot1;
        [SerializeField] SkillSlotBinding _skillSlot2;

        [Header("Item Slots (0=Item1, 1=Item2, 2=Item3)")]
        [SerializeField] ItemSlotBinding _itemSlot0;
        [SerializeField] ItemSlotBinding _itemSlot1;
        [SerializeField] ItemSlotBinding _itemSlot2;

        [Header("Character / Status")]
        [SerializeField] StatusCatalog _statusCatalog;
        [SerializeField] PlayerMovementConfig _playerMovement;

        [Header("Input")]
        [SerializeField] InputActionAsset _inputActions;

        public SkillCatalog SkillCatalog => _skillCatalog;
        public ItemCatalog ItemCatalog => _itemCatalog;
        public StatusCatalog StatusCatalog => _statusCatalog;
        public PlayerMovementConfig PlayerMovement => _playerMovement;
        public InputActionAsset InputActions => _inputActions;

        public SkillSlotBinding GetSkillSlot(int index)
        {
            return index switch
            {
                0 => _skillSlot0,
                1 => _skillSlot1,
                2 => _skillSlot2,
                _ => default
            };
        }

        public ItemSlotBinding GetItemSlot(int index)
        {
            return index switch
            {
                0 => _itemSlot0,
                1 => _itemSlot1,
                2 => _itemSlot2,
                _ => default
            };
        }

        public SkillData[] ResolveSkills()
        {
            var result = new SkillData[SkillSlotCount];
            for (var i = 0; i < SkillSlotCount; i++)
                result[i] = ResolveSkill(GetSkillSlot(i).SkillId);
            return result;
        }

        public ItemDefinition[] ResolveItems()
        {
            var result = new ItemDefinition[ItemSlotCount];
            for (var i = 0; i < ItemSlotCount; i++)
                result[i] = ResolveItem(GetItemSlot(i).ItemId);
            return result;
        }

        public SkillData ResolveSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
                return null;

            if (_skillCatalog != null && _skillCatalog.TryGet(skillId, out var fromCatalog))
                return fromCatalog;

            Debug.LogWarning($"GameplaySessionConfig: 未找到技能 Id={skillId}");
            return null;
        }

        public ItemDefinition ResolveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;

            if (_itemCatalog != null && _itemCatalog.TryGet(itemId, out var fromCatalog))
                return fromCatalog;

            Debug.LogWarning($"GameplaySessionConfig: 未找到道具 Id={itemId}");
            return null;
        }
    }
}
