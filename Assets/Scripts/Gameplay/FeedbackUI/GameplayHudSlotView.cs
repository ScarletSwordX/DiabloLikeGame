using Gameplay.Core;
using Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 单个技能/道具槽位：图标、冷却遮罩、次数与快捷键标签（由 Prefab 预置）。
    /// </summary>
    public class GameplayHudSlotView : MonoBehaviour
    {
        [SerializeField] Image _icon;
        [SerializeField] Image _cooldownFill;
        [SerializeField] TextMeshProUGUI _chargeText;
        [SerializeField] TextMeshProUGUI _hotkeyText;
        [SerializeField] CanvasGroup _canvasGroup;

        static readonly Color ReadyTint = Color.white;
        static readonly Color CooldownTint = new Color(0.55f, 0.55f, 0.55f, 1f);
        static readonly Color EmptyTint = new Color(1f, 1f, 1f, 0.25f);
        static readonly Color CooldownOverlayColor = new Color(0f, 0f, 0f, 0.55f);

        void Awake() => EnsureReferencesBound();

        public void EnsureReferencesBound()
        {
            if (_icon == null)
            {
                var icon = transform.Find("Icon");
                if (icon != null)
                    _icon = icon.GetComponent<Image>();
            }

            if (_cooldownFill == null)
            {
                var fill = transform.Find("CooldownFill");
                if (fill != null)
                    _cooldownFill = fill.GetComponent<Image>();
            }

            if (_hotkeyText == null)
            {
                var hotkey = transform.Find("Hotkey");
                if (hotkey != null)
                    _hotkeyText = hotkey.GetComponent<TextMeshProUGUI>();
            }

            if (_chargeText == null)
            {
                var charges = transform.Find("Charges");
                if (charges != null)
                    _chargeText = charges.GetComponent<TextMeshProUGUI>();
            }

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            ConfigureImageWidgets();
        }

        void ConfigureImageWidgets()
        {
            if (_icon != null)
            {
                _icon.type = Image.Type.Simple;
                _icon.preserveAspect = true;
                _icon.raycastTarget = false;
                _icon.enabled = _icon.sprite != null;
            }

            if (_cooldownFill == null)
                return;

            _cooldownFill.type = Image.Type.Filled;
            _cooldownFill.fillMethod = Image.FillMethod.Radial360;
            _cooldownFill.fillOrigin = (int)Image.Origin360.Top;
            _cooldownFill.fillClockwise = false;
            _cooldownFill.raycastTarget = false;
            _cooldownFill.enabled = _cooldownFill.sprite != null;
            SetCooldownOverlay(false, 0f);
        }

        void SetCooldownOverlay(bool active, float fill01)
        {
            if (_cooldownFill == null || _cooldownFill.sprite == null)
                return;

            _cooldownFill.enabled = true;
            _cooldownFill.fillAmount = active ? Mathf.Clamp01(fill01) : 0f;
            _cooldownFill.color = active ? CooldownOverlayColor : new Color(0f, 0f, 0f, 0f);
        }

        void ApplyIconSprite(Sprite sprite)
        {
            if (_icon == null)
                return;

            _icon.sprite = sprite;
            _icon.enabled = sprite != null;
        }

        public void SetEmpty(string hotkeyLabel = null)
        {
            EnsureReferencesBound();

            ApplyIconSprite(null);
            if (_icon != null)
                _icon.color = EmptyTint;

            SetCooldownOverlay(false, 0f);

            if (_chargeText != null)
                _chargeText.text = string.Empty;

            if (_hotkeyText != null)
                _hotkeyText.text = hotkeyLabel ?? string.Empty;

            SetReadyVisual(false);
        }

        public void SetSkill(SkillData skill, CooldownQueryResult cd, bool isCasting, string hotkeyLabel)
        {
            EnsureReferencesBound();

            if (skill == null)
            {
                SetEmpty(hotkeyLabel);
                return;
            }

            ApplyIconSprite(skill.Icon);

            if (_hotkeyText != null)
                _hotkeyText.text = hotkeyLabel ?? string.Empty;

            if (_chargeText != null)
                _chargeText.text = string.Empty;

            var onCooldown = !cd.CanCast && cd.RemainingSeconds > 0f;
            if (onCooldown && skill.CooldownSeconds > 0f)
                SetCooldownOverlay(true, cd.RemainingSeconds / skill.CooldownSeconds);
            else
                SetCooldownOverlay(false, 0f);

            if (_icon != null)
            {
                if (skill.Icon == null)
                    _icon.color = onCooldown || isCasting ? CooldownTint : EmptyTint;
                else
                    _icon.color = onCooldown || isCasting ? CooldownTint : ReadyTint;
            }

            SetReadyVisual(!onCooldown && !isCasting && skill.IsActiveSkill);
        }

        public void SetItem(ItemDefinition item, int charges, string hotkeyLabel)
        {
            EnsureReferencesBound();

            if (item == null)
            {
                SetEmpty(hotkeyLabel);
                return;
            }

            ApplyIconSprite(item.Icon);
            if (_icon != null)
                _icon.color = charges > 0 && item.Icon != null ? ReadyTint : EmptyTint;

            if (_hotkeyText != null)
                _hotkeyText.text = hotkeyLabel ?? string.Empty;

            if (_chargeText != null)
                _chargeText.text = charges > 0 ? charges.ToString() : "0";

            SetCooldownOverlay(false, 0f);
            SetReadyVisual(charges > 0);
        }

        void SetReadyVisual(bool ready)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = ready ? 1f : 0.75f;
        }
    }
}
