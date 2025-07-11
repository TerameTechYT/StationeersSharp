#region

#endregion

namespace StationeersLibrary.Args;

public class SceneLoadArgs {
		public Scene Scene { get; private set; }
		public LoadSceneMode? LoadMode { get; private set; }

		public string SceneName => this.Scene.name;
		public string ScenePath => this.Scene.path;
		public bool SceneIsSubScene => this.Scene.isSubScene;

		public SceneLoadArgs(Scene scene, LoadSceneMode? loadSceneMode = null) {
				this.Scene = scene;
				this.LoadMode = loadSceneMode;
		}
}
