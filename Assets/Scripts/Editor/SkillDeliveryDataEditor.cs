#if UNITY_EDITOR
using Gameplay.Data;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(SkillDeliveryData))]
    public class SkillDeliveryDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "Area",
                "ProjectilePrefab",
                "SpawnOnAnimationEvent",
                "Projectile");

            var kind = (SkillDeliveryKind)serializedObject.FindProperty("Kind").enumValueIndex;

            if (kind == SkillDeliveryKind.InstantArea)
            {
                EditorGUILayout.LabelField("作用区域", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Area"), true);
            }
            else
            {
                EditorGUILayout.LabelField("投射物", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectilePrefab"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SpawnOnAnimationEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Projectile"), true);
                EditorGUILayout.HelpBox("Projectile 不使用作用区域；命中由投射物碰撞判定。", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
