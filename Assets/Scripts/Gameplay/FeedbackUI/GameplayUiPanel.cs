using UnityEngine;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 场景 Canvas 下的单个 UI 面板（通过 CanvasGroup 控制显隐与交互）。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameplayUiPanel : MonoBehaviour
    {
        [SerializeField] string _panelId = "gameplay";

        CanvasGroup _group;

        public string PanelId => _panelId;

        void Awake() => _group = GetComponent<CanvasGroup>();

        public void SetVisible(bool visible)
        {
            if (_group == null)
                _group = GetComponent<CanvasGroup>();

            _group.alpha = visible ? 1f : 0f;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
        }
    }
}
