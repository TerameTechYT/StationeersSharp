#region

#endregion

namespace BetterWasteTank;

internal static class Functions {
    internal static Suit? GetSuit(Human human) => human.SuitSlot.Contains(out Suit suit) ? suit : null;

    internal static float GetCanisterFullRatio(GasCanister canister) => canister == null ? 0.0f : (canister.Pressure / canister.MaxPressure).ToFloat();
    internal static float GetCanisterDelta(GasCanister canister) => canister == null ? 0.0f : (canister.MaxPressure - canister.Pressure).ToFloat();
    internal static float GetCanisterMax(GasCanister canister) => canister == null ? 10132.5f : canister.MaxPressure.ToFloat();
    internal static float GetCanisterMoles(GasCanister canister, Chemistry.GasType gasType) => canister == null ? 0.0f : canister.InternalAtmosphere.GetMoles(gasType);
    internal static float GetCanisterMoles(GasCanister canister, SpeciesClass species) => GetCanisterMoles(canister, Utilities.GetSpeciesAirType(species));

    internal static bool IsWasteCritical(ref Suit suit) => IsWasteCritical(suit.WasteTank);
    internal static bool IsWasteCritical(GasCanister canister) => canister == null || GetCanisterFullRatio(canister) >= ConfigData.WasteCriticalRatio;

    internal static bool IsWasteCaution(ref Suit suit) => !IsWasteCritical(ref suit) && IsWasteCaution(suit.WasteTank);
    internal static bool IsWasteCaution(GasCanister canister) => canister != null && GetCanisterFullRatio(canister) >= ConfigData.WasteCautionRatio;

    internal static bool IsAirCritical(ref Suit suit, SpeciesClass species) => IsAirCritical(suit.AirTank, Utilities.GetSpeciesAirType(species));
    internal static bool IsAirCritical(GasCanister canister, Chemistry.GasType breathable) => canister == null || GetCanisterMoles(canister, breathable) <= ConfigData.AirTankMolesCritical;

    internal static bool IsAirCaution(ref Suit suit, SpeciesClass species) => !IsAirCritical(ref suit, species) && IsAirCaution(suit.AirTank, Utilities.GetSpeciesAirType(species));
    internal static bool IsAirCaution(GasCanister canister, Chemistry.GasType breathable) => canister != null && GetCanisterMoles(canister, breathable) <= ConfigData.AirTankMolesCaution;

    internal static float GetMoles(this Atmosphere atmosphere, Chemistry.GasType gasType) => ConfigData.AirCountOnlyBreathable ? atmosphere.PartialMoles(gasType) : atmosphere.TotalMoles.ToFloat();
}