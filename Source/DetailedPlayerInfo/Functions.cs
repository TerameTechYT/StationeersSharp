// ReSharper disable InconsistentNaming

#pragma warning disable CA1305

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DetailedPlayerInfo;

internal class Functions
{
    private static TextMeshProUGUI _internalTempUnit;
    private static TextMeshProUGUI _externalTempUnit;
    private static bool _kelvinMode;

    internal static bool ReadyToExecute(ref PlayerStateWindow window)
    {
        return window != null && new List<bool>
        {
            GameManager.GameState == GameState.Running,

            window.Parent != null,
            window.InfoExternalDays != null,
            window.InfoExternalPressure != null,
            window.InfoInternalPressure != null,
            window.InfoInternalPressureSetting != null,
            window.InfoExternalTemperature != null,
            window.InfoInternalTemperature != null,
            window.InfoInternalTemperatureSetting != null,
            window.InfoExternalVelocity != null,
            window.CognitionPercentage != null,
            window.ToxinPercentage != null,
            window.HealthPercentage != null,
            window.HungerPercentage != null,
            window.HydrationPercentage != null,
            window.NavigationText != null,
            window.InfoJetpackPressureDeltaText != null,
            window.InfoJetpackThrust != null
        }.All(boolean => boolean);
    }

    internal static T1 CatchAndReturnDefault<T1, T2>(T1 fallbackValue, Func<T1> action) where T2 : Exception
    {
        if (action == null)
            return fallbackValue;

        try
        {
            return action();
        }
        catch (T2)
        {
            return fallbackValue;
        }
    }

    internal static async UniTaskVoid FrameCounterUpdate(TextMeshProUGUI frameText)
    {
        while (Settings.CurrentData.ShowFps && frameText != null)
        {
            const int maxFrames = 1000;
            const int minFrames = 30;
            var framesCap = CatchAndReturnDefault<int, FormatException>(maxFrames,
                () => int.Parse(Settings.CurrentData.FrameLock).Clamp(minFrames, maxFrames));
            var frames = (1.0f / Time.smoothDeltaTime).Clamp(0, framesCap);

            frameText.text = string.Concat([
                frames.ToString("F"),
                Settings.CurrentData.FrameLock == "Off" ? string.Empty : " / ",
                Settings.CurrentData.FrameLock == "Off" ? string.Empty : Settings.CurrentData.FrameLock,
                " FPS"
            ]);

            if (!GameManager.IsBatchMode && GameManager.GameState != GameState.Running)
                Application.targetFrameRate = Settings.CurrentData.FrameLock != "Off" ? framesCap : 250;

            await UniTask.NextFrame();
        }
    }

    internal static bool EnableFrameCounter(ref TextMeshProUGUI frameCounter)
    {
        if (frameCounter == null)
            return true;

        frameCounter.transform.parent.gameObject.SetActive(Settings.CurrentData.ShowFps);
        if (Settings.CurrentData.ShowFps)
            FrameCounterUpdate(frameCounter).Forget();

        return false;
    }

    internal static void Initialize()
    {
        // This was the most annoying part of all this, it took 3 hours to figure out this was even a thing
        _internalTempUnit = GameObject.Find(Data.InternalTemperatureUnit).GetComponent<TextMeshProUGUI>();
        _externalTempUnit = GameObject.Find(Data.ExternalTemperatureUnit).GetComponent<TextMeshProUGUI>();
    }

