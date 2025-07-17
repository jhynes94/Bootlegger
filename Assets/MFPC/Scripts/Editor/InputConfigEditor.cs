using MFPC.Input;
using UnityEditor;
using UnityEngine;

namespace MFPC.EditorScripts
{
    [CustomEditor(typeof(InputConfig))]
    public class InputConfigEditor : Editor
    {
        private const string DocumentationLink = "https://github.com/Nicel193/MFPCDocs/blob/main/InputConfig.md";
        private InputConfig _inputConfig;

        private void OnEnable()
        {
            _inputConfig = target as InputConfig;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Input Config documentation", GUILayout.Height(30)))
                Application.OpenURL(DocumentationLink);
            
#if !ENABLE_INPUT_SYSTEM
            EditorGUILayout.HelpBox("The Gamepad option will only work for New Input System", MessageType.Warning);
#endif         
                  
      base.OnInspectorGUI();
        }
    }
}