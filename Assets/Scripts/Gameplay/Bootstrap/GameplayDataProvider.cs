using Gameplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Bootstrap
{
    /// <summary>
    /// 接受 <see cref="GameplaySessionConfig"/>，解析并对外提供本局数据（含 Editor 缺省加载）。
    /// </summary>
    [DefaultExecutionOrder(-1100)]
    public class GameplayDataProvider : MonoBehaviour
    {
        const string ConfigAssetPath = "Assets/Data/GameplaySessionConfig.asset";

        [SerializeField] GameplaySessionConfig _config;

        bool _coreResolved;

        public GameplaySessionConfig Config => _config;
        public StatusCatalog StatusCatalog { get; private set; }
        public SkillData[] EquippedSkills { get; private set; }
        public ItemDefinition[] HotbarItems { get; private set; }
        public ItemDefinition[] ItemDefinitions { get; private set; }
        public PlayerMovementConfig PlayerMovement { get; private set; }
        public InputActionAsset InputActions { get; private set; }

        void Awake() => Resolve();

        public void Resolve()
        {
            EnsureConfigLoaded();
            ResolveInputActions();
            ResolveStatusCatalog();
            ResolveItemDefinitions();
            ResolveMovement();
            ResolveLoadout();
            _coreResolved = true;
        }

        public void ResolveLoadout()
        {
            EnsureConfigLoaded();

            if (_config != null)
            {
                EquippedSkills = _config.ResolveSkills();
                HotbarItems = _config.ResolveItems();
                return;
            }

            EquippedSkills = CreateFallbackSkills();
            HotbarItems = CreateFallbackHotbar();
        }

        void EnsureConfigLoaded()
        {
            if (_config == null)
                _config = LoadConfigAsset();
        }

        void ResolveInputActions()
        {
            InputActions = _config != null ? _config.InputActions : null;
            if (InputActions != null) return;
#if UNITY_EDITOR
            InputActions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Input/GameInput.inputactions");
#endif
            if (InputActions == null)
                InputActions = Resources.Load<InputActionAsset>("GameInput");
        }

        void ResolveStatusCatalog()
        {
            if (_coreResolved && StatusCatalog != null)
                return;

            StatusCatalog = _config != null ? _config.StatusCatalog : null;
            if (StatusCatalog == null)
                StatusCatalog = LoadStatusCatalog();
            if (StatusCatalog == null)
                StatusCatalog = CreateRuntimeStatusCatalog();
        }

        void ResolveItemDefinitions()
        {
            if (_config?.ItemCatalog != null && _config.ItemCatalog.Entries != null &&
                _config.ItemCatalog.Entries.Length > 0)
            {
                ItemDefinitions = _config.ItemCatalog.Entries;
                return;
            }

            var potion = LoadItem("HealPotion");
            if (potion == null)
            {
                potion = ScriptableObject.CreateInstance<ItemDefinition>();
                potion.SetHealPotionDefaults();
            }

            ItemDefinitions = new[] { potion };
        }

        void ResolveMovement()
        {
            if (_coreResolved && PlayerMovement != null)
                return;

            PlayerMovement = _config?.PlayerMovement ?? LoadMovementConfig();
            if (PlayerMovement == null)
            {
                PlayerMovement = ScriptableObject.CreateInstance<PlayerMovementConfig>();
                PlayerMovement.MoveSpeed = 6f;
            }
        }

        static SkillData[] CreateFallbackSkills()
        {
            var fireball = ScriptableObject.CreateInstance<SkillData>();
            fireball.SetFireballDefaults();
            var shield = ScriptableObject.CreateInstance<SkillData>();
            shield.SetShieldDefaults();
            return new[] { fireball, shield, null };
        }

        static ItemDefinition[] CreateFallbackHotbar()
        {
            var potion = ScriptableObject.CreateInstance<ItemDefinition>();
            potion.SetHealPotionDefaults();
            return new[] { potion, null, null };
        }

        static StatusCatalog CreateRuntimeStatusCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<StatusCatalog>();
            var shield = ScriptableObject.CreateInstance<StatusDefinition>();
            shield.SetShieldDefaults();
            var slow = ScriptableObject.CreateInstance<StatusDefinition>();
            slow.SetSlowDefaults();
            var speed = ScriptableObject.CreateInstance<StatusDefinition>();
            speed.SetSpeedBoostDefaults();
            var dizzy = ScriptableObject.CreateInstance<StatusDefinition>();
            dizzy.SetDizzyDefaults();
            var burn = ScriptableObject.CreateInstance<StatusDefinition>();
            burn.SetBurnDefaults();
            catalog.Entries = new[] { shield, slow, speed, dizzy, burn };
            catalog.RebuildIndex();
            return catalog;
        }

#if UNITY_EDITOR
        static GameplaySessionConfig LoadConfigAsset() =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<GameplaySessionConfig>(ConfigAssetPath);

        static ItemDefinition LoadItem(string name) =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDefinition>($"Assets/Data/Items/{name}.asset");

        static PlayerMovementConfig LoadMovementConfig() =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>("Assets/Data/PlayerMovement.asset");

        static StatusCatalog LoadStatusCatalog() =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<StatusCatalog>("Assets/Data/Statuses/StatusCatalog.asset");
#else
        static GameplaySessionConfig LoadConfigAsset() => null;
        static ItemDefinition LoadItem(string name) => null;
        static PlayerMovementConfig LoadMovementConfig() => null;
        static StatusCatalog LoadStatusCatalog() => null;
#endif
    }
}
