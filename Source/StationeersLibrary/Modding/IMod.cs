#region

using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using StationeersLibrary.Args;
using System.Reflection;
using UnityEngine;

#endregion

namespace StationeersLibrary.Modding;

public interface IMod {
    /// <summary>
    /// Called by <see cref="StationeersLaunchPad"/> with any prefabs.
    /// </summary>
    /// <param name="prefabs">Prefabs this mod should have</param>
    public abstract void OnLoaded(List<GameObject> prefabs, List<Assembly> assemblies, ConfigFile config, ModData data);

    /// <summary>
    /// Called when the mod is being unloaded.
    /// </summary>
    public abstract void OnUnloaded();

    /// <summary>
    /// Called by <see cref="IMod"/> when the GameObject is created.
    /// </summary>
    public abstract void OnAwake();

    /// <summary>
    /// Called by <see cref="IMod"/> every frame.
    /// </summary>
    public abstract void OnUpdate(float deltaTime);

    /// <summary>
    /// Called by <see cref="IMod"/> after <see cref="OnUpdate"/>
    /// </summary>
    public abstract void OnLateUpdate(float deltaTime);

    /// <summary>
    /// Called by <see cref="IMod"/> on a fixed framerate.
    /// </summary>
    public abstract void OnFixedUpdate(float deltaTime);

    /// <summary>
    /// Called by Unity for handling UGUI events.
    /// </summary>
    public abstract void OnGUI();

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="IMod"/> attached is enabled.
    /// </summary>
    public abstract void OnEnable();

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="IMod"/> attached is disabled.
    /// </summary>
    public abstract void OnDisable();

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="IMod"/> attached is about to be destroyed.
    /// </summary>
    public abstract void OnDestroy();

    /// <summary>
    /// Called when the application focus has changed.
    /// </summary>
    public abstract void OnApplicationFocus(bool hasFocus);

    /// <summary>
    /// Called when the application pause status has changed.
    /// </summary>
    public abstract void OnApplicationPause(bool pauseStatus);

    /// <summary>
    /// Called when the application is quitting.
    /// </summary>
    public abstract void OnApplicationQuit();

    /// <summary>
    /// Called when any scene is loaded
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnSceneLoaded(SceneLoadArgs args);

    /// <summary>
    /// Called when the inital game loading scene is loaded. 
    /// This is unlikely to ever be called.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnSplashLoaded(SceneLoadArgs args);

    /// <summary>
    /// Called when the base scene is loaded.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract UniTask OnBaseLoaded(SceneLoadArgs args);

    /// <summary>
    /// Called when the character customization screen is loaded.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnCharacterCustomizationLoaded(SceneLoadArgs args);

    /// <summary>
    /// Called when any scene is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnSceneUnloaded(SceneLoadArgs args);

    /// <summary>
    /// Called when inital game loading scene is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnSplashUnloaded(SceneLoadArgs args);

    /// <summary>
    /// Called when the base scene is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnBaseUnloaded(SceneLoadArgs args);

    /// <summary>
    /// Called when the character customization screen is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnCharacterCustomizationUnloaded(SceneLoadArgs args);

    /// <summary>
    /// Called when any main menu page is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnMenuPageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the main menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnMainMenuPageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the new game menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnNewGamePageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the load game menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnLoadGamePageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the difficulty selection menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnDifficultySelectionPageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the start conditions menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnStartingConditionsPageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the tutorials menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnTutorialsPageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the workshop IMods menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnWorkshopPageEnabled(MenuPageEnabledArgs args);

    /// <summary>
    /// Called when the settings menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public abstract UniTask OnSettingsPageEnabled(MenuPageEnabledArgs args);
}
