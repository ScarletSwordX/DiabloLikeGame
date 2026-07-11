using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Input
{
    /// <summary>
    /// 可选测试用 Input 适配：Action 回调 → GameplayInputBus。运行时由 PlayerInput + PlayerInputBridge 承担。
    /// </summary>
    public class GameInputReader : MonoBehaviour
    {
        [SerializeField] InputActionAsset _inputActions;

        InputActionMap _gameplayMap;
        InputAction _moveAction;
        readonly List<ButtonBinding> _buttonBindings = new List<ButtonBinding>();

        sealed class ButtonBinding
        {
            public InputAction Action;
            public string ActionName;
            public Action<InputAction.CallbackContext> OnPerformed;
        }

        void Awake() => BuildActions();

        void OnEnable() => EnableActions();

        void OnDisable() => DisableActions();

        void BuildActions()
        {
            ClearButtonBindings();
            if (_inputActions == null) return;

            _gameplayMap = _inputActions.FindActionMap(GameInputActions.MapGameplay, true);
            _moveAction = _gameplayMap.FindAction(GameInputActions.Move, true);

            foreach (var actionName in GameInputActions.ButtonActions)
            {
                var action = _gameplayMap.FindAction(actionName, true);
                _buttonBindings.Add(new ButtonBinding
                {
                    Action = action,
                    ActionName = actionName,
                    OnPerformed = _ => NotifyButton(actionName)
                });
            }
        }

        void EnableActions()
        {
            if (_gameplayMap == null) return;
            _gameplayMap.Enable();
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;

            foreach (var b in _buttonBindings)
                b.Action.performed += b.OnPerformed;
        }

        void DisableActions()
        {
            if (_gameplayMap == null) return;
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;

            foreach (var b in _buttonBindings)
                b.Action.performed -= b.OnPerformed;
            _gameplayMap.Disable();
        }

        void ClearButtonBindings() => _buttonBindings.Clear();

        void OnMove(InputAction.CallbackContext ctx) =>
            GameplayInputBus.NotifyMove(ctx.ReadValue<Vector2>());

        static void NotifyButton(string actionName)
        {
            switch (actionName)
            {
                case GameInputActions.Attack: GameplayInputBus.NotifyAttack(); break;
                case GameInputActions.Skill1: GameplayInputBus.NotifySkill1(); break;
                case GameInputActions.Skill2: GameplayInputBus.NotifySkill2(); break;
                case GameInputActions.Skill3: GameplayInputBus.NotifySkill3(); break;
                case GameInputActions.Item1: GameplayInputBus.NotifyItem1(); break;
                case GameInputActions.Item2: GameplayInputBus.NotifyItem2(); break;
                case GameInputActions.Item3: GameplayInputBus.NotifyItem3(); break;
            }
        }

        public void AssignAsset(InputActionAsset asset)
        {
            DisableActions();
            _inputActions = asset;
            BuildActions();
            EnableActions();
        }

        public InputActionAsset Asset => _inputActions;
    }
}
