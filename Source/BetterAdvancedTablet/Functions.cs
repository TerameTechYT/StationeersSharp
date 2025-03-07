



using SimpleSpritePacker;

namespace BetterAdvancedTablet;

public static class Functions {
    internal static Slot CloneSlot(Slot slot) => new() {
        StringKey = slot.StringKey,
        StringHash = slot.StringHash,
        UseInternalAtmosphere = slot.UseInternalAtmosphere,
        RealWorldScale = slot.RealWorldScale,
        ScaleMultiplier = slot.ScaleMultiplier,
        Type = slot.Type,
        Interactable = slot.Interactable,
        OccupantCastsShadows = slot.OccupantCastsShadows,
        HidesOccupant = slot.HidesOccupant,
        IsHiddenInSeat = slot.IsHiddenInSeat,
        IsInteractable = slot.IsInteractable,
        IsSwappable = slot.IsSwappable,
        AllowDragging = slot.AllowDragging,
        IsLocked = slot.IsLocked,
        Action = slot.Action,
        OccupantAlwaysVisible = slot.OccupantAlwaysVisible,
    };

    /*internal static ControlsGroup RegisterControlsGroup() => new(Data.ModName);
    internal static List<KeyItem> RegisterKeys() => [
        new KeyItem(Data.NextCartridge, KeyCode.PageUp),
        new KeyItem(Data.PrevCartridge, KeyCode.PageDown),
    ];*/

    internal static void ToNextCartridge(ref AdvancedTablet advancedTablet) {
        bool alt = KeyManager.GetButton(KeyMap.QuantityModifier);
        Interaction interaction = new(InventoryManager.Parent, InventoryManager.ActiveHandSlot, CursorManager.CursorThing, alt);
        Interactable interactable = alt ? advancedTablet.InteractButton2 : advancedTablet.InteractButton1;
        advancedTablet.InteractWith(interactable, interaction);
    }

    internal static Atmosphere GetScannedAtmosphere(ref AtmosAnalyser analyzer, ref string selectedText) {
        Thing cursorThing = CursorManager.CursorThing;

        if (cursorThing == null || cursorThing.RootParent && cursorThing.RootParent.HasAuthority) {
            return analyzer.WorldAtmosphere;
        }

        if (cursorThing is GasTankStorage gasTankStorage) {
            Atmosphere totalAtmosphere = new();
            foreach (GasCanister canister in gasTankStorage.ConnectedGasCanisters) {
                totalAtmosphere.Add(canister.InternalAtmosphere.GasMixture);
                totalAtmosphere.Volume += canister.InternalAtmosphere.Volume;
            }

            totalAtmosphere.Thing = gasTankStorage;
            selectedText = gasTankStorage.DisplayName.ToUpperInvariant();
            return totalAtmosphere;
        }

        if (cursorThing is INetworkedAtmospherics networkedAtmospherics && networkedAtmospherics.StructureNetwork is AtmosphericsNetwork atmosphericsNetwork) {
            return atmosphericsNetwork.Atmosphere;
        }

        if (cursorThing is VendingMachineRefrigerated vendingMachine && vendingMachine.HasReadableAtmosphere) {
            return vendingMachine.InternalAtmosphere;
        }

        if (cursorThing is Human human) {
            Atmosphere totalAtmosphere = new();
            totalAtmosphere.Thing = human;

            if (human.HelmetSlot.Contains<GasMask>(out GasMask mask) && mask.HasReadableAtmosphere) {
                totalAtmosphere.Add(mask.InternalAtmosphere.GasMixture);
                totalAtmosphere.Volume += mask.InternalAtmosphere.Volume;
            }

            if (human.SuitSlot.Contains<Suit>(out Suit suit) && suit.HasReadableAtmosphere) {
                totalAtmosphere.Add(suit.InternalAtmosphere.GasMixture);
                totalAtmosphere.Volume += suit.InternalAtmosphere.Volume;
            }

            selectedText = human.DisplayName.ToUpperInvariant();
            return totalAtmosphere;
        }

        Atmosphere atmosphere = new();
        Traverse traverse = Traverse.Create(cursorThing);
        Atmosphere internalAtmosphere1 = traverse.Field("InternalAtmosphere2")?.GetValue<Atmosphere>();
        Atmosphere internalAtmosphere2 = traverse.Field("InternalAtmosphere2")?.GetValue<Atmosphere>();
        Atmosphere internalAtmosphere3 = traverse.Field("InternalAtmosphere3")?.GetValue<Atmosphere>();

        if (cursorThing.InternalAtmosphere != null) {
            atmosphere.Add(cursorThing.InternalAtmosphere.GasMixture);
            atmosphere.Volume += cursorThing.InternalAtmosphere.Volume;
        }

        if (internalAtmosphere1 != null) {
            atmosphere.Add(internalAtmosphere1.GasMixture);
            atmosphere.Volume += internalAtmosphere1.Volume;
        }

        if (internalAtmosphere2 != null) {
            atmosphere.Add(internalAtmosphere2.GasMixture);
            atmosphere.Volume += internalAtmosphere2.Volume;
        }

        if (internalAtmosphere3 != null) {
            atmosphere.Add(internalAtmosphere3.GasMixture);
            atmosphere.Volume += internalAtmosphere3.Volume;
        }

        atmosphere.Thing = cursorThing;
        selectedText = cursorThing.DisplayName.ToUpperInvariant();

        return atmosphere;
    }
}