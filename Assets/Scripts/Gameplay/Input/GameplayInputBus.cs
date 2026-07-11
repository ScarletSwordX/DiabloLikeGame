using System;
using UnityEngine;

namespace Gameplay.Input
{
    /// <summary>
    /// 玩法输入事件总线：与 Gameplay Action 一一对应，由 PlayerInputBridge 写入，PlayerPresenter 订阅。
    /// </summary>
    public static class GameplayInputBus
    {
        public static event Action<Vector2> Move;
        public static event Action Attack;
        public static event Action Skill1;
        public static event Action Skill2;
        public static event Action Skill3;
        public static event Action Item1;
        public static event Action Item2;
        public static event Action Item3;

        public static void NotifyMove(Vector2 value)
        {
            if (GameplayInputLog.LogMoveChanges)
                GameplayInputLog.Bus(GameInputActions.Move, FormatMove(value));
            Move?.Invoke(value);
        }

        public static void NotifyAttack()
        {
            GameplayInputLog.Bus(GameInputActions.Attack, SubscriberCount(Attack));
            Attack?.Invoke();
        }

        public static void NotifySkill1()
        {
            GameplayInputLog.Bus(GameInputActions.Skill1, SubscriberCount(Skill1));
            Skill1?.Invoke();
        }

        public static void NotifySkill2()
        {
            GameplayInputLog.Bus(GameInputActions.Skill2, SubscriberCount(Skill2));
            Skill2?.Invoke();
        }

        public static void NotifySkill3()
        {
            GameplayInputLog.Bus(GameInputActions.Skill3, SubscriberCount(Skill3));
            Skill3?.Invoke();
        }

        public static void NotifyItem1()
        {
            GameplayInputLog.Bus(GameInputActions.Item1, SubscriberCount(Item1));
            Item1?.Invoke();
        }

        public static void NotifyItem2()
        {
            GameplayInputLog.Bus(GameInputActions.Item2, SubscriberCount(Item2));
            Item2?.Invoke();
        }

        public static void NotifyItem3()
        {
            GameplayInputLog.Bus(GameInputActions.Item3, SubscriberCount(Item3));
            Item3?.Invoke();
        }

        public static void ResetForTests()
        {
            Move = null;
            Attack = null;
            Skill1 = null;
            Skill2 = null;
            Skill3 = null;
            Item1 = null;
            Item2 = null;
            Item3 = null;
        }

        static string FormatMove(Vector2 value) =>
            $"({value.x:0.##}, {value.y:0.##}) subs={Move?.GetInvocationList().Length ?? 0}";

        static string SubscriberCount(Action action) =>
            $"subs={action?.GetInvocationList().Length ?? 0}";
    }
}
