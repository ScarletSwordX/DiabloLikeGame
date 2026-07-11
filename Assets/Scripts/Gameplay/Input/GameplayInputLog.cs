using UnityEngine;

namespace Gameplay.Input
{
    /// <summary>
    /// 玩家输入链路调试日志：Bridge → Bus → Presenter / View。
    /// </summary>
    public static class GameplayInputLog
    {
        const string Tag = "[PlayerInput]";

        public static bool Enabled = true;

        public static bool LogMoveChanges = false;

        public static void Bridge(string action, string detail = null)
        {
            if (!Enabled) return;
            Log("Bridge", action, detail);
        }

        public static void BridgeIgnored(string action, string reason)
        {
            if (!Enabled) return;
            Log("Bridge", action, $"ignored: {reason}");
        }

        public static void Bus(string action, string detail = null)
        {
            if (!Enabled) return;
            Log("Bus", action, detail);
        }

        public static void Presenter(string handler, string detail = null)
        {
            if (!Enabled) return;
            Log("Presenter", handler, detail);
        }

        public static void View(string detail)
        {
            if (!Enabled) return;
            Log("View", "Move", detail);
        }

        public static void ViewAction(string detail)
        {
            if (!Enabled) return;
            Log("View", "Action", detail);
        }

        public static void Skill(string action, string detail = null)
        {
            if (!Enabled) return;
            Log("Skill", action, detail);
        }

        public static void Item(string action, string detail = null)
        {
            if (!Enabled) return;
            Log("Item", action, detail);
        }

        public static void Bootstrap(string detail)
        {
            if (!Enabled) return;
            Log("Bootstrap", "GameBootstrap", detail);
        }

        static void Log(string stage, string action, string detail)
        {
            if (string.IsNullOrEmpty(detail))
                Debug.Log($"{Tag} {stage} → {action}");
            else
                Debug.Log($"{Tag} {stage} → {action} | {detail}");
        }
    }
}
