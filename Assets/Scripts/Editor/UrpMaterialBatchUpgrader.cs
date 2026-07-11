#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Editor
{
    /// <summary>
    /// 将 Built-in 材质批量升级为 URP（保留贴图/颜色/关键词，映射规则对齐 SampleScene 中 MaleCharacterPBR 使用的 PBR_Default）。
    /// </summary>
    public static class UrpMaterialBatchUpgrader
    {
        const string UrpLitShaderName = "Universal Render Pipeline/Lit";

        /// <summary>与 MaleCharacterPBR 预制体一致，用于菜单快捷升级。</summary>
        public const string MaleCharacterPbrPrefabPath = "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPBR.prefab";

        public const string RpgHeroMaterialFolder = "Assets/RPG Tiny Hero Duo/Material";

        static readonly string[] BuiltInShaderNames =
        {
            "Standard",
            "Standard (Specular setup)",
            "Standard (Roughness setup)",
            "Autodesk Interactive",
            "Autodesk Interactive Transparent",
            "Legacy Shaders/Diffuse",
            "Legacy Shaders/Specular",
            "Legacy Shaders/Bumped Diffuse",
            "Legacy Shaders/Bumped Specular",
            "Mobile/Diffuse",
            "Mobile/Bumped Diffuse",
            "Mobile/Bumped Specular",
            "Mobile/Specular",
        };

        static readonly (string src, string dst)[] CommonPropertyMap =
        {
            ("_Color", "_BaseColor"),
            ("_MainTex", "_BaseMap"),
            ("_Cutoff", "_Cutoff"),
            ("_BumpMap", "_BumpMap"),
            ("_BumpScale", "_BumpScale"),
            ("_Parallax", "_Parallax"),
            ("_ParallaxMap", "_ParallaxMap"),
            ("_OcclusionMap", "_OcclusionMap"),
            ("_OcclusionStrength", "_OcclusionStrength"),
            ("_EmissionMap", "_EmissionMap"),
            ("_EmissionColor", "_EmissionColor"),
            ("_DetailMask", "_DetailMask"),
            ("_DetailAlbedoMap", "_DetailAlbedoMap"),
            ("_DetailNormalMap", "_DetailNormalMap"),
            ("_DetailNormalMapScale", "_DetailNormalMapScale"),
        };

        static readonly (string src, string dst)[] MetallicWorkflowMap =
        {
            ("_Metallic", "_Metallic"),
            ("_Glossiness", "_Smoothness"),
            ("_GlossMapScale", "_Smoothness"),
            ("_MetallicGlossMap", "_MetallicGlossMap"),
        };

        static readonly (string src, string dst)[] SpecularWorkflowMap =
        {
            ("_SpecColor", "_SpecColor"),
            ("_SpecGlossMap", "_SpecGlossMap"),
            ("_Glossiness", "_Smoothness"),
            ("_GlossMapScale", "_Smoothness"),
        };

        [MenuItem("Gameplay/Rendering/Upgrade Materials (MaleCharacterPBR Reference)", false, 100)]
        public static void UpgradeMaleCharacterPbrReference()
        {
            var materials = CollectMaterialsFromPrefab(MaleCharacterPbrPrefabPath);
            if (materials.Count == 0)
            {
                materials = FindMaterialsInFolder(RpgHeroMaterialFolder);
                Debug.LogWarning($"未在预制体上找到材质，改为升级文件夹: {RpgHeroMaterialFolder}");
            }

            UpgradeMaterials(materials, "MaleCharacterPBR Reference");
        }

        [MenuItem("Gameplay/Rendering/Upgrade All Materials In RPG Tiny Hero Duo", false, 101)]
        public static void UpgradeRpgTinyHeroDuoFolder()
        {
            UpgradeMaterials(FindMaterialsInFolder("Assets/RPG Tiny Hero Duo"), "RPG Tiny Hero Duo");
        }

        [MenuItem("Gameplay/Rendering/Upgrade Selected Materials To URP", false, 102)]
        public static void UpgradeSelectedMaterials()
        {
            var mats = Selection.objects
                .OfType<Material>()
                .Distinct()
                .ToList();

            if (mats.Count == 0)
            {
                EditorUtility.DisplayDialog("URP 材质升级",
                    "请在 Project 窗口选中一个或多个 Material 资产。", "确定");
                return;
            }

            UpgradeMaterials(mats, "Selection");
        }

        [MenuItem("Gameplay/Rendering/Upgrade All Built-in Materials In Project", false, 103)]
        public static void UpgradeAllBuiltInMaterialsInProject()
        {
            if (!EditorUtility.DisplayDialog("URP 材质升级",
                    "将扫描整个 Assets 下所有 Built-in 材质并升级为 URP。\n建议先提交或备份。是否继续？",
                    "继续", "取消"))
                return;

            var guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            var list = new List<Material>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && NeedsUpgrade(mat))
                    list.Add(mat);
            }

            UpgradeMaterials(list, "Entire Project");
        }

        public static List<Material> CollectMaterialsFromPrefab(string prefabPath)
        {
            var result = new HashSet<Material>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"找不到预制体: {prefabPath}");
                return new List<Material>();
            }

            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (r.sharedMaterials == null) continue;
                foreach (var m in r.sharedMaterials)
                {
                    if (m != null) result.Add(m);
                }
            }

            return result.ToList();
        }

        public static List<Material> FindMaterialsInFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return new List<Material>();

            return AssetDatabase.FindAssets("t:Material", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .Where(m => m != null)
                .Distinct()
                .ToList();
        }

        public static bool NeedsUpgrade(Material material)
        {
            if (material == null || material.shader == null) return false;
            var name = material.shader.name;
            if (name.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal) ||
                name.StartsWith("Shader Graphs/", StringComparison.Ordinal) ||
                name.StartsWith("HDRP/", StringComparison.Ordinal))
                return false;

            if (BuiltInShaderNames.Contains(name)) return true;
            return name.Contains("Legacy") || name.StartsWith("Mobile/", StringComparison.Ordinal);
        }

        public static void UpgradeMaterials(IReadOnlyList<Material> materials, string contextLabel)
        {
            var lit = Shader.Find(UrpLitShaderName);
            if (lit == null)
            {
                EditorUtility.DisplayDialog("URP 材质升级",
                    "未找到 URP Lit Shader。请确认项目已安装 URP 且处于 URP 渲染路径。", "确定");
                return;
            }

            var upgraded = 0;
            var skipped = 0;

            try
            {
                AssetDatabase.StartAssetEditing();
                for (var i = 0; i < materials.Count; i++)
                {
                    var mat = materials[i];
                    if (mat == null) continue;

                    EditorUtility.DisplayProgressBar("URP 材质升级",
                        $"{mat.name} ({i + 1}/{materials.Count})",
                        (float)(i + 1) / materials.Count);

                    if (!NeedsUpgrade(mat))
                    {
                        skipped++;
                        continue;
                    }

                    Undo.RecordObject(mat, "Upgrade Material To URP");
                    if (UpgradeSingleMaterial(mat, lit))
                    {
                        EditorUtility.SetDirty(mat);
                        upgraded++;
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"[URP Upgrade][{contextLabel}] 升级 {upgraded} 个，跳过 {skipped} 个，共 {materials.Count} 个。");
            EditorUtility.DisplayDialog("URP 材质升级完成",
                $"上下文: {contextLabel}\n升级: {upgraded}\n跳过: {skipped}\n总计: {materials.Count}", "确定");
        }

        public static bool UpgradeSingleMaterial(Material material, Shader urpLit)
        {
            if (material == null || material.shader == null) return false;

            var sourceShaderName = material.shader.name;
            if (!NeedsUpgrade(material)) return false;

            var isSpecularSetup = sourceShaderName.Contains("Specular", StringComparison.Ordinal) &&
                                  material.HasProperty("_SpecColor");

            var snapshot = CaptureSnapshot(material, isSpecularSetup);

            material.shader = urpLit;
            ApplySnapshot(material, snapshot, isSpecularSetup);
            ApplySurfaceFromLegacyMode(material, snapshot.Mode);
            ApplyKeywordMigration(material, snapshot);

            return true;
        }

        static MaterialSnapshot CaptureSnapshot(Material material, bool isSpecularSetup)
        {
            var snap = new MaterialSnapshot
            {
                Mode = material.HasProperty("_Mode") ? material.GetFloat("_Mode") : 0f,
                RenderQueue = material.renderQueue,
                Keywords = material.shaderKeywords != null
                    ? (string[])material.shaderKeywords.Clone()
                    : Array.Empty<string>()
            };

            CopyIfExists(material, snap.Floats, CommonPropertyMap);
            CopyIfExists(material, snap.Colors, CommonPropertyMap);
            CopyIfExists(material, snap.Textures, CommonPropertyMap);
            CopyTextureScaleIfExists(material, snap.Scales, CommonPropertyMap);
            CopyTextureOffsetIfExists(material, snap.Offsets, CommonPropertyMap);

            if (isSpecularSetup)
            {
                CopyIfExists(material, snap.Floats, SpecularWorkflowMap);
                CopyIfExists(material, snap.Colors, SpecularWorkflowMap);
                CopyIfExists(material, snap.Textures, SpecularWorkflowMap);
            }
            else
            {
                CopyIfExists(material, snap.Floats, MetallicWorkflowMap);
                CopyIfExists(material, snap.Textures, MetallicWorkflowMap);
            }

            return snap;
        }

        static void ApplySnapshot(Material material, MaterialSnapshot snap, bool isSpecularSetup)
        {
            if (isSpecularSetup)
                material.EnableKeyword("_SPECULAR_SETUP");

            SetIfExists(material, snap.Floats, CommonPropertyMap);
            SetIfExists(material, snap.Colors, CommonPropertyMap);
            SetTexturesWithST(material, snap);

            if (isSpecularSetup)
            {
                SetIfExists(material, snap.Floats, SpecularWorkflowMap);
                SetIfExists(material, snap.Colors, SpecularWorkflowMap);
                SetIfExists(material, snap.Textures, SpecularWorkflowMap);
            }
            else
            {
                SetIfExists(material, snap.Floats, MetallicWorkflowMap);
                SetIfExists(material, snap.Textures, MetallicWorkflowMap);
            }

            if (snap.RenderQueue >= 0)
                material.renderQueue = snap.RenderQueue;
        }

        static void SetTexturesWithST(Material material, MaterialSnapshot snap)
        {
            foreach (var pair in CommonPropertyMap)
            {
                if (!snap.Textures.TryGetValue(pair.src, out var tex) || tex == null) continue;
                if (!material.HasProperty(pair.dst)) continue;

                material.SetTexture(pair.dst, tex);
                if (snap.Scales.TryGetValue(pair.src, out var scale))
                    material.SetTextureScale(pair.dst, scale);
                if (snap.Offsets.TryGetValue(pair.src, out var offset))
                    material.SetTextureOffset(pair.dst, offset);
            }
        }

        static void ApplySurfaceFromLegacyMode(Material material, float legacyMode)
        {
            var mode = Mathf.RoundToInt(legacyMode);

            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", mode is 2 or 3 ? 1f : 0f);

            switch (mode)
            {
                case 1: // Cutout
                    if (material.HasProperty("_AlphaClip"))
                        material.SetFloat("_AlphaClip", 1f);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    break;
                case 2: // Fade
                case 3: // Transparent
                    if (material.HasProperty("_AlphaClip"))
                        material.SetFloat("_AlphaClip", 0f);
                    if (material.HasProperty("_Blend"))
                        material.SetFloat("_Blend", 0f);
                    if (material.HasProperty("_SrcBlend"))
                        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    if (material.HasProperty("_DstBlend"))
                        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    if (material.HasProperty("_ZWrite"))
                        material.SetFloat("_ZWrite", 0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
                default: // Opaque
                    if (material.HasProperty("_AlphaClip"))
                        material.SetFloat("_AlphaClip", 0f);
                    if (material.HasProperty("_ZWrite"))
                        material.SetFloat("_ZWrite", 1f);
                    material.renderQueue = (int)RenderQueue.Geometry;
                    break;
            }
        }

        static void ApplyKeywordMigration(Material material, MaterialSnapshot snap)
        {
            material.shaderKeywords = Array.Empty<string>();

            foreach (var kw in snap.Keywords)
            {
                switch (kw)
                {
                    case "_EMISSION":
                        material.EnableKeyword("_EMISSION");
                        if (material.HasProperty("_EmissionColor"))
                            material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                        break;
                    case "_NORMALMAP":
                        material.EnableKeyword("_NORMALMAP");
                        break;
                    case "_METALLICGLOSSMAP":
                        material.EnableKeyword("_METALLICSPECGLOSSMAP");
                        break;
                    case "_PARALLAXMAP":
                        material.EnableKeyword("_PARALLAXMAP");
                        break;
                    case "_ALPHATEST_ON":
                        material.EnableKeyword("_ALPHATEST_ON");
                        break;
                    case "_ALPHAPREMULTIPLY_ON":
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                }
            }

            if (material.HasProperty("_EmissionMap") && material.GetTexture("_EmissionMap") != null)
                material.EnableKeyword("_EMISSION");
        }

        static void CopyIfExists(Material src, Dictionary<string, float> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!src.HasProperty(pair.src)) continue;
                dict[pair.src] = src.GetFloat(pair.src);
            }
        }

        static void CopyIfExists(Material src, Dictionary<string, Color> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!src.HasProperty(pair.src)) continue;
                dict[pair.src] = src.GetColor(pair.src);
            }
        }

        static void CopyIfExists(Material src, Dictionary<string, Texture> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!src.HasProperty(pair.src)) continue;
                var tex = src.GetTexture(pair.src);
                if (tex != null) dict[pair.src] = tex;
            }
        }

        static void CopyTextureScaleIfExists(Material src, Dictionary<string, Vector2> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!src.HasProperty(pair.src)) continue;
                dict[pair.src] = src.GetTextureScale(pair.src);
            }
        }

        static void CopyTextureOffsetIfExists(Material src, Dictionary<string, Vector2> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!src.HasProperty(pair.src)) continue;
                dict[pair.src] = src.GetTextureOffset(pair.src);
            }
        }

        static void SetIfExists(Material dst, Dictionary<string, float> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!dict.TryGetValue(pair.src, out var value)) continue;
                if (dst.HasProperty(pair.dst)) dst.SetFloat(pair.dst, value);
            }
        }

        static void SetIfExists(Material dst, Dictionary<string, Color> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!dict.TryGetValue(pair.src, out var value)) continue;
                if (dst.HasProperty(pair.dst)) dst.SetColor(pair.dst, value);
            }
        }

        static void SetIfExists(Material dst, Dictionary<string, Texture> dict,
            IEnumerable<(string src, string dst)> map)
        {
            foreach (var pair in map)
            {
                if (!dict.TryGetValue(pair.src, out var tex) || tex == null) continue;
                if (dst.HasProperty(pair.dst)) dst.SetTexture(pair.dst, tex);
            }
        }

        class MaterialSnapshot
        {
            public float Mode;
            public int RenderQueue = -1;
            public string[] Keywords = Array.Empty<string>();
            public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
            public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();
            public readonly Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
            public readonly Dictionary<string, Vector2> Scales = new Dictionary<string, Vector2>();
            public readonly Dictionary<string, Vector2> Offsets = new Dictionary<string, Vector2>();
        }
    }
}
#endif
