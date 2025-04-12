namespace BetterFabricator;

public static class Functions {
    public static void LoadFabricatorRecipes() {
        Traverse traverse = Traverse.Create(Autolathe.RecipeComparable);
        List<WorldManager.RecipeData> autolatheRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in autolatheRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(HydraulicPipeBender.RecipeComparable);
        List<WorldManager.RecipeData> hydraulicPipeBenderRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in hydraulicPipeBenderRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(ElectronicsPrinter.RecipeComparable);
        List<WorldManager.RecipeData> electronicsPrinterRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in electronicsPrinterRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(ToolManufactory.RecipeComparable);
        List<WorldManager.RecipeData> toolManufactoryRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in toolManufactoryRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(RocketManufactory.RecipeComparable);
        List<WorldManager.RecipeData> rocketManufactoryRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in rocketManufactoryRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(SecurityPrinter.RecipeComparable);
        List<WorldManager.RecipeData> securityPrinterRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in securityPrinterRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(ChemistryStation.RecipeComparable);
        List<WorldManager.RecipeData> chemistryStationRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in chemistryStationRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        traverse = Traverse.Create(PaintMixer.RecipeComparable);
        List<WorldManager.RecipeData> paintMixerRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

        foreach (WorldManager.RecipeData recipe in paintMixerRecipes) {
            Functions.AddFabricatorRecipe(recipe);
        }

        // for when terraforming dlc is released
        /*if (DLCManager.GetOwnedDLC().HasFlag(DLCType.Terraforming)) {
            traverse = Traverse.Create(TerraformingManufactory.RecipeComparable);
            List<WorldManager.RecipeData> terraformingManufactoryRecipes = traverse.Field("_recipeDataList").GetValue<List<WorldManager.RecipeData>>();

            foreach (WorldManager.RecipeData recipe in terraformingManufactoryRecipes) {
                Functions.AddFabricatorRecipe(recipe);
            }
        }*/
    }

    // helper function
    public static void AddFabricatorRecipe(WorldManager.RecipeData recipeData) {
        if (recipeData == null) {
            return;
        }

        recipeData.Recipe.Check();
        Fabricator.RecipeComparable.AddRecipe(recipeData);
    }
}