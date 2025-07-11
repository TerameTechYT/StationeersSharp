#region

using System.Linq.Expressions;
using LaunchPadBooster.Utils;
using InternalMod = LaunchPadBooster.Mod;

#endregion

namespace StationeersLibrary;

public static class Utilities {
		public static bool IsLoaded(string guid) =>
				Harmony.HasAnyPatches(guid) ||
				InternalMod.AllMods.Find((mod) => mod.ID.Name == guid) != null ||
				Chainloader.PluginInfos.ContainsKey(guid);

		public static KeyCode GetConsoleKeyCode() => KeyManager.AllKeys.Find((key) => key.Name == "ToggleConsole").Key;

		public static Chemistry.GasType GetSpeciesAirType(SpeciesClass species) => species switch {
				SpeciesClass.Human => Chemistry.GasType.Oxygen,
				SpeciesClass.Zrilian => Chemistry.GasType.Volatiles,
				_ => Chemistry.GasType.Undefined,
		};

		public static string GetTemperatureSymbol(TemperatureUnit unit) => unit switch {
				TemperatureUnit.Fahrenheit => Constants.FAHRENHEIT_SYMBOL,
				TemperatureUnit.Kelvin => Constants.KELVIN_SYMBOL,
				_ => Constants.CELCIUS_SYMBOL,
		};

		public static TValue CatchAndReturnDefault<TValue, TException>(TValue fallbackValue, Func<TValue> action) where TException : Exception {
				if (action == null) {
						return fallbackValue;
				}

				try {
						return action.Invoke();
				}
				catch (TException) {
						return fallbackValue;
				}
		}

		public static string GetPressureSymbol(PressureUnit unit, bool pascal = false) => unit switch {
				PressureUnit.Bar => Constants.BAR_SYMBOL,
				PressureUnit.PSI => Constants.POUNDS_PER_SQUARE_INCH_SYMBOL,
				PressureUnit.Torr => Constants.TORR_SYMBOL,
				_ => pascal ? Constants.PASCAL_SYMBOL : Constants.KILOPASCAL_SYMBOL,
		};

		public static string GetVolumeSymbol(VolumeUnit unit) => unit switch {
				VolumeUnit.Liter => Constants.LITER_SYMBOL,
				_ => Constants.GALLON_SYMBOL,
		};

		public static string GetVelocitySymbol(VelocityUnit unit) => unit switch {
				VelocityUnit.Knot => Constants.KNOT_SYMBOL,
				VelocityUnit.Miles => Constants.FEET_PER_SECOND_SYMBOL,
				_ => Constants.METERS_PER_SECOND_SYMBOL,
		};

		public static string GetEnergySymbol(EnergyUnit unit) => unit switch {
				EnergyUnit.FootPound => Constants.FOOTPOUND_SYMBOL,
				EnergyUnit.Calorie => Constants.CALORIE_SYMBOL,
				_ => Constants.JOULE_SYMBOL,
		};
}

public static class ReflectionUtilities {
		// Expressive

		public static FieldInfo Field<T>(Expression<Func<T>> expression) =>
				ReflectionUtils.Field<T>(expression);

		public static PropertyInfo Property<T>(Expression<Func<T>> expression) =>
				(expression?.Body as MemberExpression)?.Member as PropertyInfo;
		public static MethodInfo PropertyGetter<T>(Expression<Func<T>> expression) =>
				ReflectionUtils.PropertyGetter<T>(expression);
		public static MethodInfo PropertySetter(Expression<Action> action) =>
				ReflectionUtils.PropertySetter(action);

		public static ConstructorInfo Constructor<T>(Expression<Func<T>> expression) =>
				ReflectionUtils.Constructor<T>(expression);

		public static MethodInfo Operator<T>(Expression<Func<T>> expression) =>
				ReflectionUtils.Operator<T>(expression);

		public static MethodInfo Method<T>(Expression<Func<T>> expression) =>
				ReflectionUtils.Method<T>(expression);
		public static MethodInfo Method(Expression<Action> action) =>
				ReflectionUtils.Method(action);
		public static MethodInfo Method(MethodCallExpression call) =>
				ReflectionUtils.Method(call);

		public static MethodInfo VirtualMethod(MethodInfo method, Type type) =>
				ReflectionUtils.VirtualMethodIn(method, type);

