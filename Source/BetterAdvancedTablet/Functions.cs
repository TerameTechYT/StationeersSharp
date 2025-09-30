#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Networks;
using Objects.Electrical;
using StationeersLibrary;

#endregion



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

    internal static void PrefabsLoaded() {
        try {
            AdvancedTablet? tabletPrefab = Prefab.AllPrefabs.Find((thing) => thing is AdvancedTablet) as AdvancedTablet;
            if (tabletPrefab == null) {
                return;
            }

            Plugin.Instance.LogDebug($"Found {ConfigData.AdvancedTabletPrefabName} Prefab!");
            tabletPrefab.AllowSelfUse = true;

            Slot template = tabletPrefab.Slots.Find((slot) => slot.Type == Slot.Class.Cartridge);
            tabletPrefab.Slots[1].StringKey = template.StringKey;
            tabletPrefab.Slots[1].StringHash = template.StringHash;
            for (int i = 0; i < ConfigData.AdditionalTabletSlots; i++) {
                tabletPrefab.Slots.Add(Functions.CloneSlot(template));
            }

            Plugin.Instance.LogDebug($"Added {ConfigData.AdditionalTabletSlots} slots to {ConfigData.AdvancedTabletPrefabName} Prefab");
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    internal static void UsePrimary(ref AdvancedTablet advancedTablet) {
        if (KeyManager.GetMouseDown("Primary")) {
            bool alt = KeyManager.GetButton(KeyMap.QuantityModifier);
            Interaction interaction = new(InventoryManager.Parent, InventoryManager.ActiveHandSlot, CursorManager.CursorThing, alt);
            Interactable interactable = alt ? advancedTablet.InteractButton2 : advancedTablet.InteractButton1;

            OnServer.InteractWith(interactable, interaction);
        }
    }

    internal static Atmosphere GetScannedAtmosphere(ref AtmosAnalyser analyzer, ref string selectedText) {
        Thing cursorThing = CursorManager.CursorThing;

        if (cursorThing == null || (cursorThing.RootParent && cursorThing.RootParent.HasAuthority)) {
            return analyzer.WorldAtmosphere;
        }

        if (cursorThing is GasTankStorage gasTankStorage) {
            Atmosphere totalAtmosphere = new() {
                Thing = gasTankStorage,
            };

            foreach (GasCanister canister in gasTankStorage.ConnectedGasCanisters) {
                totalAtmosphere.Add(canister.InternalAtmosphere.GasMixture);
                totalAtmosphere.Volume += canister.InternalAtmosphere.Volume;
            }

            selectedText = gasTankStorage.DisplayName.ToUpperInvariant();
            return totalAtmosphere;
        }

        if (cursorThing is StirlingEngine stirling && stirling.HasReadableAtmosphere) {
            Atmosphere totalAtmosphere = new() {
                Thing = stirling,
            };

            foreach (Slot slot in stirling.Slots) {
                if (slot.Contains<GasCanister>(out GasCanister gasCanister)) {
                    totalAtmosphere.Add(gasCanister.InternalAtmosphere.GasMixture);
                    totalAtmosphere.Volume += gasCanister.InternalAtmosphere.Volume;
                }
            }

            totalAtmosphere.Add(stirling.InternalAtmosphere.GasMixture);
            totalAtmosphere.Volume += stirling.InternalAtmosphere.Volume;

            selectedText = stirling.DisplayName.ToUpperInvariant();
            return totalAtmosphere;
        }

        if (cursorThing is Human human) {
            Atmosphere totalAtmosphere = new() {
                Thing = human
            };

            if (human.HelmetSlot.Contains(out GasMask mask) && mask.HasReadableAtmosphere) {
                totalAtmosphere.Add(mask.InternalAtmosphere.GasMixture);
                totalAtmosphere.Volume += mask.InternalAtmosphere.Volume;
            }

            if (human.SuitSlot.Contains(out Suit suit) && suit.HasReadableAtmosphere) {
                totalAtmosphere.Add(suit.InternalAtmosphere.GasMixture);
                totalAtmosphere.Volume += suit.InternalAtmosphere.Volume;
            }

            selectedText = human.DisplayName.ToUpperInvariant();
            return totalAtmosphere;
        }

        if (cursorThing is INetworkedAtmospherics networkedAtmospherics && networkedAtmospherics.StructureNetwork is AtmosphericsNetwork atmosphericsNetwork) {
            selectedText = atmosphericsNetwork.DisplayName.ToUpperInvariant();
            return atmosphericsNetwork.Atmosphere;
        }

        if (cursorThing is Fridge fridge && fridge.HasReadableAtmosphere) {
            selectedText = fridge.DisplayName.ToUpperInvariant();
            return fridge.InternalAtmosphere;
        }

        if (cursorThing is FridgePowered fridge2 && fridge2.HasReadableAtmosphere) {
            selectedText = fridge2.DisplayName.ToUpperInvariant();
            return fridge2.InternalAtmosphere;
        }

        if (cursorThing is VendingMachineRefrigerated vendingMachine && vendingMachine.HasReadableAtmosphere) {
            selectedText = vendingMachine.DisplayName.ToUpperInvariant();
            return vendingMachine.InternalAtmosphere;
        }

        Atmosphere atmosphere = new() {
            Thing = cursorThing,
        };

        Traverse traverse = Traverse.Create(cursorThing);
        Atmosphere? internalAtmosphere1 = traverse.Field("InternalAtmosphere1")?.GetValue<Atmosphere>();
        Atmosphere? internalAtmosphere2 = traverse.Field("InternalAtmosphere2")?.GetValue<Atmosphere>();
        Atmosphere? internalAtmosphere3 = traverse.Field("InternalAtmosphere3")?.GetValue<Atmosphere>();

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

        selectedText = cursorThing.DisplayName.ToUpperInvariant();
        return atmosphere;
    }
}