#region

using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using StationeersLibrary;
using ThingImport;
using UnityEngine;

#endregion


namespace BetterLoadingScreens;

[HarmonyPatch]
public static class PatchFunctions {

    public static List<TextureReference> Textures = [];

    public static bool Initialized = false;

    [HarmonyPatch(typeof(ImGuiManager), nameof(ImGuiManager.RandomLoadingTexture)), HarmonyPostfix]
    public static void ImGuiManagerRandomLoadingTexturePrefix(ref ImGuiManager ___current, ref Texture __result) {
        try {
            if (!Initialized) {
                foreach (Texture? t in ___current.LoadingScreenTextures) {
                    Texture.DestroyImmediate(t);
                }
                Array.Clear(___current.LoadingScreenTextures, 0, ___current.LoadingScreenTextures.Length);
                Initialized = true;
            }

            Plugin.Instance.LogDebug("RandomLoadingTexture called");
            TextureReference texture = PatchFunctions.Textures.Pick();
            texture.Load();

            __result = texture.Texture;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }
}