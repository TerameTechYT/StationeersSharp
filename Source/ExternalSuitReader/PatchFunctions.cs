#region

#endregion

namespace ExternalSuitReader;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.CanLogicRead))]
    [HarmonyPostfix]
    public static void AdvancedSuitCanLogicRead(ref AdvancedSuit __instance, ref bool __result, LogicType logicType) {
        if (__instance == null) {
            return;
        }

        try {
            __result = __result || Functions.CanLogicRead(logicType);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.GetLogicValue))]
    [HarmonyPostfix]
    public static void AdvancedSuitGetLogicValue(ref AdvancedSuit __instance, ref double __result, LogicType logicType) {
        if (__instance == null) {
            return;
        }

        try {
            if (!Functions.CanLogicRead(logicType)) {
                return;
            }

            __result = Functions.GetLogicValue(__instance, logicType);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.Awake))]
    [HarmonyPostfix]
    public static void AdvancedSuitAwake(ref AdvancedSuit __instance) {
        if (__instance == null) {
            return;
        }

        try {
            List<DoubleReference> channels = [];
            for (int i = 0; i < Data.ChannelCount; i++) {
                channels.Add(new DoubleReference(0.0));
            }

            Data.AllAdvancedSuits.Add(__instance.ReferenceId, channels);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }*/

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.OnDestroy))]
    [HarmonyPostfix]
    public static void AdvancedSuitOnDestroy(ref AdvancedSuit __instance) {
        if (__instance == null) {
            return;
        }

        try {
            Data.AllAdvancedSuits.Remove(__instance.ReferenceId);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }*/

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.SerializeSave))]
    [HarmonyPostfix]
    public static void AdvancedSuitSerializeSave(ref Suit __instance, ref ThingSaveData __result) {
        if (!Data.EnableExperimentalSaving || __instance == null || __result == null || __instance is not AdvancedSuit suit) {
            return;
        }

        try {
            if (!Data.AllAdvancedSuits.TryGetValue(suit.ReferenceId, out List<DoubleReference> channels)) {
                return;
            }

            __result = AdvancedSuitSaveData.Create(__result, channels);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }*/

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.DeserializeSave))]
    [HarmonyPostfix]
    public static void AdvancedSuitDeserializeSave(ref Suit __instance, ref ThingSaveData savedData) {
        if (!Data.EnableExperimentalSaving || __instance == null || savedData == null || __instance is not AdvancedSuit suit) {
            return;
        }

        try {
            if (!Data.AllAdvancedSuits.TryGetValue(suit.ReferenceId, out List<DoubleReference> channels)) {
                return;
            }

            if (savedData is AdvancedSuitSaveData data) {
                for (int i = 0; i < Data.ChannelCount; i++) {
                    channels[i].Value = data.Channels[i].Value;
                }
            }
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }*/

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.SerializeOnJoin))]
    [HarmonyPostfix]
    public static void AdvancedSuitSerializeOnJoin(ref AdvancedSuit __instance, ref RocketBinaryWriter writer) {
        if (__instance == null || writer == null)
            return;

        try {
            if (!Data.AllAdvancedSuits.TryGetValue(__instance.ReferenceId, out List<DoubleReference> channels))
                return;

            for (int i = 0; i < Data.ChannelCount; i++) {
                writer.WriteDouble(channels[i].Value);
            }
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.DeserializeOnJoin))]
    [HarmonyPostfix]
    public static void AdvancedSuitDeserializeOnJoin(ref AdvancedSuit __instance, ref RocketBinaryReader reader) {
        if (__instance == null || reader == null)
            return;

        try {
            if (!Data.AllAdvancedSuits.TryGetValue(__instance.ReferenceId, out List<DoubleReference> channels))
                return;

            for (int i = 0; i < Data.ChannelCount; i++) {
                channels[i] = new DoubleReference(reader.ReadDouble());
            }
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }*/
}