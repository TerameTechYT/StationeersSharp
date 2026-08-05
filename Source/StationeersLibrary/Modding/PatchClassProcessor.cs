#region

using HarmonyLib;

using System.Collections;
using System.Reflection;

#endregion

namespace StationeersLibrary.Modding;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public abstract class HarmonyPatchConditionAttribute : Attribute {
    public virtual bool ShouldPatch => false;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchVersion(string minimum, string maximum) : HarmonyPatchConditionAttribute {
    public Version MinimumVersion => new(minimum);
    public Version MaximumVersion => new(maximum);

    public override bool ShouldPatch => this.MinimumVersion < Constants.GAME_VERSION && this.MaximumVersion > Constants.GAME_VERSION;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchVersionMinimum(string version) : HarmonyPatchConditionAttribute {
    public Version MinimumVersion => new(version);

    public override bool ShouldPatch => this.MinimumVersion < Constants.GAME_VERSION;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchVersionMaximum(string version) : HarmonyPatchConditionAttribute {
    public Version MaximumVersion => new(version);

    public override bool ShouldPatch => this.MaximumVersion > Constants.GAME_VERSION;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchVersions(params string[] versions) : HarmonyPatchConditionAttribute {
    public Version[] Versions => [.. versions.Select(v => new Version(v))];

    public override bool ShouldPatch => this.Versions.Contains(Constants.GAME_VERSION);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchBranch(GameBranch branch) : HarmonyPatchConditionAttribute {
    public GameBranch GameBranch => branch;

    public override bool ShouldPatch => Constants.GAME_BRANCH == this.GameBranch;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HarmonyPatchConfig<T>(string section, string key) : HarmonyPatchConditionAttribute where T : Mod<T> {
    public string Section => section;
    public string Key => key;

    private T? DeclaringMod => typeof(T).FieldGetValue<T>("Instance", BindingFlags.Public | BindingFlags.Static);
    public override bool ShouldPatch => this.DeclaringMod?.GetConfigValue<bool>(this.Section, this.Key) ?? false;
}

public class ConditionalPatchClassProcessor : PatchClassProcessor {
    public ConditionalPatchClassProcessor(Harmony harmony, Type type, bool allowUnannotatedType = false) : base(harmony, type, allowUnannotatedType) {
        Traverse<IList> patchMethods = new Traverse(this).Field<IList>("patchMethods");
        if (patchMethods.Value == null || patchMethods.Value.Count == 0) {
            return;
        }

        FieldInfo attributePatchInfo = patchMethods.Value[0].GetType().GetField("info", BindingFlags.NonPublic | BindingFlags.Instance);
        if (attributePatchInfo == null) {
            return;
        }

        List<object> toRemove = [];
        foreach (object? patchMethod in patchMethods.Value) {
            if (attributePatchInfo.GetValue(patchMethod) is not HarmonyMethod info) {
                continue;
            }

            foreach (HarmonyPatchConditionAttribute condition in info.method.GetCustomAttributes(true).OfType<HarmonyPatchConditionAttribute>()) {
                if (condition.ShouldPatch) {
                    continue;
                }

                toRemove.Add(patchMethod);
                break;
            }
        }

        foreach (object patchMethod in toRemove) {
            patchMethods.Value.Remove(patchMethod);
        }
    }
}