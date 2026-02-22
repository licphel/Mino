#region
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
#endregion

namespace Mino.Framework.Resource;

/// <summary>
///     An interface-backend locator and instantiator.
/// </summary>
public class ResourceFactory<S> {
	private ConcurrentDictionary<Type, (List<ConstructorInfo>, Action<S>?)>
		_ctorMap = new ConcurrentDictionary<Type, (List<ConstructorInfo>, Action<S>?)>();

	/// <summary>
	///     Registers an interface implementation type.
	/// </summary>
	/// <typeparam name="I">Interface type.</typeparam>
	/// <typeparam name="B">Backend type.</typeparam>
	/// <exception cref="Error"></exception>
	public void RegisterInterface<I, B>(Action<S>? postprocessor) where B : S {
		ConstructorInfo[] ctorList = typeof(B).GetConstructors();
		var validCtorList = new List<ConstructorInfo>();

		foreach (ConstructorInfo ctor in ctorList) {
			if (ctor.GetCustomAttribute<ResourceCreation>() != null) {
				validCtorList.Add(ctor);
			}
		}

		if (validCtorList.Count == 0) {
			throw new Error("[Target] ctor not found");
		}
		_ctorMap[typeof(I)] = (validCtorList, postprocessor);
	}

	/// <summary>
	///     Creates a resource interface.
	/// </summary>
	/// <param name="args">Ctor args.</param>
	/// <typeparam name="I">Interface type.</typeparam>
	/// <returns>A backend-powered interface.</returns>
	/// <exception cref="Error">Thrown if not registered or args do not match.</exception>
	public I Create<I>(params object[] args) {
		if (_ctorMap.TryGetValue(typeof(I), out (List<ConstructorInfo>, Action<S>?) value)) {
			List<ConstructorInfo> validCtorList = value.Item1;

			if (validCtorList == null) {
				throw new Error("no such interface backend");
			}

			object? created = null;

			// Optimization for single ctor: do not check type matching.
			if (validCtorList.Count == 1) {
				created = validCtorList[0].Invoke(args);
			} else {
				foreach (ConstructorInfo ctor in validCtorList) {
					if (checkCtorMatch(ctor, args)) {
						created = ctor.Invoke(args);
					}
				}
			}

			if (created != null) {
				value.Item2?.Invoke((S) created);
				return (I) created;
			}
		}
		throw new Error("no matching ctor");
	}

	private static bool checkCtorMatch(ConstructorInfo ctor, object[] args) {
		ParameterInfo[] constructorParams = ctor.GetParameters();

		if (constructorParams.Length != args.Length) {
			return false;
		}

		for (int i = 0; i < constructorParams.Length; i++) {
			Type paramType = constructorParams[i].ParameterType;
			object argument = args[i];

			if (argument == null) {
				if (paramType.IsValueType && Nullable.GetUnderlyingType(paramType) == null) {
					return false;
				}
				continue;
			}

			TypeConverter converter = TypeDescriptor.GetConverter(argument.GetType());
			if (!converter.CanConvertTo(paramType) && !paramType.IsInstanceOfType(argument)) {
				return false;
			}
		}

		return true;
	}
}
