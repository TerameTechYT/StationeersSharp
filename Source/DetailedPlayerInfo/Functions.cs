// ReSharper disable InconsistentNaming

#pragma warning disable CA1305

using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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
        return window != null && new List<bool> {
            GameManager.GameState == GameState.Running,

            _internalTempUnit != null,
            _externalTempUnit != null,

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
        {
            return fallbackValue;
        }

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
            int framesCap = CatchAndReturnDefault<int, FormatException>(maxFrames,
                () => int.Parse(Settings.CurrentData.FrameLock).Clamp(minFrames, maxFrames));
            float frames = (1.0f / Time.smoothDeltaTime).Clamp(0, framesCap);

            frameText.text = string.Concat([
                frames.ToString("F"),
                Settings.CurrentData.FrameLock == "Off" ? string.Empty : " / ",
                Settings.CurrentData.FrameLock == "Off" ? string.Empty : Settings.CurrentData.FrameLock,
                " FPS"
            ]);

            // Hide counter when no ui mode is enabled
            frameText.transform.parent.gameObject.SetActive(InventoryManager.ShowUi);

            if (!GameManager.IsBatchMode && GameManager.GameState != GameState.Running)
            {
                Application.targetFrameRate = Settings.CurrentData.FrameLock != "Off" ? framesCap : 250;
            }

            await UniTask.NextFrame();
        }
    }

    internal static bool EnableFrameCounter(ref TextMeshProUGUI frameCounter)
    {
        if (frameCounter == null)
        {
            return true;
        }

        frameCounter.transform.parent.gameObject.SetActive(Settings.CurrentData.ShowFps);
        if (Settings.CurrentData.ShowFps)
        {
            FrameCounterUpdate(frameCounter).Forget();
        }

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
        if (!ReadyToExecute(ref window))
        {
            return;
        }

        _kelvinMode = Input.GetKey(KeyCode.K);

        Human human = window.Parent;
        Suit suit = (Suit)human.SuitSlot.Get();
        Jetpack jetpack = (Jetpack)human.BackpackSlot.Get();
        GasCanister jetpackPropellant = (GasCanister)jetpack?.PropellentSlot.Get();

        string temperatureUnit = _kelvinMode ? "°K" : "°C";

        // Set Temperature Unit
        _internalTempUnit.text = _externalTempUnit.text = temperatureUnit;

        // Little fix for initially logging in day counter is just "0" until next day update
        window.InfoExternalDays.text = "DAY " + WorldManager.DaysPast;

        // Suit External Pressure
        float externalPressure = window._pressureExternal;
        string externalPressureText = externalPressure.ToString("F");
        window.InfoExternalPressure.text = externalPressureText;
        window.InfoExternalPressure.fontSize = 21;

        // Suit Internal Pressure
        float internalPressure = window._pressureInternal;
        string internalPressureText = internalPressure.ToString("F");
        window.InfoInternalPressure.text = internalPressureText;
        window.InfoInternalPressure.fontSize = 21;

        // Suit Pressure Setting
        float pressureSetting = suit?.OutputSetting ?? 0f;
        string pressureSettingText = pressureSetting.ToString("F");
        window.InfoInternalPressureSetting.text = pressureSettingText;

        // Suit Temperature Setting
        float temperatureSetting = suit?.OutputTemperature ?? 0f;
        string temperatureSettingText =
            _kelvinMode
                ? temperatureSetting.ToString("F")
                : (temperatureSetting - 273.15f).ToString("F");
        window.InfoInternalTemperatureSetting.text = temperatureSettingText;

        // Suit External Temperature
        float externalTemperature = _kelvinMode ? window._tempExternalK : window._tempExternal;
        string externalTemperatureText = window._tempExternalK <= Data.TemperatureMinimum
            ? "Nil"
            : externalTemperature.ToString("F");
        window.InfoExternalTemperature.text = externalTemperatureText;
        window.InfoExternalTemperature.fontSize = 21;

        // Suit Internal Temperature
        float internalTemperature = _kelvinMode ? window._tempInternalK : window._tempInternal;
        string internalTemperatureText = window._tempInternalK <= Data.TemperatureMinimum
            ? "Nil"
            : internalTemperature.ToString("F");
        window.InfoInternalTemperature.text = internalTemperatureText;
        window.InfoInternalTemperature.fontSize = 21;

        // Jetpack Delta Pressure
        float jetpackPressure = jetpackPropellant?.Pressure ?? 0f;
        float pressureDelta = jetpackPressure - externalPressure;
        string pressureDeltaText = pressureDelta.ToString("F");
        window.InfoJetpackPressureDeltaText.text = pressureDeltaText;
        window.InfoJetpackPressureDeltaText.fontSize = 21;

        // Jetpack Thrust Setting
        float jetpackSetting = jetpack?.OutputSetting ?? 0f;
        double jetpackSettingRounded = Math.Ceiling(jetpackSetting * 10f) * 5f;
        string jetpackSettingText = ((int)jetpackSettingRounded).ToString() + "%";
        window.InfoJetpackThrust.text = jetpackSettingText;

        // Character Velocity
        float velocity = human.VelocityMagnitude;
        string velocityText = velocity.ToString("F");
        window.InfoExternalVelocity.text = velocityText;
        window.InfoExternalVelocity.fontSize = 21;

        // Character Stun Damage
        float stunDamage = human.DamageState.Stun;
        string stunDamageText = stunDamage.ToString("F");
        window.CognitionPercentage.text = stunDamageText;
        window.CognitionPercentage.fontSize = 21;

        // Character Toxin Damage
        float toxinDamage = human.DamageState.Toxic;
        string toxinDamageText = toxinDamage.ToString("F");
        window.ToxinPercentage.text = toxinDamageText;
        window.ToxinPercentage.fontSize = 21;

        // Character Total Damage
        float totalDamage = human.DamageState.TotalRatio * 100f;
        float healthLeft = 100f - totalDamage;
        string healthLeftText = healthLeft.ToString("F");
        window.HealthPercentage.text = healthLeftText;
        window.HealthPercentage.fontSize = 21;

        // Character Hunger Left
        float hunger = human.Nutrition;
        float hungerDivisor = Data.PNNInstalled ? Data.PNNMaxNutrition : human.GetNutritionStorage();
        float hungerClamp = hunger / hungerDivisor;
        float hungerLeft = hungerClamp * 100f;
        string hungerLeftText = hungerLeft.ToString("F");
        window.HungerPercentage.text = hungerLeftText;
        window.HungerPercentage.fontSize = 21;

        // Character Hydration Left
        float hydration = human.Hydration;
        float hydrationDivisor = Data.PNNInstalled ? Data.PNNMaxHydration : human.GetHydrationStorage();
        float hydrationClamp = hydration / hydrationDivisor;
        float hydrationLeft = hydrationClamp * 100f;
        string hydrationLeftText = hydrationLeft.ToString("F");
        window.HydrationPercentage.text = hydrationLeftText;
        window.HydrationPercentage.fontSize = 21;

        // Character Look Angle
        float eulerAnglesY = human.EntityRotation.eulerAngles.y;
        float orientation = (eulerAnglesY + 270f) % 360f;
        string orientationText = orientation.ToString("F");
        window.NavigationText.text = orientationText;
        window.NavigationText.fontSize = 21;
    }
}