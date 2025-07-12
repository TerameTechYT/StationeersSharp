namespace BetterFabricator;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.LoadXmlFileData))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> WorldManagerLoadXmlFileDataTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            List<CodeInstruction> instructionsList = [.. instructions];

            // we want to override the original data with our own, so we find the field with the list of recipies
            Type gameData = typeof(WorldManager.GameData);
            FieldInfo fabricatorRecipes = gameData.GetField("FabricatorRecipes");

            // next we find our list to change it to
            Type modData = typeof(ConfigData);
            FieldInfo modFabricatorRecipes = modData.GetField("FabricatorRecipes");

            instructionsList.Manipulator(
                    // we loop through our instructions until we find an instruction thast loads the original field
                    (instruction) => instruction.LoadsField(fabricatorRecipes),

                    // next we set the operand (or value) to our modified list
                    (instruction) => instruction.operand = modFabricatorRecipes
            );

            // now obviously, we only want to populate the fabricator recipes once all other ones are loaded
            // all of the foreach loops are wrapped in try {} finally {} blocks, so we will find the index of the last finally block
            int endFinallyLastIndex = instructionsList.FindLastIndex((instruction) => instruction.OpcodeIs(OpCodes.Endfinally));
            // insert our custom instructions directly after the last finally block
            instructionsList.InsertRange(endFinallyLastIndex + 1, [
                    // insert instruction that will call our custom function after all game data for mod (or base) is loaded
                    CodeInstruction.Call(typeof(Functions), nameof(Functions.LoadFabricatorRecipes))
            ]);

            return instructionsList;
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return instructions;
    }
}