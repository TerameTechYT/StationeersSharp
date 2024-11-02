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
    private static GasCanister GetWasteCanister([CanBeNull] Suit suit) {
        return suit != null && suit.WasteTankSlot.Contains(out GasCanister canister) ? canister : null;
    }

    [CanBeNull]
    private static Suit GetSuit([CanBeNull] Human human) {
        return human != null && human.SuitSlot.Contains(out Suit suit) ? suit : null;
    }

    internal static float GetWasteMaxPressure(Suit suit) {
        var wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.MaxPressure.ToFloat() - Chemistry.OneAtmosphere.ToFloat() ??
               Suit.DEFAULT_MAX_WASTE_PRESSURE;
    }

    private static float GetWastePressure(Suit suit) {
        var wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.Pressure.ToFloat() ?? 0;
    }

    private static bool GetWasteBroken(Suit suit) {
        var wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.IsBroken ?? false;
    }

    private static bool GetWasteNull(Suit suit) {
        var wasteCanister = GetWasteCanister(suit);

        return wasteCanister == null;
    }

    internal static bool IsWasteCritical(Suit suit) {
        var pressure = GetWastePressure(suit);
        var maxPressure = GetWasteMaxPressure(suit);
        var wasteBroken = GetWasteBroken(suit);
        var wasteNull = GetWasteNull(suit);
        var overThreshold = pressure != 0f && maxPressure != 0f && pressure / maxPressure >= Data.WasteCriticalRatio;

        return suit != null && (wasteBroken || wasteNull || overThreshold);
    }

    internal static bool IsWasteCaution(Suit suit) {
        var pressure = GetWastePressure(suit);
        var maxPressure = GetWasteMaxPressure(suit);
        var overThreshold = pressure != 0f && maxPressure != 0f && pressure / maxPressure >= Data.WasteCautionRatio;


        return suit != null && !IsWasteCritical(suit) && overThreshold;
    }

    internal static void UpdateIcons(ref TMP_Text wasteText, ref Human human) {
        var suit = GetSuit(human);

        if (!IsWasteCaution(suit) && !IsWasteCritical(suit))
            return;

        var pressure = GetWastePressure(suit);
        var maxPressure = GetWasteMaxPressure(suit);
        var fullRatio = pressure == 0f || maxPressure == 0f ? 0 : Mathf.RoundToInt(pressure / maxPressure);
        var text = $"{fullRatio}%";

        wasteText?.SetText(text);
    }
}