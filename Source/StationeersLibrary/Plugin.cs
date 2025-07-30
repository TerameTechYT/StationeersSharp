#region

#endregion

namespace StationeersLibrary;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => false;

    public override ModInfo Data => new ModInfo() {
        Name = "StationeersLibrary",
        Guid = "stationeerslibrary",
        Version = new Version(2, 0, 0, 191),
        WorkshopId = 3389894703ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() {}
}