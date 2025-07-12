namespace BetterInventory;

public static class Functions {
    public static void CustomSmartStow(ref Slot selectedSlot) {
        Stackable selectedOccupant = selectedSlot.Get<Stackable>();
        DynamicThing leftHandOccupant = InventoryManager.LeftHandSlot.Get();
        DynamicThing rightHandOccupant = InventoryManager.RightHandSlot.Get();

        if (leftHandOccupant != selectedOccupant && rightHandOccupant != selectedOccupant) {
            return;
        }

        Slot slotToFill = Functions.GetSlotToFill(selectedOccupant, selectedSlot.Type);
    }

    public static Slot? GetSlotToFill(Stackable stackable, Slot.Class slotType) {
        if (stackable == null) {
            return null;
        }

        foreach (Slot slot in InventoryManager.ParentHuman.Slots) {
            if (slot.IsEmpty()) {
                continue;
            }

            Stackable slotOccupant = slot.Get<Stackable>();

            int targetSpaceLeft = slotOccupant.MaxQuantity - slotOccupant.Quantity;

            if (slot.Type != slotType || slot.IsHandSlot || targetSpaceLeft < stackable.Quantity) {
                continue;
            }

            return slot;
        }

        return null;
    }
}