#if UNITY_EDITOR
using Gameplay.Data;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(SkillData))]
    public class SkillDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script", "PreCastSeconds", "DurationSeconds", "PostCastSeconds");

            var kind = (SkillKind)serializedObject.FindProperty("Kind").enumValueIndex;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("PreCastSeconds"));

            if (kind == SkillKind.Channeled)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DurationSeconds"));

            if (kind == SkillKind.Projectile || kind == SkillKind.Channeled)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PostCastSeconds"));

            if (kind == SkillKind.StatusApply)
            {
                EditorGUILayout.HelpBox("状态赋予：仅使用前摇，效果在前摇结束后结算。", MessageType.Info);
            }
            else if (kind == SkillKind.Projectile)
            {
                EditorGUILayout.HelpBox("投射物：前摇 + 后摇；Duration 无效。", MessageType.None);
            }
            else if (kind == SkillKind.Channeled)
            {
                EditorGUILayout.HelpBox("持续型：前摇 + 持续 + 后摇。", MessageType.None);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
