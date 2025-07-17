using UnityEditor;

namespace Lachuga.KineticTool.Editor
{
    public abstract class Tool
    {
        public abstract bool OnEnter();
        public abstract void OnExit();
        public abstract void OnSceneViewGUI(SceneView sceneView);
    }

}