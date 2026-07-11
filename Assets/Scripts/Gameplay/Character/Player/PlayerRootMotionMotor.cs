using UnityEngine;

namespace Gameplay.Character.Player
{
    /// <summary>
    /// Animator Root Motion → CharacterController.Move（碰撞由 CC 处理）。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRootMotionMotor : MonoBehaviour
    {
        Animator _animator;
        CharacterController _controller;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
        }

        void OnAnimatorMove()
        {
            if (_animator == null || _controller == null) return;

            var delta = _animator.deltaPosition;
            delta.y = 0f;
            _controller.Move(delta);
            transform.rotation *= _animator.deltaRotation;
        }
    }
}
