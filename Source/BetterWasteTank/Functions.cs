#region

#endregion

namespace BetterWasteTank;

internal static class Functions {
    internal static Suit GetSuit(Human human) => human.SuitSlot.Contains(out Suit suit) ? suit : null;

    internal static double GetCanisterRatio(GasCanister canister) => canister == null ? 0.0 : canister.Pressure.ToDouble() / canister.MaxPressure.ToDouble();
    internal static double GetCanisterMax(GasCanister canister) => canister == null ? 10132.5f : canister.MaxPressure.ToDouble();

    internal static bool IsWasteCritical(Suit suit) => IsWasteCritical(suit.WasteTank);
    internal static bool IsWasteCritical(GasCanister canister) => canister == null || GetCanisterRatio(canister) >= Data.WasteCriticalRatio;

    internal static bool IsWasteCaution(Suit suit) => IsWasteCaution(suit.WasteTank);
    internal static bool IsWasteCaution(GasCanister canister) => canister != null && GetCanisterRatio(canister) >= Data.WasteCautionRatio;

    /*internal static bool IsAirCritical(Suit suit) => IsAirCritical(suit.AirTank);
    internal static bool IsAirCritical(GasCanister canister) => canister == null || GetCanisterRatio(canister) < Data.AirCriticalRatio;

    internal static bool IsAirCaution(Suit suit) => IsAirCaution(suit.AirTank);
    internal static bool IsAirCaution(GasCanister canister) => canister != null && GetCanisterRatio(canister) < Data.AirCautionRatio;*/
}