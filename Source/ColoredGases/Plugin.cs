#region

using BepInEx.Configuration;
using StationeersLibrary;
using StationeersLibrary.Modding;

#endregion

namespace ColoredGases;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "ColoredGases",
        Guid = "coloredgases",
        Version = new Version(1, 4, 0, 564),
        WorkshopId = 3523162910ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnStart() { }

    public override void OnConfigLoad() {
        ConfigData.enableAirVisualizer = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Enable Colored Air Visualier",
            "Enable or disable custom colored air visualizers."
        ));

        ConfigData.enableFogVisualizer = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Enable Colored Fog Visualier",
            "Enable or disable custom colored fog visualizers."
        ));
    }

}

internal struct ConfigData {
    //
    public static ConfigEntry<bool>? enableAirVisualizer;
    public static bool EnableAirVisualizer => enableAirVisualizer?.Value ?? false;

    //
    public static ConfigEntry<bool>? enableFogVisualizer;
    public static bool EnableFogVisualizer => enableFogVisualizer?.Value ?? false;
}