using Gameplay.Bootstrap;
using Gameplay.Character;
using Gameplay.Character.Enemy.Presenter;
using Gameplay.DebugTest;
using Gameplay.Effect;
using Gameplay.Input;
using Gameplay.Item;
using Gameplay.Skill;
using Gameplay.WorldInteraction;
using UnityEngine;

namespace Gameplay.Bootstrap
{
    /// <summary>
    /// MVP 运行时组合：同物体挂载 Effect / Item / Skill，在 Start 阶段（场景 Awake 完成后）接线 Player 与 Session。
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] GameplayDataProvider _dataProvider;
        [SerializeField] GameplayLoadoutService _loadoutService;

        [Header("Systems（同 GameObject 或子物体）")]
        [SerializeField] EffectSystem _effectSystem;
        [SerializeField] ItemSystem _itemSystem;
        [SerializeField] SkillSystem _skillSystem;
        [SerializeField] GameplaySelfTestRunner _selfTestRunner;

        [Header("Scene refs")]
        [SerializeField] PlayerController _player;
        [SerializeField] CharacterEntity _enemy;
        [SerializeField] PickupItem _pickup;

        bool _wired;

        void Awake() => CacheReferences();

        void Start()
        {
            if (_wired) return;
            WireMvpSystems();
        }

        void OnDestroy() => GameplayMvpSession.Clear();

        void CacheReferences()
        {
            if (_dataProvider == null)
                _dataProvider = GetComponent<GameplayDataProvider>();
            if (_loadoutService == null)
                _loadoutService = GetComponent<GameplayLoadoutService>();
            if (_effectSystem == null)
                _effectSystem = GetComponent<EffectSystem>();
            if (_itemSystem == null)
                _itemSystem = GetComponent<ItemSystem>();
            if (_skillSystem == null)
                _skillSystem = GetComponent<SkillSystem>();
            if (_selfTestRunner == null)
                _selfTestRunner = GetComponentInChildren<GameplaySelfTestRunner>(true);
        }

        void WireMvpSystems()
        {
            CacheReferences();
            ResolveSceneReferences();

            if (_dataProvider == null)
            {
                Debug.LogError("GameBootstrap: 需要 GameplayDataProvider（同物体或手动指定）。");
                return;
            }

            if (_effectSystem == null || _itemSystem == null || _skillSystem == null)
            {
                Debug.LogError("GameBootstrap: 同物体缺少 EffectSystem / ItemSystem / SkillSystem。");
                return;
            }

            _dataProvider.Resolve();

            var data = _dataProvider;
            _effectSystem.Initialize(data.StatusCatalog);
            _itemSystem.Initialize(_effectSystem, data.ItemDefinitions);
            _skillSystem.Initialize(_effectSystem, data.EquippedSkills);

            _pickup?.AssignItemSystem(_itemSystem);

            if (_player != null)
            {
                _player.Initialize(
                    data.PlayerMovement,
                    _skillSystem,
                    data.EquippedSkills,
                    _itemSystem,
                    data.HotbarItems,
                    data.InputActions);
                GameplayInputLog.Bootstrap("Player wired skill+item systems");
            }
            else
                GameplayInputLog.Bootstrap("Player reference missing — skill/item input will reject");

            GameplayMvpSession.Publish(
                _effectSystem,
                _itemSystem,
                _skillSystem,
                data.EquippedSkills,
                data.HotbarItems,
                _player,
                _enemy,
                _selfTestRunner);

            if (_loadoutService != null)
                _loadoutService.ApplyFromConfig();
            else
                Debug.LogWarning("GameBootstrap: 缺少 GameplayLoadoutService，Loadout 热重载不可用。");

            _wired = true;
            GameplayInputLog.Bootstrap("MVP session published");
        }

        void ResolveSceneReferences()
        {
            if (_player != null && !IsSceneInstance(_player))
                _player = null;

            if (_player == null)
                _player = FindObjectOfType<PlayerController>();

            if (_enemy != null && !IsSceneInstance(_enemy))
                _enemy = null;

            if (_enemy == null)
                _enemy = FindSceneEnemy();

            if (_enemy == null)
                Debug.LogWarning("GameBootstrap: 未找到场景中的敌人 CharacterEntity，HUD 敌人血量将不可用。");
        }

        static bool IsSceneInstance(Component component) =>
            component != null && component.gameObject.scene.IsValid();

        static CharacterEntity FindSceneEnemy()
        {
            var enemyPresenter = FindObjectOfType<EnemyPresenter>();
            if (enemyPresenter != null)
                return enemyPresenter.GetComponent<CharacterEntity>();

            var marker = FindObjectOfType<TrainingDummyMarker>();
            if (marker != null)
                return marker.GetComponent<CharacterEntity>();

            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (entity != null && entity.IsSceneInstance && entity.Faction != Core.Faction.Player)
                    return entity;
            }

            return null;
        }
    }
}
