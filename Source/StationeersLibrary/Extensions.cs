#region

using Assets.Scripts.Atmospherics;
using HarmonyLib;
using StationeersLaunchPad;
using StationeersLibrary.Enums;
using StationeersLibrary.Modding;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

#endregion

namespace StationeersLibrary;

public static class HarmonyExtensions {
    public static List<ConditionalPatchClassProcessor> CreateConditionalClassProcessors(this Harmony harmony, Assembly? assembly = null) {
        List<ConditionalPatchClassProcessor> processors = [];

        foreach (Type type in AccessTools.GetTypesFromAssembly(assembly ?? Assembly.GetCallingAssembly())) {
            bool cancel = false;
            foreach (HarmonyPatchCondition condition in type.GetCustomAttributes(true).OfType<HarmonyPatchCondition>()) {
                if (!condition.ShouldPatch) {
                    cancel = true;
                    break;
                }
            }

            if (cancel) {
                continue;
            }

            processors.Add(new ConditionalPatchClassProcessor(harmony, type));
        }

        return processors;
    }

    public static Dictionary<LoadedAssembly, List<ConditionalPatchClassProcessor>> CreatePatchersForAssemblies(this Harmony harmony, IEnumerable<LoadedAssembly> assemblies) {
        Dictionary<LoadedAssembly, List<ConditionalPatchClassProcessor>> processors = [];

        foreach (LoadedAssembly assembly in assemblies) {
            processors.TryAdd(assembly, harmony.CreateConditionalClassProcessors(assembly.Assembly));
        }

        return processors;
    }
}

public static class EnumExtensions {
    public static bool IsDefinedByDefault<TEnum>(this TEnum enumValue) where TEnum : Enum {
        return !EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache? manager) || !manager.Keys.Contains(enumValue);
    }
}

public static class ReflectionExtensions {
    public static string FullName(this Assembly method) => method.GetName().FullName;
    public static string FullName(this MethodInfo method) => method.GetType().FullName;

    public static string FullDescription(this MethodInfo method) {
        if (method == null) {
            return "null";
        }

        StringBuilder stringBuilder = new();
        if (method.IsAssembly) {
            stringBuilder.Append("internal ");
        } else {
            if (method.IsPublic) {
                stringBuilder.Append("public ");
            } else {
                if (method.IsFamily) {
                    stringBuilder.Append("protected ");
                } else {
                    stringBuilder.Append("private ");
                }
            }
        }

        if (method.IsStatic) {
            stringBuilder.Append("static ");
        }

        if (method.IsAbstract) {
            stringBuilder.Append("abstract ");
        }

        if (method.IsVirtual) {
            stringBuilder.Append("virtual ");
        }

        if (method.ReturnType != null) {
            stringBuilder.Append(method.ReturnType.FullDescription() + " ");
        }

        if (method.DeclaringType != null) {
            stringBuilder.Append(method.DeclaringType.FullDescription() + "::");
        }

        if (method.IsConstructor) {
            stringBuilder.Append($".cctor{method.FormatParameters()}\n");
        } else {
            stringBuilder.Append($"{method.Name}{method.FormatParameters()}\n");
        }

        return stringBuilder.ToString();
    }

    public static string FormatParameters(this MethodInfo method) =>
        method.GetParameters().FormatParameters();

    public static string FormatParameters(this ParameterInfo[] parameters) =>
        $"({(
            parameters?.Length == 0
            ? ""
            : $"\n{parameters?.Join((type) => $"\t{type?.FullDescription() ?? "null"} {type?.Name ?? "null"}", $",\n")}\n"
        )})";

    public static string FullDescription(this ParameterInfo parameter) =>
        parameter?.ParameterType?.FullDescription() ?? "null";
}

public static class StringExtensions {
    /// <summary>
    /// Convert a double to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this double value, string unit = "", string color = "") => value.ToStringPrefix(unit, color);

    /// <summary>
    /// Convert a float to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this float value, string unit = "", string color = "") => value.ToStringPrefix(unit, color);

