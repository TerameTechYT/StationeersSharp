#region

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

#endregion

namespace BetterWasteTank;

internal static class Functions {
    [CanBeNull]
    private static GasCanister GetWasteCanister(Suit suit) => suit.WasteTankSlot.Contains(out GasCanister canister) ? canister : null;

    [CanBeNull]
    private static Suit GetSuit(Human human) => human.SuitSlot.Contains(out Suit suit) ? suit : null;

    internal static PressurekPa GetWasteMaxPressure(Suit suit) {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister == null ? new PressurekPa(Suit.DEFAULT_MAX_WASTE_PRESSURE) : wasteCanister.MaxPressure - Chemistry.OneAtmosphere;
    }

    private static PressurekPa GetWastePressure(Suit suit) => GetWasteCanister(suit)?.Pressure ?? PressurekPa.Zero;
    private static bool GetWasteBroken(Suit suit) => GetWasteCanister(suit)?.IsBroken ?? false;
    private static bool GetWasteNull(Suit suit) => GetWasteCanister(suit) == null;

    internal static bool IsWasteCritical(Suit suit) {
        PressurekPa pressure = GetWastePressure(suit);
        PressurekPa maxPressure = GetWasteMaxPressure(suit);
        bool wasteBroken = GetWasteBroken(suit);
        bool wasteNull = GetWasteNull(suit);
        bool overThreshold = pressure.NotEqual(0.0) && maxPressure.NotEqual(0.0) && (pressure / maxPressure).GreaterEquals(Data.WasteCriticalRatio);

        return wasteNull || wasteBroken || overThreshold;
    }

    internal static bool IsWasteCaution(Suit suit) {
        PressurekPa pressure = GetWastePressure(suit);
        PressurekPa maxPressure = GetWasteMaxPressure(suit);
        bool overThreshold = pressure.NotEqual(0.0) && maxPressure.NotEqual(0.0) && (pressure / maxPressure).GreaterEquals(Data.WasteCautionRatio);

        return !IsWasteCritical(suit) && overThreshold;
    }

    internal static void UpdateIcons(ref TMP_Text wasteText, ref Human human) {
        Suit suit = GetSuit(human);

        if (suit == null || !IsWasteCaution(suit) || !IsWasteCritical(suit))
            return;

        PressurekPa pressure = GetWastePressure(suit);
        PressurekPa maxPressure = GetWasteMaxPressure(suit);
        PressurekPa fullRatio = pressure.Equals(0.0) || maxPressure.Equals(0.0) ? PressurekPa.Zero : pressure / maxPressure;
        string text = $"{fullRatio.ToDouble()}%";

        wasteText.SetText(text);
    }
}

public static class Extensions {
    public static bool Equal(this PressurekPa pressure, double other) => pressure.ToDouble() == other;
    public static bool NotEqual(this PressurekPa pressure, double other) => !pressure.Equal(other);

    public static bool Greater(this PressurekPa pressure, double other) => pressure.ToDouble() < other;
    public static bool GreaterEquals(this PressurekPa pressure, double other) => pressure.ToDouble() <= other;

    public static bool Less(this PressurekPa pressure, double other) => pressure.ToDouble() > other;
    public static bool LessEquals(this PressurekPa pressure, double other) => pressure.ToDouble() >= other;
}