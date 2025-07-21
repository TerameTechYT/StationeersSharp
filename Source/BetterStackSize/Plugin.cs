#region

using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Flour = Assets.Scripts.Objects.Items.Flour;
using Milk = Assets.Scripts.Objects.Items.Milk;
using Sugar = Assets.Scripts.Objects.Items.Sugar;

#endregion

namespace BetterStackSize;

public class Plugin : Mod, IModSingleton<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public const string SECTION = "Configurables";
    public const string KEY = "Max Stack Size";
    public const int DEFAULT_MAX_STACK_SIZE = 500;
    public int MAX_STACK_SIZE => this.GetConfigValue(SECTION, KEY, DEFAULT_MAX_STACK_SIZE);

    public override ModInfo Data => new ModInfo() {
        Name = "BetterStackSize",
        Guid = "betterstacksize",
        Version = new Version(1, 0, 0, 31),
        WorkshopId = 3530757130ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() {}

    public override void OnConfigLoad() {
        this.RegisterConfig(new ConfigData<int>(
            DEFAULT_MAX_STACK_SIZE,
            "Configurables", "Max Stack Size",
            "The max stack size for stack size values."
        ));
    }

    public override void OnConfigRegistered<T>(ConfigEntry<T> entry) {
        object? prefabHash = entry.Description?.Tags?.First();
        if (prefabHash is int prefab) {
            if (entry.BoxedValue is float v1) {
                SetStacksize(prefab, v1);
            }

            if (entry.BoxedValue is int v2) {
                SetStacksize(prefab, v2);
            }
        }
    }

    public override void OnConfigChanged(ConfigEntryBase entry, SettingChangedEventArgs args) {
        object? prefabHash = entry.Description?.Tags?.First();
        if (prefabHash is int prefab) {
            if (entry.BoxedValue is float v1) {
                SetStacksize(prefab, v1);
            }

            if (entry.BoxedValue is int v2) {
                SetStacksize(prefab, v2);
            }
        }
    }

    public void SetStacksize(int hash, float value) {
        if (Prefab.TryFind(hash, out Thing thing)) {
             this.SetStacksize(thing, value);
        }
    }

    public void SetStacksize(Thing thing, float value) {
        if (thing is Stackable stackable) {
            stackable.MaxQuantity = Mathf.CeilToInt(value);
        }

        if (thing is Consumable consumable) {
            consumable.MaxQuantity = value;
        }
    }

    //

    public void ProcessThing(Thing thing) {
        switch (thing) {
            case Stackable stackable: {
                Plugin.Instance.ProcessStackable(ref stackable);
            } return;
            case Consumable consumable: {
                Plugin.Instance.ProcessConsumable(ref consumable);
            } return;
        }
    }

    public void ProcessDeprecated(ref Stackable stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Deprecated", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    public void ProcessDeprecated(ref Consumable stackable) {
        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Deprecated", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    public void ProcessStackable(ref Stackable stackable) {
        switch (stackable) {
            case Constructor constructor: {
                ProcessConstructor(ref constructor);
            } return;
            case MultiConstructor multiConstructor: {
                ProcessMultiConstructor(ref multiConstructor);
            } return;

            case DynamicThingConstructor dynamicConstructor: {
                ProcessDynamicConstructor(ref dynamicConstructor);
            } return;

            //
            case Ore ore: {
                ProcessOre(ref ore);
            } return;
            case DirtyOre dirtyOre: {
                ProcessDirtyOre(ref dirtyOre);
            } return;

            // Consumables
            case StackableFood stackableFood: {
                ProcessStackableFood(ref stackableFood);
            } return;
            case Pill pill: {
                ProcessPill(ref pill);
            } return;

            case Hay hay: {
                ProcessHay(ref hay);
            } return;
            case Plant plant: {
                ProcessPlant(ref plant);
            } return;
            case DecayedFood decayedFood: {
                ProcessDecayedFood(ref decayedFood);
            } return;

            case StackableLight stackableLight: {
                ProcessStackableLight(ref stackableLight);
            } return;

            case ItemExplosive explosive: {
                ProcessExplosive(ref explosive);
            } return;

            case ResearchPod:
            case Wreckage: return;
        }

        ProcessStackableFallback(ref stackable);
    }

    private void ProcessStackableFallback(ref Stackable stackable) {
        if (!stackable.CompareTag("NotSpawnable")) {
            ProcessDeprecated(ref stackable);
            return;
        }

        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Miscellaneous", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    public string GetDescriptor(Structure structure) {
        return structure switch {
            Cable => "Cable",
            DeviceCableMounted => "Cable Mounted",

            Pipe => "Pipe",
            DevicePipeMounted => "Pipe Mounted",

            Chute => "Chute",
            ChuteOutlet => "Chute Mounted",
            ChuteBin => "Chute Mounted",
            ChuteExportBin => "Chute Mounted",

            Wall => "Building",
            Frame => "Building",

            FabricatorBase => "Fabricator",

            RocketEngineBase => "Rocket Engine",

            LogicUnitBase => "Logic",

            RadioscopicThermalGenerator => "Generator",
            StirlingEngine => "Generator",
            GasFuelGenerator => "Generator",
            SolidFuelGenerator => "Generator",
            SolarPanel => "Generator",
            WindTurbineGenerator => "Generator",

            SmallDevice => "Device Small",
            LargeDevice => "Device Large",
            DeviceAtmospherics => "Device Atmospherics",
            Device => "Device",
            _ => "",
        };
    }

    private void ProcessConstructor(ref Constructor stackable) {
        string type = GetDescriptor(stackable.BuildStructure);
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            $"Constructable{(!string.IsNullOrEmpty(type) ? $" ({type})" : "")}", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessMultiConstructor(ref MultiConstructor stackable) {
        string type = GetDescriptor(stackable.Constructables.First());
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            $"Constructable{(!string.IsNullOrEmpty(type) ? $" ({type})" : "")}", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessDynamicConstructor(ref DynamicThingConstructor stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Constructable (Kits)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessOre(ref Ore stackable) {
        switch (stackable) {
            case Ice ice: {
                ProcessIce(ref ice);
            } return;
        }

        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Ore", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessDirtyOre(ref DirtyOre stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Ore", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessIce(ref Ice stackable) {
        switch (stackable) {
            case PureIce pureIce: {
                ProcessPureIce(ref pureIce);
            } return;
        }

        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Ore (Ice)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessPureIce(ref PureIce stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Ore (Pure Ice)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessStackableFood(ref StackableFood stackable) {
        this.RegisterConfig(new ConfigData<int>(
           stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
           "Consumable (Food)", $"{stackable.PrefabName}",
           $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
       ));
    }

    private void ProcessPill(ref Pill stackable) {
          this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Consumable (Pill)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessHay(ref Hay stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Plant", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessPlant(ref Plant stackable) {
        switch (stackable) {
            case Seed seed: {
                ProcessSeed(ref seed);
            } return;
            case Flower flower: {
                ProcessFlower(ref flower);
            } return;
        }

        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Plant", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessSeed(ref Seed stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Plant (Seed)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessFlower(ref Flower stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Plant (Flower)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessDecayedFood(ref DecayedFood stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Consumable (Food)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessStackableLight(ref StackableLight stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Consumable (Light)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessExplosive(ref ItemExplosive stackable) {
        this.RegisterConfig(new ConfigData<int>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Consumable (Explosive)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    //
    public void ProcessConsumable(ref Consumable stackable) {
        switch (stackable) {
            case CocoaPowder:
            case Flour:
            case Milk:
            case SoyOil:
            case Sugar:
            {
                ProcessCookingIngredient(ref stackable);
            } return;

            case IngredientBase ingredientBase: {
                ProcessIngridientBase(ref ingredientBase);
            } return;

            case Ingot ingot: {
                ProcessIngot(ref ingot);
            } return;

            case DisposableBatteryCharger:
            case MoonFries:
            case PumpkinPie:
            case WaterBottle:
            case SprayCan:
            case CannedFood:
            case RocketScanningHead:
            case RocketMiningDrillHead:
            case HandSanitizer:
            case Cake:
            case CerealBar:
            case DuctTape:
            case GasFilter:
            case Bread: return;
            }

        if (!stackable.CompareTag("NotSpawnable")) {
            ProcessDeprecated(ref stackable);
            return;
        }
    }

    private void ProcessConsumableFallback(ref Consumable stackable) {
        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1, MAX_STACK_SIZE,
            "Miscellaneous", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessIngridientBase(ref IngredientBase stackable) {
        switch (stackable) {
            case ColorDye colorDye: {
                ProcessColorDye(ref colorDye);
            } return;
            case Ingredient ingredient: {
                ProcessIngredient(ref ingredient);
            } return;
        }

        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1.0f, MAX_STACK_SIZE,
            "Consumable (Ingredient)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessColorDye(ref ColorDye stackable) {
        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1.0f, MAX_STACK_SIZE,
            "Consumable (Dye)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessIngredient(ref Ingredient stackable) {
        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1.0f, MAX_STACK_SIZE,
            "Consumable (Ingredient)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessCookingIngredient(ref Consumable stackable) {
        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1.0f, MAX_STACK_SIZE,
            "Consumable (Ingredient)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }

    private void ProcessIngot(ref Ingot stackable) {
        this.RegisterConfig(new ConfigData<float>(
            stackable.MaxQuantity,
            1.0f, MAX_STACK_SIZE,
            "Ore (Ingot)", $"{stackable.PrefabName}",
            $"The max stack size for {stackable.DisplayName}",
            stackable.PrefabHash
        ));
    }
}