    /// <summary>
    /// Convert a PressurekPa to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this PressurekPa value, string unit = "", string color = "") => value.ToFloat().ToStringPrefix(unit, color);

    /// <summary>
    /// Convert a TemperatureKelvin to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this TemperatureKelvin value, string unit = "", string color = "") => value.ToFloat().ToStringPrefix(unit, color);

    /// <summary>
    /// Convert a VolumeLitres to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this VolumeLitres value, string unit = "", string color = "") => value.ToFloat().ToStringPrefix(unit, color);

    /// <summary>
    /// Convert a MoleQuantity to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this MoleQuantity value, string unit = "", string color = "") => value.ToFloat().ToStringPrefix(unit, color);

    /// <summary>
    /// Convert a MoleEnergy to a prefixed string
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="unit">Unit string (optional)</param>
    /// <param name="color">Color name (optional)</param>
    /// <returns></returns>
    public static string ToStringPrefix(this MoleEnergy value, string unit = "", string color = "") => value.ToFloat().ToStringPrefix(unit, color);

    public static string ToStringSuffix(this int value, string word, string singular = "", string plural = "") => $"{value} {word}{(value == 0 ? plural : value == 1 ? singular : plural)}";
}

public static class CodeInstructionExtensions {
    /// <summary>
    /// Returns true of the CodeInstructions opcode is equal to the given opcode
    /// </summary>
    /// <param name="instruction">Instruction</param>
    /// <param name="opcode">Oppcode</param>
    /// <returns></returns>
    public static bool OpcodeIs(this CodeInstruction instruction, OpCode opcode) => instruction?.opcode == opcode;
}

public static class UnitExtensions {
    public static bool IsKelvinNil(this float value) => value <= Constants.MINIMUM_KELVIN;
    public static bool IsKelvinNil(this TemperatureKelvin value) => value.ToFloat().IsKelvinNil();

    public static bool IsCelciusNil(this float value) => value <= Constants.MINIMUM_CELCIUS;
    public static bool IsCelciusNil(this TemperatureKelvin value) => value.ToFloat().IsCelciusNil();

    public static float CelciusToKelvin(this float celcius) => celcius + Constants.ZERO_CELCIUS_KELVIN;
    public static float CelciusToFahrenheit(this float celcius) => (celcius * 1.8f) + 32f;

    public static float KelvinToCelcius(this float kelvin) => kelvin - Constants.ZERO_CELCIUS_KELVIN;
    public static float KelvinToCelcius(this TemperatureKelvin kelvin) => kelvin.ToFloat().KelvinToCelcius();
    public static float KelvinToFahrenheit(this float kelvin) => (kelvin.KelvinToCelcius() * 1.8f) + 32f;
    public static float KelvinToFahrenheit(this TemperatureKelvin kelvin) => kelvin.ToFloat().KelvinToFahrenheit();

    public static float FahrenheitToCelcius(this float fahrenheit) => (fahrenheit - 32f) * 1.8f;
    public static float FahrenheitToKelvin(this float fahrenheit) => ((fahrenheit - 32f) * 1.8f).CelciusToFahrenheit();

    public static float MicroPascalToMilliPascal(this float uPa) => uPa * 1000f;
    public static float MicroPascalToPascal(this float uPa) => uPa.MicroPascalToMilliPascal() * 1000f;
    public static float MicroPascalToKiloPascal(this float uPa) => uPa.MicroPascalToPascal() * 1000f;
    public static float MicroPascalToMegaPascal(this float uPa) => uPa.MicroPascalToKiloPascal() * 1000f;
    public static float MicroPascalToGigaPascal(this float uPa) => uPa.MicroPascalToMegaPascal() * 1000f;

    public static float MilliPascalToMicroPascal(this float mPa) => mPa / 1000f;
    public static float MilliPascalToPascal(this float mPa) => mPa * 1000f;
    public static float MilliPascalToKiloPascal(this float mPa) => mPa.MilliPascalToPascal() * 1000f;
    public static float MilliPascalToMegaPascal(this float mPa) => mPa.MilliPascalToKiloPascal() * 1000f;
    public static float MilliPascalToGigaPascal(this float mPa) => mPa.MilliPascalToGigaPascal() * 1000f;

