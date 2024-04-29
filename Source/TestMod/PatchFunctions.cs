#pragma warning disable CA1707

using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;

namespace TestMod;

[HarmonyPatch]
public static class PatchFunctions
{

    [HarmonyPatch(typeof(SorterMotherboard), "AddRecipe")]
    public static bool DynamicThingRecipeComparableAddRecipe(ref bool __result, ref WorldManager.RecipeData recipe)
    {
    }
}