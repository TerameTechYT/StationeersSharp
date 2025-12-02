#region

using Assets.Scripts.UI;
using StationeersLibrary;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion

namespace StationpediaCalculator;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "StationpediaCalculator",
        Guid = "stationpediacalculator",
        Version = new Version(1, 5, 0, 495),
        WorkshopId = 3305312105ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() { }
}

internal struct ConfigData {
    public static SPDAListItem? CalculatorItem;
}