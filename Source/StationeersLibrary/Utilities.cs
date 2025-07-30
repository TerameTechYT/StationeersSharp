#region

using LaunchPadBooster.Utils;
using System.Diagnostics;
using System.Linq.Expressions;

#endregion

namespace StationeersLibrary;

public static class Utilities {
    private static readonly Dictionary<string, bool> _patches = [];

    /// <summary>
    /// Is a given mod Guid already loaded
    /// </summary>
    /// <param name="guid"></param>
    /// <returns>If mod's guid is loaded</returns>
    public static bool IsLoaded(string guid) =>
            IsHarmonyPatched(guid) ||
            IsModLoaded(guid) ||
            IsBoosterModLoaded(guid) ||
            IsBepInExModLoaded(guid);

    private static bool IsHarmonyPatched(string guid) => Harmony.HasAnyPatches(guid);

    private static bool IsModLoaded(string guid) => ModBase.AllMods.Find((modb) => modb is ModBase mod && mod.ModGuid == guid) != null;

    private static bool IsBoosterModLoaded(string guid) => LaunchPadBooster.Mod.AllMods.Find((mod) => mod.ID.Name == guid) != null;

    private static bool IsBepInExModLoaded(string guid) => Chainloader.PluginInfos.ContainsKey(guid);

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

    public static void ExceptionReporter(ModBase mod, ref Exception ex) {
        StackTrace stackTrace = new StackTrace(1);
        StackFrame stackFrame = stackTrace.GetFrame(0);
        MethodInfo method = (MethodInfo) stackFrame.GetMethod();
        Type type = method.GetType();

        if (mod == null || _patches.TryGetValue(type.FullName, out _)) {
            return;
        }

        _patches.TryAdd(type.FullName, true);
        mod.LogError($"Exception thrown! Please press {GetConsoleKeyCode()} and run 'slib report'!");
        mod.LogException(ex);
    }

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

    public static FieldInfo? Field<T>(Expression<Func<T>> expression) =>
            ReflectionUtils.Field<T>(expression);

    public static PropertyInfo? Property<T>(Expression<Func<T>> expression) =>
            (expression?.Body as MemberExpression)?.Member as PropertyInfo;
    public static MethodInfo? PropertyGetter<T>(Expression<Func<T>> expression) =>
            ReflectionUtils.PropertyGetter<T>(expression);
    public static MethodInfo? PropertySetter(Expression<Action> action) =>
            ReflectionUtils.PropertySetter(action);

    public static ConstructorInfo? Constructor<T>(Expression<Func<T>> expression) =>
            ReflectionUtils.Constructor<T>(expression);

    public static MethodInfo? Operator<T>(Expression<Func<T>> expression) =>
            ReflectionUtils.Operator<T>(expression);

    public static MethodInfo? Method<T>(Expression<Func<T>> expression) =>
            ReflectionUtils.Method<T>(expression);
    public static MethodInfo? Method(Expression<Action> action) =>
            ReflectionUtils.Method(action);
    public static MethodInfo? Method(MethodCallExpression call) =>
            ReflectionUtils.Method(call);

    public static MethodInfo? VirtualMethod(this MethodInfo method, Type type) =>
            ReflectionUtils.VirtualMethodIn(method, type);

    public static MethodInfo? AsyncMethod<T>(Expression<Func<T>> expression) =>
            ReflectionUtils.AsyncMethod<T>(expression);

    public static T? CreateDelegate<T>(this MethodInfo method) where T : Delegate =>
            ReflectionUtils.CreateDelegate<T>(method);

    // Direct

