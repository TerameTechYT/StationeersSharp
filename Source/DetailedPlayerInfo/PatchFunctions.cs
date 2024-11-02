#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;

#endregion

namespace DetailedPlayerInfo;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
        typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);


    [UsedImplicitly]
    [HarmonyPatch(typeof(WorldManager), "UpdateFrameRate")]
    [HarmonyPrefix]
    public static bool WorldManagerUpdateFrameRate(ref TextMeshProUGUI ___FrameRate) {
        try {
            return (Data.CustomFramerate?.Value ?? false) && Functions.EnableFrameCounter(ref ___FrameRate);
        }
        catch (Exception ex) {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) _patches[currentMethod] = true;
            //ConsoleWindow.PrintError($"[{Data.Name}]: Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
            //ConsoleWindow.PrintError($"[{Data.Name}]: {ex.Source}: {ex.Message.Trim()} {ex.StackTrace.Trim()}");
            return true;
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Awake")]
    [HarmonyPostfix]
    public static void PlayerStateWindowAwake(PlayerStateWindow __instance) {
        try {
            Functions.Initialize();
        }
        catch (Exception ex) {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) _patches[currentMethod] = true;
            //ConsoleWindow.PrintError($"[{Data.Name}]: Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
            //ConsoleWindow.PrintError($"[{Data.Name}]: {ex.Source}: {ex.Message.Trim()} {ex.StackTrace.Trim()}");
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PlayerStateWindow), "Update")]
    [HarmonyPostfix]
    public static void PlayerStateWindowUpdate(ref PlayerStateWindow __instance) {
        try {
            Functions.Update(ref __instance);
        }
        catch (Exception ex) {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) _patches[currentMethod] = true;
            //ConsoleWindow.PrintError($"[{Data.Name}]: Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
            //ConsoleWindow.PrintError($"[{Data.Name}]: {ex.Source}: {ex.Message.Trim()} {ex.StackTrace.Trim()}");
        }
    }
}