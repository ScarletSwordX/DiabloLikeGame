using Gameplay.Bootstrap;
using Gameplay.EventBus;
using UnityEngine;

namespace Gameplay.FeedbackUI
{
    /// <summary>
    /// 统计玩家造成的总伤害与 DPS（自上次重置起）。
    /// </summary>
    public class GameplayDpsTracker : MonoBehaviour
    {
        public static GameplayDpsTracker Instance { get; private set; }

        float _totalDamage;
        float _windowStartTime;
        int _playerEntityId = -1;

        public float TotalDamage => _totalDamage;

        public float Dps
        {
            get
            {
                var elapsed = Time.time - _windowStartTime;
                return elapsed > 0.001f ? _totalDamage / elapsed : 0f;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            ResetWindow();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void OnEnable()
        {
            GameplayMvpSession.Ready += OnMvpReady;
            GameEventBus.Instance.Subscribe<DamageDealtEvent>(OnDamageDealt);
        }

        void OnDisable()
        {
            GameplayMvpSession.Ready -= OnMvpReady;
            GameEventBus.Instance.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
        }

        void OnMvpReady() => TryResolvePlayer();

        void OnDamageDealt(DamageDealtEvent e)
        {
            if (_playerEntityId < 0)
                TryResolvePlayer();

            if (_playerEntityId < 0 || e.SourceId != _playerEntityId || e.Amount <= 0f)
                return;

            _totalDamage += e.Amount;
        }

        void TryResolvePlayer()
        {
            var player = GameplayMvpSession.Player;
            _playerEntityId = player != null ? player.EntityId : -1;
        }

        public void ResetWindow()
        {
            _totalDamage = 0f;
            _windowStartTime = Time.time;
        }
    }
}
