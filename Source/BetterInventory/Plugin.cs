#region

#endregion

namespace BetterInventory;

public class Plugin : Mod {
    public static Plugin Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;
    protected override LaunchPadBooster.Mod InternalMod => new(this.ModGuid, this.ModVersionString);

    public override ModInfo Data => new ModInfo() {
        Name = "BetterInventory",
        Guid = "betterinventory",
        Version = new Version(1, 2, 0),
        WorkshopId = 0ul,
        GameType = GameType.Client,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnAwake() { }
}

internal struct Data {
    public static ControlsGroup ControlsGroup = new(Plugin.Instance.ModName);
    public static List<KeyItem> ControlKeys => [
    ];
}