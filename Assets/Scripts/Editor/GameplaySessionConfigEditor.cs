#if UNITY_EDITOR
using Gameplay.Bootstrap;
using Gameplay.Data;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(GameplaySessionConfig))]
    public class GameplaySessionConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var config = (GameplaySessionConfig)target;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Loadout Preview", EditorStyles.boldLabel);
            DrawSlotPreview("Skill 1", config.ResolveSkill(config.GetSkillSlot(0).SkillId));
            DrawSlotPreview("Skill 2", config.ResolveSkill(config.GetSkillSlot(1).SkillId));
            DrawSlotPreview("Skill 3", config.ResolveSkill(config.GetSkillSlot(2).SkillId));
            DrawSlotPreview("Item 1", config.ResolveItem(config.GetItemSlot(0).ItemId));
            DrawSlotPreview("Item 2", config.ResolveItem(config.GetItemSlot(1).ItemId));
            DrawSlotPreview("Item 3", config.ResolveItem(config.GetItemSlot(2).ItemId));

            if (!Application.isPlaying)
                return;

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Apply Loadout Now"))
            {
                var service = Object.FindObjectOfType<GameplayLoadoutService>();
                if (service == null)
                    Debug.LogWarning("GameplaySessionConfig: 场景中未找到 GameplayLoadoutService。");
                else if (!service.Reload())
                    Debug.LogWarning("GameplaySessionConfig: Reload 未成功（可能正在施法）。");
            }
        }

        static void DrawSlotPreview(string label, Object resolved)
        {
            var value = resolved != null ? resolved.name : "(空)";
            EditorGUILayout.LabelField(label, value);
        }
    }
}
#endif
