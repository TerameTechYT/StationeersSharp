#region

#endregion


namespace Template;

public class Plugin : Mod {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "Template",
        Guid = "template",
        Version = new Version(1, 0, 0, 0),
        WorkshopId = 0ul,
        GameType = GameType.Both,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnStart() {}
}