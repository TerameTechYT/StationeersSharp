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
    private static TextMeshProUGUI _internalTempUnit;
    private static TextMeshProUGUI _externalTempUnit;
    private static bool _kelvinMode;

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Awake")]
    [HarmonyPostfix]
    public static void PlayerStateWindowAwake(PlayerStateWindow __instance)
    {
        // this was the most annoying part of all of this, it took 3 hours to figure out this was even a thing
        _internalTempUnit = GameObject.Find(
                "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelTemp/ValueTemp/TextUnitTemp")
            .GetComponent<TextMeshProUGUI>();
        _externalTempUnit = GameObject.Find(
                "GameCanvas/PanelStatusInfo/PanelVerticalGroup/Internals/PanelInternal/PanelTemp/ValueTemp/TextUnitTemp")
            .GetComponent<TextMeshProUGUI>();
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Update")]
    [HarmonyPostfix]
    public static void PlayerStateWindowUpdate(PlayerStateWindow __instance)
    {
        if (GameManager.GameState == GameState.Running && __instance != null && __instance.Parent != null)
        {
            var human = __instance.Parent;

            if (WorldManager.DaysPast == 0)
                __instance.InfoExternalDays.text = "DAY 0";

            _kelvinMode = Input.GetKey(KeyCode.K);

            if (__instance.InfoExternalPressure != null)
            {
                var pressure = __instance._pressureExternal;
                var text = pressure <= Chemistry.Pressure.Minimum ? "0" : pressure.ToString("F");
                __instance.InfoExternalPressure.text = text;
            }

            if (__instance.InfoInternalPressure != null)
            {
                var pressure = __instance._pressureInternal;
                var text = pressure <= Chemistry.Pressure.Minimum ? "0" : pressure.ToString("F");

                __instance.InfoInternalPressure.text = text;
            }

            if (__instance.InfoExternalPressure != null)
            {
                var temp = _kelvinMode ? __instance._tempExternalK : __instance._tempExternal;
                var text = temp <= Chemistry.Temperature.Minimum ? "Nil" : temp.ToString("F");
                __instance.InfoExternalTemperature.text = text;
                _externalTempUnit.text = _kelvinMode ? "°K" : "°C";
            }

            if (__instance.InfoInternalTemperature != null && human.BreathingAtmosphere != null)
            {
                var temp = _kelvinMode ? __instance._tempInternalK : __instance._tempInternal;
                var text = temp <= Chemistry.Temperature.Minimum ? "Nil" : temp.ToString("F");

                __instance.InfoInternalTemperature.text = text;
                _internalTempUnit.text = _kelvinMode ? "°K" : "°C";
            }

            if (__instance.InfoExternalVelocity != null)
            {
                var velocity = human.VelocityMagnitude;
                var text = velocity <= 0f ? "0" : velocity.ToString("F1");

                __instance.InfoExternalVelocity.text = text;
            }

            if (__instance.CognitionPercentage != null)
            {
                var stunDamage = human.DamageState.Stun;

                __instance.CognitionPercentage.text = stunDamage.ToString("F1");
            }

            if (__instance.ToxinPercentage != null)
            {
                var toxinDamage = human.DamageState.Toxic;

                __instance.ToxinPercentage.text = toxinDamage.ToString("F1");
            }

            if (__instance.HealthPercentage != null)
            {
                var totalDamage = human.DamageState.TotalRatio;
                var healthLeft = 100f - totalDamage * 100f;

                __instance.HealthPercentage.text = healthLeft.ToString("F1");
            }

            if (__instance.HungerPercentage != null)
            {
                var hunger = human.Nutrition;
                var hungerLeft = hunger / human.MaxNutritionStorage * 100;
                var text = hungerLeft <= 0f ? "0" : hungerLeft.ToString("F1");

                __instance.HungerPercentage.text = text;
            }

            if (__instance.HydrationPercentage != null)
            {
                var hydration = human.Hydration;
                var hydrationLeft = hydration / Entity.MAX_HYDRATION_STORAGE * 100f;
                var text = hydrationLeft <= 0f ? "0" : hydrationLeft.ToString("F1");

                __instance.HydrationPercentage.text = text;
            }

            if (__instance.NavigationText != null)
            {
                var eulerAnglesY = human.EntityRotation.eulerAngles.y;
                var orientation = (eulerAnglesY + 270f) % 360f;

                __instance.NavigationText.text = orientation.ToString("F1");
            }

            if (__instance.InfoJetpackPressureDeltaText != null && human.BackpackSlot.Contains<Jetpack>() && human.BackpackSlot.Get<Jetpack>().PropellentSlot.Contains<GasCanister>())
            {
                var jetpack = human.BackpackSlot.Get<Jetpack>();
                var jetpackPressure = jetpack.PropellentSlot.Get<GasCanister>().Pressure;
                var pressureDelta = jetpackPressure - __instance._pressureExternal;

                __instance.InfoJetpackPressureDeltaText.text = pressureDelta.ToString("F1");
            }
        }
    }
}