#region

#endregion

namespace BetterInventory;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterInventory",
        Guid = "betterinventory",
        Version = new Version(1, 4, 0, 181),
        WorkshopId = 0ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() {}
}

internal struct Data {
    public static ControlsGroup ControlsGroup = new(Plugin.Instance.ModName);
    public static List<KeyItem> ControlKeys => [
    ];
}