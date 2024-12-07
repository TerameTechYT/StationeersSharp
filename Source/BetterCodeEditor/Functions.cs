#region

using Assets.Scripts.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

#endregion

namespace BetterCodeEditor;

internal static class Functions {
    internal static int ToIndex(this int value, bool index) => index ? value - 1 : value;

    internal static bool OpcodeIs(this CodeInstruction code, OpCode opcode) => code.opcode == opcode;

    internal static bool OpcodeOperandIs(this CodeInstruction code, OpCode opcode, object operand) => code.OpcodeIs(opcode) && code.OperandIs(operand);

    internal static IEnumerable<CodeInstruction> ReplaceMaxLines(this IEnumerable<CodeInstruction> instructions, bool index = false) =>
        instructions.Manipulator(
            (instruction) => // if instruction opcode is Ldc_I4 AND the operand is MAX_LINES
                instruction.OpcodeOperandIs(OpCodes.Ldc_I4, InputSourceCode.MAX_LINES.ToIndex(index)),
            (instruction) => // change operand to our value or default if null
                instruction.operand = Data.MaxLines.ToIndex(index)
        );

    internal static IEnumerable<CodeInstruction> ReplaceLineLength(this IEnumerable<CodeInstruction> instructions) =>
        instructions.Manipulator(
            (instruction) => // if instruction opcode is Ldc_I4 AND the operand is LINE_LENGTH_LIMIT
                instruction.OpcodeOperandIs(OpCodes.Ldc_I4, InputSourceCode.LINE_LENGTH_LIMIT),
            (instruction) => // change operand to our value or default if null
                instruction.operand = Data.MaxLineLength
        );

    internal static IEnumerable<CodeInstruction> ReplaceMaxFileSize(this IEnumerable<CodeInstruction> instructions) =>
        instructions.Manipulator(
            (instruction) => // if instruction opcode is Ldc_I4 AND the operand is MAX_FILE_SIZE
                instruction.OpcodeOperandIs(OpCodes.Ldc_I4, InputSourceCode.MAX_FILE_SIZE),
            (instruction) => // change operand to our value or default if null
                instruction.operand = Data.MaxFileSize
        );
}