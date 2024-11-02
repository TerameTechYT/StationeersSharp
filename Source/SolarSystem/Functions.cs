namespace SolarSystem;

public static class Functions {
    public static void ReorderPlanetList() {
        foreach (var worldName in Data.WorldOrder) {
            var item = NewWorldMenu.Instance.WorldPresetItems.Find(worldItem => worldItem.WorldSetting.Id == worldName);
            var index = Data.WorldOrder.FindIndex(name => name == worldName);

            if (item != null && index != -1) item.transform.SetSiblingIndex(index);
        }
    }
}