using System;

namespace Events
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> callback) where T : IEvent;
        void Unsubscribe<T>(Action<T> callback) where T : IEvent;
        void Publish<T>(T eventData) where T : IEvent;
    }
} 