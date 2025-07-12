#region

#endregion

namespace ColoredGases;

[HarmonyPatch]
public static class PatchFunctions {


    [UsedImplicitly]
    [HarmonyPatch(typeof(AtmosphericsManager), "Emit")]
    [HarmonyPrefix]
    public static bool AtmosphericsManagerEmitAirVisualizerParticles(List<Atmosphere> targetContainer, ParticleSystem emitter, Vector3 particleAtmosphereSpawnOffset, Predicate<Atmosphere> emitCondition, bool localSpace = false) {
        try {
            return ConfigData.EnableAirVisualizer && Functions.EmitAirParticles(targetContainer, emitter, particleAtmosphereSpawnOffset, emitCondition, localSpace);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ex);
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AtmosphericFog), nameof(AtmosphericFog.EmitAtmosphericFogParticles))]
    [HarmonyPrefix]
    public static bool AtmosphericFogEmitAtmosphericFogParticles() {
        try {
            return ConfigData.EnableFogVisualizer && Functions.EmitFogParticles();
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ex);
        }

        return true;
    }
}