#region

#endregion

namespace StationeersLibrary;

public static class Utilities {
    public static bool IsLoaded(string guid) => Chainloader.PluginInfos.ContainsKey(guid) || Harmony.HasAnyPatches(guid);

    public static void SetModVersion(ulong handle, string version) {
        if (handle == 0) {
            return;
        }

        ModData mod = WorkshopMenu.ModsConfig.Mods.Find((mod) => mod.GetAboutData().WorkshopHandle == handle);
        if (mod == null) {
            return;
        }

        ModAbout aboutData = mod.GetAboutData();
        aboutData.Version = version;

        Traverse.Create(mod).Field("_modAboutData").SetValue(aboutData);
    }

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

    internal static TValue CatchAndReturnDefault<TValue, TException>(TValue fallbackValue, Func<TValue> action) where TException : Exception {
        if (action == null) {
            return fallbackValue;
        }

        try {
            return action();
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