#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterFabricator;

public class Plugin : Mod {
    public static Plugin Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterFabricator",
        Guid = "betterfabricator",
        Version = new Version(1, 1, 0),
        WorkshopId = 0ul,
        GameType = GameType.Both,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnAwake() { }
}


internal struct ConfigData {
    // Yes, this is purposefully empty, technically you could add some hardcoded recipes.
    public static List<WorldManager.RecipeData> FabricatorRecipes = [];
}