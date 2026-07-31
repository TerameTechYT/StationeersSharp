#region

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using HarmonyLib;
using StationeersLibrary;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion

namespace ColoredGases;

[HarmonyPatch]
public static class PatchFunctions {
    [HarmonyPatch(typeof(AtmosphericsManager), "Emit")]
    [HarmonyPatchConfig<Plugin>("Configurables", "Enable Colored Air Visualier")]
    [HarmonyPrefix]
    public static bool AtmosphericsManagerEmitAirVisualizerParticles(DensePool<Atmosphere> targetContainer, ParticleSystem emitter, Vector3 particleAtmosphereSpawnOffset, Predicate<Atmosphere> emitCondition, bool localSpace = false) {
        try {
            return Functions.EmitAirParticles(targetContainer, emitter, particleAtmosphereSpawnOffset, emitCondition, localSpace);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }

    [HarmonyPatch(typeof(AtmosphericFog), nameof(AtmosphericFog.EmitAtmosphericFogParticles))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Enable Colored Fog Visualier")]
    [HarmonyPrefix]
    public static bool AtmosphericFogEmitAtmosphericFogParticles() {
        try {
            return Functions.EmitFogParticles();
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }
}