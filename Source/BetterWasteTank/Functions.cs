#pragma warning disable CA1305

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using TMPro;
using UnityEngine;

namespace BetterWasteTank;

public static class Functions
{
    public static GasCanister GetWasteCanister(Suit suit)
    {
        return suit is not null && suit.WasteTankSlot.Contains(out GasCanister canister) ? canister : null;
    }

    public static Suit GetSuit(Human human)
    {
        return human is not null && human.SuitSlot.Contains(out Suit suit) ? suit : null;
    }

    public static float GetWasteMaxPressure(Suit suit)
    {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.MaxPressure - Chemistry.OneAtmosphere ?? 0f;
    }

    public static float GetWastePressure(Suit suit)
    {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.Pressure ?? 0;
    }

    public static bool IsWasteCritical(Suit suit)
    {
        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);

        return pressure == 0f || maxPressure == 0f || (pressure / maxPressure) >= Data.WasteCriticalRatio;
    }

    public static bool IsWasteCaution(Suit suit)
    {
        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);

        return !IsWasteCritical(suit) && (pressure / maxPressure) >= Data.WasteCautionRatio;
    }

    public static void UpdateIcons(ref TMP_Text wasteText, ref Human human)
    {
        Suit suit = GetSuit(human);

        if (IsWasteCaution(suit) || IsWasteCritical(suit))
        {
            float pressure = GetWastePressure(suit);
            float maxPressure = GetWasteMaxPressure(suit);
            int fullRatio = Mathf.RoundToInt(pressure / maxPressure);
            string text = fullRatio.ToString("p");

            wasteText?.SetText(text);
        }
    }
}