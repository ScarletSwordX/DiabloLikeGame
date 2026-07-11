using System;
using System.Collections.Generic;

namespace Gameplay.EventBus
{
    public sealed class GameEventBus
    {
        static GameEventBus _instance;
        public static GameEventBus Instance => _instance ??= new GameEventBus();

        readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Publish<T>(T gameEvent) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var del) && del is Action<T> action)
                action.Invoke(gameEvent);
        }

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var t = typeof(T);
            if (_handlers.TryGetValue(t, out var existing))
                _handlers[t] = Delegate.Combine(existing, handler);
            else
                _handlers[t] = handler;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var t = typeof(T);
            if (_handlers.TryGetValue(t, out var existing))
                _handlers[t] = Delegate.Remove(existing, handler);
        }

        public static void ResetForTests()
        {
            _instance = new GameEventBus();
        }
    }
}
