using Gameplay.Character.Player.Presenter;
using Gameplay.Character.View;
using UnityEngine;

namespace Gameplay.Character.Player.View
{
    /// <summary>
    /// 挂在单次动作态：进入时重置，播完（time）后切到序列化的下一状态。
    /// 经 <see cref="ICharacterAnimationView.PlayAnimation"/> 统一切换；退出时同步 <see cref="PlayerModel"/> 回 Idle。
    /// </summary>
    public class OnFinish : StateMachineBehaviour
    {
        [SerializeField] string _nextStateName = "Walk";
        [SerializeField] float _crossFadeDuration = 0.2f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var view = animator.GetComponentInParent<ICharacterAnimationView>();
            if (view != null)
                view.PlayAnimation(_nextStateName, _crossFadeDuration, stateInfo.length);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            NotifyPresenterIdle(animator);
        }

        static void NotifyPresenterIdle(Animator animator)
        {
            var presenter = animator.GetComponentInParent<PlayerPresenter>();
            presenter?.NotifyActionAnimationFinished();
        }
    }
}
