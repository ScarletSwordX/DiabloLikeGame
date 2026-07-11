using Gameplay.Bootstrap;
using Gameplay.Combat;
using Gameplay.Character;
using Gameplay.Character.Player;
using Gameplay.Character.Enemy.Presenter;
using Gameplay.Character.Enemy.View;
using Gameplay.Character.Player.Presenter;
using Gameplay.Character.Player.View;
using Gameplay.Character.Presenter;
using Gameplay.Character.View;
using Gameplay.DebugTest;
using Gameplay.Effect;
using Gameplay.Feedback;
using Gameplay.FeedbackUI;
using Gameplay.Input;
using Gameplay.Item;
using Gameplay.Item.View;
using Gameplay.Skill;
using Gameplay.WorldInteraction;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Editor
{
    /// <summary>
    /// 生成/更新 Prefab 资产；玩法系统在 Main 场景挂于 GameBootstrap 同物体（见 EnsureSceneBootstrap）。
    /// </summary>
    public static class GameplaySceneSetup
    {
        const string PrefabFolder = "Assets/Prefab";
        const string PlayerPrefabPath = PrefabFolder + "/Player.prefab";
        const string EnemyPrefabPath = PrefabFolder + "/Enemy.prefab";
        const string SystemsPrefabPath = PrefabFolder + "/GameplaySystems.prefab";
        const string HudPrefabPath = PrefabFolder + "/GameplayHud.prefab";
        const string HudSlotPrefabPath = PrefabFolder + "/GameplayHudSlot.prefab";
        const string HudHotbarPrefabPath = PrefabFolder + "/GameplayHudHotbarPanel.prefab";
        const string PickupPrefabPath = PrefabFolder + "/HealPotionPickup.prefab";
        const string CameraPrefabPath = PrefabFolder + "/GameplayCamera.prefab";
        const string CombatProjectilePrefabPath = PrefabFolder + "/CombatProjectile.prefab";
        const string DamageNumberPrefabPath = PrefabFolder + "/Red Glow.prefab";
        const string UiFontPath = "Assets/TextFont/SourceHanSans SDF.asset";

        public static void BuildPrefabAssets()
        {
            EnsureFolder(PrefabFolder);
            EnsurePlayerPrefab();
            EnsureEnemyPrefab();
            SaveOrUpdatePrefab(BuildSystemsRoot(), SystemsPrefabPath);
            SaveOrUpdatePrefab(BuildHudSlotRoot(), HudSlotPrefabPath);
            SaveOrUpdatePrefab(BuildHudHotbarPanelRoot(), HudHotbarPrefabPath);
            SaveOrUpdatePrefab(BuildHudRoot(), HudPrefabPath);
            SaveOrUpdatePrefab(BuildPickupRoot(), PickupPrefabPath);
            SaveOrUpdatePrefab(BuildCameraRoot(), CameraPrefabPath);
            SaveOrUpdatePrefab(BuildCombatProjectileRoot(), CombatProjectilePrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prefab 已写入 Assets/Prefab/。Main 场景 GameBootstrap 请用 Tools → Gameplay → Ensure Scene Bootstrap 挂载系统组件。");
        }

        /// <summary>
        /// 在当前打开场景为 GameBootstrap 补齐 Effect/Item/Skill/Feedback 与 DebugTest 子物体。
        /// </summary>
        public static void EnsureSceneBootstrap()
        {
            var bootstrap = Object.FindObjectOfType<GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("场景中未找到 GameBootstrap。");
                return;
            }

            var go = bootstrap.gameObject;
            EnsureComponent<EffectSystem>(go);
            EnsureComponent<ItemSystem>(go);
            EnsureComponent<SkillSystem>(go);
            EnsureComponent<GameplayFeedbackProvider>(go);
            EnsureComponent<GameplayDpsTracker>(go);
            WireDamageNumberPrefab(go.GetComponent<GameplayFeedbackProvider>());
            EnsureComponent<GameplayDataProvider>(go);
            EnsureComponent<GameplayLoadoutService>(go);

            var debug = go.transform.Find("DebugTest");
            if (debug == null)
            {
                var debugGo = new GameObject("DebugTest");
                debugGo.transform.SetParent(go.transform, false);
                debugGo.AddComponent<GameplaySelfTestRunner>();
            }
            else if (debug.GetComponent<GameplaySelfTestRunner>() == null)
            {
                debug.gameObject.AddComponent<GameplaySelfTestRunner>();
            }

            EditorUtility.SetDirty(go);
            Debug.Log("GameBootstrap 系统组件已就绪。");
        }

        static void EnsurePlayerPrefab()
        {
            if (!System.IO.File.Exists(PlayerPrefabPath))
            {
                Debug.LogWarning($"未找到 {PlayerPrefabPath}，请先在场景中配置 Player 模型并另存为 Prefab。");
                return;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(PlayerPrefabPath);
            var root = scope.prefabContentsRoot;
            EnsureComponent<CharacterView>(root);
            EnsureComponent<CharacterPresenter>(root);
            EnsureComponent<CharacterEntity>(root);
            EnsureComponent<PlayerSkillAnimationOverride>(root);
            var playerView = EnsureComponent<PlayerView>(root);
            EnsureComponent<PlayerPresenter>(root);
            EnsureComponent<PlayerController>(root);
            EnsureComponent<PlayerInputBridge>(root);
            EnsureComponent<PlayerRootMotionMotor>(root);
            EnsureComponent<PlayerAttackHitbox>(root);

            var sword = FindDeepChild(root.transform, "PlayerSword");
            if (sword != null)
            {
                var legacyHitbox = sword.GetComponent<PlayerAttackHitbox>();
                if (legacyHitbox != null)
                    Object.DestroyImmediate(legacyHitbox, true);

                var swordCol = sword.GetComponent<BoxCollider>();
                if (swordCol != null)
                    Object.DestroyImmediate(swordCol, true);
            }

            var animator = root.GetComponent<Animator>() ?? root.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = true;
                var pvSo = new SerializedObject(playerView);
                pvSo.FindProperty("_animator").objectReferenceValue = animator;
                pvSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var controller = root.GetComponent<CharacterController>() ?? root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);

            Object.DestroyImmediate(root.GetComponent<Rigidbody>());
            Object.DestroyImmediate(root.GetComponent<CapsuleCollider>());

            var cpSo = new SerializedObject(root.GetComponent<CharacterPresenter>());
            cpSo.FindProperty("_defaultFaction").enumValueIndex = (int)Core.Faction.Player;
            cpSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureEnemyPrefab()
        {
            if (!System.IO.File.Exists(EnemyPrefabPath))
            {
                Debug.LogWarning($"未找到 {EnemyPrefabPath}，请先配置 Enemy 模型并另存为 Prefab。");
                return;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(EnemyPrefabPath);
            var root = scope.prefabContentsRoot;
            EnsureComponent<CharacterView>(root);
            var presenter = EnsureComponent<CharacterPresenter>(root);
            EnsureComponent<CharacterEntity>(root);
            var enemyView = EnsureComponent<EnemyView>(root);
            var enemyPresenter = EnsureComponent<EnemyPresenter>(root);
            EnsureComponent<TrainingDummyMarker>(root);

            // 暂不按 Enemy Layer 设置 Prefab 根节点（PlayerAttackHitbox 已关闭 Layer 过滤）。
            // var enemyLayer = LayerMask.NameToLayer(PlayerAttackHitbox.EnemyLayerName);
            // if (enemyLayer >= 0)
            //     root.layer = enemyLayer;

            var animator = root.GetComponent<Animator>() ?? root.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                var evSo = new SerializedObject(enemyView);
                evSo.FindProperty("_animator").objectReferenceValue = animator;
                evSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var col = root.GetComponent<CapsuleCollider>() ?? root.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.35f;
            col.center = new Vector3(0f, 1f, 0f);

            var cpSo = new SerializedObject(presenter);
            cpSo.FindProperty("_defaultFaction").enumValueIndex = (int)Core.Faction.Enemy;
            cpSo.ApplyModifiedPropertiesWithoutUndo();

            var epSo = new SerializedObject(enemyPresenter);
            epSo.FindProperty("_maxHp").floatValue = 100f;
            epSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject BuildSystemsRoot()
        {
            var root = new GameObject("GameplaySystems");
            root.AddComponent<EffectSystem>();
            root.AddComponent<ItemSystem>();
            root.AddComponent<SkillSystem>();
            root.AddComponent<GameplayFeedbackProvider>();
            root.AddComponent<GameplayDpsTracker>();

            var debug = new GameObject("DebugTest");
            debug.transform.SetParent(root.transform, false);
            debug.AddComponent<GameplaySelfTestRunner>();
            return root;
        }

        static GameObject BuildHudRoot()
        {
            var panelGo = new GameObject("GameplayHudPanel");
            var rect = panelGo.AddComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panelGo.AddComponent<CanvasGroup>();
            panelGo.AddComponent<GameplayUiPanel>();

            var hud = panelGo.AddComponent<GameplayHud>();
            var log = CreateTmp(panelGo.transform, "Log", new Vector2(10, 10), new Vector2(420, 160), 14);
            var playerHp = CreateTmp(panelGo.transform, "PlayerHp", new Vector2(10, -80), new Vector2(320, 30), 18);
            var enemyHp = CreateTmp(panelGo.transform, "EnemyHp", new Vector2(10, -110), new Vector2(320, 30), 18);
            var skills = CreateTmp(panelGo.transform, "Skills", new Vector2(10, -145), new Vector2(420, 200), 14);

            var hotbarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudHotbarPrefabPath);
            GameplayHudHotbarView hotbarView = null;
            if (hotbarPrefab != null)
            {
                var hotbarInstance = (GameObject)PrefabUtility.InstantiatePrefab(hotbarPrefab, panelGo.transform);
                hotbarInstance.name = "HotbarPanel";
                var hotbarRect = hotbarInstance.GetComponent<RectTransform>();
                hotbarRect.anchorMin = new Vector2(0f, 0f);
                hotbarRect.anchorMax = new Vector2(0f, 0f);
                hotbarRect.pivot = new Vector2(0f, 0f);
                hotbarRect.anchoredPosition = new Vector2(10f, 10f);
                hotbarRect.sizeDelta = new Vector2(200f, 120f);
                hotbarView = hotbarInstance.GetComponent<GameplayHudHotbarView>();
            }
            else
            {
                Debug.LogWarning("GameplayHudHotbarPanel.prefab 未找到，请先执行 Build Prefab Assets。");
            }

            CreateButton(panelGo.transform, "Run Self Test", new Vector2(-10, 10), new Vector2(160, 36))
                .gameObject.AddComponent<GameplaySelfTestButton>();

            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("_logText").objectReferenceValue = log;
            hudSo.FindProperty("_playerHpText").objectReferenceValue = playerHp;
            hudSo.FindProperty("_enemyHpText").objectReferenceValue = enemyHp;
            hudSo.FindProperty("_skillStatusText").objectReferenceValue = skills;
            hudSo.FindProperty("_hotbarView").objectReferenceValue = hotbarView;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            return panelGo;
        }

        static GameObject BuildHudSlotRoot()
        {
            const float slotSize = 56f;
            var root = new GameObject("GameplayHudSlot");
            var rect = root.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(slotSize, slotSize);

            root.AddComponent<CanvasGroup>();
            var slotView = root.AddComponent<GameplayHudSlotView>();

            var icon = CreateHudImage(root.transform, "Icon", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var cooldown = CreateHudImage(root.transform, "CooldownFill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            cooldown.color = new Color(0f, 0f, 0f, 0f);
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Radial360;
            cooldown.fillOrigin = (int)Image.Origin360.Top;
            cooldown.fillClockwise = false;
            cooldown.fillAmount = 0f;
            cooldown.raycastTarget = false;

            var hotkey = CreateHudTmp(root.transform, "Hotkey", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(4f, -4f), 12f);
            hotkey.alignment = TextAlignmentOptions.TopLeft;

            var charges = CreateHudTmp(root.transform, "Charges", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4f, 4f), 14f);
            charges.alignment = TextAlignmentOptions.BottomRight;

            var slotSo = new SerializedObject(slotView);
            slotSo.FindProperty("_icon").objectReferenceValue = icon;
            slotSo.FindProperty("_cooldownFill").objectReferenceValue = cooldown;
            slotSo.FindProperty("_hotkeyText").objectReferenceValue = hotkey;
            slotSo.FindProperty("_chargeText").objectReferenceValue = charges;
            slotSo.FindProperty("_canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            slotSo.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        static GameObject BuildHudHotbarPanelRoot()
        {
            const float slotSize = 56f;
            const float slotSpacing = 8f;
            const int slotCount = 3;
            var rowWidth = slotCount * slotSize + (slotCount - 1) * slotSpacing;

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudSlotPrefabPath);

            var root = new GameObject("GameplayHudHotbarPanel");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(rowWidth, slotSize * 2f + slotSpacing);

            var hotbarView = root.AddComponent<GameplayHudHotbarView>();

            var skillRow = CreateHudRow(root.transform, "SkillRow", new Vector2(0f, 1f), new Vector2(0f, slotSize + slotSpacing), rowWidth, slotSize);
            var itemRow = CreateHudRow(root.transform, "ItemRow", new Vector2(0f, 0f), Vector2.zero, rowWidth, slotSize);

            var skillSlots = new GameplayHudSlotView[slotCount];
            var itemSlots = new GameplayHudSlotView[slotCount];

            for (var i = 0; i < slotCount; i++)
            {
                skillSlots[i] = InstantiateHudSlot(slotPrefab, skillRow, $"SkillSlot{i + 1}", i, slotSize, slotSpacing);
                itemSlots[i] = InstantiateHudSlot(slotPrefab, itemRow, $"ItemSlot{i + 1}", i, slotSize, slotSpacing);
            }

            var hotbarSo = new SerializedObject(hotbarView);
            hotbarSo.FindProperty("_skillSlots").arraySize = slotCount;
            hotbarSo.FindProperty("_itemSlots").arraySize = slotCount;
            for (var i = 0; i < slotCount; i++)
            {
                hotbarSo.FindProperty("_skillSlots").GetArrayElementAtIndex(i).objectReferenceValue = skillSlots[i];
                hotbarSo.FindProperty("_itemSlots").GetArrayElementAtIndex(i).objectReferenceValue = itemSlots[i];
            }
            hotbarSo.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        static RectTransform CreateHudRow(Transform parent, string name, Vector2 anchorY, Vector2 anchoredPos, float width, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, anchorY.y);
            rect.anchorMax = new Vector2(0f, anchorY.y);
            rect.pivot = new Vector2(0f, anchorY.y);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        static GameplayHudSlotView InstantiateHudSlot(
            GameObject slotPrefab,
            RectTransform row,
            string name,
            int index,
            float slotSize,
            float slotSpacing)
        {
            GameObject instanceGo;
            if (slotPrefab != null)
            {
                instanceGo = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, row);
                instanceGo.name = name;
            }
            else
            {
                instanceGo = BuildHudSlotRoot();
                instanceGo.name = name;
                instanceGo.transform.SetParent(row, false);
            }

            var rect = instanceGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(index * (slotSize + slotSpacing), 0f);
            rect.sizeDelta = new Vector2(slotSize, slotSize);
            return instanceGo.GetComponent<GameplayHudSlotView>();
        }

        static Image CreateHudImage(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return go.AddComponent<Image>();
        }

        static TextMeshProUGUI CreateHudTmp(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPos,
            float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(48f, 20f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UiFontPath);
            if (font != null)
                tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        static GameObject BuildCombatProjectileRoot()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "CombatProjectile";
            root.transform.localScale = Vector3.one * 0.35f;
            var col = root.GetComponent<SphereCollider>();
            col.isTrigger = true;
            root.AddComponent<CombatHitVolume>();
            root.AddComponent<CombatProjectile>();
            return root;
        }

        static GameObject BuildCameraRoot()
        {
            var go = new GameObject("GameplayCamera");
            var camera = go.AddComponent<Camera>();
            camera.tag = "MainCamera";
            go.AddComponent<AudioListener>();
            var rig = go.AddComponent<GameplayCameraRig>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.position = new Vector3(0f, 14f, 0f);

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab != null)
            {
                var rigSo = new SerializedObject(rig);
                rigSo.FindProperty("_followTarget").objectReferenceValue =
                    playerPrefab.transform;
                rigSo.ApplyModifiedPropertiesWithoutUndo();
            }

            return go;
        }

        static GameObject BuildPickupRoot()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "HealPotionPickup";
            root.transform.localScale = Vector3.one * 0.6f;
            root.GetComponent<Collider>().isTrigger = true;
            root.AddComponent<ItemPickupView>();
            root.AddComponent<PickupItem>();
            return root;
        }

        static void SaveOrUpdatePrefab(GameObject root, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing == null)
                PrefabUtility.SaveAsPrefabAsset(root, path);
            else
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(root);
        }

        static T EnsureComponent<T>(GameObject go) where T : Component =>
            go.GetComponent<T>() ?? go.AddComponent<T>();

        static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child;
            }
            return null;
        }

        static TextMeshProUGUI CreateTmp(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UiFontPath);
            if (font != null)
                tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            return tmp;
        }

        static Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("SelfTestButton");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            go.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            var btn = go.AddComponent<Button>();
            var text = CreateTmp(go.transform, "Text", Vector2.zero, size, 14);
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;
            var tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            return btn;
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

        static void WireDamageNumberPrefab(GameplayFeedbackProvider provider)
        {
            if (provider == null)
                return;

            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(DamageNumberPrefabPath);
            if (prefabRoot == null)
                return;

            Component damageNumber = null;
            foreach (var behaviour in prefabRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && behaviour.GetType().Name == "DamageNumberMesh")
                {
                    damageNumber = behaviour;
                    break;
                }
            }

            if (damageNumber == null)
                return;

            var so = new SerializedObject(provider);
            so.FindProperty("_damageNumberPrefab").objectReferenceValue = damageNumber;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