    internal static void Update(ref PlayerStateWindow window)
    {
        if (window == null || !ReadyToExecute(ref window)) return;

        var human = window.Parent;
        var suit = human.SuitSlot.Get<Suit>();
        var jetpack = human.BackpackSlot.Get<Jetpack>();
        var jetpackPropellant = jetpack?.PropellentSlot.Get<GasCanister>();

        var temperatureUnit = _kelvinMode ? "°K" : "°C";

        _kelvinMode = Input.GetKey(KeyCode.K);

        // Little fix for initially logging in day counter is just "0" until next day update
        window.InfoExternalDays.text = "DAY " + WorldManager.DaysPast;

        // Suit External Pressure
        var externalPressure = window._pressureExternal;
        var externalPressureText =
            externalPressure <= Chemistry.Pressure.Minimum ? "None" : externalPressure.ToString("F");
        window.InfoExternalPressure.text = externalPressureText;

        // Suit Internal Pressure
        var internalPressure = window._pressureInternal;
        var internalPressureText =
            internalPressure <= Chemistry.Pressure.Minimum ? "None" : internalPressure.ToString("F");
        window.InfoInternalPressure.text = internalPressureText;

        // Suit Pressure Setting
        var pressureSetting = suit?.OutputSetting ?? 0f;
        var pressureSettingText = pressureSetting.ToString("F");
        window.InfoInternalPressureSetting.text = pressureSettingText;

        // Suit Temperature Setting
        var temperatureSetting = suit?.OutputTemperature ?? 0f;
        var temperatureSettingText =
            _kelvinMode
                ? temperatureSetting.ToString("F")
                : (temperatureSetting - Chemistry.Temperature.ZeroDegrees).ToString("F");
        window.InfoInternalTemperatureSetting.text = temperatureSettingText;

        // Suit External Temperature
        var externalTemperature = _kelvinMode ? window._tempExternalK : window._tempExternal;
        var externalTemperatureText = window._tempExternalK <= Chemistry.Temperature.Minimum
            ? "Nil"
            : externalTemperature.ToString("F");
        window.InfoExternalTemperature.text = externalTemperatureText;

        // Suit Internal Temperature
        var internalTemperature = _kelvinMode ? window._tempInternalK : window._tempInternal;
        var internalTemperatureText = window._tempInternalK <= Chemistry.Temperature.Minimum
            ? "Nil"
            : internalTemperature.ToString("F");
        window.InfoInternalTemperature.text = internalTemperatureText;

        // Set Temperature Unit
        _internalTempUnit.text = _externalTempUnit.text = temperatureUnit;

        // Jetpack Delta Pressure
        var jetpackPressure = jetpackPropellant?.Pressure ?? 0f;
        var pressureDelta = jetpackPressure - externalPressure;
        var pressureDeltaText = pressureDelta.ToString("F1");
        window.InfoJetpackPressureDeltaText.text = pressureDeltaText;

        // Jetpack Thrust Setting
        var jetpackSetting = jetpack?.OutputSetting ?? 0f;
        // Rounding of thrust so your thrust is more accurate. 0.1 - 2;
        var jetpackSettingRounded = Math.Round(jetpackSetting, 1) * 100f;
        var jetpackSettingText = jetpackSettingRounded.ToString("F1");
        window.InfoJetpackThrust.text = jetpackSettingText;

        // Character Velocity
        var velocity = human.VelocityMagnitude;
        var velocityText = velocity <= 0f ? "0" : velocity.ToString("F1");
        window.InfoExternalVelocity.text = velocityText;

        // Character Stun Damage
        var stunDamage = human.DamageState.Stun;
        var stunDamageText = stunDamage.ToString("F1");
        window.CognitionPercentage.text = stunDamageText;

        // Character Toxin Damage
        var toxinDamage = human.DamageState.Toxic;
        var toxinDamageText = toxinDamage.ToString("F1");
        window.ToxinPercentage.text = toxinDamageText;

        // Character Total Damage
        var totalDamage = human.DamageState.TotalRatio * 100f;
        var healthLeft = 100f - totalDamage;
        var healthLeftText = healthLeft.ToString("F1");
        window.HealthPercentage.text = healthLeftText;

        // Character Hunger Left
        var hunger = human.Nutrition;
        var hungerLeft = hunger / human.MaxNutritionStorage * 100;
        var hungerLeftText = hungerLeft <= 0f ? "0" : hungerLeft.ToString("F1");
        window.HungerPercentage.text = hungerLeftText;

        // Character Hydration Left
        var hydration = human.Hydration;
        var hydrationLeft = hydration / Entity.MAX_HYDRATION_STORAGE * 100f;
        var hydrationLeftText = hydrationLeft <= 0f ? "0" : hydrationLeft.ToString("F1");
        window.HydrationPercentage.text = hydrationLeftText;

        // Character Look Angle
        var eulerAnglesY = human.EntityRotation.eulerAngles.y;
        var orientation = (eulerAnglesY + 270f) % 360f;
        var orientationText = orientation.ToString("F1");
        window.NavigationText.text = orientationText;
    }
}