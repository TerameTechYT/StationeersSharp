#region

using static Assets.Scripts.Atmospherics.Chemistry;

#endregion

namespace ExtendedGases.Elements;

public interface IElement {
    string LiquidName { get; }
    GasType GasType { get; }
    GasType LiquidType { get; }

    double MolarMass { get; }
    double MolarVolume { get; }

    double EvaporationCoefficientA { get; }
    double EvaporationCoefficientB { get; }

    float ThermalEfficiency { get; }
    float SpecificHeat { get; }
    float LatentHeat { get; }

    float CriticalTemperature { get; }
    float CriticalPressure { get; }

    float BoilingPoint { get; }
    float FreezingPoint { get; }

    float TriplePointTemperature { get; }
    float TriplePointPressure { get; }

    bool Fuel { get; }
    bool Oxidiser { get; }
    float UpperFlammableLimit { get; }
    float LowerFlammableLimit { get; }
}

public class Element : IElement {
    public string GasName { get; private set; }
    public string LiquidName { get; private set; }

    public GasType GasType => this._gasTypeBuilder?.Value ?? GasType.Undefined;
    public GasType LiquidType => this._liquidTypeBuilder?.Value ?? GasType.Undefined;

    public double MolarMass { get; private set; }

    public double MolarVolume { get; private set; }

    public float ThermalEfficiency { get; private set; }

    public float SpecificHeat { get; private set; }

    public double EvaporationCoefficientA { get; private set; }

    public double EvaporationCoefficientB { get; private set; }

    public float LatentHeat { get; private set; }

    public float CriticalTemperature { get; private set; }

    public float CriticalPressure { get; private set; }

    public float BoilingPoint { get; private set; }

    public float FreezingPoint { get; private set; }

    public float TriplePointTemperature { get; private set; }

    public float TriplePointPressure { get; private set; }

    public bool Fuel { get; private set; }

    public bool Oxidiser { get; private set; }

    public float UpperFlammableLimit { get; private set; }

    public float LowerFlammableLimit { get; private set; }

    private readonly EnumBuilder<GasType>? _gasTypeBuilder;
    private readonly EnumBuilder<GasType>? _liquidTypeBuilder;

    public Element(string name) {
        this.GasName = name;
        EnumHandler.TryAddEntry(this.GasName, out this._gasTypeBuilder);

        this.LiquidName = $"Liquid{name}";
        EnumHandler.TryAddEntry(this.LiquidName, out this._gasTypeBuilder);

        ElementManager.Add(this);
    }
}
