using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace DetailedPlayerInfo.Patches;

[HarmonyPatch]
public static class DetailedPlayerInfo
{
    private static StateInstance ExternalPressureState;
    private static StateInstance ExternalTemperatureState;
    private static StateInstance InternalPressureState;
    private static StateInstance InternalTemperatureState;
    private static StateInstance VelocityState;
    private static StateInstance JetpackPressureState;
    private static StateInstance CompassAngleState;
    private static StateInstance HungerState;
    private static StateInstance HydrationState;
    private static StateInstance HealthState;
    private static StateInstance CognitionState;
    private static Traverse traverse;
    private static TextMeshProUGUI InternalTempUnit;
    private static TextMeshProUGUI ExternalTempUnit;

    private static bool kelvinMode;
    private static bool justChanged;

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Awake")]
    [HarmonyPostfix]
    public static void PlayerStateWindowAwake(PlayerStateWindow __instance)
    {
        traverse = Traverse.Create(__instance);
        ExternalPressureState = Utilities.GetFieldValue<StateInstance>(traverse, "_externalPressureState");
        ExternalTemperatureState = Utilities.GetFieldValue<StateInstance>(traverse, "_externalTemperatureState");
        InternalPressureState = Utilities.GetFieldValue<StateInstance>(traverse, "_internalPressureState");
        InternalTemperatureState = Utilities.GetFieldValue<StateInstance>(traverse, "_internalTemperatureState");
        VelocityState = Utilities.GetFieldValue<StateInstance>(traverse, "_externalVelocityState");
        JetpackPressureState = Utilities.GetFieldValue<StateInstance>(traverse, "_jetpackPressureDeltaState");
        CompassAngleState = Utilities.GetFieldValue<StateInstance>(traverse, "_compassState");
        HungerState = Utilities.GetFieldValue<StateInstance>(traverse, "_hungerState");
        HydrationState = Utilities.GetFieldValue<StateInstance>(traverse, "_hydrationState");
        HealthState = Utilities.GetFieldValue<StateInstance>(traverse, "_healthState");
        CognitionState = Utilities.GetFieldValue<StateInstance>(traverse, "_cognitionState");

        // this was the most annoying part of all of this, it took 3 hours to figure out this was even a thing
        InternalTempUnit = GameObject.Find(
                "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelTemp/ValueTemp/TextUnitTemp")
            .GetComponent<TextMeshProUGUI>();
        ExternalTempUnit = GameObject.Find(
                "GameCanvas/PanelStatusInfo/PanelVerticalGroup/Internals/PanelInternal/PanelTemp/ValueTemp/TextUnitTemp")
            .GetComponent<TextMeshProUGUI>();
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Update")]
    [HarmonyPostfix]
    public static void PlayerStateWindowUpdate(PlayerStateWindow __instance)
    {
        if (GameManager.GameState != GameState.Running || __instance.Parent == null)
            return;

        if (WorldManager.DaysPast == 0)
            Utilities.GetFieldValue<StateInstance>(traverse, "_daysPassed").Text.text = "DAY 0";

        if (KeyManager.GetButtonDown(KeyCode.K))
            kelvinMode = !kelvinMode;

        if (ExternalPressureState != null)
            ExternalPressureState.Text.text = __instance._pressureExternal.ToString("F");
        if (InternalPressureState != null)
            InternalPressureState.Text.text = __instance._pressureInternal.ToString("F");
        if (ExternalTemperatureState != null)
            if (__instance.Parent.WorldAtmosphere != null)
            {
                var world = __instance.Parent.WorldAtmosphere;
                var temp = world.IsValid() && world.Temperature > Chemistry.Temperature.Minimum
                    ? world.Temperature
                    : 0f;

                ExternalTemperatureState.Text.text = temp == 0f ? "Nil" :
                    kelvinMode ? temp.ToString("F") : (temp - Chemistry.Temperature.ZeroDegrees).ToString("F");
                ExternalTempUnit.text = kelvinMode ? "°K" : "°C";
            }

        if (InternalTemperatureState != null)
        {
            if (__instance.Parent.BreathingAtmosphere != null)
            {
                var breathing = __instance.Parent.BreathingAtmosphere;
                var temp = breathing.IsValid() && breathing.Temperature > Chemistry.Temperature.Minimum
                    ? breathing.Temperature
                    : 0f;

                InternalTemperatureState.Text.text = temp == 0f ? "Nil" :
                    kelvinMode ? temp.ToString("F") : (temp - Chemistry.Temperature.ZeroDegrees).ToString("F");
                InternalTempUnit.text = kelvinMode ? "°K" : "°C";
            }
        }

        if (VelocityState != null)
            VelocityState.Text.text = __instance.Parent.VelocityMagnitude.ToString("F1");
        if (CognitionState != null)
            CognitionState.Text.text = __instance.Parent.DamageState.Stun.ToString("F1");
        if (HealthState != null)
            HealthState.Text.text = (100 - __instance.Parent.DamageState.Total * 100f).ToString("F1");
        if (HungerState != null)
            HungerState.Text.text =
                (__instance.Parent.Nutrition / __instance.Parent.MaxNutritionStorage * 100f).ToString("F1");
        if (HydrationState != null)
            HydrationState.Text.text =
                (__instance.Parent.Hydration / Entity.MaxHydrationStorage * 100f).ToString("F1");

        if (CompassAngleState != null)
            CompassAngleState.Text.text =
                ((__instance.Parent.EntityRotation.eulerAngles.y + 270f) % 360f).ToString("F1");

        if (JetpackPressureState != null)
        {
            var human = __instance.Parent.AsHuman;
#pragma warning disable CS0618 // Type or member is obsolete
            var backAsJetpack = human.BackpackSlot.Occupant as Jetpack;
#pragma warning restore CS0618 // Type or member is obsolete
            if (backAsJetpack != null)
            {
                var jetpackPressure = (backAsJetpack.PropellentSlot.Occupant as GasCanister).Pressure;
                var pressureDelta = jetpackPressure -
                                    (backAsJetpack.WorldAtmosphere?.PressureGasses ?? 0f);

                JetpackPressureState.Text.text = pressureDelta.ToString("F1");
            }
        }
    }
}