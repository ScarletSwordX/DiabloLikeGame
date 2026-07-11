using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 场景唯一 Canvas 上的 UI 根控制器：按 PanelId 切换 CanvasGroup 显隐。
    /// </summary>
    public class GameplayCanvasController : MonoBehaviour
    {
        [SerializeField] GameplayUiPanel[] _panels;
        [SerializeField] string _defaultPanelId = "gameplay";

        Dictionary<string, GameplayUiPanel> _panelById;

        void Awake()
        {
            if (_panels == null || _panels.Length == 0 || !HasScenePanel(_panels))
                _panels = GetComponentsInChildren<GameplayUiPanel>(true);

            _panelById = new Dictionary<string, GameplayUiPanel>();
            foreach (var panel in _panels)
            {
                if (panel == null || string.IsNullOrEmpty(panel.PanelId))
                    continue;
                _panelById[panel.PanelId] = panel;
            }

            if (!string.IsNullOrEmpty(_defaultPanelId))
                ShowOnly(_defaultPanelId);
        }

        public void ShowOnly(string panelId)
        {
            foreach (var pair in _panelById)
                pair.Value.SetVisible(pair.Key == panelId);
        }

        public void SetPanelVisible(string panelId, bool visible)
        {
            if (_panelById.TryGetValue(panelId, out var panel))
                panel.SetVisible(visible);
        }

        static bool HasScenePanel(GameplayUiPanel[] panels)
        {
            if (panels == null)
                return false;

            foreach (var panel in panels)
            {
                if (panel != null && panel.gameObject.scene.IsValid())
                    return true;
            }

            return false;
        }
    }
}
