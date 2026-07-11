#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Gameplay.Data;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(ItemCatalog))]
    public class ItemCatalogEditor : UnityEditor.Editor
    {
        const string ItemsFolder = "Assets/Data/Items";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Rebuild From Folder"))
                RebuildFromFolder((ItemCatalog)target);
        }

        public static void RebuildFromFolder(ItemCatalog catalog)
        {
            if (catalog == null)
                return;

            var list = new List<ItemDefinition>();
            if (Directory.Exists(ItemsFolder))
            {
                foreach (var path in Directory.GetFiles(ItemsFolder, "*.asset", SearchOption.TopDirectoryOnly))
                {
                    var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path.Replace('\\', '/'));
                    if (item != null)
                        list.Add(item);
                }
            }

            catalog.Entries = list.ToArray();
            catalog.RebuildIndex();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"ItemCatalog: rebuilt {catalog.Entries.Length} entries from {ItemsFolder}");
        }
    }
}
#endif
