#region

using StationeersLibrary.Enums;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion


namespace BetterFabricator;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterFabricator",
        Guid = "betterfabricator",
        Version = new Version(1, 3, 0, 345),
        WorkshopId = 0ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() { }
}

internal struct ConfigData {
    // Yes, this is purposefully empty, technically you could add some hardcoded recipes.
    public static List<WorldManager.RecipeData> FabricatorRecipes = [];
}