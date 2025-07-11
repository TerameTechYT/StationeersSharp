namespace StationeersLibrary.Mods;

public class ModInfo {
		public string Name { get; set; }
		public string Guid { get; set; }
		public Version Version { get; set; }
		public string VersionString => this.Version.ToString();
		public ulong WorkshopId { get; set; }

		public GameType GameType { get; set; }

		public ModInfo() { }

		public bool IsGameCompatible() => this.GameType switch {
				GameType.Both => true,
				GameType.Server => GameManager.IsBatchMode,
				GameType.Client => !GameManager.IsBatchMode,
				_ => false,
		};

		public bool Newer(ModInfo info) => this.Version > info?.Version;
		public bool Older(ModInfo info) => !this.Newer(info);
}