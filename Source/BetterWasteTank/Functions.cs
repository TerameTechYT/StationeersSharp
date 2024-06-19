
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
        return suit != null && suit.WasteTankSlot.Contains(out GasCanister canister) ? canister : null;
    }

    public static Suit GetSuit(Human human)
    {
        return human != null && human.SuitSlot.Contains(out Suit suit) ? suit : null;
    }

    public static float GetWasteMaxPressure(Suit suit)
    {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.MaxPressure - Chemistry.OneAtmosphere ?? Suit.DEFUALT_MAX_WASTE_PRESSURE;
    }

    public static float GetWastePressure(Suit suit)
    {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.Pressure ?? 0;
    }

    public static bool GetWasteBroken(Suit suit)
    {
        GasCanister wasteCanister = GetWasteCanister(suit);

        return wasteCanister?.IsBroken ?? false;
    }

    public static bool IsWasteCritical(Suit suit)
    {
        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);
        bool wasteBroken = GetWasteBroken(suit);
        bool overThreshold = pressure != 0f && maxPressure != 0f && (pressure / maxPressure) >= Data.WasteCriticalRatio;

        return suit != null && (wasteBroken || overThreshold);
    }

    public static bool IsWasteCaution(Suit suit)
    {
        float pressure = GetWastePressure(suit);
        float maxPressure = GetWasteMaxPressure(suit);
        bool overThreshold = pressure != 0f && maxPressure != 0f && (pressure / maxPressure) >= Data.WasteCautionRatio;


        return suit != null && !IsWasteCritical(suit) && overThreshold;
    }

    public static void UpdateIcons(ref TMP_Text wasteText, ref Human human)
    {
        Suit suit = GetSuit(human);

        if (IsWasteCaution(suit) || IsWasteCritical(suit))
        {
            float pressure = GetWastePressure(suit);
            float maxPressure = GetWasteMaxPressure(suit);
            int fullRatio = pressure == 0f || maxPressure == 0f ? 0 : Mathf.RoundToInt(pressure / maxPressure);
            string text = $"{fullRatio}%";

            wasteText?.SetText(text);
        }
    }
}