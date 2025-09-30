#region

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using HarmonyLib;
using StationeersLibrary;
using UnityEngine;

#endregion

namespace ColoredGases;

[HarmonyPatch]
public static class PatchFunctions {


    [HarmonyPatch(typeof(AtmosphericsManager), "Emit")]
    [HarmonyPrefix]
    public static bool AtmosphericsManagerEmitAirVisualizerParticles(DensePool<Atmosphere> targetContainer, ParticleSystem emitter, Vector3 particleAtmosphereSpawnOffset, Predicate<Atmosphere> emitCondition, bool localSpace = false) {
        try {
            return ConfigData.EnableAirVisualizer && Functions.EmitAirParticles(targetContainer, emitter, particleAtmosphereSpawnOffset, emitCondition, localSpace);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }

    [HarmonyPatch(typeof(AtmosphericFog), nameof(AtmosphericFog.EmitAtmosphericFogParticles))]
    [HarmonyPrefix]
    public static bool AtmosphericFogEmitAtmosphericFogParticles() {
        try {
            return ConfigData.EnableFogVisualizer && Functions.EmitFogParticles();
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }
}