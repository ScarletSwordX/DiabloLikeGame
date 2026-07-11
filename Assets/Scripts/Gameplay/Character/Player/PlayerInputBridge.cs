using Gameplay.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Character.Player
{
    /// <summary>
    /// PlayerInput（Send Messages）入口：方法名须与 Action 名一致（OnMove / OnAttack / …）。
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBridge : MonoBehaviour
    {
        int _lastAttackFrame = -1;
        int _lastSkill1Frame = -1;
        int _lastSkill2Frame = -1;
        int _lastSkill3Frame = -1;
        int _lastItem1Frame = -1;
        int _lastItem2Frame = -1;
        int _lastItem3Frame = -1;

        public void OnMove(InputValue value)
        {
            GameplayInputBus.NotifyMove(value.Get<Vector2>());
        }

        public void OnAttack(InputValue value)
        {
            if (!value.isPressed)
            {
                GameplayInputLog.BridgeIgnored(GameInputActions.Attack, "released");
                return;
            }

            if (!TryConsumeButton(ref _lastAttackFrame))
            {
                GameplayInputLog.BridgeIgnored(GameInputActions.Attack, "duplicate same frame");
                return;
            }

            GameplayInputLog.Bridge(GameInputActions.Attack, "pressed");
            GameplayInputBus.NotifyAttack();
        }

        public void OnSkill1(InputValue value) => HandleButton(GameInputActions.Skill1, value, ref _lastSkill1Frame, GameplayInputBus.NotifySkill1);

        public void OnSkill2(InputValue value) => HandleButton(GameInputActions.Skill2, value, ref _lastSkill2Frame, GameplayInputBus.NotifySkill2);

        public void OnSkill3(InputValue value) => HandleButton(GameInputActions.Skill3, value, ref _lastSkill3Frame, GameplayInputBus.NotifySkill3);

        public void OnItem1(InputValue value) => HandleButton(GameInputActions.Item1, value, ref _lastItem1Frame, GameplayInputBus.NotifyItem1);

        public void OnItem2(InputValue value) => HandleButton(GameInputActions.Item2, value, ref _lastItem2Frame, GameplayInputBus.NotifyItem2);

        public void OnItem3(InputValue value) => HandleButton(GameInputActions.Item3, value, ref _lastItem3Frame, GameplayInputBus.NotifyItem3);

        void HandleButton(string actionName, InputValue value, ref int lastFrame, System.Action notify)
        {
            if (!value.isPressed)
            {
                GameplayInputLog.BridgeIgnored(actionName, "released");
                return;
            }

            if (!TryConsumeButton(ref lastFrame))
            {
                GameplayInputLog.BridgeIgnored(actionName, "duplicate same frame");
                return;
            }

            GameplayInputLog.Bridge(actionName, "pressed");
            notify();
        }

        static bool TryConsumeButton(ref int lastFrame)
        {
            var frame = Time.frameCount;
            if (lastFrame == frame) return false;
            lastFrame = frame;
            return true;
        }
    }
}
