#region

#endregion

namespace BetterCodeEditor;

internal static class Extensions {
    internal static int ToIndex(this int value, bool index) => index ? value - 1 : value;

    internal static IEnumerable<CodeInstruction> ReplaceMaxLines(this IEnumerable<CodeInstruction> instructions, bool index = false) =>
        instructions.Manipulator(
            (instruction) => // if instruction opcode is Ldc_I4 AND the operand is MAX_LINES
                instruction.Is(OpCodes.Ldc_I4, InputSourceCode.MAX_LINES.ToIndex(index)),
            (instruction) => // change operand to our value or default if null
                instruction.operand = Data.CodeEditorLines.ToIndex(index)
        );

    internal static IEnumerable<CodeInstruction> ReplaceMaxLineLength(this IEnumerable<CodeInstruction> instructions) =>
        instructions.Manipulator(
            (instruction) => // if instruction opcode is Ldc_I4 AND the operand is LINE_LENGTH_LIMIT
                instruction.Is(OpCodes.Ldc_I4, InputSourceCode.LINE_LENGTH_LIMIT),
            (instruction) => // change operand to our value or default if null
                instruction.operand = Data.CodeEditorLineLength
        );

    internal static IEnumerable<CodeInstruction> ReplaceMaxFileSize(this IEnumerable<CodeInstruction> instructions) =>
        instructions.Manipulator(
            (instruction) => // if instruction opcode is Ldc_I4 AND the operand is MAX_FILE_SIZE
                instruction.Is(OpCodes.Ldc_I4, InputSourceCode.MAX_FILE_SIZE),
            (instruction) => // change operand to our value or default if null
                instruction.operand = Data.MaxFileSize
        );
}

/*public class CodeTooltip : UserInterfaceBase {
    private static CodeTooltip _instance = new();
    public static CodeTooltip Instance => _instance;

    private static string _tooltipText;
    public static string TooltipText => _tooltipText;

    private CodeTooltipPanel _tooltipPanel;

    public static void SetTooltip(string text = "") => _tooltipText = text;

    public void DoUpdate() {
        if (!string.IsNullOrEmpty(_tooltipText) && InputSourceCode.Instance.IsVisible) {
            _tooltipPanel.Show(_tooltipText, EditorLineOfCode.CurrentLine.transform.position);
        }
        else {
            _tooltipPanel.Hide();
        }
    }
}

public class CodeTooltipPanel : UserInterfaceBase {
    private TextMeshProUGUI _textMesh;

    private void UpdateLayout(Vector3 position) => Transform.position = position;

    public void Show(string text, Vector3 position) {
        _textMesh.text = text;
        SetActive(true);
        UpdateLayout(position);
    }

    public void Hide() {
        SetActive(false);
        _textMesh.text = string.Empty;
    }
}*/