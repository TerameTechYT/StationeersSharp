#region

#endregion

namespace StationeersLibrary;

public static class Constants {
    //
    public const string STATIONEERS_LIBRARY_NAME = Data.ModName;
    public const string STATIONEERS_LIBRARY_VERSION = Data.ModVersion;
    public const string STATIONEERS_LIBRARY_GUID = Data.ModGuid;
    public const ulong STATIONEERS_LIBRARY_HANDLE = Data.ModHandle;

#if DEBUG
    public const bool DEBUG_MODE = true;
#else
    public const bool DEBUG_MODE = false;
#endif

    // FILE NAMES
    public const string CLIENT_EXECUTABLE_NAME = "rocketstation.exe";
    public const string HEADLESS_EXECUTABLE_NAME = "rocketstation_DedicatedServer.exe";

    // SCENE NAMES
    public const string BASE_SCENE_NAME = "Base";
    public const string SPLASH_SCENE_NAME = "Splash";
    public const string CHARACTER_CUSTOMIZATION_SCENE_NAME = "CharacterCustomisation";

    // MENU PAGE NAMES
    public const string MAIN_MENU_PAGE = "MainMenu";
    public const string NEW_GAME_PAGE = "NewGame";
    public const string WORKSHOP_PAGE = "WorkshopMenu";
    public const string WORLD_CONFIG_PAGE = "WorldConfiguration";
    public const string STARTING_CONDITIONS_PAGE = "StartingConditions";
    public const string TUTORIALS_PAGE = "TutorialScenarios";

    public const string DEGREE_SYMBOL = "°";

    // UNITS
    public const string FAHRENHEIT_SYMBOL = "°F";
    public const string CELCIUS_SYMBOL = "°C";
    public const string KELVIN_SYMBOL = "°K";

    public const string PASCAL_SYMBOL = "Pa";
    public const string KILOPASCAL_SYMBOL = "kPa";
    public const string POUNDS_PER_SQUARE_INCH_SYMBOL = "psi";
    public const string BAR_SYMBOL = "bar";
    public const string TORR_SYMBOL = "torr";

    public const string LITER_SYMBOL = "L";
    public const string GALLON_SYMBOL = "gal";

    public const string METERS_PER_SECOND_SYMBOL = "m/s";
    public const string FEET_PER_SECOND_SYMBOL = "ft/s";
    public const string KNOT_SYMBOL = "kn";

    public const string JOULE_SYMBOL = "J";
    public const string FOOTPOUND_SYMBOL = "ftlb";
    public const string CALORIE_SYMBOL = "cal";

    // ENERGY: KW
    public const float ONE_KILOWATT = 1000f;
    public const float TWO_KILOWATTS = ONE_KILOWATT * 2f;
    public const float TWO_POINT_FIVE_KILOWATTS = ONE_KILOWATT * 2.5f;
    public const float FIVE_KILOWATTS = ONE_KILOWATT * 5f;
    public const float EIGHT_KILOWATTS = ONE_KILOWATT * 8f;
    public const float TEN_KILOWATTS = ONE_KILOWATT * 10f;
    public const float TWENTY_KILOWATTS = ONE_KILOWATT * 20f;
    public const float TWENTY_FIVE_KILOWATTS = ONE_KILOWATT * 25f;
    public const float FIFTY_KILOWATTS = ONE_KILOWATT * 50f;
    public const float SEVENTY_FIVE_KILOWATTS = ONE_KILOWATT * 75f;
    public const float ONE_HUNDRED_KILOWATTS = ONE_KILOWATT * 100f;
    public const float ONE_HUNDRED_ONE_KILOWATTS = ONE_KILOWATT * 101f;

    // TEMPERATURE: CELCIUS
    public const float ABSOLUTE_ZERO_CELCIUS = -273.15f;
    public const float ZERO_CELCIUS = 0f;
    public const float ONE_CELCIUS = 1f;
    public const float FIVE_CELCIUS = 5f;

    public const float TEN_CELCIUS = 10f;
    public const float FIFTEEN_CELCIUS = 15f;

