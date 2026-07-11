using Gameplay.Character.Presenter;
using Gameplay.Core;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay.Character
{
    /// <summary>
    /// 对外门面，委托 CharacterPresenter（MVP）。受伤能力见 <see cref="IDamageable"/>（由 Presenter 实现）。
    /// </summary>
    [RequireComponent(typeof(CharacterPresenter))]
    public class CharacterEntity : MonoBehaviour
    {
        CharacterPresenter _presenter;

        public int EntityId
        {
            get
            {
                EnsurePresenter();
                return _presenter != null ? _presenter.Model.EntityId : 0;
            }
        }

        public Faction Faction
        {
            get
            {
                EnsurePresenter();
                return _presenter != null ? _presenter.Model.Faction : Faction.Player;
            }
        }

        public float CurrentHp
        {
            get
            {
                EnsurePresenter();
                return _presenter != null ? _presenter.Model.CurrentHp : 0f;
            }
        }

        public float MaxHp
        {
            get
            {
                EnsurePresenter();
                return _presenter != null ? _presenter.Model.MaxHp : 0f;
            }
        }

        public float BaseMoveSpeed
        {
            get
            {
                EnsurePresenter();
                return _presenter != null ? _presenter.Model.BaseMoveSpeed : 0f;
            }
        }

        public Transform AimPoint
        {
            get
            {
                EnsurePresenter();
                return _presenter != null ? _presenter.View.AimPoint : transform;
            }
        }

        public bool IsAlive
        {
            get
            {
                EnsurePresenter();
                return _presenter != null && _presenter.Model.IsAlive;
            }
        }

        public bool IsSceneInstance => gameObject.scene.IsValid();

        public bool TryGetHp(out float current, out float max)
        {
            EnsurePresenter();
            if (_presenter == null)
            {
                current = 0f;
                max = 0f;
                return false;
            }

            current = _presenter.Model.CurrentHp;
            max = _presenter.Model.MaxHp;
            return true;
        }

        void Awake()
        {
            _presenter = GetComponent<CharacterPresenter>();
            if (_presenter == null)
                _presenter = gameObject.AddComponent<CharacterPresenter>();
        }

        void EnsurePresenter()
        {
            if (_presenter != null)
                return;

            _presenter = GetComponent<CharacterPresenter>();
            if (_presenter == null && gameObject.scene.IsValid())
                _presenter = gameObject.AddComponent<CharacterPresenter>();
        }

        public void ConfigureAsEnemy(float maxHp, float moveSpeed)
        {
            EnsurePresenter();
            _presenter.ConfigureAsEnemy(maxHp, moveSpeed);
        }

        public void ConfigureMovement(float moveSpeed)
        {
            EnsurePresenter();
            _presenter.ConfigureMovement(moveSpeed);
        }

        public MoveResult ProcessMove(MoveIntent intent)
        {
            EnsurePresenter();
            return _presenter.ProcessMove(intent);
        }

        public DamageResult ProcessDamage(DamageRequest request)
        {
            EnsurePresenter();
            return _presenter.ProcessDamage(request);
        }

        public HealResult ProcessHeal(HealRequest request)
        {
            EnsurePresenter();
            return _presenter.ProcessHeal(request);
        }

        public void SetCurrentHp(float currentHp)
        {
            EnsurePresenter();
            _presenter.SetCurrentHp(currentHp);
        }

        public BuffApplyResult ApplyBuff(BuffApplyRequest request)
        {
            EnsurePresenter();
            return _presenter.ApplyBuff(request);
        }

        public void SetStatusCatalog(StatusCatalog catalog)
        {
            EnsurePresenter();
            _presenter.SetStatusCatalog(catalog);
        }
    }
}
