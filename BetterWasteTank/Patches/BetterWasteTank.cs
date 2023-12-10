using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BetterWasteTank.Patches;

[HarmonyPatch]
public static class BetterWasteTank
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCritical))]
    [HarmonyPostfix]
    public static void StatusUpdatesIsWasteCritical(ref bool __result, ref Suit ____suit)
    {
        __result = ____suit != null && ____suit.WasteTank != null &&
                   ____suit.WasteTank.Pressure >= ____suit.WasteMaxPressure * 0.975f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCaution))]
    [HarmonyPostfix]
    public static void StatusUpdatesIsWasteCaution(ref bool __result, ref Suit ____suit)
    {
        __result = ____suit != null && ____suit.WasteTank != null &&
                   ____suit.WasteTank.Pressure >= ____suit.WasteMaxPressure * 0.75f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), "HandleIconUpdates")]
    [HarmonyPostfix]
    public static void StatusUpdatesHandleIconUpdates(ref TMP_Text ___TextWaste, ref Human ____human)
    {
        if (____human.Suit != null && ____human.Suit.WasteTank != null && ___TextWaste != null)
        {
            var percent = Mathf.RoundToInt(____human.Suit && ____human.Suit.WasteTank
                ? ____human.Suit.WasteTank.Pressure / ____human.Suit.WasteMaxPressure * 100f
                : 0f) + "%";

            ___TextWaste.text = percent;
        }

        if (____human.Suit != null && ____human.Suit.WasteTank != null)
            ____human.Suit.WasteMaxPressure = ____human.Suit.WasteTank.MaxPressure - 101f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InternalAtmosphereConditioner), "AirConditioning")]
    [HarmonyPrefix]
    public static bool InternalAtmosphereConditionerAirConditioning(InternalAtmosphereConditioner __instance,
        ref float __result, Atmosphere selectedAtmosphere)
    {
        if (__instance.Thing is not Suit) return true;
        var traverse = Traverse.Create(__instance);

        var _desiredEnergyDelta = Utilities.GetFieldValue<float>(traverse, "_desiredEnergyDelta");
        var _desiredEnergy = Utilities.GetFieldValue<float>(traverse, "_desiredEnergy");
        var _usedEnergy = Utilities.GetFieldValue<float>(traverse, "_usedEnergy");

        if (!__instance.OnOff || __instance.Battery == null ||
            __instance.WasteTank == null || __instance.Battery.IsEmpty)
        {
            __result = 0.0f;
            return false;
        }

        if (__instance.WasteTank.IsOpen)
            OnServer.Interact(__instance.WasteTank.InteractOpen, 0);
        if (__instance.WasteTank.InternalAtmosphere.PressureGassesAndLiquids >= __instance.WasteMaxPressure)
        {
            __result = 0.0f;
            return false;
        }

        _desiredEnergy = __instance.OutputTemperature * selectedAtmosphere.GasMixture.HeatCapacity;
        _desiredEnergyDelta = _desiredEnergy - selectedAtmosphere.GasMixture.TotalEnergy;
        _usedEnergy = Mathf.Abs(Mathf.Clamp(_desiredEnergyDelta, -__instance.MaxEnergy * __instance.Efficiency,
            __instance.MaxEnergy * __instance.Efficiency));
        if (_desiredEnergyDelta < 0.0) // cooling suit
        {
            __instance.Battery.PowerStored -= _usedEnergy * 0.01f;
            _usedEnergy = selectedAtmosphere.GasMixture.RemoveEnergy(_usedEnergy * 1.015f);
            __instance.WasteTank.InternalAtmosphere.GasMixture.AddEnergy(_usedEnergy / 3f);
            __result = _usedEnergy / 2000f;
            return false;
        }

        // heating suit
        __instance.Battery.PowerStored -= _usedEnergy * 0.05f;
        selectedAtmosphere.GasMixture.AddEnergy(_usedEnergy);
        __result = 0.0f;
        return false;
    }
}