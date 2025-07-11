#region

#endregion

namespace BetterCodeEditor;

public class Plugin : Mod {
    public static Plugin Instance {
        get; private set;
    }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;
    protected override LaunchPadBooster.Mod InternalMod => new(this.ModGuid, this.ModVersionString);

    public override ModInfo Data => new ModInfo() {
        Name = "BetterCodeEditor",
        Guid = "bettercodeeditor",
        Version = new Version(1, 2, 0),
        WorkshopId = 0ul,
        GameType = GameType.Client,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnLoadConfiguration() {
        ConfigData.codeEditorLines = Config.Bind("Configurables",
             "Code Editor Lines",
             InputSourceCode.MAX_LINES,
             "Number of lines in the code editor.");

        ConfigData.codeEditorLineLength = Config.Bind("Configurables",
                "Code Editor Line Length",
                InputSourceCode.LINE_LENGTH_LIMIT,
                "The length of the code editor lines");
    }

    public override void OnAwake() { }
}

internal struct ConfigData {
    public static ConfigEntry<int> codeEditorLines;
    public static int CodeEditorLines => codeEditorLines?.Value ?? InputSourceCode.MAX_LINES;

    public static ConfigEntry<int> codeEditorLineLength;
    public static int CodeEditorLineLength => codeEditorLineLength?.Value ?? InputSourceCode.LINE_LENGTH_LIMIT;

    public static int BytesPerLine => InputSourceCode.MAX_FILE_SIZE / InputSourceCode.MAX_LINES;
    public static int MaxFileSize => BytesPerLine * CodeEditorLines;
}