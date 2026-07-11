#if UNITY_EDITOR
using Gameplay.Bootstrap;
using Gameplay.Data;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Editor
{
    [CustomPropertyDrawer(typeof(ItemSlotBinding))]
    public class ItemSlotPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var itemIdProp = property.FindPropertyRelative("ItemId");
            var catalog = ResolveItemCatalog(property);

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (catalog == null || catalog.Entries == null || catalog.Entries.Length == 0)
            {
                EditorGUI.LabelField(line, label.text, "请先指定 Item Catalog");
                EditorGUI.EndProperty();
                return;
            }

            var options = BuildOptions(catalog, out var ids);
            var currentIndex = IndexOfId(ids, itemIdProp.stringValue);
            var newIndex = EditorGUI.Popup(line, label.text, currentIndex, options);
            if (newIndex >= 0 && newIndex < ids.Length && ids[newIndex] != itemIdProp.stringValue)
                itemIdProp.stringValue = ids[newIndex];

            EditorGUI.EndProperty();
        }

        static ItemCatalog ResolveItemCatalog(SerializedProperty property)
        {
            if (property.serializedObject.targetObject is GameplaySessionConfig config)
                return config.ItemCatalog;

            var catalogProp = property.serializedObject.FindProperty("_itemCatalog");
            return catalogProp != null ? catalogProp.objectReferenceValue as ItemCatalog : null;
        }

        static string[] BuildOptions(ItemCatalog catalog, out string[] ids)
        {
            var count = 1 + (catalog.Entries?.Length ?? 0);
            var options = new string[count];
            ids = new string[count];
            options[0] = "(空)";
            ids[0] = string.Empty;
            var write = 1;
            if (catalog.Entries == null)
                return options;

            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                    continue;
                var display = string.IsNullOrEmpty(entry.DisplayName) ? entry.Id : entry.DisplayName;
                options[write] = $"{display} ({entry.Id})";
                ids[write] = entry.Id;
                write++;
            }

            if (write == count)
                return options;

            var trimmedOptions = new string[write];
            var trimmedIds = new string[write];
            System.Array.Copy(options, trimmedOptions, write);
            System.Array.Copy(ids, trimmedIds, write);
            ids = trimmedIds;
            return trimmedOptions;
        }

        static int IndexOfId(string[] ids, string id)
        {
            if (ids == null || ids.Length == 0)
                return 0;
            for (var i = 0; i < ids.Length; i++)
            {
                if (ids[i] == id)
                    return i;
            }
            return 0;
        }
    }
}
#endif
