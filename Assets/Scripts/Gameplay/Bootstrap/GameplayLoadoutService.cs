using Gameplay.Bootstrap;
using Gameplay.Character;
using Gameplay.Character.Player.Presenter;
using Gameplay.Data;
using Gameplay.EventBus;
using Gameplay.FeedbackUI;
using Gameplay.Skill;
using UnityEngine;

namespace Gameplay.Bootstrap
{
    /// <summary>
    /// 运行时 Loadout 真相源：从 Config 解析并推送到 SkillSystem / Player / Session / HUD。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameplayLoadoutService : MonoBehaviour
    {
        [SerializeField] GameplayDataProvider _dataProvider;
        [SerializeField] SkillSystem _skillSystem;

        public SkillData[] CurrentSkills { get; private set; }
        public ItemDefinition[] CurrentItems { get; private set; }

        void Awake()
        {
            if (_dataProvider == null)
                _dataProvider = GetComponent<GameplayDataProvider>();
            if (_skillSystem == null)
                _skillSystem = GetComponent<SkillSystem>();
        }

        public bool ApplyFromConfig()
        {
            if (_dataProvider == null)
            {
                Debug.LogWarning("GameplayLoadoutService: 缺少 GameplayDataProvider。");
                return false;
            }

            _dataProvider.ResolveLoadout();
            return ApplyLoadout(_dataProvider.EquippedSkills, _dataProvider.HotbarItems);
        }

        public bool ApplyLoadout(SkillData[] skills, ItemDefinition[] items)
        {
            if (_skillSystem != null && _skillSystem.IsCasting)
            {
                Debug.LogWarning("GameplayLoadoutService: 施法中，拒绝 Reload。");
                return false;
            }

            CurrentSkills = NormalizeSkillSlots(skills);
            CurrentItems = NormalizeItemSlots(items);

            if (_skillSystem != null)
                _skillSystem.SetEquipped(CurrentSkills);

            var player = GameplayMvpSession.Player;
            if (player != null)
            {
                var presenter = player.GetComponent<PlayerPresenter>();
                presenter?.ApplyLoadout(CurrentSkills, CurrentItems);
            }

            if (GameplayMvpSession.IsReady)
                GameplayMvpSession.UpdateLoadout(CurrentSkills, CurrentItems);

            RefreshHud();

            GameEventBus.Instance.Publish(new LoadoutChangedEvent
            {
                Skills = CurrentSkills,
                Items = CurrentItems
            });

            return true;
        }

        public bool Reload() => ApplyFromConfig();

        void RefreshHud()
        {
            var hud = Object.FindObjectOfType<GameplayHud>();
            hud?.OnLoadoutChanged(CurrentSkills, CurrentItems);
        }

        static SkillData[] NormalizeSkillSlots(SkillData[] skills)
        {
            var result = new SkillData[GameplaySessionConfig.SkillSlotCount];
            if (skills == null) return result;
            for (var i = 0; i < result.Length && i < skills.Length; i++)
                result[i] = skills[i];
            return result;
        }

        static ItemDefinition[] NormalizeItemSlots(ItemDefinition[] items)
        {
            var result = new ItemDefinition[GameplaySessionConfig.ItemSlotCount];
            if (items == null) return result;
            for (var i = 0; i < result.Length && i < items.Length; i++)
                result[i] = items[i];
            return result;
        }
    }
}
