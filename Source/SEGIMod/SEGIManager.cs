#region

#endregion

namespace SEGI;

public class SEGIManager : MonoBehaviour {
    public static SEGIManager Instance {
        get; private set;
    }

    public static SEGI SEGIInstance {
        get; private set;
    }

    [UsedImplicitly]
    private void Awake() {
        SEGIManager.Instance = this;
        SEGIManager.SEGIInstance = Camera.main.gameObject.AddComponent<SEGI>();
    }

    [UsedImplicitly]
    private void Update() {
        if (SEGIManager.SEGIInstance == null) {
            return;
        }

        SEGIManager.SEGIInstance.enabled = Data.Enabled;
        SEGIManager.SEGIInstance.sun = WorldManager.Instance.WorldSun.TargetLight;
    }
}