    public const float TWENTY_CELCIUS = 20f;
    public const float TWENTY_FIVE_CELCIUS = 25f;

    public const float THIRTY_CELCIUS = 30f;
    public const float THIRTY_FIVE_CELCIUS = 35f;

    public const float FOURTY_CELCIUS = 40f;
    public const float FOURTY_FIVE_CELCIUS = 45f;

    public const float FIFTY_CELCIUS = 50f;

    public const float MINIMUM_SAFE_CELCIUS = ZERO_CELCIUS;
    public const float MAXIMUM_SAFE_CELCIUS = FIFTY_CELCIUS;

    public const float MINIMUM_CELCIUS = -272.15f;
    public const float MAXIMUM_CELCIUS = 79726.85f;

    // TEMPERATURE: KELVIN
    public const float ABSOLUTE_ZERO_KELVIN = 0f;
    public const float ZERO_CELCIUS_KELVIN = 273.15f;
    public const float ONE_KELVIN = ZERO_CELCIUS_KELVIN + ONE_CELCIUS;
    public const float FIVE_KELVIN = ZERO_CELCIUS_KELVIN + FIVE_CELCIUS;

    public const float TEN_KELVIN = ZERO_CELCIUS_KELVIN + TEN_CELCIUS;
    public const float FIFTEEN_KELVIN = ZERO_CELCIUS_KELVIN + FIFTEEN_CELCIUS;

    public const float TWENTY_KELVIN = ZERO_CELCIUS_KELVIN + TWENTY_CELCIUS;
    public const float TWENTY_FIVE_KELVIN = ZERO_CELCIUS_KELVIN + TWENTY_FIVE_CELCIUS;

    public const float THIRTY_KELVIN = ZERO_CELCIUS_KELVIN + THIRTY_CELCIUS;
    public const float THIRTY_FIVE_KELVIN = ZERO_CELCIUS_KELVIN + THIRTY_FIVE_CELCIUS;

    public const float FOURTY_KELVIN = ZERO_CELCIUS_KELVIN + FOURTY_CELCIUS;
    public const float FOURTY_FIVE_KELVIN = ZERO_CELCIUS_KELVIN + FOURTY_FIVE_CELCIUS;

    public const float FIFTY_KELVIN = ZERO_CELCIUS_KELVIN + FIFTY_CELCIUS;

    public const float MINIMUM_SAFE_KELVIN = ZERO_CELCIUS_KELVIN;
    public const float MAXIMUM_SAFE_KELVIN = FIFTY_KELVIN;

    public const float MINIMUM_KELVIN = ZERO_CELCIUS_KELVIN + MINIMUM_CELCIUS;
    public const float MAXIMUM_KELVIN = ZERO_CELCIUS_KELVIN + MAXIMUM_CELCIUS;

    // PRESSURE
    public const float ONE_ATMOSPHERE_PRESSURE_KPA = 101.325f;
    public const float TWO_ATMOSPHERE_PRESSURE_KPA = ONE_ATMOSPHERE_PRESSURE_KPA * 2f;
    public const float THREE_ATMOSPHERE_PRESSURE_KPA = ONE_ATMOSPHERE_PRESSURE_KPA * 3f;
    public const float FOUR_ATMOSPHERE_PRESSURE_KPA = ONE_ATMOSPHERE_PRESSURE_KPA * 4f;
    public const float FIVE_ATMOSPHERE_PRESSURE_KPA = ONE_ATMOSPHERE_PRESSURE_KPA * 5f;
    public const float SIX_ATMOSPHERE_PRESSURE_KPA = ONE_ATMOSPHERE_PRESSURE_KPA * 6f;

    public const float MINIMUM_SAFE_PRESSURE_KPA = 20f;
    public const float MAXIMUM_SAFE_PRESSURE_KPA = SIX_ATMOSPHERE_PRESSURE_KPA;

    public const float MINIMUM_PRESSURE_KPA = 0f;
    public const float MAXIMUM_PRESSURE_KPA = 1000000f;
}