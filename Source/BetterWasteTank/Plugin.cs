#region

#endregion

namespace BetterWasteTank;

public class Plugin : Mod, IModSingleton<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterWasteTank",
        Guid = "betterwastetank",
        Version = new Version(1, 8, 0, 172),
        WorkshopId = 3071913936ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() { }

    public override void OnConfigLoad() {
        ConfigData.wasteCriticalRatio = this.RegisterConfig(new ConfigData<float>(
            0.975f,
            "Configurables", "Waste Critical Ratio",
            "Ratio when \"Waste Tank Critical!\" alarm goes off.",
            new AcceptableValueRange<float>(0.0f, 1.0f)
        ));

        ConfigData.wasteCautionRatio = this.RegisterConfig(new ConfigData<float>(
            0.75f,
            "Configurables", "Waste Caution Ratio",
            "Ratio when \"Waste Tank Caution!\" alarm goes off.",
            new AcceptableValueRange<float>(0.0f, 1.0f)
        ));

        ConfigData.airCountOnlyBreathable = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Air Count Only Breathable",
            "Should Air Tank warnings count moles of only breathable gas or total moles"
        ));

        ConfigData.airCriticalMoles = this.RegisterConfig(new ConfigData<float>(
            7.5f,
            "Configurables", "Air Critical Moles",
            "Quantity of moles when \"Air Tank Critical!\" alarm goes off. (this number will be multiplied by how many moles a human breaths per tick)"
        ));

        ConfigData.airCautionMoles = this.RegisterConfig(new ConfigData<float>(
            50f,
            "Configurables", "Air Caution Moles",
            "Quanitity of moles when \"Air Tank Caution\" alarm goes off. (this number will be multiplied by how many moles a human breaths per tick)"
        ));
    }
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