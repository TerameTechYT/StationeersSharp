#region

using Assets.Scripts.UI;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using JetBrains.Annotations;

#endregion

namespace BetterCodeEditor;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "HandleInput")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeHandleInputTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxLines(true).ReplaceLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "RemoveLine")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeRemoveLineTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "UpdateFileSize")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeUpdateFileSizeTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxFileSize();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Initialize))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeInitializeTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxLines().ReplaceLineLength();


    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Copy))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeCopyTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceMaxFileSize().ReplaceLineLength();

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Paste))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodePasteTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.ReplaceLineLength();
}