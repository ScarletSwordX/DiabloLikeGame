using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// DPS 面板：显示玩家 DPS，Reset 按钮重置统计窗口。
    /// </summary>
    public class GameplayDpsBoardView : MonoBehaviour
    {
        [SerializeField] TMP_Text _dpsLabel;
        [SerializeField] Button _resetButton;
        [SerializeField] string _labelFormat = "DPS: {0:F1}";

        void Awake()
        {
            if (_dpsLabel == null)
                _dpsLabel = GetComponentInChildren<TMP_Text>(true);

            if (_resetButton == null)
            {
                var buttons = GetComponentsInChildren<Button>(true);
                foreach (var button in buttons)
                {
                    if (button.name.Contains("Reset"))
                    {
                        _resetButton = button;
                        break;
                    }
                }
            }

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);
        }

        void OnDestroy()
        {
            if (_resetButton != null)
                _resetButton.onClick.RemoveListener(OnResetClicked);
        }

        void Update()
        {
            if (_dpsLabel == null)
                return;

            var tracker = GameplayDpsTracker.Instance;
            _dpsLabel.text = tracker != null
                ? string.Format(_labelFormat, tracker.Dps)
                : string.Format(_labelFormat, 0f);
        }

        void OnResetClicked()
        {
            GameplayDpsTracker.Instance?.ResetWindow();
        }
    }
}
