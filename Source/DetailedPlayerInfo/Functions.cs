#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Clothing.Suits;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using StationeersLibrary;
using TMPro;
using UnityEngine;
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
        ISuit? suit = human.SuitSlot.Get<ISuit>();
        BatteryCell? suitBattery = suit.Battery;

        // Jetpack stuff
        Jetpack? jetpack = human?.BackpackSlot.Get<Jetpack>();
        GasCanister? jetpackPropellant = jetpack?.PropellentSlot.Get<GasCanister>();

        JetpackElectric? jetpackElectric = jetpack is JetpackElectric ? jetpack as JetpackElectric : null;
        BatteryCell? jetpackBattery = jetpackElectric?.Battery;

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
            Suit? normalSuit = suit as Suit;
            GasFilter? filter1 = normalSuit?.Filter1;
            GasFilter? filter2 = normalSuit?.Filter2;
            GasFilter? filter3 = normalSuit?.Filter3;
            AdvancedSuit? advancedSuit = suit as AdvancedSuit;
            HARMSuit? harmSuit = suit as HARMSuit;
            GasFilter? filter4 = advancedSuit?.Filter4 ?? harmSuit?.FilterSlot4.Get<GasFilter>();

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

        float jetpackCharge = jetpackBattery?.PowerRatio ?? 0f;
        float jetpackChargeDisplay = jetpackCharge * 100f;
        string jetpackChargeText = jetpackChargeDisplay.ToStringPrecision();
        window.InfoJetpackPowerText.text = jetpackChargeText;
        window.InfoJetpackPowerText.fontSize = ConfigData.FontSize;

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
        if (ConfigData.AlwaysDisplayCognition) {
            window.CognitionPercentageObject.SetActive(true);
        }

        // Character Toxin Damage
        float toxinDamage = human?.DamageState.Toxic ?? 0f;
        string toxinDamageText = toxinDamage.ToStringPrecision();
        window.ToxinPercentage.text = toxinDamageText;
        window.ToxinPercentage.fontSize = ConfigData.FontSize;
        window.ToxinPercentageObject.SetActive(true);
        if (ConfigData.AlwaysDisplayToxin) {
            window.ToxinPercentageObject.SetActive(true);
        }

        // Character Total Damage
        float totalDamage = (human?.DamageState.TotalRatio * 100f) ?? 0f;
        float healthLeft = 100f - totalDamage;
        string healthLeftText = healthLeft.ToStringPrecision();
        window.HealthPercentage.text = healthLeftText;
        window.HealthPercentage.fontSize = ConfigData.FontSize;
        if (ConfigData.AlwaysDisplayHealth) {
            window.HealthPercentageObject.SetActive(true);
        }

        // Character Body Health
        // TODO: add text for each limb health, maybe a tooltip with the limb healths
        if (ConfigData.AlwaysDisplayBodyHealth) {
            window.PersonDamageObject.SetActive(true);
        }

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

        // Character Santiation
        float sanitation = human?.SanitationRatio ?? 0f;
        float sanitationDisplay = sanitation * 100f;
        string sanitationText = sanitationDisplay.ToStringPrecision();
        window.WastePercentage.text = sanitationText;
        window.WastePercentage.fontSize = ConfigData.FontSize;
        if (ConfigData.AlwaysDisplaySanitation) {
            window.WastePercentageObject.SetActive(true);
        }

        // Character Mood
        float mood = human?.Mood ?? 0f;
        float moodDisplay = mood * 100f;
        string moodText = moodDisplay.ToStringPrecision();
        window.MoodText.text = moodText;
        window.MoodText.fontSize = ConfigData.FontSize;

        // Character Hygiene
        float hygiene = human?.Hygiene ?? 0f;
        float hygieneDisplay = hygiene * 100f;
        string hygieneText = hygieneDisplay.ToStringPrecision();
        window.HygieneText.text = hygieneText;
        window.HygieneText.fontSize = ConfigData.FontSize;

        // Character Food Quality
        float foodQuality = human?.FoodQuality ?? 0f;
        float foodQualityDisplay = foodQuality * 100f;
        string foodQualityText = foodQualityDisplay.ToStringPrecision();
        window.FoodQualityText.text = foodQualityText;
        window.FoodQualityText.fontSize = ConfigData.FontSize;

        // Character Look Angle
        float eulerAnglesY = human?.EntityRotation.eulerAngles.y ?? 0f;
        float orientation = (eulerAnglesY + 180f) % 360f;
        string orientationText = orientation.ToStringPrecision();
        window.NavigationText.text = orientationText;
        window.NavigationText.fontSize = ConfigData.FontSize;

        // Room Number Display
        ExteriorState exteriorState = human?.GetExteriorState() ?? ExteriorState.World;
        string exteriorStateText = EnumCollections.ExteriorStates.GetName(exteriorState);
        string externalText = exteriorStateText;

        if (exteriorState == ExteriorState.Room && human?.Room is Room room && room.IsValid()) {
            externalText = $"{exteriorStateText} {room.RoomId}";
        }

        window.HeaderExternalText.text = externalText;
        //window.HeaderExternalText.fontSize = ConfigData.FontSize;
    }

    /*internal static void UpdateAnalyzer(ref AtmosAnalyser analyser, ref bool isGasPipe, ref string pressureValueText, ref string liquidVolumeValueText, ref string capacityValueText, ref string temperatureValueText, ref string energyConvectedText, ref string energyRadiatedText, ref string latentText, ref string stressText) {
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

    internal static void DisplayGasInfo(ref StringBuilder stringBuilder, ref Pipe.ContentType contentType, ref Atmosphere atmosphere) {
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