#region

using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using SimpleSpritePacker;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace StationeersLibrary.Patches;

[HarmonyPatch]
public static class SettingsPatcher {
    public static SettingType CheckboxType => SettingType.AutoSave;
    public static SettingType InputType => SettingType.MaxAutoSaves;
    public static SettingType SliderType => SettingType.SaveDelay;
    public static SettingType DropdownType => SettingType.OverallQuality;

    public static List<SettingItem> SettingItems => [.. Settings.Instance.Pages.SelectMany((page) => page.GetComponentsInChildren<SettingItem>(true))];

    public static SettingItem CheckboxItem { get => field ??= SettingsPatcher.SettingItems.Where((item) => item.SettingType == SettingsPatcher.CheckboxType).FirstOrDefault(); } = null;
    public static SettingItem InputItem { get => field ??= SettingsPatcher.SettingItems.Where((item) => item.SettingType == SettingsPatcher.InputType).FirstOrDefault(); } = null;
    public static SettingItem SliderItem { get => field ??= SettingsPatcher.SettingItems.Where((item) => item.SettingType == SettingsPatcher.SliderType).FirstOrDefault(); } = null;
    public static SettingItem DropdownItem { get => field ??= SettingsPatcher.SettingItems.Where((item) => item.SettingType == SettingsPatcher.DropdownType).FirstOrDefault(); } = null;

    [HarmonyPatch(typeof(Settings), "PopulateSettingItems"), HarmonyPrefix]
    public static void SettingsPopulateSettingItemsPrefix() {

    }
}