		public static MethodInfo AsyncMethod<T>(Expression<Func<T>> expression) =>
				ReflectionUtils.AsyncMethod<T>(expression);

		public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate =>
				ReflectionUtils.CreateDelegate<T>(method);

		// Direct

		public static MemberInfo[] Member(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				type?.GetMember(name, bindingFlags);

		public static FieldInfo Field(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				type?.GetField(name, bindingFlags);

		public static PropertyInfo Property(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				type?.GetProperty(name, bindingFlags);

		public static MethodInfo PropertyGetter(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				Property(type, name, bindingFlags)?.GetGetMethod(bindingFlags.HasFlag(BindingFlags.NonPublic));
		public static MethodInfo PropertySetter(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				Property(type, name, bindingFlags)?.GetSetMethod(bindingFlags.HasFlag(BindingFlags.NonPublic));
		
		public static object PropertyGetValue(Type type, string name, object obj = null, object[] index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
				Property(type, name, bindingFlags)?.GetValue(obj, index);
		public static T PropertyGetValue<T>(Type type, string name, object obj = null, object[] index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
				(T) PropertyGetValue(type, name, obj, index, bindingFlags);

		public static void PropertySetValue(Type type, string name, object value, object obj = null, object[] index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
				Property(type, name, bindingFlags)?.SetValue(obj, value, index);
		public static void PropertySetValue<T>(Type type, string name, T value, object obj = null, object[] index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
				Property(type, name, bindingFlags)?.SetValue(obj, value, index);

		public static MethodInfo Method(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				type?.GetMethod(name, bindingFlags);

		public const string ASYNC_METHOD = "MoveNext";
		public static MethodInfo AsyncMethod(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) {
				AsyncStateMachineAttribute attribute = Attribute<AsyncStateMachineAttribute>(type);
				InterfaceMapping? mapping = InterfaceMapping(attribute.StateMachineType, typeof(AsyncStateMachineAttribute));
				MethodInfo moveNext = Method(typeof(IAsyncStateMachine), ASYNC_METHOD, bindingFlags);

				foreach (MethodInfo method in mapping?.TargetMethods) {
						if (method == moveNext) {
								return method;
						}
				}

				throw new TypeLoadException("Could not find async method implementation");
		}

		public static ConstructorInfo Constructor(Type type, BindingFlags bindingFlags = BindingFlags.Default, Binder binder = null, Type[] types = null, ParameterModifier[] modifiers = null) =>
				type?.GetConstructor(bindingFlags, binder, types, modifiers);

		public static EventInfo Event(Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
				type?.GetEvent(name, bindingFlags);

		public static T Attribute<T>(Type type) where T : Attribute =>
				type?.GetCustomAttribute<T>();
		public static Attribute Attribute(Type type, Type attributeType) =>
				type?.GetCustomAttribute(attributeType);

		public static Type Interface(Type type, string name) =>
				type?.GetInterface(name);
		public static InterfaceMapping? InterfaceMapping(Type type, Type interfaceType) =>
				type?.GetInterfaceMap(interfaceType);

		// Typed Utils

		public static bool IsPrivate(Type type) => type?.IsNotPublic ?? false;
		public static bool IsInternal(Type type) => type?.IsVisible ?? false;
		public static bool IsProtected(FieldInfo type) => type?.IsFamily ?? false;
		public static bool IsProtected(MethodInfo type) => type?.IsFamily ?? false;
		public static bool IsPublic(Type type) => type?.IsPublic ?? false;

		public static bool IsAbstract(Type type) => type?.IsAbstract ?? false;
		public static bool IsSealed(Type type) => type?.IsSealed ?? false;
		public static bool IsStatic(MethodInfo type) => type?.IsStatic ?? false;
		public static bool IsStatic(FieldInfo type) => type?.IsStatic ?? false;

		public static bool IsClass(Type type) => type?.IsClass ?? false;
		public static bool IsSubclass(Type type, Type subClass) => type?.IsSubclassOf(subClass) ?? false;
		public static bool IsInterface(Type type) => type?.IsInterface ?? false;

		public static bool IsInstance(Type type, object obj) => type?.IsInstanceOfType(obj) ?? false;
		public static bool IsInstance<T>(Type type, T obj) => type?.IsInstanceOfType(obj) ?? false;
}