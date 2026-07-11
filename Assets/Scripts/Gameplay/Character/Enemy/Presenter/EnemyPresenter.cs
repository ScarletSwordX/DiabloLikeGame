using Gameplay.Character.Enemy.Model;
using Gameplay.Character.Enemy.View;
using Gameplay.Character.Presenter;
using Gameplay.Character.View;
using UnityEngine;

namespace Gameplay.Character.Enemy.Presenter
{
    [RequireComponent(typeof(CharacterPresenter))]
    [RequireComponent(typeof(EnemyView))]
    public class EnemyPresenter : MonoBehaviour
    {
        readonly EnemyModel _model = new EnemyModel();
        EnemyView _view;
        CharacterPresenter _character;

        [SerializeField] float _maxHp = 100f;
        [SerializeField] float _moveSpeed;

        public EnemyModel Model => _model;

        void Awake()
        {
            _view = GetComponent<EnemyView>();
            _character = GetComponent<CharacterPresenter>();
            _model.MaxHp = _maxHp;
            _model.MoveSpeed = _moveSpeed;
            _view.Bind(_model, GetComponent<CharacterView>());
            _character.ConfigureAsEnemy(_maxHp, _moveSpeed);
        }
    }
}
