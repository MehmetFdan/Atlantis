using System;
using System.Collections.Generic;
using UnityEngine;

namespace Atlantis.Events 
{
    [CreateAssetMenu(fileName = "EventBus", menuName = "Game/EventBus")]
    public class EventBus : ScriptableObject, IEventBus
    {
        private readonly Dictionary<Type, List<object>> subscribers = new Dictionary<Type, List<object>>();

        private void OnEnable()
        {
            subscribers.Clear();
        }

        public void Subscribe<T>(Action<T> callback) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<object>();
            }

            if (!subscribers[eventType].Contains(callback))
            {
                subscribers[eventType].Add(callback);
            }
        }

        public void Unsubscribe<T>(Action<T> callback) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                return;
            }

            if (subscribers[eventType].Contains(callback))
            {
                subscribers[eventType].Remove(callback);
            }
        }

        public void Publish<T>(T eventData) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                return;
            }

            var subscribersCopy = new List<object>(subscribers[eventType]);

            foreach (var subscriber in subscribersCopy)
            {
                try
                {
                    var callback = subscriber as Action<T>;
                    callback?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EventBus error during publish of {typeof(T).Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
