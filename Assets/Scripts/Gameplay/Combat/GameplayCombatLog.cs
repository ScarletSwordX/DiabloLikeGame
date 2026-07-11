using Gameplay.Core;
using UnityEngine;

namespace Gameplay.Combat
{
    /// <summary>
    /// 战斗受伤链路调试日志：Model 扣血 → View 受击反馈。
    /// </summary>
    public static class GameplayCombatLog
    {
        const string Tag = "[Combat]";

        public static bool Enabled = true;

        public static void DamageReceived(
            Faction faction,
            int entityId,
            in DamageRequest request,
            in DamageResult result,
            float hpBefore,
            float hpAfter,
            float maxHp)
        {
            if (!Enabled) return;

            var role = RoleLabel(faction);
            var source = request.SourceId > 0 ? request.SourceId.ToString() : "?";
            var shield = result.AbsorbedByShield ? " shield=absorbed" : string.Empty;
            var killed = result.Killed ? " → KILLED" : string.Empty;

            Debug.Log(
                $"{Tag} Hurt → {role} entity={entityId} | source={source} " +
                $"raw={request.RawDamage:0.#} final={result.FinalDamage:0.#} " +
                $"hp {hpBefore:0.#}→{hpAfter:0.#}/{maxHp:0.#}{shield}{killed}");
        }

        public static void HitReaction(Faction faction, int entityId, string detail)
        {
            if (!Enabled) return;
            Debug.Log($"{Tag} View → {RoleLabel(faction)} entity={entityId} | {detail}");
        }

        public static void HitboxTriggerTouch(string hitboxName, Collider other, string outcome)
        {
            if (!Enabled) return;

            var go = other != null ? other.gameObject : null;
            if (go == null)
            {
                Debug.Log($"{Tag} Hitbox → {hitboxName} | touch (null collider) | {outcome}");
                return;
            }

            var layerName = LayerMask.LayerToName(go.layer);
            if (string.IsNullOrEmpty(layerName))
                layerName = go.layer.ToString();

            var root = go.transform.root != null ? go.transform.root.name : go.name;
            Debug.Log(
                $"{Tag} Hitbox → {hitboxName} | touch collider={other.name} go={go.name} root={root} " +
                $"layer={layerName}({go.layer}) trigger={other.isTrigger} | {outcome}");
        }

        static string RoleLabel(Faction faction)
        {
            switch (faction)
            {
                case Faction.Player: return "Player";
                case Faction.Enemy: return "Enemy";
                default: return faction.ToString();
            }
        }
    }
}
