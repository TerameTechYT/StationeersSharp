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

    internal static float GetWasteMaxPressure(Suit suit) {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.MaxPressure.ToFloat() - Chemistry.OneAtmosphere.ToFloat() ??
               Suit.DEFAULT_MAX_WASTE_PRESSURE;
    }

    private static float GetWastePressure(Suit suit) {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.Pressure.ToFloat() ?? 0;
    }

    private static bool GetWasteBroken(Suit suit) {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.IsBroken ?? false;
    }

    private static bool GetWasteNull(Suit suit) {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister == null;
    }

    internal static bool IsWasteCritical(Suit suit) {
        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);
        bool wasteBroken = GetWasteBroken(suit);
        bool wasteNull = GetWasteNull(suit);
        bool overThreshold = pressure != 0f && maxPressure != 0f && pressure / maxPressure >= Data.WasteCriticalRatio;

        return wasteNull || wasteBroken || overThreshold;
    }

    internal static bool IsWasteCaution(Suit suit) {
        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);
        bool overThreshold = pressure != 0f && maxPressure != 0f && pressure / maxPressure >= Data.WasteCautionRatio;


        return !IsWasteCritical(suit) && overThreshold;
    }

    internal static void UpdateIcons(ref TMP_Text wasteText, ref Human human) {
        Suit suit = GetSuit(human);

        if (!IsWasteCaution(suit) && !IsWasteCritical(suit))
            return;

        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);
        int fullRatio = pressure == 0f || maxPressure == 0f ? 0 : Mathf.RoundToInt(pressure / maxPressure);
        string text = $"{fullRatio}%";

        wasteText.SetText(text);
    }
}