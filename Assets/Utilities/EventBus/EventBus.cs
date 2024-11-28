using System.Collections.Generic;

namespace Utilities.EventBus {
    public static class EventBus<T> where T : IEvent {
        private static readonly HashSet<IEventBinding<T>> Bindings = new ();
    
        public static void Register(EventBinding<T> binding) => Bindings.Add(binding);
        public static void Deregister(EventBinding<T> binding) => Bindings.Remove(binding);

        public static void Raise(T @event) {
            foreach (var binding in Bindings) {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }
        
        private static void Clear() {
            // Debug.Log($"Clearing {typeof(T).Name} bindings");
            Bindings.Clear();
        }
    }
}