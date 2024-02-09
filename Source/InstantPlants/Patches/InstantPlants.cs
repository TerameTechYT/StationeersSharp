using Assets.Scripts.Objects.Items;
using HarmonyLib;
using JetBrains.Annotations;

namespace InstantPlants.Patches;

[HarmonyPatch]
public static class InstantPlantsPatches
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    [HarmonyPostfix]
    public static void PlantAwake(Plant __instance)
    {
        for (var i = 1; i < __instance.GrowthStates.Count; i++)
            if (i < __instance.GrowthStates.Count - 2)
            {
                __instance.GrowthStates[i].Length = float.Epsilon;
            }
            else
            {
                if (i == __instance.GrowthStates.Count - 2)
                {
                    __instance.GrowthStates[i].Length = -1f;
                }
                else
                {
                    if (i == __instance.GrowthStates.Count - 1) __instance.GrowthStates[i].Length = 0f;
                }
            }
    }
}