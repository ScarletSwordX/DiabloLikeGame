using Gameplay.Character.Player.Presenter;
using Gameplay.Data;
using Gameplay.Item;
using Gameplay.Skill;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Character
{
    [RequireComponent(typeof(CharacterEntity))]
    [RequireComponent(typeof(PlayerPresenter))]
    public class PlayerController : MonoBehaviour
    {
        PlayerPresenter _presenter;

        public CharacterEntity Entity => GetComponent<CharacterEntity>();
        public int EntityId => _presenter.EntityId;

        void Awake() => EnsurePresenter();

        void EnsurePresenter()
        {
            if (_presenter == null)
                _presenter = GetComponent<PlayerPresenter>();
        }

        public void Initialize(
            PlayerMovementConfig config,
            SkillSystem skillSystem,
            SkillData[] skillDefinitions,
            ItemSystem itemSystem = null,
            ItemDefinition[] hotbarItems = null,
            InputActionAsset inputActions = null)
        {
            EnsurePresenter();
            if (_presenter == null)
            {
                Debug.LogError("PlayerController: 缺少 PlayerPresenter。");
                return;
            }

            if (inputActions != null)
            {
                var playerInput = GetComponent<PlayerInput>();
                if (playerInput != null)
                    playerInput.actions = inputActions;
            }

            _presenter.Initialize(config, skillSystem, skillDefinitions, itemSystem, hotbarItems);
        }
    }
}
