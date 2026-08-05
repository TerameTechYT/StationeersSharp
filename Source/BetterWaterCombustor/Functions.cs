#region

#endregion

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using StationeersLibrary;
using System.Reflection;

namespace BetterWaterCombustor;

public static class Functions {
    /*public static void OnAtmosphericTick(ref CombustorMachine combustor) {
        if (!combustor.OnOff || !combustor.Powered || !combustor.GetType().PropertyGetValue<bool>("IsOperable", BindingFlags.Instance | BindingFlags.NonPublic)) {
            if (combustor.Activate == 1) {
                OnServer.Interact(combustor.InteractActivate, 0);
            }

            combustor.ProcessedMoles = MoleQuantity.Zero;
            return;
        }

        AtmosphereHelper.MoveToEqualize(combustor.InternalAtmosphere, combustor.OutputNetwork.Atmosphere, PressurekPa.MaxValue, AtmosphereHelper.MatterState.All);
        if (combustor.Mode == 0) {
            if (combustor.Activate == 1) {
                OnServer.Interact(combustor.InteractActivate, 0);
            }

            combustor.ProcessedMoles = MoleQuantity.Zero;
            return;
        }

        float liquidRatio = combustor.Ratio1;
        bool onlyLiquid = liquidRatio == combustor.MaxSetting;
        Atmosphere liquidAtmosphere = combustor.InputNetwork.Atmosphere;

        float gasRatio = combustor.Ratio2;
        bool onlyGas = gasRatio == combustor.MaxSetting;
        Atmosphere gasAtmosphere = combustor.InputNetwork2.Atmosphere;

        if (onlyLiquid &&
             (!combustor.IsInputValid
                || combustor.InputNetwork.Atmosphere == null
                || combustor.InputNetwork.Atmosphere.TotalMoles < AtmosphereHelper.MinimumMolesForProcessing
             )
        ) {
            if (combustor.Activate == 1) {
                OnServer.Interact(combustor.InteractActivate, 0);
            }

            combustor.ProcessedMoles = MoleQuantity.Zero;
            return;
        }

        if (onlyGas &&
             (!combustor.IsInput2Valid
                || combustor.InputNetwork2.Atmosphere == null
                || combustor.InputNetwork2.Atmosphere.TotalMoles < AtmosphereHelper.MinimumMolesForProcessing
             )
        ) {
            if (combustor.Activate == 1) {
                OnServer.Interact(combustor.InteractActivate, 0);
            }
            combustor.ProcessedMoles = MoleQuantity.Zero;
            return;
        }

        TemperatureKelvin temperature = (liquidRatio, gasRatio) switch {
            (float liquid, float gas) when liquid > 0 => liquidAtmosphere.Temperature,
            (float liquid, float gas) when gas > 0 => gasAtmosphere.Temperature,
            _ => (gasAtmosphere.Temperature + liquidAtmosphere.Temperature) / 2,
        };

        MoleQuantity totalMoles = IdealGas.Quantity(combustor.PressurePerTick, combustor.Volume / 10.0f, temperature);
        MoleQuantity liquidMoles = RocketMath.Min(liquidAtmosphere.TotalMoles, totalMoles * (liquidRatio / 100.0f));
        MoleQuantity gasMoles = RocketMath.Min(gasAtmosphere.TotalMoles, totalMoles * (gasRatio / 100.0f));

        //MoleQuantity remain = totalMoles - (liquidMoles + gasMoles);
        //if (remain > MoleQuantity.Zero) {
        //    MoleQuantity temp1 = RocketMath.Min(remain, liquidAtmosphere.TotalMoles - liquidMoles);
        //    liquidMoles += temp1;
        //    remain -= temp1;

        //    MoleQuantity temp2 = RocketMath.Min(remain, gasAtmosphere.TotalMoles - gasMoles);
        //    gasMoles += temp2;
        //}

        bool combust = false;
        if (liquidMoles > MoleQuantity.Zero) {
            combustor.InternalAtmosphere.Add(liquidAtmosphere.Remove(liquidMoles, AtmosphereHelper.MatterState.All));
            combust = true;
        }

        if (gasMoles > MoleQuantity.Zero) {
            combustor.InternalAtmosphere.Add(gasAtmosphere.Remove(gasMoles, AtmosphereHelper.MatterState.All));
            combust = true;
        }

        if (combust) {
            if (combustor.Activate == 0) {
                OnServer.Interact(combustor.InteractActivate, 1);
            }
        } else if (combustor.Activate == 1) {
            OnServer.Interact(combustor.InteractActivate, 0);
        }

        combustor.InternalAtmosphere.TryCombust(0.99, true);
        combustor.InternalAtmosphere.StateChange();
        combustor.ProcessedMoles = liquidMoles + gasMoles;
    }*/
}