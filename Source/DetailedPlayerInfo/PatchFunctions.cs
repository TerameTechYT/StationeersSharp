// ReSharper disable InconsistentNaming

#pragma warning disable CA1707
#pragma warning disable IDE0060

using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;

namespace DetailedPlayerInfo.Patches;

[HarmonyPatch]
public static class PatchFunctions
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(WorldManager), "UpdateFrameRate")]
    [HarmonyPrefix]
    public static bool WorldManagerUpdateFrameRate(ref TextMeshProUGUI ___FrameRate)
    {
        return Functions.EnableFrameCounter(ref ___FrameRate);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Awake")]
    [HarmonyPostfix]
    public static void PlayerStateWindowAwake(PlayerStateWindow __instance)
    {
        Functions.Initialize();
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Update")]
    [HarmonyPostfix]
    public static void PlayerStateWindowUpdate(ref PlayerStateWindow __instance)
    {
        Functions.Update(ref __instance);
    }
}