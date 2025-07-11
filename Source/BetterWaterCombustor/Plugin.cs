#region

using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterWaterCombustor;

public class Plugin : Mod {
		public static Plugin Instance { get; private set; }

		public override bool UseConfig => true;
		public override bool UseHarmony => true;

		public override ModInfo Data => new ModInfo() {
				Name = "BetterWaterCombustor",
				Guid = "betterwatercombustor",
				Version = new Version(1, 2, 0),
				WorkshopId = 3404201609ul,
				GameType = GameType.Both,
		};

		public Plugin() => Plugin.Instance = this;

		public override void OnAwake() { }
}

internal struct ConfigData {

}