    public static float PascalToMicoPascal(this float Pa) => Pa.PascalToMilliPascal() * 1000f;
    public static float PascalToMilliPascal(this float Pa) => Pa * 1000f;
    public static float PascalToKiloPascal(this float Pa) => Pa / 1000f;
    public static float PascalToMegaPascal(this float Pa) => Pa.PascalToKiloPascal() / 1000f;
    public static float PascalToGigaPascal(this float Pa) => Pa.PascalToMegaPascal() / 1000f;

    public static float KiloPascalToMicroPascal(this float kPa) => kPa.KiloPascalToMilliPascal() * 1000f;
    public static float KiloPascalToMicroPascal(this PressurekPa kPa) => kPa.ToFloat().KiloPascalToMilliPascal() * 1000f;
    public static float KiloPascalToMilliPascal(this float kPa) => kPa.KiloPascalToPascal() * 1000f;
    public static float KiloPascalToMilliPascal(this PressurekPa kPa) => kPa.ToFloat().KiloPascalToPascal() * 1000f;
    public static float KiloPascalToPascal(this float kPa) => kPa * 1000f;
    public static float KiloPascalToPascal(this PressurekPa kPa) => kPa.ToFloat() * 1000f;
    public static float KiloPascalToMegaPascal(this float kPa) => kPa / 1000f;
    public static float KiloPascalToMegaPascal(this PressurekPa kPa) => kPa.ToFloat() / 1000f;
    public static float KiloPascalToGigaPascal(this float kPa) => kPa.KiloPascalToMegaPascal() / 1000f;
    public static float KiloPascalToGigaPascal(this PressurekPa kPa) => kPa.ToFloat().KiloPascalToMegaPascal() / 1000f;

    public static float MegaPascalToMicroPascal(this float MPa) => MPa.MegaPascalToMilliPascal() / 1000f;
    public static float MegaPascalToMilliPascal(this float MPa) => MPa.MegaPascalToPascal() / 1000f;
    public static float MegaPascalToPascal(this float MPa) => MPa.MegaPascalToKiloPascal() / 1000f;
    public static float MegaPascalToKiloPascal(this float MPa) => MPa / 1000f;
    public static float MegaPascalToGigaPascal(this float MPa) => MPa * 1000f;

    public static float GigaPascalToMicroPascal(this float GPa) => GPa.GigaPascalToMilliPascal() / 1000f;
    public static float GigaPascalToMilliPascal(this float GPa) => GPa.GigaPascalToPascal() / 1000f;
    public static float GigaPascalToPascal(this float GPa) => GPa.GigaPascalToKiloPascal() / 1000f;
    public static float GigaPascalToKiloPascal(this float GPa) => GPa.GigaPascalToMegaPascal() / 1000f;
    public static float GigaPascalToMegaPascal(this float GPa) => GPa / 1000f;

    public static float MicroPascalToPsi(this float uPa) => uPa.MicroPascalToKiloPascal() / 6.89476f;
    public static float MilliPascalToPsi(this float mPa) => mPa.MilliPascalToKiloPascal() / 6.89476f;
    public static float PascalToPsi(this float Pa) => Pa.PascalToKiloPascal() / 6.89476f;
    public static float KiloPascalToPsi(this float kPa) => kPa / 6.89476f;
    public static float KiloPascalToPsi(this PressurekPa kPa) => kPa.ToFloat() / 6.89476f;
    public static float MegaPascalToPsi(this float MPa) => MPa.MegaPascalToKiloPascal() / 6.89476f;
    public static float GigaPascalToPsi(this float GPa) => GPa.GigaPascalToKiloPascal() / 6.89476f;

    public static float PsiToMicroPascal(this float Psi) => Psi.PsiToMilliPascal() * 1000;
    public static float PsiToMilliPascal(this float Psi) => Psi.PsiToPascal() * 1000;
    public static float PsiToPascal(this float Psi) => Psi.PsiToKiloPascal() * 1000;
    public static float PsiToKiloPascal(this float Psi) => Psi * 6.89476f;
    public static float PsiToMegaPascal(this float Psi) => Psi.PsiToKiloPascal() / 1000;
    public static float PsiToGigaPascal(this float Psi) => Psi.PsiToMegaPascal() / 1000;

