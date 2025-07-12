#region

#endregion

namespace BetterWasteTank;

public class Plugin : Mod {
    public static Plugin? Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "BetterWasteTank",
        Guid = "betterwastetank",
        Version = new Version(1, 8, 0, 64),
        WorkshopId = 3071913936ul,
        GameType = GameType.Both,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnLoadConfiguration() {
        ConfigData.wasteCriticalRatio = Config.Bind(new ConfigDefinition("Configurables", "Waste Critical Ratio"),
                0.975f,
                new ConfigDescription("Ratio when \"Waste Tank Critical!\" alarm goes off.", new AcceptableValueRange<float>(0.0f, 1.0f)));

        ConfigData.wasteCautionRatio = Config.Bind(new ConfigDefinition("Configurables", "Waste Caution Ratio"),
                0.75f,
                new ConfigDescription("Ratio when \"Waste Tank Caution\" alarm goes off.", new AcceptableValueRange<float>(0.0f, 1.0f)));

        ConfigData.airCountOnlyBreathable = Config.Bind(new ConfigDefinition("Configurables", "Air Count Only Breathable"),
                true,
                new ConfigDescription("Should Air Tank warnings count moles of only breathable gas or total moles"));

        ConfigData.airCriticalMoles = Config.Bind(new ConfigDefinition("Configurables", "Air Critical Moles"),
                7.5f,
                new ConfigDescription("Quantity of moles when \"Air Tank Critical!\" alarm goes off. (this number will be multiplied by how many moles a human breaths per tick)"));

        ConfigData.airCautionMoles = Config.Bind(new ConfigDefinition("Configurables", "Air Caution Moles"),
                50f,
                new ConfigDescription("Quanitity of moles when \"Air Tank Caution\" alarm goes off. (this number will be multiplied by how many moles a human breaths per tick)"));
    }

    public override void Start() { }
}

internal struct ConfigData {
    //
    public static ConfigEntry<float>? wasteCriticalRatio;
    public static float WasteCriticalRatio => wasteCriticalRatio?.Value ?? 0.75f;

    //
    public static ConfigEntry<float>? wasteCautionRatio;
    public static float WasteCautionRatio => wasteCautionRatio?.Value ?? 0.975f;

    //
    public static ConfigEntry<bool>? airCountOnlyBreathable;
    public static bool AirCountOnlyBreathable => airCountOnlyBreathable?.Value ?? false;

    //
    public static ConfigEntry<float>? airCautionMoles;
    public static float AirCautionMoles => airCautionMoles?.Value ?? 7.5f;

    //
    public static ConfigEntry<float>? airCriticalMoles;
    public static float AirCriticalMoles => airCriticalMoles?.Value ?? 50f;

    //
    internal static float AirTankMolesCritical => Human.MolesPerMinute.ToFloat() * AirCriticalMoles;
    internal static float AirTankMolesCaution => Human.MolesPerMinute.ToFloat() * AirCautionMoles;
}