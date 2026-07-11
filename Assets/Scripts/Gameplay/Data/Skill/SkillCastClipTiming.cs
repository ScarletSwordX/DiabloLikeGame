using UnityEngine;

namespace Gameplay.Data
{
    /// <summary>
    /// 从 CastClip 读取 Event 时刻，并按技能类型与 PreCast/PostCast 目标时长计算 Animator 播放速度。
    /// </summary>
    public static class SkillCastClipTiming
    {
        public const string LaunchProjectileEventName = "OnAnimationLaunchProjectile";

        public struct ResolvedTiming
        {
            public bool Valid;
            public SkillKind Kind;
            public float ClipLength;
            public float LaunchTime;
            public float PreTargetSeconds;
            public float PostTargetSeconds;
            public float PrePlaybackSpeed;
            public float PostPlaybackSpeed;
            public float TotalWallClockSeconds;

            public bool UsesPostSegment => PostTargetSeconds > 0f;

            public bool UsesSpeedScaling =>
                Valid && (Mathf.Abs(PrePlaybackSpeed - 1f) > 0.001f
                          || (UsesPostSegment && Mathf.Abs(PostPlaybackSpeed - 1f) > 0.001f));
        }

        public static ResolvedTiming Resolve(SkillData skill)
        {
            var result = new ResolvedTiming();
            if (skill == null)
                return result;

            result.Kind = skill.Kind;

            var clip = skill.CastClip;
            if (clip == null)
                return result;

            result.ClipLength = clip.length;
            if (result.ClipLength <= 0f)
                return result;

            switch (skill.Kind)
            {
                case SkillKind.Projectile:
                    ResolveProjectileTiming(skill, clip, ref result);
                    break;
                case SkillKind.StatusApply:
                    ResolveStatusApplyTiming(skill, clip, ref result);
                    break;
                case SkillKind.Channeled:
                    ResolveChanneledPreTiming(skill, clip, ref result);
                    break;
            }

            return result;
        }

        static void ResolveProjectileTiming(SkillData skill, AnimationClip clip, ref ResolvedTiming result)
        {
            result.LaunchTime = FindLaunchEventTime(clip);
            if (result.LaunchTime < 0f)
                return;

            var preClipDuration = Mathf.Clamp(result.LaunchTime, 0f, result.ClipLength);
            var postClipDuration = Mathf.Max(0f, result.ClipLength - preClipDuration);

            result.PreTargetSeconds = skill.EffectivePreCastSeconds > 0f
                ? skill.EffectivePreCastSeconds
                : preClipDuration;
            result.PostTargetSeconds = skill.EffectivePostCastSeconds > 0f
                ? skill.EffectivePostCastSeconds
                : postClipDuration;

            result.PrePlaybackSpeed = ComputeSpeed(preClipDuration, result.PreTargetSeconds);
            result.PostPlaybackSpeed = ComputeSpeed(postClipDuration, result.PostTargetSeconds);
            result.TotalWallClockSeconds = result.PreTargetSeconds + result.PostTargetSeconds;
            result.Valid = true;
        }

        static void ResolveStatusApplyTiming(SkillData skill, AnimationClip clip, ref ResolvedTiming result)
        {
            var preClipDuration = result.ClipLength;
            result.LaunchTime = preClipDuration;
            result.PreTargetSeconds = skill.EffectivePreCastSeconds > 0f
                ? skill.EffectivePreCastSeconds
                : preClipDuration;
            result.PostTargetSeconds = 0f;
            result.PrePlaybackSpeed = ComputeSpeed(preClipDuration, result.PreTargetSeconds);
            result.PostPlaybackSpeed = 1f;
            result.TotalWallClockSeconds = result.PreTargetSeconds;
            result.Valid = true;
        }

        static void ResolveChanneledPreTiming(SkillData skill, AnimationClip clip, ref ResolvedTiming result)
        {
            var launchTime = FindLaunchEventTime(clip);
            var preClipDuration = launchTime >= 0f
                ? Mathf.Clamp(launchTime, 0f, result.ClipLength)
                : result.ClipLength;

            result.LaunchTime = preClipDuration;
            result.PreTargetSeconds = skill.EffectivePreCastSeconds > 0f
                ? skill.EffectivePreCastSeconds
                : preClipDuration;
            result.PostTargetSeconds = skill.EffectivePostCastSeconds;
            result.PrePlaybackSpeed = ComputeSpeed(preClipDuration, result.PreTargetSeconds);
            result.PostPlaybackSpeed = 1f;
            result.TotalWallClockSeconds = result.PreTargetSeconds;
            result.Valid = true;
        }

        static float ComputeSpeed(float clipSegmentSeconds, float targetSeconds)
        {
            return targetSeconds > 0.0001f ? clipSegmentSeconds / targetSeconds : 1f;
        }

        public static float FindLaunchEventTime(AnimationClip clip)
        {
            if (clip == null)
                return -1f;

            var events = clip.events;
            if (events == null || events.Length == 0)
                return -1f;

            for (var i = 0; i < events.Length; i++)
            {
                var evt = events[i];
                if (evt != null && evt.functionName == LaunchProjectileEventName)
                    return evt.time;
            }

            return -1f;
        }
    }
}
