using Assets.Scripts.UI;

namespace SolarSystem;

public static class Functions
{
    public static void ReorderPlanetList()
    {
        foreach (string worldName in Data.WorldOrder)
        {
            WorldPresetItem item = NewWorldMenu.Instance.WorldPresetItems.Find((worldItem) => worldItem.WorldSetting.Id == worldName);
            int index = Data.WorldOrder.FindIndex((name) => name == worldName);

            if (item != null && index != -1)
            {
                item.transform.SetSiblingIndex(index);
            }
        }
    }
}