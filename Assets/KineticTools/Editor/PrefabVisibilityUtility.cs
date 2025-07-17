using System.Reflection;
using UnityEngine;

namespace Lachuga.KineticTool.Editor
{
    public enum InContextRenderMode
    {
        Normal = 0,
        Gray = 1,
        Hidden = 2,
        Unknown = -1,
    }
    
    public static class PrefabInContextUtility
    {
        public static InContextRenderMode GetRenderMode()
        {
            // Unfortunately Unity doesn't expose UnityEditor.SceneManagement.StageNavigationManager.instance.renderModeProperty and its type.
            // Using Reflection until better API
            
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneManagement.StageNavigationManager");
            var instance = type?.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)?.GetValue(null);
            var renderModeProperty = type?.GetProperty("contextRenderMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (type == null || instance == null || renderModeProperty == null)
            {
                Debug.LogWarning("Can't get Prefab InContext render mode via Reflection. Probably Unity API changed");
                return InContextRenderMode.Unknown;
            }
            
            return (InContextRenderMode)(int)renderModeProperty.GetValue(instance);
        }
    }
}