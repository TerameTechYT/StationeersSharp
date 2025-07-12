#region

#endregion

namespace BetterInventory;

public class Plugin : Mod {
    public static Plugin? Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "BetterInventory",
        Guid = "betterinventory",
        Version = new Version(1, 4, 0, 17),
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
