    public static MemberInfo[]? Member(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type?.GetMember(name, bindingFlags);

    public static FieldInfo? Field(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type?.GetField(name, bindingFlags);

    public static T? FieldGetValue<T>(this Type type, string name, object? obj = null, BindingFlags bindingFlags = BindingFlags.Default) =>
            (T?) Field(type, name, bindingFlags)?.GetValue(obj);

    public static void FieldGetValue<T>(this Type type, string name, T value, object? obj = null, BindingFlags bindingFlags = BindingFlags.Default) =>
            Field(type, name, bindingFlags)?.SetValue(obj, value);

    public static PropertyInfo? Property(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type?.GetProperty(name, bindingFlags);

    public static MethodInfo? PropertyGetter(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type.Property(name, bindingFlags)?.GetGetMethod(bindingFlags.HasFlag(BindingFlags.NonPublic));
    public static MethodInfo? PropertySetter(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type.Property(name, bindingFlags)?.GetSetMethod(bindingFlags.HasFlag(BindingFlags.NonPublic));

    public static object? PropertyGetValue(this Type type, string name, object? obj = null, object[]? index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
            type.Property(name, bindingFlags)?.GetValue(obj, index);
    public static T? PropertyGetValue<T>(this Type type, string name, object? obj = null, object[]? index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
            (T?) type.PropertyGetValue(name, obj, index, bindingFlags);

    public static void PropertySetValue(this Type type, string name, object value, object? obj = null, object[]? index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
            type.Property(name, bindingFlags)?.SetValue(obj, value, index);
    public static void PropertySetValue<T>(this Type type, string name, T value, object? obj = null, object[]? index = null, BindingFlags bindingFlags = BindingFlags.Default) =>
            type.Property(name, bindingFlags)?.SetValue(obj, value, index);

    public static MethodInfo? Method(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type?.GetMethod(name, bindingFlags);

    public const string ASYNC_METHOD = "MoveNext";
    public static MethodInfo? AsyncMethod(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) {
        AsyncStateMachineAttribute? attribute = Attribute<AsyncStateMachineAttribute>(type);
        InterfaceMapping? mapping = InterfaceMapping(attribute?.StateMachineType, typeof(AsyncStateMachineAttribute));
        MethodInfo? moveNext = Method(typeof(IAsyncStateMachine), ASYNC_METHOD, bindingFlags);

        foreach (MethodInfo method in mapping?.TargetMethods ?? []) {
            if (method == moveNext) {
                return method;
            }
        }

        throw new TypeLoadException("Could not find async method implementation");
    }

    public static ConstructorInfo? Constructor(this Type type, BindingFlags bindingFlags = BindingFlags.Default, Binder? binder = null, Type[]? types = null, ParameterModifier[]? modifiers = null) =>
            type?.GetConstructor(bindingFlags, binder, types, modifiers);

    public static EventInfo? Event(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Default) =>
            type?.GetEvent(name, bindingFlags);

    public static T? Attribute<T>(this Type type) where T : Attribute =>
            type?.GetCustomAttribute<T>();
    public static Attribute? Attribute(this Type type, Type attributeType) =>
            type?.GetCustomAttribute(attributeType);

    public static T? Attribute<T>(this FieldInfo type) where T : Attribute =>
            type?.GetCustomAttribute<T>();
    public static Attribute? Attribute(this FieldInfo type, Type attributeType) =>
            type?.GetCustomAttribute(attributeType);

    public static T? Attribute<T>(this MethodInfo type) where T : Attribute =>
            type?.GetCustomAttribute<T>();
    public static Attribute? Attribute(this MethodInfo type, Type attributeType) =>
            type?.GetCustomAttribute(attributeType);

    public static bool HasAttribute<T>(this Type type) where T : Attribute =>
            type?.GetCustomAttribute<T>() != null;
    public static bool HasAttribute(this Type type, Type attributeType) =>
            type?.GetCustomAttribute(attributeType) != null;

    public static bool HasAttribute<T>(this FieldInfo type) where T : Attribute =>
            type?.GetCustomAttribute<T>() != null;
    public static bool HasAttribute(this FieldInfo type, Type attributeType) =>
            type?.GetCustomAttribute(attributeType) != null;

    public static bool HasAttribute<T>(this MethodInfo type) where T : Attribute =>
            type?.GetCustomAttribute<T>() != null;
    public static bool HasAttribute(this MethodInfo type, Type attributeType) =>
            type?.GetCustomAttribute(attributeType) != null;

    public static Type? Interface(this Type type, string name) =>
            type?.GetInterface(name);
    public static InterfaceMapping? InterfaceMapping(this Type type, Type interfaceType) =>
            type?.GetInterfaceMap(interfaceType);
}