using Gameplay.Character;
using Gameplay.Core;
using Gameplay.Data;
using Gameplay.Effect;
using UnityEngine;

namespace Gameplay.Combat
{
    /// <summary>
    /// 投射物：Trigger 碰撞敌人 Collider 时结算一次效果并销毁（默认）。
    /// </summary>
    [RequireComponent(typeof(CombatHitVolume))]
    public class CombatProjectile : MonoBehaviour
    {
        [SerializeField] float _speed = 12f;
        [SerializeField] float _lifetime = 4f;
        [SerializeField] bool _destroyOnHit = true;

        CombatHitVolume _hitVolume;
        Vector3 _direction;
        Vector3 _launchOrigin;
        float _maxFlightDistance;
        bool _launched;

        void Awake()
        {
            _hitVolume = GetComponent<CombatHitVolume>();
        }

        public void SetSpeed(float speed)
        {
            if (speed > 0f)
                _speed = speed;
        }

        public float ConfiguredSpeed => _speed;

        public void Launch(
            CharacterEntity source,
            EffectSystem effectSystem,
            ActionEffectProfile profile,
            Vector3 direction,
            float speedOverride = 0f,
            float maxFlightDistance = 0f)
        {
            if (source == null || effectSystem == null || profile == null)
            {
                Destroy(gameObject);
                return;
            }

            if (speedOverride > 0f)
                SetSpeed(speedOverride);

            _maxFlightDistance = maxFlightDistance > 0f ? maxFlightDistance : 0f;
            _launchOrigin = transform.position;

            _direction = direction.sqrMagnitude > 0.01f ? direction : source.transform.forward;
            _direction.y = 0f;
            if (_direction.sqrMagnitude < 0.0001f)
            {
                var forward = source.transform.forward;
                forward.y = 0f;
                _direction = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            }
            else
                _direction = _direction.normalized;

            transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);

            _hitVolume.TargetHit -= OnTargetHit;
            _hitVolume.TargetHit += OnTargetHit;
            _hitVolume.ConfigureEffectHit(source.EntityId, source.Faction, effectSystem, profile);
            _hitVolume.BeginHitWindow();
            _launched = true;

            if (_maxFlightDistance > 0f && _speed > 0f)
                Destroy(gameObject, _maxFlightDistance / _speed + 0.25f);
            else
                Destroy(gameObject, _lifetime);
        }

        void OnTargetHit(CharacterEntity _)
        {
            if (_destroyOnHit)
                Destroy(gameObject);
        }

        void Update()
        {
            if (!_launched) return;
            var delta = _direction * (_speed * Time.deltaTime);
            delta.y = 0f;
            transform.position += delta;

            if (_maxFlightDistance > 0f)
            {
                var traveled = HorizontalDistance(_launchOrigin, transform.position);
                if (traveled >= _maxFlightDistance)
                    Destroy(gameObject);
            }
        }

        static float HorizontalDistance(Vector3 from, Vector3 to)
        {
            from.y = 0f;
            to.y = 0f;
            return Vector3.Distance(from, to);
        }

        void OnDestroy()
        {
            if (_hitVolume != null)
                _hitVolume.TargetHit -= OnTargetHit;
        }
    }
}
