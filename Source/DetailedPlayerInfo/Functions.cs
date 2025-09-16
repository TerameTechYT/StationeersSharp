#region

using TMPro;
using UnityObject = UnityEngine.Object;

#endregion

namespace DetailedPlayerInfo;

public static class Extensions {
    public static string ToStringPrecision(this float value) => value.ToPrecision().ToString();
    public static string ToStringPrecision(this PressurekPa value) => value.ToPrecision().ToString();
    public static string ToStringPrecision(this TemperatureKelvin value) => value.ToPrecision().ToString();
    public static string ToStringPrecision(this VolumeLitres value) => value.ToPrecision().ToString();
    public static string ToStringPrecision(this MoleQuantity value) => value.ToPrecision().ToString();

    public static float ToPrecision(this float value) => (float) Math.Round(value, ConfigData.NumberPrecision);
    public static float ToPrecision(this PressurekPa value) => value.ToFloat().ToPrecision();
    public static float ToPrecision(this TemperatureKelvin value) => value.ToFloat().ToPrecision();
    public static float ToPrecision(this VolumeLitres value) => value.ToFloat().ToPrecision();
    public static float ToPrecision(this MoleQuantity value) => value.ToFloat().ToPrecision();
}

internal static class Functions {
    // temperature text objects
    private static TextMeshProUGUI? _internalTempUnit;
    private static TextMeshProUGUI? _externalTempUnit;

    // pressure text objects
    private static TextMeshProUGUI? _internalPressureUnit;
    private static TextMeshProUGUI? _externalPressureUnit;
    private static TextMeshProUGUI? _jetpackPressureUnit;

    // template object to be cloned
    private static GameObject? _wasteTextPanel;

    // more info objects
    private static GameObject? _batteryStatus;
    private static GameObject? _batteryTextPanel;
    private static TextMeshProUGUI? _batteryText;

    private static GameObject? _filterStatus;
    private static GameObject? _filterTextPanel;
    private static TextMeshProUGUI? _filterText;

    private static float smoothUnscaledDeltaTime;

    internal static async UniTaskVoid FrameCounterUpdate(TextMeshProUGUI frameText) {
        while (Settings.CurrentData.ShowFps && frameText != null) {
            int framesCap = Utilities.CatchAndReturnDefault<int, FormatException>(60, () => int.Parse(Settings.CurrentData.FrameLock));
            float frames = (1f / smoothUnscaledDeltaTime).Clamp(0, framesCap);
            string framelock = Settings.CurrentData.FrameLock == "Off" ? string.Empty : $" / {Settings.CurrentData.FrameLock}";
            frameText.text = $"{frames.ToPrecision()}{framelock} FPS";

            // Hide counter when no ui mode is enabled
            frameText.transform.parent.gameObject.SetActive(InventoryManager.ShowUi);

            if (GameManager.GameState != GameState.Running) {
                Application.targetFrameRate = Settings.CurrentData.FrameLock != "Off" ? framesCap : -1;
            }

            await UniTask.NextFrame();
        }
    }

    internal static void EnableFrameCounter(ref TextMeshProUGUI frameCounter) {
        frameCounter.transform.parent.gameObject.SetActive(Settings.CurrentData.ShowFps);
        FrameCounterUpdate(frameCounter).Forget();
    }

