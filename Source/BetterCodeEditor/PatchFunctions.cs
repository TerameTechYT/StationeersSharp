#region

#endregion

namespace BetterCodeEditor;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
    typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "HandleInput")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeHandleInputTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            return instructions.ReplaceMaxLines(true).ReplaceMaxLineLength();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return instructions;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "RemoveLine")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeRemoveLineTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            return instructions.ReplaceMaxLineLength();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return instructions;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), "UpdateFileSize")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeUpdateFileSizeTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            return instructions.ReplaceMaxFileSize();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return instructions;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Initialize))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeInitializeTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            return instructions.ReplaceMaxLines().ReplaceMaxLineLength();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return instructions;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Copy))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodeCopyTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            return instructions.ReplaceMaxFileSize().ReplaceMaxLineLength();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return instructions;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Paste))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InputSourceCodePasteTranspiler(IEnumerable<CodeInstruction> instructions) {
        try {
            return instructions.ReplaceMaxLineLength();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return instructions;
    }

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