    public static float BarToMicroPascal(this float Bar) => Bar.BarToMilliPascal() * 100f;
    public static float BarToMilliPascal(this float Bar) => Bar.BarToPascal() * 100f;
    public static float BarToPascal(this float Bar) => Bar.BarToKiloPascal() * 100f;
    public static float BarToKiloPascal(this float Bar) => Bar * 100f;
    public static float BarToMegaPascal(this float Bar) => Bar / 10f;
    public static float BarToGigaPascal(this float Bar) => Bar.BarToMegaPascal() / 1000f;

    public static float MicroPascalToBar(this float uPa) => uPa.MicroPascalToKiloPascal() / 100f;
    public static float MilliPascalToBar(this float mPa) => mPa.MilliPascalToKiloPascal() / 100f;
    public static float PascalToBar(this float Pa) => Pa.PascalToKiloPascal() / 100f;
    public static float KiloPascalToBar(this float kPa) => kPa / 100f;
    public static float KiloPascalToBar(this PressurekPa kPa) => kPa.ToFloat().KiloPascalToBar();
    public static float MegaPascalToBar(this float MPa) => MPa.MegaPascalToKiloPascal() / 100f;
    public static float GigaPascalToBar(this float GPa) => GPa.GigaPascalToKiloPascal() / 100f;

    public static float LiterToImperialGallon(this float liter) => liter / 4.546f;
    public static float LiterToUSGallon(this float liter) => liter * 1.057f;
    public static float LiterToCubicInch(this float liter) => liter * 61.024f;

    public static float LiterToImperialGallon(this VolumeLitres liter) => liter.ToFloat().LiterToImperialGallon();
    public static float LiterToUSGallon(this VolumeLitres liter) => liter.ToFloat().LiterToUSGallon();
    public static float LiterToCubicInch(this VolumeLitres liter) => liter.ToFloat().LiterToCubicInch();

    public static float PartialMoles(this Atmosphere atmosphere, Chemistry.GasType gasType) => atmosphere.TotalMoles.ToFloat() * atmosphere.GetGasTypeRatio(gasType);

    public static float ToPreferredUnit(this float value, TemperatureUnit unit) => unit switch {
        TemperatureUnit.Celcius => value.KelvinToCelcius(),
        TemperatureUnit.Fahrenheit => value.KelvinToFahrenheit(),
        _ => value,
    };

    public static float ToPreferredUnit(this TemperatureKelvin value, TemperatureUnit unit) => unit switch {
        TemperatureUnit.Celcius => value.KelvinToCelcius(),
        TemperatureUnit.Fahrenheit => value.KelvinToFahrenheit(),
        _ => value.ToFloat(),
    };

    public static float ToPreferredUnit(this float value, PressureUnit unit) => unit switch {
        PressureUnit.PSI => value.KiloPascalToPsi(),
        PressureUnit.Bar => value.KiloPascalToBar(),
        _ => value,
    };

    public static float ToPreferredUnit(this PressurekPa value, PressureUnit unit) => unit switch {
        PressureUnit.PSI => value.KiloPascalToPsi(),
        PressureUnit.Bar => value.KiloPascalToBar(),
        _ => value.ToFloat(),
    };

    public static float ToPreferredUnit(this float value, VolumeUnit unit) => unit switch {
        VolumeUnit.ImperialGallon => value.LiterToImperialGallon(),
        VolumeUnit.USGallon => value.LiterToUSGallon(),
        _ => value,
    };

    public static float ToPreferredUnit(this VolumeLitres value, VolumeUnit unit) => unit switch {
        VolumeUnit.ImperialGallon => value.LiterToImperialGallon(),
        VolumeUnit.USGallon => value.LiterToUSGallon(),
        _ => value.ToFloat(),
    };
}
