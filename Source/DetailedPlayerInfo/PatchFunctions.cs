#region

using TMPro;

#endregion

namespace DetailedPlayerInfo;

[HarmonyPatch]
public static class PatchFunctions {


    [UsedImplicitly]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.UpdateFrameRate))]
    [HarmonyPrefix]
    public static bool WorldManagerUpdateFrameRate(ref TextMeshProUGUI ___FrameRate) {
        if (!ConfigData.CustomFramerate || ___FrameRate == null) {
            return true;
        }

        try {
            Functions.EnableFrameCounter(ref ___FrameRate);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), nameof(PlayerStateWindow.Awake))]
    [HarmonyPostfix]
    public static void PlayerStateWindowAwake(ref PlayerStateWindow __instance) {
        if (__instance == null) {
            return;
        }

        try {
            Functions.Initialize(ref __instance);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Update")]
    [HarmonyPostfix]
    public static void PlayerStateWindowUpdate(ref PlayerStateWindow __instance) {
        if (__instance == null) {
            return;
        }

        try {
            Functions.Update(ref __instance);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    public static bool PNNPatch() {
        return false;
    }

    /*[UsedImplicitly]
    [HarmonyPriority(Priority.Last)]
    [DoHarmonyPatch(typeof(AtmosAnalyser), "PrepareText")]
    [HarmonyPostfix]
    public static void AtmosAnalyserPrepareText(ref AtmosAnalyser __instance, ref bool ____isGasPipe, ref string ____pressureValueText, ref string ____liquidVolumeValueText, ref string ____capacityValueText, ref string ____temperatureValueText, ref string ____energyConvectedText, ref string ____energyRadiatedText, ref string ____latentText, ref string ____stressText) {
            if (__instance == null || __instance.ScannedAtmosphere == null) {
                    return;
            }

            try {
                    Functions.UpdateAnalyzer(ref __instance, ref ____isGasPipe, ref ____pressureValueText, ref ____liquidVolumeValueText, ref ____capacityValueText, ref ____temperatureValueText, ref ____energyConvectedText, ref ____energyRadiatedText, ref ____latentText, ref ____stressText);
            }
            catch (Exception ex) {
                    Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }
    }*/

    /*[UsedImplicitly]
    [DoHarmonyPatch(typeof(AtmosAnalyser), "SetHash")]
    [HarmonyPrefix]
    public static bool AtmosAnalyserSetHash(ref AtmosAnalyser __instance, Mole mole, Atmosphere atmos, string volumeTextColor, ref Dictionary<int, GasItem> ____moleDisplay) {
            if (__instance == null || atmos == null || ____moleDisplay == null) {
                    return true;
            }

            if (!____moleDisplay.TryGetValue((int) mole.Type, out GasItem item)) {
                    return true;
            }

            try {

                    Functions.UpdateMoleDisplays(ref __instance, ref mole, ref atmos, ref volumeTextColor, ref item);
            }
            catch (Exception ex) {
                    Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }

            return false;
    }*/

    /*[UsedImplicitly]
    [DoHarmonyPatch(typeof(AtmosphericsManager), nameof(AtmosphericsManager.DisplayBasicAtmosphere), [typeof(Atmosphere), typeof(StringBuilder), typeof(Pipe.ContentType)])]
    [HarmonyPrefix]
    public static bool AtmosphericsManagerDisplayBasicAtmosphere(ref Atmosphere atmosphere, ref StringBuilder stringBuilder, Pipe.ContentType contentType = Pipe.ContentType.All) {
            if (atmosphere == null || stringBuilder == null || contentType == null) {
                    return true;
            }

            try {
                    Functions.DisplayGasInfo(ref stringBuilder, ref contentType, ref atmosphere);
            }
            catch (Exception ex) {
                    Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }

            return false;
    }*/
}