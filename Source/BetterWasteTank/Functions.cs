#region

#endregion

namespace BetterWasteTank;

internal static class Functions {
    private static float AirTankMolesCritical => Human.MolesPerMinute.ToFloat() * Data.AirCriticalMoles;
    private static float AirTankMolesCaution => Human.MolesPerMinute.ToFloat() * Data.AirCautionMoles;

    internal static Suit GetSuit(Human human) => human.SuitSlot.Contains(out Suit suit) ? suit : null;

    internal static float GetCanisterFullRatio(GasCanister canister) => canister == null ? 0.0f : (canister.Pressure / canister.MaxPressure).ToFloat();
    internal static float GetCanisterDelta(GasCanister canister) => canister == null ? 0.0f : (canister.MaxPressure - canister.Pressure).ToFloat();
    internal static float GetCanisterMax(GasCanister canister) => canister == null ? 10132.5f : canister.MaxPressure.ToFloat();

    internal static bool IsWasteCritical(Suit suit) => IsWasteCritical(suit.WasteTank);
    internal static bool IsWasteCritical(GasCanister canister) => canister == null || GetCanisterFullRatio(canister) >= Data.WasteCriticalRatio;

    internal static bool IsWasteCaution(Suit suit) => !IsWasteCritical(suit) && IsWasteCaution(suit.WasteTank);
    internal static bool IsWasteCaution(GasCanister canister) => canister != null && GetCanisterFullRatio(canister) >= Data.WasteCautionRatio;

    internal static bool IsAirCritical(Suit suit, SpeciesClass species) => IsAirCritical(suit.AirTank, species.GetSpeciesAirType());
    internal static bool IsAirCritical(GasCanister canister, Chemistry.GasType breathable) => canister == null || canister.InternalAtmosphere.GetMoles(breathable) <= AirTankMolesCritical;

    internal static bool IsAirCaution(Suit suit, SpeciesClass species) => !IsAirCritical(suit, species) && IsAirCaution(suit.AirTank, species.GetSpeciesAirType());
    internal static bool IsAirCaution(GasCanister canister, Chemistry.GasType breathable) => canister != null && canister.InternalAtmosphere.GetMoles(breathable) <= AirTankMolesCaution;

    internal static float GetMoles(this Atmosphere atmosphere, Chemistry.GasType gasType) => Data.AirCountOnlyBreathable ? atmosphere.PartialMoles(gasType) : atmosphere.TotalMoles.ToFloat();
}