#if UNITY_EDITOR
using Gameplay.Bootstrap;
using Gameplay.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Editor
{
    public static class GameplayP0Setup
    {
        [MenuItem("Tools/Gameplay/Create P0 Default Assets", false, 0)]
        [MenuItem("Gameplay/Create P0 Default Assets", false, 0)]
        public static void CreateDefaultAssets() => GameplayP0SetupCore.CreateDefaultAssets();

        [MenuItem("Tools/Gameplay/Build Prefab Assets", false, 1)]
        [MenuItem("Gameplay/Build Prefab Assets", false, 1)]
        public static void BuildPrefabAssets() => GameplaySceneSetup.BuildPrefabAssets();

        [MenuItem("Tools/Gameplay/Ensure Scene Bootstrap", false, 2)]
        [MenuItem("Gameplay/Ensure Scene Bootstrap", false, 2)]
        public static void EnsureSceneBootstrap() => GameplaySceneSetup.EnsureSceneBootstrap();
    }

    static class GameplayP0SetupCore
    {
        const string SkillsPath = "Assets/Data/Skills";
        const string SkillDeliveryPath = "Assets/Data/Skills/Delivery";
        const string SkillEffectsPath = "Assets/Data/Skills/Effects";
        const string ItemsPath = "Assets/Data/Items";
        const string StatusesPath = "Assets/Data/Statuses";
        const string DataPath = "Assets/Data";
        const string InputPath = "Assets/Input/GameInput.inputactions";
        const string SessionConfigPath = "Assets/Data/GameplaySessionConfig.asset";

        public static void CreateDefaultAssets()
        {
            EnsureFolder(DataPath);
            EnsureFolder(SkillsPath);
            EnsureFolder(SkillDeliveryPath);
            EnsureFolder(SkillEffectsPath);
            EnsureFolder(ItemsPath);
            EnsureFolder(StatusesPath);

            var catalog = CreateStatusCatalog();
            var fireball = CreateSkill("Fireball", s => s.SetFireballDefaults());
            var shield = CreateSkill("Shield", s => s.SetShieldDefaults());
            var potion = CreateItem("HealPotion", i => i.SetHealPotionDefaults());
            var movement = GetOrCreateAsset<PlayerMovementConfig>($"{DataPath}/PlayerMovement.asset");
            if (movement.MoveSpeed < 1f) movement.MoveSpeed = 6f;
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);

            var skillCatalog = CreateSkillCatalog(fireball, shield);
            var itemCatalog = CreateItemCatalog(potion);
            CreateSessionConfig(skillCatalog, itemCatalog, catalog, movement, input);

            AssetDatabase.SaveAssets();
            Debug.Log("Gameplay P0 assets created under Assets/Data/ (含 Catalog 与 GameplaySessionConfig.asset)");
        }

        static SkillCatalog CreateSkillCatalog(params SkillData[] skills)
        {
            var path = $"{SkillsPath}/SkillCatalog.asset";
            var catalog = GetOrCreateAsset<SkillCatalog>(path);
            catalog.Entries = skills;
            catalog.RebuildIndex();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        static ItemCatalog CreateItemCatalog(params ItemDefinition[] items)
        {
            var path = $"{ItemsPath}/ItemCatalog.asset";
            var catalog = GetOrCreateAsset<ItemCatalog>(path);
            catalog.Entries = items;
            catalog.RebuildIndex();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        static void CreateSessionConfig(
            SkillCatalog skillCatalog,
            ItemCatalog itemCatalog,
            StatusCatalog catalog,
            PlayerMovementConfig movement,
            InputActionAsset input)
        {
            var session = GetOrCreateAsset<GameplaySessionConfig>(SessionConfigPath);
            var so = new SerializedObject(session);
            so.FindProperty("_skillCatalog").objectReferenceValue = skillCatalog;
            so.FindProperty("_itemCatalog").objectReferenceValue = itemCatalog;
            so.FindProperty("_skillSlot0").FindPropertyRelative("SkillId").stringValue = "fireball";
            so.FindProperty("_skillSlot1").FindPropertyRelative("SkillId").stringValue = "shield";
            so.FindProperty("_skillSlot2").FindPropertyRelative("SkillId").stringValue = string.Empty;
            so.FindProperty("_itemSlot0").FindPropertyRelative("ItemId").stringValue = "heal_potion";
            so.FindProperty("_itemSlot1").FindPropertyRelative("ItemId").stringValue = string.Empty;
            so.FindProperty("_itemSlot2").FindPropertyRelative("ItemId").stringValue = string.Empty;
            so.FindProperty("_statusCatalog").objectReferenceValue = catalog;
            so.FindProperty("_playerMovement").objectReferenceValue = movement;
            so.FindProperty("_inputActions").objectReferenceValue = input;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(session);
        }

        static SkillData CreateSkill(string name, System.Action<SkillData> init)
        {
            var delivery = GetOrCreateAsset<SkillDeliveryData>($"{SkillDeliveryPath}/{name}Delivery.asset");
            var effect = GetOrCreateAsset<SkillEffectData>($"{SkillEffectsPath}/{name}Effect.asset");
            var asset = GetOrCreateAsset<SkillData>($"{SkillsPath}/{name}.asset");
            asset.Delivery = delivery;
            asset.Effect = effect;
            init(asset);
            EditorUtility.SetDirty(delivery);
            EditorUtility.SetDirty(effect);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static StatusCatalog CreateStatusCatalog()
        {
            var shield = CreateStatus("Shield", s => s.SetShieldDefaults());
            var slow = CreateStatus("Slow", s => s.SetSlowDefaults());
            var speed = CreateStatus("SpeedBoost", s => s.SetSpeedBoostDefaults());
            var dizzy = CreateStatus("Dizzy", s => s.SetDizzyDefaults());
            var burn = CreateStatus("Burn", s => s.SetBurnDefaults());
            var path = $"{StatusesPath}/StatusCatalog.asset";
            var catalog = GetOrCreateAsset<StatusCatalog>(path);
            catalog.Entries = new[] { shield, slow, speed, dizzy, burn };
            catalog.RebuildIndex();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        static StatusDefinition CreateStatus(string name, System.Action<StatusDefinition> init)
        {
            var path = $"{StatusesPath}/{name}.asset";
            var asset = GetOrCreateAsset<StatusDefinition>(path);
            init(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static ItemDefinition CreateItem(string name, System.Action<ItemDefinition> init)
        {
            var path = $"{ItemsPath}/{name}.asset";
            var asset = GetOrCreateAsset<ItemDefinition>(path);
            init(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
