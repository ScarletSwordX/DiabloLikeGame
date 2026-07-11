using Gameplay.Bootstrap;
using Gameplay.DebugTest;
using UnityEngine;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 场景 Canvas 上的 UI 入口：MVP 就绪后显示 Gameplay 面板并完成 HUD Bind。
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class GameplayUiBootstrap : MonoBehaviour
    {
        [SerializeField] GameplayCanvasController _canvasController;
        [SerializeField] GameplayHud _hud;
        [SerializeField] GameplaySelfTestButton _selfTestButton;
        [SerializeField] string _gameplayPanelId = "gameplay";

        void Awake()
        {
            if (_canvasController == null)
                _canvasController = GetComponent<GameplayCanvasController>();

            ResolveSceneReferences();
        }

        void ResolveSceneReferences()
        {
            if (!IsSceneInstance(_hud))
                _hud = null;
            if (_hud == null)
                _hud = GetComponentInChildren<GameplayHud>(true);

            if (!IsSceneInstance(_selfTestButton))
                _selfTestButton = null;
            if (_selfTestButton == null)
                _selfTestButton = GetComponentInChildren<GameplaySelfTestButton>(true);
        }

        static bool IsSceneInstance(Component component) =>
            component != null && component.gameObject.scene.IsValid();

        void OnEnable() => GameplayMvpSession.Ready += OnMvpReady;
        void OnDisable() => GameplayMvpSession.Ready -= OnMvpReady;

        void Start()
        {
            if (GameplayMvpSession.IsReady)
                OnMvpReady();
        }

        void OnMvpReady()
        {
            ResolveSceneReferences();
            _canvasController?.ShowOnly(_gameplayPanelId);

            if (_hud == null || !IsSceneInstance(_hud) || GameplayMvpSession.SkillSystem == null)
                return;

            var player = GameplayMvpSession.Player;
            _hud.Bind(
                GameplayMvpSession.SkillSystem,
                GameplayMvpSession.ItemSystem,
                GameplayMvpSession.EquippedSkills,
                GameplayMvpSession.HotbarItems,
                player != null ? player.Entity : null,
                GameplayMvpSession.Enemy);

            if (_selfTestButton != null && GameplayMvpSession.SelfTestRunner != null)
                _selfTestButton.AssignRunner(GameplayMvpSession.SelfTestRunner);
        }
    }
}
