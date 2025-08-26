#region

#endregion

namespace StationeersLibrary.Mods;

public interface IMod {
    /// <summary>
    /// Called by <see cref="StationeersLaunchPad"/> with any prefabs.
    /// </summary>
    /// <param name="prefabs">Prefabs this mod should have</param>
    public abstract void OnLoaded(List<GameObject> prefabs);

    /// <summary>
    /// Called when the mod is being unloaded.
    /// </summary>
    public abstract void OnUnloaded();

    /// <summary>
    /// Called by <see cref="IMod"/> when the GameObject is created.
    /// </summary>
    public virtual void OnAwake() { }

    /// <summary>
    /// Called by <see cref="IMod"/> every frame.
    /// </summary>
    public virtual void OnUpdate(float deltaTime) { }

    /// <summary>
    /// Called by <see cref="IMod"/> after <see cref="OnUpdate"/>
    /// </summary>
    public virtual void OnLateUpdate(float deltaTime) { }

    /// <summary>
    /// Called by <see cref="IMod"/> on a fixed framerate.
    /// </summary>
    public virtual void OnFixedUpdate(float deltaTime) { }

    /// <summary>
    /// Called by Unity for handling UGUI events.
    /// </summary>
    public virtual void OnGUI() { }

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="IMod"/> attached is enabled.
    /// </summary>
    public virtual void OnEnable() { }

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="IMod"/> attached is disabled.
    /// </summary>
    public virtual void OnDisable() { }

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="IMod"/> attached is about to be destroyed.
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// Called when any scene is loaded
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnSceneLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the inital game loading scene is loaded. 
    /// This is unlikely to ever be called.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnSplashLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the base scene is loaded.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public virtual UniTask OnBaseLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the character customization screen is loaded.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnCharacterCustomizationLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when any scene is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnSceneUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when inital game loading scene is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnSplashUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the base scene is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnBaseUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the character customization screen is unloaded.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnCharacterCustomizationUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when any main menu page is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnMenuPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the main menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnMainMenuPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the new game menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnNewGamePageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the load game menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnLoadGamePageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the difficulty selection menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnDifficultySelectionPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the start conditions menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnStartingConditionsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the tutorials menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnTutorialsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the workshop IMods menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnWorkshopPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the settings menu is enabled.
    /// </summary>
    /// <param name="args"></param>
    public virtual UniTask OnSettingsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;
}
