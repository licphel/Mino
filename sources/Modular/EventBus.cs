#region
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
#endregion

namespace Mino.Modular;

/// <summary>
///		High speed event bus.
/// </summary>
public sealed class EventBus {
	/// <summary>
	///		A default global event bus instance.
	/// </summary>
	public static readonly EventBus Instance = new EventBus();
	
	private readonly ConcurrentDictionary<Type, SortedList<int, List<EventHandler>>> _handlers =
		new ConcurrentDictionary<Type, SortedList<int, List<EventHandler>>>();
	private readonly ConcurrentDictionary<Type, List<EventHandler>?> _handlerCache =
		new ConcurrentDictionary<Type, List<EventHandler>?>();
	private static readonly ConcurrentDictionary<Assembly, bool> _scannedAssemblies =
		new ConcurrentDictionary<Assembly, bool>();
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    ///		Registers a event.
    /// </summary>
    /// <param name="fn">Function delegate.</param>
    /// <param name="priority">Event priority.</param>
    /// <param name="receiveCanceled">Whether to receive canceled event.</param>
	public void Register<T>(EventFn<T> fn, EventPriority priority = EventPriority.Normal, bool receiveCanceled = false)
		where T : Event {
		regInternal(typeof(T), new EventHandlerWrapper<T>(fn, priority, receiveCanceled));
	}

    /// <summary>
    ///     Scans all assemblies and finds out subscribers.
    /// </summary>
    public void ScanSubscribers() {
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			if (!_scannedAssemblies.TryAdd(assembly, true)) {
				return;
			}

			Type[] types = assembly.GetTypes();
			foreach (Type type in types) {
				MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

				foreach (MethodInfo method in methods) {
					SubscribeEventAttribute? attr = method.GetCustomAttribute<SubscribeEventAttribute>();
					if (attr == null) {
						continue;
					}

					ParameterInfo[] parameters = method.GetParameters();
					if (parameters.Length != 1) {
						continue;
					}

					Type eventType = parameters[0].ParameterType;
					if (!typeof(Event).IsAssignableFrom(eventType)) {
						continue;
					}

					EventHandler handler = mkDelegate(method, eventType);
					regInternal(eventType, handler);
				}
			}
		}
	}

    /// <summary>
    ///     Posts the event synchronously.
    /// </summary>
    /// <param name="event">Event to post.</param>
    /// <returns>True if the event is not canceled.</returns>
    public bool Post(Event @event) {
		Type type = @event.GetType();
		List<EventHandler>? handlers = getHandles(type);

		if (handlers == null) {
			return true;
		}

		foreach (EventHandler handler in handlers) {
			handler.Invoke(@event);
		}
		
		return !@event.Canceled;
	}

    /// <summary>
    ///     Posts the event asynchronously.
    /// </summary>
    /// <param name="event">Event to post.</param>
    /// <returns>A task.</returns>
    public Task PostAsync(Event @event) {
		return Task.Run(() => Post(@event));
	}

    /// <summary>
    ///     Clears all handlers.
    /// </summary>
    public void Clear() {
		_lock.EnterWriteLock();
		try {
			_handlers.Clear();
			_handlerCache.Clear();
		} finally {
			_lock.ExitWriteLock();
		}
	}

	private void regInternal(Type eventType, EventHandler handler) {
		_lock.EnterWriteLock();
		try {
			SortedList<int, List<EventHandler>> priorities = _handlers.GetOrAdd(
				eventType, _ => new SortedList<int, List<EventHandler>>());

			int priorityKey = (int) handler.Priority;
			if (!priorities.TryGetValue(priorityKey, out List<EventHandler>? list)) {
				list = new List<EventHandler>();
				priorities[priorityKey] = list;
			}

			list.Add(handler);
			_handlerCache.TryRemove(eventType, out _);
		} finally {
			_lock.ExitWriteLock();
		}
	}

	private List<EventHandler>? getHandles(Type eventType) {
		if (_handlerCache.TryGetValue(eventType, out List<EventHandler>? cached)) {
			return cached;
		}

		_lock.EnterReadLock();
		try {
			var handlers = new List<EventHandler>();

			Type? currentType = eventType;
			while (currentType != null && currentType != typeof(object)) {
				if (_handlers.TryGetValue(currentType, out SortedList<int, List<EventHandler>>? priorities)) {
					foreach (List<EventHandler> list in priorities.Values) {
						handlers.AddRange(list);
					}
				}
				currentType = currentType.BaseType;
			}

			handlers.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			_handlerCache[eventType] = handlers;
			return handlers;
		} finally {
			_lock.ExitReadLock();
		}
	}

	private static EventHandler mkDelegate(MethodInfo method, Type eventType) {
		EventPriority priority = method.GetCustomAttribute<SubscribeEventAttribute>()!.Priority;
		bool receiveCanceled = method.GetCustomAttribute<SubscribeEventAttribute>()!.ReceiveCanceled;

		DynamicMethod dynamicMethod = new DynamicMethod(
			$"dynamic_{method.Name}",
			typeof(void),
			new[] { eventType },
			method.DeclaringType!,
			true
		);

		ILGenerator il = dynamicMethod.GetILGenerator();
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Call, method);
		il.Emit(OpCodes.Ret);

		Type delegateType = typeof(EventFn<>).MakeGenericType(eventType);
		Delegate handlerDelegate = dynamicMethod.CreateDelegate(delegateType);

		Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
		ConstructorInfo constructor = wrapperType.GetConstructors()[0];

		return (EventHandler) constructor.Invoke(new object[] { handlerDelegate, priority, receiveCanceled });
	}

	// A simple event handler wrapper.
	internal class EventHandlerWrapper<T> : EventHandler where T : Event {
		private readonly EventFn<T> _fn;
		private readonly bool _receiveCanceled;

		public Type EventType {
			get => typeof(T);
		}

		public EventPriority Priority { get; }

		public EventHandlerWrapper(EventFn<T> fn, EventPriority priority, bool receiveCanceled) {
			_fn = fn;
			Priority = priority;
			_receiveCanceled = receiveCanceled;
		}
		
		public void Invoke(Event @event) {
			if (!_receiveCanceled && @event.Canceled) {
				return;
			}
			_fn((T) @event);
		}
	}
}
