namespace BetterStartScreen;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyFinalizer]
    public static Exception PatchFinalizer(Exception __exception) {
        Plugin.LogException(__exception);

        // suppress all patch exceptions
        return null;
    }
}