using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using JetBrains.Annotations;
using Objects.Structures;
using UnityEngine;

namespace BetterHydrationNutrition;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(Human), "Awake")]
    [HarmonyPostfix]
    public static void HumanAwake(Human __instance) {
        if (__instance != null) {
            __instance.WarningHydration = (Data.HydrationStorage / 100) * 0.2f;
            __instance.CriticalHydration = (Data.HydrationStorage / 100) * 0.1f;

            __instance.WarningNutrition = (Data.NutritionStorage / 100) * 0.2f;
            __instance.CriticalNutrition = (Data.NutritionStorage / 100) * 0.1f;
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Human), "OnLifeCreated")]
    [HarmonyPostfix]
    public static void HumanOnLifeCreated(Human __instance, Brain brain, bool isRespawn) {
        if (__instance != null) {
            if (isRespawn) {
                __instance.Hydration = Mathf.Clamp(Data.HydrationStorage / WorldManager.DaysPast, Data.HydrationStorage * 0.1f, Data.HydrationStorage);
                __instance.Nutrition = Mathf.Clamp(Data.NutritionStorage / WorldManager.DaysPast, Data.NutritionStorage * 0.1f, Data.NutritionStorage);
            }
            else {
                __instance.Hydration = Data.HydrationStorage;
                __instance.Nutrition = Data.NutritionStorage;
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Entity), "Hydration", MethodType.Setter)]
    [HarmonyPrefix]
    public static bool EntityHydrationSetter(Entity __instance, float value, ref float ____hydration) {
        if (__instance != null) {
            ____hydration = Mathf.Clamp(value, 0, __instance.GetHydrationStorage());

            if (NetworkManager.IsServer)
                __instance.NetworkUpdateFlags |= 1024;

            return false;
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Entity), "Nutrition", MethodType.Setter)]
    [HarmonyPrefix]
    public static bool EntityNutritionSetter(Entity __instance, float value, ref float ____nutrition) {
        if (__instance != null) {
            ____nutrition = Mathf.Clamp(value, 0, __instance.GetNutritionStorage());

            if (NetworkManager.IsServer)
                __instance.NetworkUpdateFlags |= 1024;

            return false;
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Entity), "GetHydrationStorage")]
    [HarmonyPostfix]
    static public void EntityGetHydrationStorage(Entity __instance, ref float __result) {
        if (__instance != null)
            __result = Data.HydrationStorage * __instance.GetFoodQualityMultiplier();
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Entity), "GetNutritionStorage")]
    [HarmonyPostfix]
    static public void EntityGetNutritionStorage(Entity __instance, ref float __result) {
        if (__instance != null)
            __result = Data.NutritionStorage;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WaterBottle), "HydrateTime")]
    [HarmonyPostfix]
    public static void WaterBottle(ref float __result) {
        __result *= 0.1f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StructureDrinkingFountain), "HydrateTime")]
    [HarmonyPostfix]
    public static void StructureDrinkingFountainHydrateTime(ref float __result) {
        __result *= 0.1f;
    }
}