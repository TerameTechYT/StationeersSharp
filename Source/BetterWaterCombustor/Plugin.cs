#region

#endregion

namespace BetterWaterCombustor;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterWaterCombustor",
        Guid = "betterwatercombustor",
        Version = new Version(1, 4, 0, 224),
        WorkshopId = 3404201609ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() {}
}

internal struct ConfigData {

}