    internal static void Initialize(ref PlayerStateWindow window) {
        smoothUnscaledDeltaTime = Time.unscaledDeltaTime;

        _internalTempUnit = GameObject.Find(ConfigData.InternalTemperatureUnit).GetComponent<TextMeshProUGUI>();
        _externalTempUnit = GameObject.Find(ConfigData.ExternalTemperatureUnit).GetComponent<TextMeshProUGUI>();

        _internalPressureUnit = GameObject.Find(ConfigData.InternalPressureUnit).GetComponent<TextMeshProUGUI>();
        _externalPressureUnit = GameObject.Find(ConfigData.ExternalPressureUnit).GetComponent<TextMeshProUGUI>();
        _jetpackPressureUnit = GameObject.Find(ConfigData.JetpackPressureUnit).GetComponent<TextMeshProUGUI>();

        // Find object to be cloned later
        if (ConfigData.ExtraInfoPower || ConfigData.ExtraInfoFilter) {
            _wasteTextPanel = GameObject.Find(ConfigData.WasteTextPanel);
        }

        if (ConfigData.ExtraInfoPower) {
            _batteryStatus = GameObject.Find(ConfigData.BatteryStatus);
            _batteryTextPanel = UnityObject.Instantiate(_wasteTextPanel, _batteryStatus.transform);
            _batteryText = _batteryTextPanel?.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (ConfigData.ExtraInfoFilter) {
            _filterStatus = GameObject.Find(ConfigData.FilterStatus);
            _filterTextPanel = UnityObject.Instantiate(_wasteTextPanel, _filterStatus.transform);
            _filterText = _filterTextPanel?.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    internal static void Update(ref PlayerStateWindow window) {
        // smooth with lerp to reduce frame counter jitter
        smoothUnscaledDeltaTime = Mathf.Lerp(smoothUnscaledDeltaTime, Time.unscaledDeltaTime, 0.1f);

        if (GameManager.GameState != GameState.Running || window == null) {
            return;
        }

        // Suit stuff
        Human? human = window.Parent;
        Suit? suit = human?.SuitSlot.Get<Suit>();
        AdvancedSuit? advancedSuit = suit is AdvancedSuit ? suit as AdvancedSuit : null;

        // Suit slot stuff
        BatteryCell? suitBattery = suit?.BatterySlot.Get<BatteryCell>();
        GasFilter? filter1 = suit?.FilterSlot1.Get<GasFilter>();
        GasFilter? filter2 = suit?.FilterSlot2.Get<GasFilter>();
        GasFilter? filter3 = suit?.FilterSlot3.Get<GasFilter>();
        GasFilter? filter4 = advancedSuit?.FilterSlot4.Get<GasFilter>();

        // Jetpack stuff
        Jetpack? jetpack = human?.BackpackSlot.Get<Jetpack>();
        GasCanister? jetpackPropellant = jetpack?.PropellentSlot.Get<GasCanister>();

        // Set Pressure Unit
        _internalPressureUnit?.text = _externalPressureUnit?.text = _jetpackPressureUnit?.text = Utilities.GetPressureSymbol(ConfigData.PreferredPressureUnit);

        // Set Temperature Unit
        _internalTempUnit?.text = _externalTempUnit?.text = Utilities.GetTemperatureSymbol(ConfigData.PreferredTemperatureUnit);

        // Change battery percentage text
        if (ConfigData.ExtraInfoPower && (StatusUpdates.Instance.IsPowerCaution() || StatusUpdates.Instance.IsPowerCritical())) {
            float ratio = suitBattery?.PowerRatio ?? 0f;
            float percentage = ratio * 100f;

            _batteryText?.text = $"{percentage.ToStringRounded()}%";
        }

        // Change filter percentage text
        if (ConfigData.ExtraInfoFilter && (StatusUpdates.Instance.IsFilterCaution() || StatusUpdates.Instance.IsFilterCritical())) {
            float[] filterRatios = [filter1?.RemainingRatio ?? -1f, filter2?.RemainingRatio ?? -1f, filter3?.RemainingRatio ?? -1f, filter4?.RemainingRatio ?? -1f];
            float filterRatio = Mathf.Min(filterRatios.Where((value) => value != -1f).ToArray());
            float percentage = filterRatio * 100f;

            _filterText?.text = $"{percentage.ToStringRounded()}%";
        }

        // Suit External Pressure
        float externalPressure = window._pressureExternal.ToFloat();
        string externalPressureText = externalPressure == 0
                ? "None"
                : externalPressure.ToPreferredUnit(ConfigData.PreferredPressureUnit).ToStringPrecision();
        window.InfoExternalPressure.text = externalPressureText;
        window.InfoExternalPressure.fontSize = ConfigData.FontSize;

        // Suit Internal Pressure
        float internalPressure = window._pressureInternal.ToFloat();
        string internalPressureText = internalPressure == 0
                ? "None"
                : internalPressure.ToPreferredUnit(ConfigData.PreferredPressureUnit).ToStringPrecision();
        window.InfoInternalPressure.text = internalPressureText;
        window.InfoInternalPressure.fontSize = ConfigData.FontSize;

        // Suit Pressure Setting
        float pressureSetting = suit?.OutputSetting ?? 0f;
        string pressureSettingText = pressureSetting.ToPreferredUnit(ConfigData.PreferredPressureUnit).ToStringPrecision();
        window.InfoInternalPressureSetting.text = pressureSettingText;

        // Suit External Temperature
        float externalTemperature = window._tempExternalK.ToFloat();
        string externalTemperatureText = externalTemperature.IsKelvinNil()
                ? "Nil"
                : externalTemperature.ToPreferredUnit(ConfigData.PreferredTemperatureUnit).ToStringPrecision();
        window.InfoExternalTemperature.text = externalTemperatureText;
        window.InfoExternalTemperature.fontSize = ConfigData.FontSize;

        // Suit Internal Temperature
        float internalTemperature = window._tempInternalK.ToFloat();
        string internalTemperatureText = internalTemperature.IsKelvinNil()
                ? "Nil"
                : internalTemperature.ToPreferredUnit(ConfigData.PreferredTemperatureUnit).ToStringPrecision();
        window.InfoInternalTemperature.text = internalTemperatureText;
        window.InfoInternalTemperature.fontSize = ConfigData.FontSize;

        // Suit Temperature Setting
        float temperatureSetting = suit?.OutputTemperature.ToFloat() ?? 0f;
        string temperatureSettingText = temperatureSetting.ToPreferredUnit(ConfigData.PreferredTemperatureUnit).ToStringPrecision();
        window.InfoInternalTemperatureSetting.text = temperatureSettingText;

        // Jetpack Delta Pressure
        float jetpackPressure = jetpackPropellant?.Pressure.ToFloat() ?? 0f;
        float pressureDelta = jetpackPressure - externalPressure;
        string pressureDeltaText = pressureDelta.ToPreferredUnit(ConfigData.PreferredTemperatureUnit).ToStringPrecision();
        window.InfoJetpackPressureDeltaText.text = pressureDeltaText;
        window.InfoJetpackPressureDeltaText.fontSize = ConfigData.FontSize;

        // Jetpack Thrust Setting
        float jetpackSetting = jetpack?.OutputSetting ?? 0f;
        int jetpackSettingRounded = Mathf.CeilToInt(jetpackSetting * 10f) * 5;
        string jetpackSettingText = $"{jetpackSettingRounded}%";
        window.InfoJetpackThrust.text = jetpackSettingText;

        // Character Velocity
        float velocity = human?.VelocityMagnitude ?? 0f;
        string velocityText = (velocity < 0.01f ? 0f : velocity).ToStringPrecision();
        window.InfoExternalVelocity.text = velocityText;
        window.InfoExternalVelocity.fontSize = ConfigData.FontSize;

        // Character Stun Damage
        float stunDamage = human?.DamageState.Stun ?? 0f;
        string stunDamageText = stunDamage.ToStringPrecision();
        window.CognitionPercentage.text = stunDamageText;
        window.CognitionPercentage.fontSize = ConfigData.FontSize;

        // Character Toxin Damage
        float toxinDamage = human?.DamageState.Toxic ?? 0f;
        string toxinDamageText = toxinDamage.ToStringPrecision();
        window.ToxinPercentage.text = toxinDamageText;
        window.ToxinPercentage.fontSize = ConfigData.FontSize;

        // Character Total Damage
        float totalDamage = human?.DamageState.TotalRatio * 100f ?? 0f;
        float healthLeft = 100f - totalDamage;
        string healthLeftText = healthLeft.ToStringPrecision();
        window.HealthPercentage.text = healthLeftText;
        window.HealthPercentage.fontSize = ConfigData.FontSize;

        // Character Hunger Left
        float hunger = human?.Nutrition ?? 0f;
        float hungerDivisor = human?.GetNutritionStorage() ?? 1f;
        float hungerClamp = hunger / hungerDivisor;
        float hungerLeft = hungerClamp * 100f;
        string hungerLeftText = hungerLeft.ToStringPrecision();
        window.HungerPercentage.text = hungerLeftText;
        window.HungerPercentage.fontSize = ConfigData.FontSize;

        // Character Hydration Left
        float hydration = human?.Hydration ?? 0f;
        float hydrationDivisor = human?.GetHydrationStorage() ?? 1f;
        float hydrationClamp = hydration / hydrationDivisor;
        float hydrationLeft = hydrationClamp * 100f;
        string hydrationLeftText = hydrationLeft.ToStringPrecision();
        window.HydrationPercentage.text = hydrationLeftText;
        window.HydrationPercentage.fontSize = ConfigData.FontSize;

        // Character Look Angle
        float eulerAnglesY = human?.EntityRotation.eulerAngles.y ?? 0f;
        float orientation = (eulerAnglesY + 180f) % 360f;
        string orientationText = orientation.ToStringPrecision();
        window.NavigationText.text = orientationText;
        window.NavigationText.fontSize = ConfigData.FontSize;
    }

    internal static void UpdateAnalyzer(ref AtmosAnalyser analyser, ref bool isGasPipe, ref string pressureValueText, ref string liquidVolumeValueText, ref string capacityValueText, ref string temperatureValueText, ref string energyConvectedText, ref string energyRadiatedText, ref string latentText, ref string stressText) {
        Atmosphere atmosphere = analyser.ScannedAtmosphere;
        float pressure = atmosphere.PressureGassesAndLiquidsInPa.ToPreferredUnit(ConfigData.PreferredPressureUnit);
        float temperature = atmosphere.Temperature.ToPreferredUnit(ConfigData.PreferredTemperatureUnit);
        float volume = atmosphere.Volume.ToPreferredUnit(ConfigData.PreferredVolumeUnit);
        float liquids = atmosphere.TotalVolumeLiquids.ToPreferredUnit(ConfigData.PreferredVolumeUnit);
        float stress = liquids / volume;
        string stressColor = "white";
        if (isGasPipe) {
            if (stress > 0.006f) {
                stressColor = "orange";
            }

            if (stress > 0.018f) {
                stressColor = "red";
            }
        }

        pressureValueText = pressure > 0
                ? pressure.ToPrecision().ToStringPrefix(Utilities.GetPressureSymbol(ConfigData.PreferredPressureUnit, true))
                : "N/A";

        temperatureValueText = atmosphere.Temperature > TemperatureKelvin.Zero
                ? temperature.ToPrecision().ToStringPrefix(Utilities.GetTemperatureSymbol(ConfigData.PreferredTemperatureUnit))
                : "N/A";

        capacityValueText = volume > 0
                ? volume.ToPrecision().ToStringPrefix(Utilities.GetVolumeSymbol(ConfigData.PreferredVolumeUnit))
                : "N/A";

        liquidVolumeValueText = liquids > 0
                ? liquids.ToPrecision().ToStringPrefix(Utilities.GetVolumeSymbol(ConfigData.PreferredVolumeUnit))
                : "N/A";

        string text = (stress / 0.02f * 100f).ToStringPrecision();
        stressText = isGasPipe ?
                $"<color={stressColor}>{text}%</color>"
                : "N/A";
    }

    internal static void UpdateMoleDisplays(ref AtmosAnalyser instance, ref Mole mole, ref Atmosphere atmosphere, ref string volumeTextColor, ref GasItem moleDisplay) {
        string quantity = mole.Quantity.ToStringPrefix("mol");
        moleDisplay.Moles = $"{quantity}";

        string volume = mole.Volume.ToStringPrefix(Utilities.GetVolumeSymbol(ConfigData.PreferredVolumeUnit));
        moleDisplay.Volume = $"<color={volumeTextColor}>{volume}</color>";

        string percent = (mole.Quantity / atmosphere.TotalMoles * 100f).ToStringPrecision();
        moleDisplay.Percent = $"{percent}%";

        GasItem.StateSymbolType stateSymbolType = GasItem.StateSymbolType.none;
        if (mole.Quantity > Chemistry.MINIMUM_QUANTITY_MOLES && atmosphere.Temperature <= mole.FreezingTemperature() + TemperatureKelvin.One) {
            stateSymbolType |= GasItem.StateSymbolType.freezing;
        }

        if (mole.CheckChangeState(atmosphere.PressureGassesAndLiquids) == GasItem.StateSymbolType.condensation) {
            stateSymbolType |= GasItem.StateSymbolType.condensation;
        }

        if (mole.CheckChangeState(atmosphere.PressureGassesAndLiquids) == GasItem.StateSymbolType.evaporation) {
            stateSymbolType |= GasItem.StateSymbolType.evaporation;
        }

        moleDisplay.SetSymbol(stateSymbolType);
        moleDisplay.VolumeValue = mole.Volume.ToFloat();
        moleDisplay.MolesValue = mole.Quantity.ToFloat();
        moleDisplay.IsActive = mole.Quantity > MoleQuantity.Zero;
    }

    /*internal static void DisplayGasInfo(ref StringBuilder stringBuilder, ref Pipe.ContentType contentType, ref Atmosphere atmosphere) {
        string none = GameStrings.None.AsColor("yellow");

        float temperature = atmosphere.Temperature.ToPreferredUnit(ConfigData.PreferredTemperatureUnit);
        float pressure = atmosphere.PressureGassesAndLiquidsInPa.ToPreferredUnit(ConfigData.PreferredTemperatureUnit);
        float volume = atmosphere.Volume.ToPreferredUnit(ConfigData.PreferredVolumeUnit);
        float liquidVolume = atmosphere.TotalVolumeLiquids.ToPreferredUnit(ConfigData.PreferredVolumeUnit);

        if (atmosphere.Temperature > TemperatureKelvin.Zero) {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.Temperature, temperature.ToStringPrefix(Utilities.GetTemperatureSymbol(ConfigData.PreferredTemperatureUnit), "yellow"));
        }
        else {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.Temperature, none);
        }

        if (atmosphere.PressureGassesAndLiquids > PressurekPa.Zero) {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.Pressure, pressure.ToStringPrefix(Utilities.GetPressureSymbol(ConfigData.PreferredPressureUnit), "yellow"));
        }
        else {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.Pressure, none);
        }

        if (atmosphere.Volume > VolumeLitres.One) {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.AtmosphereVolume, volume.ToStringPrefix(Utilities.GetVolumeSymbol(ConfigData.PreferredVolumeUnit), "yellow"));
        }
        else {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.AtmosphereVolume, none);
        }

        if (atmosphere.TotalVolumeLiquids > VolumeLitres.One) {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.LiquidsVolume, liquidVolume.ToStringPrefix(Utilities.GetVolumeSymbol(ConfigData.PreferredVolumeUnit), "yellow"));
        }
        else {
            StringManager.DisplayKeyValue(stringBuilder, GameStrings.LiquidsVolume, none);
        }
    }*/
}