// ReSharper disable InconsistentNaming

#pragma warning disable CA1062

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using TMPro;
using UnityEngine;

namespace BetterWasteTank;

public static class Functions
{
    public static bool IsWasteCritical(Suit suit)
    {
        return (suit != null && suit.WasteTank == null) || (suit != null &&
                                                            suit.WasteTank != null &&
                                                            suit.WasteTank.Pressure >=
                                                            suit.WasteMaxPressure * 0.975f);
    }

    public static bool IsWasteCaution(Suit suit)
    {
        return suit != null
&& suit.WasteTank != null &&
               suit.WasteTank.Pressure >=
               suit.WasteMaxPressure * 0.75f;
    }

    public static void UpdateIcons(ref TMP_Text wasteText, ref Human human)
    {
        if (human.Suit != null && human.Suit.WasteTank != null && wasteText != null)
        {
            string percent = Mathf.RoundToInt(human.Suit.WasteTank
                ? human.Suit.WasteTank.Pressure / human.Suit.WasteMaxPressure * 100f
                : 0f) + "%";

            wasteText.text = percent;
        }

        if (human.SuitSlot.Contains(out Suit suit) && human.Suit.WasteTank != null)
        {
            suit.WasteMaxPressure = suit.WasteTank.MaxPressure - 101f;
        }
    }

    public static float SuitAirConditioner(ref InternalAtmosphereConditioner conditioner,
        ref Atmosphere selectedAtmosphere)
    {
        if (conditioner.WasteTank.IsOpen)
        {
            OnServer.Interact(conditioner.WasteTank.InteractOpen, 0, true);
        }

        float desiredEnergy = conditioner.OutputTemperature * selectedAtmosphere.GasMixture.HeatCapacity;
        float desiredEnergyDelta = desiredEnergy - selectedAtmosphere.GasMixture.TotalEnergy;
        float usedEnergy = Mathf.Abs(Mathf.Clamp(desiredEnergyDelta, -conditioner.MaxEnergy * conditioner.Efficiency,
            conditioner.MaxEnergy * conditioner.Efficiency));

        if (desiredEnergyDelta < 0.0) // cooling suit
        {
            conditioner.Battery.PowerStored -= usedEnergy * 0.01f;
            usedEnergy = selectedAtmosphere.GasMixture.RemoveEnergy(usedEnergy * 1.015f);
            conditioner.WasteTank.InternalAtmosphere.GasMixture.AddEnergy(usedEnergy / 3f);
            return usedEnergy / 2000f;
        }

        // heating suit
        conditioner.Battery.PowerStored -= usedEnergy * 0.05f;
        selectedAtmosphere.GasMixture.AddEnergy(usedEnergy);
        return 0f;
    }
}