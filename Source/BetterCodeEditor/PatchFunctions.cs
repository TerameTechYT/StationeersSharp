#region

using Assets.Scripts.UI;
using HarmonyLib;
using System.Collections.Generic;
using JetBrains.Annotations;
using Assets.Scripts;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using System;
using Assets.Scripts.Util;

#endregion

namespace BetterCodeEditor;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "HandleInput")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeHandleInputTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxLines(true).ReplaceMaxLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "RemoveLine")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeRemoveLineTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "UpdateFileSize")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeUpdateFileSizeTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxFileSize();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Initialize))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeInitializeTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxLines().ReplaceMaxLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Copy))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeCopyTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxFileSize().ReplaceMaxLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Paste))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodePasteTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxLineLength();

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(EditorLineOfCode), nameof(EditorLineOfCode.ReformatText), [typeof(string)])]
    [HarmonyPostfix]
    public static void EditorLineOfCodeReformatText(EditorLineOfCode __instance, string inputString) {
        if (__instance != null && EditorLineOfCode.CurrentLine == __instance && !string.IsNullOrEmpty(inputString)) {
            string[] text = inputString.Split(" ");

            List<string> validCommands = [];

            foreach (ScriptCommand command in EnumCollections.ScriptCommands.Values) {
                if (!LogicBase.IsDeprecated(command)) {
                    string name = EnumCollections.ScriptCommands.GetName(command);

                    if (name.StartsWith(text[0], StringComparison.CurrentCulture)) {
                        validCommands.Add(name);
                    }
                }
            }

            CodeTooltip.SetTooltip(validCommands.ToDelimitedString("\n"));
        }
    }*/
}