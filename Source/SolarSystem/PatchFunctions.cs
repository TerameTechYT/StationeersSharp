#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts;
using HarmonyLib;
using JetBrains.Annotations;

#endregion

namespace SolarSystem;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
        typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(NewWorldMenu), "PopulateWorldList")]
    [HarmonyPostfix]
    public static void NewWorldMenuPopulateWorldList() {
        try {
            Functions.ReorderPlanetList();
        }
        catch (Exception ex) {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                ConsoleWindow.PrintError(
                    $"[{Data.Name}]: Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                ConsoleWindow.PrintError($"[{Data.Name}]: {ex.Source}: {ex.Message.Trim()} {ex.StackTrace.Trim()}");
            }
        }
    }
}