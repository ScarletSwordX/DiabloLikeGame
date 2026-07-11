using System.Collections.Generic;
using System.Text;
using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.EventBus;
using Gameplay.Input;
using Gameplay.Item;
using Gameplay.Skill;
using TMPro;
using UnityEngine;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 战斗 HUD：日志/血量文本 + 装备栏图标（技能冷却、道具次数）。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameplayHud : MonoBehaviour
    {
        [SerializeField] SkillSystem _skills;
        [SerializeField] ItemSystem _items;
        [SerializeField] TextMeshProUGUI _logText;
        [SerializeField] TextMeshProUGUI _playerHpText;
        [SerializeField] TextMeshProUGUI _enemyHpText;
        [SerializeField] TextMeshProUGUI _skillStatusText;
        [SerializeField] GameplayHudHotbarView _hotbarView;

        readonly List<string> _logLines = new List<string>();
        readonly StringBuilder _skillTextBuilder = new StringBuilder(256);
        CharacterEntity _player;
        CharacterEntity _enemy;
        SkillData[] _equippedSkills;
        ItemDefinition[] _hotbarItems;

        bool _bound;
        bool _eventsSubscribed;
        string _lastSkillHudText;

        public void Bind(
            SkillSystem skills,
            ItemSystem items,
            SkillData[] equippedSkills,
            ItemDefinition[] hotbarItems,
            CharacterEntity player,
            CharacterEntity enemy)
        {
            _skills = skills;
            _items = items;
            _equippedSkills = equippedSkills;
            _hotbarItems = hotbarItems;
            _player = player;
            _enemy = enemy;
            _bound = true;

            ResolveHotbarView();

            SubscribeEvents();
            RefreshAll(force: true);
        }

        void OnEnable()
        {
            if (_bound)
                SubscribeEvents();
        }

        void OnDisable() => UnsubscribeEvents();

        void OnDestroy() => UnsubscribeEvents();

        void SubscribeEvents()
        {
            if (_eventsSubscribed)
                return;

            GameEventBus.Instance.Subscribe<CombatLogEvent>(OnLog);
            GameEventBus.Instance.Subscribe<HealthChangedEvent>(OnHealth);
            GameEventBus.Instance.Subscribe<CooldownStateChangedEvent>(OnCooldown);
            GameEventBus.Instance.Subscribe<SkillCastFailedEvent>(OnCastFailed);
            GameEventBus.Instance.Subscribe<SkillCastSucceededEvent>(OnCastSucceeded);
            GameEventBus.Instance.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
            GameEventBus.Instance.Subscribe<LoadoutChangedEvent>(OnLoadoutChanged);
            _eventsSubscribed = true;
        }

        void UnsubscribeEvents()
        {
            if (!_eventsSubscribed)
                return;

            GameEventBus.Instance.Unsubscribe<CombatLogEvent>(OnLog);
            GameEventBus.Instance.Unsubscribe<HealthChangedEvent>(OnHealth);
            GameEventBus.Instance.Unsubscribe<CooldownStateChangedEvent>(OnCooldown);
            GameEventBus.Instance.Unsubscribe<SkillCastFailedEvent>(OnCastFailed);
            GameEventBus.Instance.Unsubscribe<SkillCastSucceededEvent>(OnCastSucceeded);
            GameEventBus.Instance.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
            GameEventBus.Instance.Unsubscribe<LoadoutChangedEvent>(OnLoadoutChanged);
            _eventsSubscribed = false;
        }

        void Update()
        {
            if (!_bound)
                return;

            RefreshHp();
            RefreshSkills();
            RefreshHotbar();
        }

        void OnLog(CombatLogEvent e)
        {
            _logLines.Add(e.Message);
            if (_logLines.Count > 8)
                _logLines.RemoveAt(0);
            if (_logText != null)
                _logText.text = string.Join("\n", _logLines);
        }

        void OnHealth(HealthChangedEvent e) => RefreshHp();

        void OnCooldown(CooldownStateChangedEvent e)
        {
            if (!IsOurPlayer(e.CasterId))
                return;
            RefreshSkills(force: true);
            RefreshHotbar();
        }

        void OnCastFailed(SkillCastFailedEvent e)
        {
            if (!IsOurPlayer(e.CasterId))
                return;
            RefreshSkills(force: true);
            RefreshHotbar();
        }

        void OnCastSucceeded(SkillCastSucceededEvent e)
        {
            if (!IsOurPlayer(e.CasterId))
                return;
            RefreshSkills(force: true);
            RefreshHotbar();
        }

        void OnItemPickedUp(ItemPickedUpEvent e)
        {
            if (!IsOurPlayer(e.PickerId))
                return;
            RefreshHotbar();
        }

        void OnLoadoutChanged(LoadoutChangedEvent e) => OnLoadoutChanged(e.Skills, e.Items);

        public void OnLoadoutChanged(SkillData[] skills, ItemDefinition[] items)
        {
            _equippedSkills = skills;
            _hotbarItems = items;
            RefreshHotbar();
            RefreshSkills(force: true);
        }

        bool IsOurPlayer(int entityId) => _player != null && _player.EntityId == entityId;

        void RefreshAll(bool force)
        {
            RefreshHp();
            RefreshSkills(force);
            RefreshHotbar();
        }

        void RefreshHp()
        {
            if (_playerHpText != null)
            {
                if (_player != null && _player.TryGetHp(out var playerHp, out var playerMax))
                    _playerHpText.text = $"玩家 HP: {playerHp:0}/{playerMax:0}";
                else
                    _playerHpText.text = "玩家 HP: --/--";
            }

            if (_enemyHpText != null)
            {
                if (_enemy != null && _enemy.TryGetHp(out var enemyHp, out var enemyMax))
                    _enemyHpText.text = $"假人 HP: {enemyHp:0}/{enemyMax:0}";
                else
                    _enemyHpText.text = "假人 HP: --/--";
            }
        }

        void ResolveHotbarView()
        {
            if (_hotbarView != null && (!_hotbarView.gameObject.scene.IsValid() || !_hotbarView.isActiveAndEnabled))
                _hotbarView = null;

            if (_hotbarView == null)
                _hotbarView = GetComponentInChildren<GameplayHudHotbarView>(true);

            _hotbarView?.EnsureSlotsBound();
        }

        void RefreshHotbar()
        {
            if (_player == null)
                return;

            ResolveHotbarView();
            if (_hotbarView == null)
                return;

            var skills = _equippedSkills;
            if (skills == null || skills.Length == 0)
                skills = _skills != null ? _skills.Equipped : null;

            _hotbarView.Refresh(_skills, _items, skills, _hotbarItems, _player.EntityId);
        }

        void RefreshSkills(bool force = false)
        {
            if (_skillStatusText == null || _skills == null || _player == null)
                return;

            var equipped = _skills.Equipped;
            if (equipped == null || equipped.Length == 0)
            {
                if (force || _lastSkillHudText != string.Empty)
                {
                    _lastSkillHudText = string.Empty;
                    _skillStatusText.text = string.Empty;
                }
                return;
            }

            _skillTextBuilder.Clear();
            if (_skills.IsCasting)
                _skillTextBuilder.AppendLine("施法中…");

            var first = true;
            for (var i = 0; i < Gameplay.Bootstrap.GameplaySessionConfig.SkillSlotCount; i++)
            {
                var slot = _skills.GetSlotInstance(i);
                if (slot.IsEmpty)
                    continue;
                if (!first)
                    _skillTextBuilder.AppendLine();
                first = false;

                var cd = _skills.QueryCooldownForSlot(_player.EntityId, i);
                AppendSkillBlock(_skillTextBuilder, slot.Skill, cd, SkillHotkeyLabel(i));
            }

            var text = _skillTextBuilder.ToString();
            if (!force && text == _lastSkillHudText)
                return;

            _lastSkillHudText = text;
            _skillStatusText.text = text;
        }

        static void AppendSkillBlock(StringBuilder sb, SkillData def, CooldownQueryResult cd, string hotkeyLabel)
        {
            var description = string.IsNullOrWhiteSpace(def.Description) ? "—" : def.Description.Trim();
            sb.AppendLine($"{def.DisplayName}  [{hotkeyLabel}]  冷却 {def.CooldownSeconds:0.#}s");
            sb.AppendLine($"描述：{description}");
            sb.Append(FormatSkillStatusLine(cd));
        }

        static string SkillHotkeyLabel(int slotIndex)
        {
            return slotIndex switch
            {
                0 => GameInputActions.Skill1,
                1 => GameInputActions.Skill2,
                2 => GameInputActions.Skill3,
                _ => "Skill?"
            };
        }

        static string FormatSkillStatusLine(CooldownQueryResult cd)
        {
            if (cd.CanCast)
                return "状态：就绪";
            return $"状态：CD中  剩余 {Mathf.Max(0f, cd.RemainingSeconds):0.0}s";
        }
    }
}
