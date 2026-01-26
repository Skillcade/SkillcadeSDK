using System;
using System.Collections.Generic;

namespace SkillcadeSDK.Events
{
    /// <summary>
    /// Event Bus for publishing and subscribing to game events.
    /// Provides loose coupling between SDK and game-specific code.
    /// </summary>
    public class GameEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
        private readonly List<Delegate> _iterationCache = new();

        /// <summary>
        /// Subscribe to an event of type TEvent.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="handler">The handler to invoke when the event is published.</param>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            var eventType = typeof(TEvent);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            _subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event of type TEvent.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The event type to publish.</typeparam>
        /// <param name="event">The event instance to publish.</param>
        public void Publish<TEvent>(TEvent @event) where TEvent : IGameEvent
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                _iterationCache.Clear();
                _iterationCache.AddRange(handlers);
                foreach (var handler in _iterationCache)
                {
                    (handler as Action<TEvent>)?.Invoke(@event);
                }
            }
        }
    }
}
