#region

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace DetailedPlayerInfo;

internal class Functions {
    // temperature text objects
    private static TextMeshProUGUI _internalTempUnit;
    private static TextMeshProUGUI _externalTempUnit;

    // template object to be cloned
    private static GameObject _wasteTextPanel;

    // more info objects
    private static GameObject _batteryStatus;
    private static GameObject _batteryTextPanel;
    private static TextMeshProUGUI _batteryText;

    private static GameObject _filterStatus;
    private static GameObject _filterTextPanel;
    private static TextMeshProUGUI _filterText;


    private static bool _kelvinMode;

    internal static bool ReadyToExecute(ref PlayerStateWindow window) {
        var checkBattery = Data.ExtraInfoPower?.Value ?? false;
        var checkFilter = Data.ExtraInfoFilter?.Value ?? false;

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
            window.InfoJetpackThrust != null,

            (!checkBattery && !checkFilter) || _wasteTextPanel != null,

            !checkBattery || _batteryStatus != null,
            !checkBattery || _batteryTextPanel != null,
            !checkBattery || _batteryText != null,

            !checkFilter || _filterStatus != null,
            !checkFilter || _filterTextPanel != null,
            !checkFilter || _filterText != null
        }.All(boolean => boolean);
    }

    internal static T1 CatchAndReturnDefault<T1, T2>(T1 fallbackValue, Func<T1> action) where T2 : Exception {
        if (action == null)
            return fallbackValue;

        try {
            return action();
        }
        catch (T2) {
            return fallbackValue;
        }
    }

    internal static async UniTaskVoid FrameCounterUpdate(TextMeshProUGUI frameText) {
        while (Settings.CurrentData.ShowFps && frameText != null) {
            const int maxFrames = 1000;
            const int minFrames = 30;

            var framesCap = CatchAndReturnDefault<int, FormatException>(maxFrames,
                () => int.Parse(Settings.CurrentData.FrameLock).Clamp(minFrames, maxFrames));
            var frames = (1f / Time.smoothDeltaTime).Clamp(0, framesCap);

            frameText.text = string.Concat([
                frames.ToStringPrecision(),
                Settings.CurrentData.FrameLock == "Off" ? string.Empty : " / ",
                Settings.CurrentData.FrameLock == "Off" ? string.Empty : Settings.CurrentData.FrameLock,
                " FPS"
            ]);

            // Hide counter when no ui mode is enabled
            frameText.transform.parent.gameObject.SetActive(InventoryManager.ShowUi);

            if (!GameManager.IsBatchMode && GameManager.GameState != GameState.Running)
                Application.targetFrameRate = Settings.CurrentData.FrameLock != "Off" ? framesCap : 250;

            await UniTask.NextFrame();
        }
    }

    internal static bool EnableFrameCounter(ref TextMeshProUGUI frameCounter) {
        if (frameCounter == null)
            return true;

        frameCounter.transform.parent.gameObject.SetActive(Settings.CurrentData.ShowFps);
        if (Settings.CurrentData.ShowFps)
            FrameCounterUpdate(frameCounter).Forget();

        return false;
    }

    internal static void Initialize() {
        // This was the most annoying part of all this, it took 3 hours to figure out this was even a thing
        _internalTempUnit = GameObject.Find(Data.InternalTemperatureUnit).GetComponent<TextMeshProUGUI>();
        _externalTempUnit = GameObject.Find(Data.ExternalTemperatureUnit).GetComponent<TextMeshProUGUI>();

        // Find object to be cloned later
        if ((Data.ExtraInfoPower?.Value ?? false) || (Data.ExtraInfoFilter?.Value ?? false))
            _wasteTextPanel = GameObject.Find(Data.WasteTextPanel);

        if (Data.ExtraInfoPower?.Value ?? false) {
            _batteryStatus = GameObject.Find(Data.BatteryStatus);
            _batteryTextPanel = Object.Instantiate(_wasteTextPanel, _batteryStatus.transform);
            _batteryText = _batteryTextPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (Data.ExtraInfoFilter?.Value ?? false) {
            _filterStatus = GameObject.Find(Data.FilterStatus);
            _filterTextPanel = Object.Instantiate(_wasteTextPanel, _filterStatus.transform);
            _filterText = _filterTextPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    internal static void Update(ref PlayerStateWindow window) {
        if (!ReadyToExecute(ref window))
            return;

        _kelvinMode = Input.GetKey(Data.KelvinMode?.Value ?? KeyCode.K);

        // Suit stuff
        var human = window.Parent;
        var suit = human?.SuitSlot.Get<Suit>();
        var advancedSuit = suit is AdvancedSuit ? suit as AdvancedSuit : null;

        // Suit slot stuff
        var suitBattery = suit?.BatterySlot.Get<BatteryCell>();
        var filter1 = suit?.FilterSlot1.Get<GasFilter>();
        var filter2 = suit?.FilterSlot2.Get<GasFilter>();
        var filter3 = suit?.FilterSlot3.Get<GasFilter>();
        var filter4 = advancedSuit?.FilterSlot4.Get<GasFilter>();

        // Jetpack stuff
        var jetpack = human?.BackpackSlot.Get<Jetpack>();
        var jetpackPropellant = jetpack?.PropellentSlot.Get<GasCanister>();

        var temperatureUnit = _kelvinMode ? "°K" : "°C";

        // Set Temperature Unit
        _internalTempUnit.text = _externalTempUnit.text = temperatureUnit;

        // Change battery percentage text
        if ((Data.ExtraInfoPower?.Value ?? false) &&
            (StatusUpdates.Instance.IsPowerCaution() || StatusUpdates.Instance.IsPowerCritical())) {
            var percentage = (suitBattery?.PowerRatio ?? 0f) * 100f;

            _batteryText.text = percentage.ToStringRounded() + "%";
        }

        // Change filter percentage text
        if ((Data.ExtraInfoFilter?.Value ?? false) &&
            (StatusUpdates.Instance.IsFilterCaution() || StatusUpdates.Instance.IsFilterCritical())) {
            var ratio = Mathf.Min(filter1?.RemainingRatio ?? 10f,
                filter2?.RemainingRatio ?? 10f,
                filter3?.RemainingRatio ?? 10f,
                filter4?.RemainingRatio ?? 10f);

            var filterRatio = ratio == 10f ? 0f : ratio * 100f;

            _filterText.text = filterRatio.ToStringRounded() + "%";
        }


        // Little fix for initially logging in day counter is just "0" until next day update
        window.InfoExternalDays.text = "DAY " + WorldManager.DaysPast;

        // Suit External Pressure
        var externalPressure = window._pressureExternal.ToFloat();
        var externalPressureText = externalPressure.ToStringPrecision();
        window.InfoExternalPressure.text = externalPressureText;

        // Suit Internal Pressure
        var internalPressure = window._pressureInternal.ToFloat();
        var internalPressureText = internalPressure.ToStringPrecision();
        window.InfoInternalPressure.text = internalPressureText;

        // Suit Pressure Setting
        var pressureSetting = suit?.OutputSetting ?? 0f;
        var pressureSettingText = pressureSetting.ToStringPrecision();
        window.InfoInternalPressureSetting.text = pressureSettingText;

        // Suit Temperature Setting
        var temperatureSetting = suit?.OutputTemperature.ToFloat() ?? 0f;
        var temperatureSettingText =
            _kelvinMode
                ? temperatureSetting.ToStringPrecision()
                : (temperatureSetting - Data.TemperatureZero).ToStringPrecision();
        window.InfoInternalTemperatureSetting.text = temperatureSettingText;

        // Suit External Temperature
        var externalTemperature = _kelvinMode ? window._tempExternalK.ToFloat() : window._tempExternal;
        var externalTemperatureText = window._tempExternalK.ToFloat() <= Data.TemperatureMinimum
            ? "Nil"
            : externalTemperature.ToStringPrecision();
        window.InfoExternalTemperature.text = externalTemperatureText;


        // Suit Internal Temperature
        var internalTemperature = _kelvinMode ? window._tempInternalK.ToFloat() : window._tempInternal;
        var internalTemperatureText = window._tempInternalK.ToFloat() <= Data.TemperatureMinimum
            ? "Nil"
            : internalTemperature.ToStringPrecision();
        window.InfoInternalTemperature.text = internalTemperatureText;

        // Jetpack Delta Pressure
        var jetpackPressure = jetpackPropellant?.Pressure.ToFloat() ?? 0f;
        var pressureDelta = jetpackPressure - externalPressure;
        var pressureDeltaText = pressureDelta.ToStringPrecision();
        window.InfoJetpackPressureDeltaText.text = pressureDeltaText;

        // Jetpack Thrust Setting
        var jetpackSetting = jetpack?.OutputSetting ?? 0f;
        var jetpackSettingRounded = Math.Ceiling(jetpackSetting * 10f) * 5f;
        var jetpackSettingText = (int)jetpackSettingRounded + "%";
        window.InfoJetpackThrust.text = jetpackSettingText;

        // Character Velocity
        var velocity = human.VelocityMagnitude;
        var velocityText = (velocity < 0.01f ? 0f : velocity).ToStringPrecision();
        window.InfoExternalVelocity.text = velocityText;

        // Character Stun Damage
        var stunDamage = human.DamageState.Stun;
        var stunDamageText = stunDamage.ToStringPrecision();
        window.CognitionPercentage.text = stunDamageText;

        // Character Toxin Damage
        var toxinDamage = human.DamageState.Toxic;
        var toxinDamageText = toxinDamage.ToStringPrecision();
        window.ToxinPercentage.text = toxinDamageText;

        // Character Total Damage
        var totalDamage = human.DamageState.TotalRatio * 100f;
        var healthLeft = 100f - totalDamage;
        var healthLeftText = healthLeft.ToStringPrecision();
        window.HealthPercentage.text = healthLeftText;

        // Character Hunger Left
        var hunger = human.Nutrition;
        var hungerDivisor = human.GetNutritionStorage();
        var hungerClamp = hunger / hungerDivisor;
        var hungerLeft = hungerClamp * 100f;
        var hungerLeftText = hungerLeft.ToStringPrecision();
        window.HungerPercentage.text = hungerLeftText;

        // Character Hydration Left
        var hydration = human.Hydration;
        var hydrationDivisor = human.GetHydrationStorage();
        var hydrationClamp = hydration / hydrationDivisor;
        var hydrationLeft = hydrationClamp * 100f;
        var hydrationLeftText = hydrationLeft.ToStringPrecision();
        window.HydrationPercentage.text = hydrationLeftText;

        // Character Look Angle
        var eulerAnglesY = human.EntityRotation.eulerAngles.y;
        var orientation = (eulerAnglesY + 270f) % 360f;
        var orientationText = orientation.ToStringPrecision();
        window.NavigationText.text = orientationText;


        if (Data.ChangeFontSize?.Value ?? false) {
            var fontSize = Data.FontSize?.Value ?? 21;

            window.NavigationText.fontSize = fontSize;
            window.HydrationPercentage.fontSize = fontSize;
            window.HungerPercentage.fontSize = fontSize;
            window.HealthPercentage.fontSize = fontSize;
            window.ToxinPercentage.fontSize = fontSize;
            window.CognitionPercentage.fontSize = fontSize;
            window.InfoExternalVelocity.fontSize = fontSize;
            window.InfoJetpackPressureDeltaText.fontSize = fontSize;
            window.InfoInternalTemperature.fontSize = fontSize;
            window.InfoExternalTemperature.fontSize = fontSize;
            window.InfoExternalPressure.fontSize = fontSize;
            window.InfoInternalPressure.fontSize = fontSize;
        }
    }
}

public static class Extensions {
    public static string ToStringPrecision(this float value) {
        var digits = Data.NumberPrecision?.Value ?? 2;

        return Math.Round(value, digits).ToString();
    }
}