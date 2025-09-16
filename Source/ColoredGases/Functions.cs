#region

#endregion

namespace ColoredGases;

internal static class Functions {
    internal static bool EmitAirParticles(DensePool<Atmosphere> atmospheres, ParticleSystem emitter, Vector3 offset, Predicate<Atmosphere> condition, bool localSpace) {
        if (atmospheres == null || atmospheres.ActiveCount <= 0 || emitter == null || condition == null) {
            return false;
        }

        int atmosphereNumber = 0;
        bool found = false;
        while (atmosphereNumber < 50 && !found) {
            try {
                Atmosphere? atmosphere = null;
                atmosphere = atmospheres.Pick();
                atmosphereNumber++;

                if (atmosphere == null || atmosphere.BeingDestroyed || atmosphere.Mode != AtmosphereHelper.AtmosphereMode.World || !condition(atmosphere)) {
                    break;
                }

                AtmosphericsManager.emitParams.position = localSpace
                        ? offset + atmosphere.Grid.ToVector3()
                        : offset + atmosphere.ParentGridController.LocalToWorld(atmosphere.Grid);

                // theres a function for getting gas color??
                AtmosphericsManager.emitParams.startColor = atmosphere.GetGasColor();

                emitter.Emit(AtmosphericsManager.emitParams, 1);
                found = true;
            }
            catch (ArgumentOutOfRangeException) {
            }
            catch (IndexOutOfRangeException) {
            }
        }

        return false;
    }

    private static Traverse __lastIndex = Traverse.Create<AtmosphericFog>().Field("_lastIndex");
    private static int _lastIndex {
        get => __lastIndex?.GetValue<int>() ?? -1;
        set => __lastIndex?.SetValue(value);
    }

    internal static bool EmitFogParticles() {
        if (AtmosphericFog.AllAtmosphericFogs.Count <= 0) {
            return false;
        }

        int unusedFogs = AtmosphericFog.MAXFogParticles - AtmosphericFog.AtmosphericFogParticleSystem.particleCount;
        int fogIndex = (_lastIndex != -1) ? _lastIndex : (AtmosphericFog.AllAtmosphericFogs.Count - 1);
        if (fogIndex > AtmosphericFog.AllAtmosphericFogs.Count - 1) {
            fogIndex = AtmosphericFog.AllAtmosphericFogs.Count - 1;
        }

        int usedFogs = 0;
        for (int fogNumber = fogIndex; fogNumber >= 0; fogNumber--) {
            try {
                _lastIndex = fogNumber;
                if (unusedFogs <= 0 || usedFogs >= 30f) {
                    _lastIndex = fogNumber;

                    break;
                }

                AtmosphericFog atmosphericFog = AtmosphericFog.AllAtmosphericFogs[fogNumber];

                Atmosphere atmosphere = Traverse.Create(atmosphericFog).Property("Atmosphere").GetValue<Atmosphere>();
                Traverse traverse = Traverse.Create(atmosphericFog).Field("_lastEmitTime");

                if (atmosphericFog.IsEmitting() && traverse.GetValue<float>() + 0.2f < Time.fixedTime) {
                    ParticleSystem.EmitParams emitParams = new() {
                        //emitParams.position = atmosphericFog.EmissionPosition();
                        startColor = Atmosphere.GetLiquidColor(atmosphere.GasMixture)
                    };

                    AtmosphericFog.AtmosphereFogVisualizerTransform.position = atmosphericFog.EmissionPosition();
                    AtmosphericFog.AtmosphericFogParticleSystem.Emit(emitParams, 1);

                    traverse.SetValue(Time.fixedTime);
                    unusedFogs--;
                    usedFogs++;
                }
            }
            catch (ArgumentOutOfRangeException) {
                _lastIndex = fogNumber;
            }
            catch (IndexOutOfRangeException) {
                _lastIndex = fogNumber;
            }
        }

        if (unusedFogs > 0) {
            _lastIndex = -1;
        }

        return false;
    }
}