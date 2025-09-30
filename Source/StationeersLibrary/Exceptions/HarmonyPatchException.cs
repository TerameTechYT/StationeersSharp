#region

using System.Reflection;

#endregion

namespace StationeersLibrary.Exceptions;

public class HarmonyPatchException : Exception {
    public HarmonyPatchException(MethodBase methodBase) : base($"{methodBase?.Name}") {
    }

    public HarmonyPatchException(Type type) : base($"{type?.Name}") {
    }

    public HarmonyPatchException(string message) : base(message) {
    }

    public HarmonyPatchException(string message, Exception innerException) : base(message, innerException) {
    }

    public HarmonyPatchException() {
    }
}

public class HarmonyPrefixException : HarmonyPatchException {
    public HarmonyPrefixException(MethodBase methodBase) : base(methodBase) {
    }

    public HarmonyPrefixException(Type type) : base(type) {
    }

    public HarmonyPrefixException(string message) : base(message) {
    }

    public HarmonyPrefixException(string message, Exception innerException) : base(message, innerException) {
    }

    public HarmonyPrefixException() {
    }
}

public class HarmonyPostfixException : HarmonyPatchException {
    public HarmonyPostfixException(MethodBase methodBase) : base(methodBase) {
    }

    public HarmonyPostfixException(Type type) : base(type) {
    }

    public HarmonyPostfixException(string message) : base(message) {
    }

    public HarmonyPostfixException(string message, Exception innerException) : base(message, innerException) {
    }

    public HarmonyPostfixException() {
    }
}

public class HarmonyTranspilerException : HarmonyPatchException {
    public HarmonyTranspilerException(MethodBase methodBase) : base(methodBase) {
    }

    public HarmonyTranspilerException(Type type) : base(type) {
    }

    public HarmonyTranspilerException(string message) : base(message) {
    }

    public HarmonyTranspilerException(string message, Exception innerException) : base(message, innerException) {
    }

    public HarmonyTranspilerException() {
    }
}

public class HarmonyFinalizerException : HarmonyPatchException {
    public HarmonyFinalizerException(MethodBase methodBase) : base(methodBase) {
    }

    public HarmonyFinalizerException(Type type) : base(type) {
    }

    public HarmonyFinalizerException(string message) : base(message) {
    }

    public HarmonyFinalizerException(string message, Exception innerException) : base(message, innerException) {
    }

    public HarmonyFinalizerException() {
    }
}

public class HarmonyReversePatchException : HarmonyPatchException {
    public HarmonyReversePatchException(MethodBase methodBase) : base(methodBase) {
    }

    public HarmonyReversePatchException(Type type) : base(type) {
    }

    public HarmonyReversePatchException(string message) : base(message) {
    }

    public HarmonyReversePatchException(string message, Exception innerException) : base(message, innerException) {
    }

    public HarmonyReversePatchException() {
    }
}