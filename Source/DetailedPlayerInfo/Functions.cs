#region

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#endregion

namespace DetailedPlayerInfo;

internal static class Functions {
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

    internal static bool ReadyToExecute(ref PlayerStateWindow window) => window != null && new List<bool> {
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

            (!Data.ExtraInfoFilter && !Data.ExtraInfoPower) || _wasteTextPanel != null,

            !Data.ExtraInfoPower || _batteryStatus != null,
            !Data.ExtraInfoPower || _batteryTextPanel != null,
            !Data.ExtraInfoPower || _batteryText != null,

            !Data.ExtraInfoFilter || _filterStatus != null,
            !Data.ExtraInfoFilter || _filterTextPanel != null,
            !Data.ExtraInfoFilter || _filterText != null
        }.All(boolean => boolean);

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

            int framesCap = CatchAndReturnDefault<int, FormatException>(maxFrames,
                () => int.Parse(Settings.CurrentData.FrameLock).Clamp(minFrames, maxFrames));
            float frames = (1f / Time.smoothDeltaTime).Clamp(0, framesCap);

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
        if (Data.ExtraInfoPower || Data.ExtraInfoFilter)
            _wasteTextPanel = GameObject.Find(Data.WasteTextPanel);

        if (Data.ExtraInfoPower) {
            _batteryStatus = GameObject.Find(Data.BatteryStatus);
            _batteryTextPanel = UnityObject.Instantiate(_wasteTextPanel, _batteryStatus.transform);
            _batteryText = _batteryTextPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (Data.ExtraInfoFilter) {
            _filterStatus = GameObject.Find(Data.FilterStatus);
            _filterTextPanel = UnityObject.Instantiate(_wasteTextPanel, _filterStatus.transform);
            _filterText = _filterTextPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    internal static void Update(ref PlayerStateWindow window) {
        if (!ReadyToExecute(ref window))
            return;

        _kelvinMode = Input.GetKey(Data.kelvinMode?.Value ?? KeyCode.K);

        // Suit stuff
        Assets.Scripts.Objects.Entities.Human human = window.Parent;
        Suit suit = human?.SuitSlot.Get<Suit>();
        AdvancedSuit advancedSuit = suit is AdvancedSuit ? suit as AdvancedSuit : null;

        // Suit slot stuff
        BatteryCell suitBattery = suit?.BatterySlot.Get<BatteryCell>();
        GasFilter filter1 = suit?.FilterSlot1.Get<GasFilter>();
        GasFilter filter2 = suit?.FilterSlot2.Get<GasFilter>();
        GasFilter filter3 = suit?.FilterSlot3.Get<GasFilter>();
        GasFilter filter4 = advancedSuit?.FilterSlot4.Get<GasFilter>();

        // Jetpack stuff
        Jetpack jetpack = human?.BackpackSlot.Get<Jetpack>();
        GasCanister jetpackPropellant = jetpack?.PropellentSlot.Get<GasCanister>();

        string temperatureUnit = _kelvinMode ? "°K" : "°C";

        // Set Temperature Unit
        _internalTempUnit.text = _externalTempUnit.text = temperatureUnit;

        // Change battery percentage text
        if (Data.ExtraInfoPower && (StatusUpdates.Instance.IsPowerCaution() || StatusUpdates.Instance.IsPowerCritical())) {
            float percentage = (suitBattery?.PowerRatio ?? 0f) * 100f;

            _batteryText.text = percentage.ToStringRounded() + "%";
        }

        // Change filter percentage text
        if (Data.ExtraInfoFilter && (StatusUpdates.Instance.IsFilterCaution() || StatusUpdates.Instance.IsFilterCritical())) {
            float ratio = Mathf.Min(filter1?.RemainingRatio ?? 10f,
                filter2?.RemainingRatio ?? 10f,
                filter3?.RemainingRatio ?? 10f,
                filter4?.RemainingRatio ?? 10f);

            float filterRatio = ratio == 10f ? 0f : ratio * 100f;

            _filterText.text = filterRatio.ToStringRounded() + "%";
        }


        // Little fix for initially logging in day counter is just "0" until next day update
        window.InfoExternalDays.text = "DAY " + WorldManager.DaysPast;

        // Suit External Pressure
        float externalPressure = window._pressureExternal.ToFloat();
        string externalPressureText = externalPressure.ToStringPrecision();
        window.InfoExternalPressure.text = externalPressureText;
        window.InfoExternalPressure.fontSize = Data.FontSize;

        // Suit Internal Pressure
        float internalPressure = window._pressureInternal.ToFloat();
        string internalPressureText = internalPressure.ToStringPrecision();
        window.InfoInternalPressure.text = internalPressureText;
        window.InfoInternalPressure.fontSize = Data.FontSize;

        // Suit Pressure Setting
        float pressureSetting = suit?.OutputSetting ?? 0f;
        string pressureSettingText = pressureSetting.ToStringPrecision();
        window.InfoInternalPressureSetting.text = pressureSettingText;

        // Suit Temperature Setting
        float temperatureSetting = suit?.OutputTemperature.ToFloat() ?? 0f;
        string temperatureSettingText =
            _kelvinMode
                ? temperatureSetting.ToStringPrecision()
                : (temperatureSetting - Data.TemperatureZero).ToStringPrecision();
        window.InfoInternalTemperatureSetting.text = temperatureSettingText;

        // Suit External Temperature
        float externalTemperature = _kelvinMode ? window._tempExternalK.ToFloat() : window._tempExternal;
        string externalTemperatureText = window._tempExternalK.ToFloat() <= Data.TemperatureMinimum
            ? "Nil"
            : externalTemperature.ToStringPrecision();
        window.InfoExternalTemperature.text = externalTemperatureText;
        window.InfoExternalTemperature.fontSize = Data.FontSize;

        // Suit Internal Temperature
        float internalTemperature = _kelvinMode ? window._tempInternalK.ToFloat() : window._tempInternal;
        string internalTemperatureText = window._tempInternalK.ToFloat() <= Data.TemperatureMinimum
            ? "Nil"
            : internalTemperature.ToStringPrecision();
        window.InfoInternalTemperature.text = internalTemperatureText;
        window.InfoInternalTemperature.fontSize = Data.FontSize;

        // Jetpack Delta Pressure
        float jetpackPressure = jetpackPropellant?.Pressure.ToFloat() ?? 0f;
        float pressureDelta = jetpackPressure - externalPressure;
        string pressureDeltaText = pressureDelta.ToStringPrecision();
        window.InfoJetpackPressureDeltaText.text = pressureDeltaText;
        window.InfoJetpackPressureDeltaText.fontSize = Data.FontSize;

        // Jetpack Thrust Setting
        float jetpackSetting = jetpack?.OutputSetting ?? 0f;
        double jetpackSettingRounded = Math.Ceiling(jetpackSetting * 10f) * 5f;
        string jetpackSettingText = (int) jetpackSettingRounded + "%";
        window.InfoJetpackThrust.text = jetpackSettingText;
        window.InfoJetpackThrust.fontSize = Data.FontSize;

        // Character Velocity
        float velocity = human.VelocityMagnitude;
        string velocityText = (velocity < 0.01f ? 0f : velocity).ToStringPrecision();
        window.InfoExternalVelocity.text = velocityText;
        window.InfoExternalVelocity.fontSize = Data.FontSize;

        // Character Stun Damage
        float stunDamage = human.DamageState.Stun;
        string stunDamageText = stunDamage.ToStringPrecision();
        window.CognitionPercentage.text = stunDamageText;
        window.CognitionPercentage.fontSize = Data.FontSize;

        // Character Toxin Damage
        float toxinDamage = human.DamageState.Toxic;
        string toxinDamageText = toxinDamage.ToStringPrecision();
        window.ToxinPercentage.text = toxinDamageText;
        window.ToxinPercentage.fontSize = Data.FontSize;

        // Character Total Damage
        float totalDamage = human.DamageState.TotalRatio * 100f;
        float healthLeft = 100f - totalDamage;
        string healthLeftText = healthLeft.ToStringPrecision();
        window.HealthPercentage.text = healthLeftText;
        window.HealthPercentage.fontSize = Data.FontSize;

        // Character Hunger Left
        float hunger = human.Nutrition;
        float hungerDivisor = human.GetNutritionStorage();
        float hungerClamp = hunger / hungerDivisor;
        float hungerLeft = hungerClamp * 100f;
        string hungerLeftText = hungerLeft.ToStringPrecision();
        window.HungerPercentage.text = hungerLeftText;
        window.HungerPercentage.fontSize = Data.FontSize;

        // Character Hydration Left
        float hydration = human.Hydration;
        float hydrationDivisor = human.GetHydrationStorage();
        float hydrationClamp = hydration / hydrationDivisor;
        float hydrationLeft = hydrationClamp * 100f;
        string hydrationLeftText = hydrationLeft.ToStringPrecision();
        window.HydrationPercentage.text = hydrationLeftText;
        window.HydrationPercentage.fontSize = Data.FontSize;

        // Character Look Angle
        float eulerAnglesY = human.EntityRotation.eulerAngles.y;
        float orientation = (eulerAnglesY + 270f) % 360f;
        string orientationText = orientation.ToStringPrecision();
        window.NavigationText.text = orientationText;
        window.NavigationText.fontSize = Data.FontSize;
    }
}

public static class Extensions {
    public static string ToStringPrecision(this float value) => Math.Round(value, Data.NumberPrecision).ToString();
}