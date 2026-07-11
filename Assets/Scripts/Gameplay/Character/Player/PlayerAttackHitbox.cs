using System.Collections.Generic;
using Gameplay.Character.Player.Model;
using Gameplay.Core;
using Gameplay.EventBus;
using UnityEngine;

namespace Gameplay.Character.Player
{
    /// <summary>
    /// 玩家近战扇形攻击判定：使用 OverlapSphere 检测前方 Collider。
    /// 同一挥砍对同一 CharacterEntity 只结算一次。
    /// Layer 过滤已临时关闭。
    /// </summary>
    public class PlayerAttackHitbox : MonoBehaviour
    {
        // 对应 Layer 名称，可在 Editor / 项目设置中配置
        public const string EnemyLayerName = "Enemy";

        [Header("扇形范围")]
        [SerializeField] float _radius = 2.2f;
        [SerializeField] float _halfAngle = 55f;
        [SerializeField] float _originHeight = 1f;

        [Header("物理查询")]
        [SerializeField] QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;

        [Header("调试")]
        [SerializeField] bool _drawDebug = true;
        [SerializeField] float _debugSeconds = 0.25f;

        PlayerModel _model;
        CharacterEntity _owner;
        // int _enemyLayer = -1;

        readonly HashSet<int> _hitThisSwing = new HashSet<int>();

        public void Bind(PlayerModel model, CharacterEntity owner)
        {
            _model = model;
            _owner = owner;
        }

        void Awake()
        {
            if (_owner == null)
                _owner = GetComponent<CharacterEntity>();

            // _enemyLayer = LayerMask.NameToLayer(EnemyLayerName);
            // if (_enemyLayer < 0)
            //     Debug.LogWarning($"PlayerAttackHitbox: 未找到 Layer「{EnemyLayerName}」，请在 TagManager 中配置");
        }

        public void BeginSwing()
        {
            if (_model == null || _owner == null)
                return;

            _hitThisSwing.Clear();

            var origin = GetAttackOrigin();
            var forward = GetFlatForward();
            if (forward.sqrMagnitude < 0.0001f)
                return;

            CastSector(origin, forward);
        }

        void CastSector(Vector3 origin, Vector3 forward)
        {
            // Layer 过滤已关闭，当前全 Layer（~0）；恢复时改回 1 << _enemyLayer 即可
            var hits = Physics.OverlapSphere(
                origin,
                _radius,
                ~0,
                _triggerInteraction);

            foreach (var hit in hits)
                HandleCandidateCollider(hit, origin, forward);

            if (!_drawDebug)
                return;

            Debug.DrawRay(origin, forward * _radius, Color.red, _debugSeconds);
            var left = Quaternion.AngleAxis(-_halfAngle, Vector3.up) * forward;
            var right = Quaternion.AngleAxis(_halfAngle, Vector3.up) * forward;
            Debug.DrawRay(origin, left * _radius, Color.yellow, _debugSeconds);
            Debug.DrawRay(origin, right * _radius, Color.yellow, _debugSeconds);
        }

        void HandleCandidateCollider(Collider other, Vector3 origin, Vector3 forward)
        {
            if (other == null)
                return;

            // if (_enemyLayer >= 0 && other.gameObject.layer != _enemyLayer)
            // {
            //     GameplayCombatLog.HitboxTriggerTouch(name, other, $"ignored: not {EnemyLayerName} layer");
            //     return;
            // }

            var target = other.GetComponentInParent<CharacterEntity>();
            if (target == null || !target.IsAlive)
                return;
            if (target == _owner)
                return;
            if (!IsTargetInSector(target, other, origin, forward))
                return;
            if (!_hitThisSwing.Add(target.EntityId))
                return;

            ApplyDamage(target);
        }

        bool IsTargetInSector(
            CharacterEntity target,
            Collider collider,
            Vector3 origin,
            Vector3 forward)
        {
            var targetPoint = target.transform.position;
            var toTarget = targetPoint - origin;
            toTarget.y = 0f;

            var distance = toTarget.magnitude;
            if (distance > _radius)
                return false;
            if (distance < 0.01f)
                return true;

            return Vector3.Angle(forward, toTarget) <= _halfAngle;
        }

        void ApplyDamage(CharacterEntity target)
        {
            var result = target.ProcessDamage(new DamageRequest
            {
                SourceId = _owner.EntityId,
                TargetId = target.EntityId,
                RawDamage = _model.AttackDamage
            });

            GameEventBus.Instance.Publish(new CombatLogEvent
            {
                Message = $"普攻 {_model.AttackDamage:0.#} → 实体 {target.EntityId}，造成 {result.FinalDamage:0.#} 伤害"
            });
        }

        Vector3 GetAttackOrigin() =>
            _owner.transform.position + Vector3.up * _originHeight;

        Vector3 GetFlatForward()
        {
            var forward = _owner.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return Vector3.zero;
            return forward.normalized;
        }

        void OnDrawGizmosSelected()
        {
            var owner = _owner != null ? _owner : GetComponent<CharacterEntity>();
            if (owner == null)
                return;

            var origin = owner.transform.position + Vector3.up * _originHeight;
            var forward = owner.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return;
            forward.Normalize();

            var left = Quaternion.AngleAxis(-_halfAngle, Vector3.up) * forward;
            var right = Quaternion.AngleAxis(_halfAngle, Vector3.up) * forward;

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.85f);
            Gizmos.DrawLine(origin, origin + left * _radius);
            Gizmos.DrawLine(origin, origin + right * _radius);
            Gizmos.DrawLine(origin + left * _radius, origin + right * _radius);

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.15f);
            Gizmos.DrawWireSphere(origin, _radius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + forward * _radius);
        }
    }
}
