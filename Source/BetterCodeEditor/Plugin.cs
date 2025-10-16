#region

using Assets.Scripts.UI;
using BepInEx.Configuration;
using StationeersLibrary.Enums;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion

namespace BetterCodeEditor;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance {
        get; private set;
    }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterCodeEditor",
        Guid = "bettercodeeditor",
        Version = new Version(1, 4, 0, 407),
        WorkshopId = 0ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() { }

    public override void OnConfigLoad() {
        ConfigData.codeEditorLines = this.RegisterConfig(new ConfigData<int>(
             InputSourceCode.MAX_LINES,
             "Configurables", "Code Editor Lines",
             "Number of lines in the code editor."
        ));

        ConfigData.codeEditorLineLength = this.RegisterConfig(new ConfigData<int>(
             InputSourceCode.LINE_LENGTH_LIMIT,
             "Configurables", "Code Editor Line Length",
             "The length of the code editor lines."
        ));
    }

}

internal struct ConfigData {
    public static ConfigEntry<int>? codeEditorLines;
    public static int CodeEditorLines => codeEditorLines?.Value ?? InputSourceCode.MAX_LINES;

    public static ConfigEntry<int>? codeEditorLineLength;
    public static int CodeEditorLineLength => codeEditorLineLength?.Value ?? InputSourceCode.LINE_LENGTH_LIMIT;

    public static int BytesPerLine => InputSourceCode.MAX_FILE_SIZE / InputSourceCode.MAX_LINES;
    public static int MaxFileSize => BytesPerLine * CodeEditorLines;
}