#region

#endregion

namespace BetterStackSize;

[HarmonyPatch]
public static class PatchFunctions {
    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll)), HarmonyPriority(Priority.Last), HarmonyPrefix]
    public static void PrefabLoadAll() {
        try {
            foreach (Thing prefab in WorldManager.Instance.SourcePrefabs) {
                Plugin.Instance.ProcessThing(prefab);
            }
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }
}