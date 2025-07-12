#region

#endregion

namespace ColoredGases;

public class Plugin : Mod {
    public static Plugin? Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "ColoredGases",
        Guid = "coloredgases",
        Version = new Version(1, 4, 0, 51),
        WorkshopId = 3523162910ul,
        GameType = GameType.Client,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnLoadConfiguration() {
        ConfigData.enableAirVisualizer = Config.Bind(
                new ConfigDefinition("Configurables", "Enable Colored Air Visualier"),
                true,
                new ConfigDescription("Enable or disable custom colored air visualizers.")
        );

        ConfigData.enableFogVisualizer = Config.Bind(
                new ConfigDefinition("Configurables", "Enable Colored Fog Visualier"),
                true,
                new ConfigDescription("Enable or disable custom colored fog visualizers.")
        );
    }

    public override void Start() { }
}

internal struct ConfigData {
    //
    public static ConfigEntry<bool>? enableAirVisualizer;
    public static bool EnableAirVisualizer => enableAirVisualizer?.Value ?? false;

    //
    public static ConfigEntry<bool>? enableFogVisualizer;
    public static bool EnableFogVisualizer => enableFogVisualizer?.Value ?? false;
}