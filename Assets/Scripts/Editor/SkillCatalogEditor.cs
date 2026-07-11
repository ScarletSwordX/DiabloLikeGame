#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Gameplay.Data;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(SkillCatalog))]
    public class SkillCatalogEditor : UnityEditor.Editor
    {
        const string SkillsFolder = "Assets/Data/Skills";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Rebuild From Folder"))
                RebuildFromFolder((SkillCatalog)target);
        }

        public static void RebuildFromFolder(SkillCatalog catalog)
        {
            if (catalog == null)
                return;

            var list = new List<SkillData>();
            if (Directory.Exists(SkillsFolder))
            {
                foreach (var path in Directory.GetFiles(SkillsFolder, "*.asset", SearchOption.TopDirectoryOnly))
                {
                    var skill = AssetDatabase.LoadAssetAtPath<SkillData>(path.Replace('\\', '/'));
                    if (skill != null)
                        list.Add(skill);
                }
            }

            catalog.Entries = list.ToArray();
            catalog.RebuildIndex();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"SkillCatalog: rebuilt {catalog.Entries.Length} entries from {SkillsFolder}");
        }
    }
}
#endif
