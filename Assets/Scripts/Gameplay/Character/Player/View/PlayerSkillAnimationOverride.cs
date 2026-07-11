using Gameplay.Bootstrap;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay.Character.Player.View
{
    /// <summary>
    /// 固定 Skill1/2/3 状态，通过 AnimatorOverrideController 按槽位替换 CastClip。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSkillAnimationOverride : MonoBehaviour
    {
        [SerializeField] Animator _animator;
        [SerializeField] RuntimeAnimatorController _baseController;
        [SerializeField] AnimationClip _skill1BaseClip;
        [SerializeField] AnimationClip _skill2BaseClip;
        [SerializeField] AnimationClip _skill3BaseClip;

        AnimatorOverrideController _overrideController;
        AnimationClip[] _baseClips;

        void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            _baseClips = new[] { _skill1BaseClip, _skill2BaseClip, _skill3BaseClip };
            EnsureOverrideController();
            ResolveMissingBaseClips();
        }

        public void ApplySkillSlots(SkillData[] skills)
        {
            if (_animator == null || _overrideController == null)
                return;

            var count = GameplaySessionConfig.SkillSlotCount;
            for (var i = 0; i < count; i++)
            {
                var baseClip = i < _baseClips.Length ? _baseClips[i] : null;
                if (baseClip == null)
                    continue;

                var skill = skills != null && i < skills.Length ? skills[i] : null;
                var castClip = skill != null ? skill.CastClip : null;
                _overrideController[baseClip] = castClip != null ? castClip : baseClip;
            }
        }

        void EnsureOverrideController()
        {
            if (_animator == null)
                return;

            if (_baseController == null)
                _baseController = UnwrapController(_animator.runtimeAnimatorController);

            if (_baseController == null)
            {
                Debug.LogWarning("PlayerSkillAnimationOverride: 缺少 Base AnimatorController。");
                return;
            }

            if (_animator.runtimeAnimatorController is AnimatorOverrideController existing)
            {
                _overrideController = existing;
                if (_overrideController.runtimeAnimatorController == null)
                    _overrideController.runtimeAnimatorController = _baseController;
                return;
            }

            _overrideController = new AnimatorOverrideController(_baseController);
            _animator.runtimeAnimatorController = _overrideController;
        }

        void ResolveMissingBaseClips()
        {
            if (_baseController == null)
                return;

            var all = _baseController.animationClips;
            for (var i = 0; i < _baseClips.Length; i++)
            {
                if (_baseClips[i] != null)
                    continue;

                var stateName = i switch
                {
                    0 => PlayerView.AnimatorContract.StateSkill1,
                    1 => PlayerView.AnimatorContract.StateSkill2,
                    2 => PlayerView.AnimatorContract.StateSkill3,
                    _ => null
                };

                if (stateName == null)
                    continue;

                _baseClips[i] = FindClipByName(all, stateName);
            }
        }

        static RuntimeAnimatorController UnwrapController(RuntimeAnimatorController controller)
        {
            if (controller == null)
                return null;
            if (controller is AnimatorOverrideController overrideController)
                return overrideController.runtimeAnimatorController;
            return controller;
        }

        static AnimationClip FindClipByName(AnimationClip[] clips, string stateName)
        {
            if (clips == null)
                return null;

            foreach (var clip in clips)
            {
                if (clip != null && clip.name.IndexOf(stateName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return clip;
            }

            return null;
        }
    }
}
