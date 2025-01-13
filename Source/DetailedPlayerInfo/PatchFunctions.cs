#region

using TMPro;

#endregion

namespace DetailedPlayerInfo;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
        typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(WorldManager), "UpdateFrameRate")]
    [HarmonyPrefix]
    public static bool WorldManagerUpdateFrameRate(ref TextMeshProUGUI ___FrameRate) {
        try {
            return Data.CustomFramerate && Functions.EnableFrameCounter(ref ___FrameRate);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Awake")]
    [HarmonyPostfix]
    public static void PlayerStateWindowAwake(PlayerStateWindow __instance) {
        try {
            Functions.Initialize();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Update")]
    [HarmonyPostfix]
    public static void PlayerStateWindowUpdate(ref PlayerStateWindow __instance) {
        try {
            Functions.Update(ref __instance);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }
}