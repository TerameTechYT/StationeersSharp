#region

using StationeersLibrary.Enums;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion

namespace Template;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "Template",
        Guid = "template",
        Version = new Version(1, 0, 0, 0),
        WorkshopId = 0ul,
        Incompatibilities = [
            new ModIncompatibilityInfo() {
                Name = "TemplateIncompatibility",
                Guid = "templateincompatibility",
                Version = new Version(1, 0, 0, 0),
                WorkshopId = 123456789ul,
            },
        ],
        Dependencies = [
            new ModDependencyInfo() {
                Name = "StationeersLibrary",
                Guid = "stationeerslibrary",
                Version = new Version(2, 0, 0, 0),
                WorkshopId = 3389894703ul,
                DependencyType = DependencyType.Hard,
            }
        ],
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() { }
}