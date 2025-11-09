#region


using Cysharp.Threading.Tasks;
using StationeersLibrary;
using StationeersLibrary.Args;
using StationeersLibrary.Modding;
using System.Reflection;
using ThingImport;
using UnityEngine;

#endregion

namespace BetterLoadingScreens;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool LogToStationeers => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterLoadingScreens",
        Guid = "betterloadingscreens",
        Version = new Version(1, 0, 0, 33),
        WorkshopId = 0ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnConfigLoad() {
        this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Enable Custom",
            $"Enable custom images in {Constants.STATIONEERS_DOCUMENTS_FOLDER}/loadingscreens"
        ));

        this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Enable Default",
            $"Enable default images that come with the mod"
        ));
    }

    public List<string> Files = [];
    public override void OnStart() {
        if (this.GetConfigValue<bool>("Configurables", "Enable Default")) {
            this.Files.AddRange(Directory.GetFiles(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Textures"), "*.*", SearchOption.AllDirectories));
        }

        string custom = Path.Combine(Constants.STATIONEERS_DOCUMENTS_FOLDER, "loadingscreens");
        if (Directory.Exists(custom) && this.GetConfigValue<bool>("Configurables", "Enable Custom")) {
            this.Files.AddRange(Directory.GetFiles(custom, "*.*", SearchOption.AllDirectories));
        }

        foreach (string file in this.Files) {
            TextureReference texture = new() {
                Path = file,
                MipMapped = false,
                Linear = false,
                Tiling = new Vector2Reference() {
                    x = 0.0f,
                    y = 0.0f
                },
                TextureFormat = TextureFormat.ARGB32,
                TextureLoadType = TextureLoadType.OnRequest,
            };

            PatchFunctions.Textures.Add(texture);
        }
    }
}