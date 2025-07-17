using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lachuga.KineticTool.Editor
{           
    // This abomination of a class required to avoid having Editor icons inside Resources folder, and have ability to move package folder.
    public static class KineticToolsIcons
    {
        // Load icon texture from folder where this script is located
        public static Texture2D LoadIcon(string iconFileName)
        {
            var scriptFolderPath = Path.GetDirectoryName(GetScriptPath());
            var iconPath = Path.Combine(scriptFolderPath, iconFileName);
            iconPath = iconPath.Replace('\\', '/');
            return EditorGUIUtility.Load(iconPath) as Texture2D;
        }
        
        private static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            sourceFilePath = sourceFilePath.Replace('\\', '/');
            var relativePath = sourceFilePath.Replace(Application.dataPath, "Assets");
            return relativePath;
        }
    }

    public class KineticToolsIcon
    {
        private readonly string filename;
        private Texture2D texture;
        private bool isLoaded;

        public KineticToolsIcon(string filename)
        {
            this.filename = filename;
        }

        public bool IsLoaded => isLoaded;

        public Texture2D GetTexture()
        {
            if (isLoaded) return texture;
            
            texture = KineticToolsIcons.LoadIcon(filename);
            if (texture != null)
            {
                isLoaded = true;
            }
            return texture;
        }
    }
}    



