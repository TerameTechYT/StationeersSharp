#region

#endregion

namespace ColoredGases;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
        typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(AtmosphericsManager), "Emit")]
    [HarmonyPrefix]
    public static bool AtmosphericsManagerEmitAirVisualizerParticles(List<Atmosphere> targetContainer, ParticleSystem emitter, Vector3 particleAtmosphereSpawnOffset, Predicate<Atmosphere> emitCondition, bool localSpace = false) {
        try {
            return Data.EnableAirVisualizer && Functions.EmitAirParticles(targetContainer, emitter, particleAtmosphereSpawnOffset, emitCondition, localSpace);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AtmosphericFog), nameof(AtmosphericFog.EmitAtmosphericFogParticles))]
    [HarmonyPrefix]
    public static bool AtmosphericFogEmitAtmosphericFogParticles() {
        try {
            return Data.EnableFogVisualizer && Functions.EmitFogParticles();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }

        return true;
    }
}