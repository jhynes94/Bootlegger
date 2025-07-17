using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lachuga.KineticTool.Editor
{
    [Overlay(typeof(SceneView), "Kinetic Tools", defaultDisplay = true, defaultDockIndex = 10000)]
    public class KineticToolsOverlay : Overlay
    {
        private readonly KineticToolsIcon icon = new("Icon_Overlay32.psd");
        
        public override void OnCreated()
        {
            collapsedIcon = icon.GetTexture();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.Add(new IMGUIContainer(DrawOverlayGUI));
            return root;
        }

        public bool ReloadIcon()
        {
            collapsedIcon = icon.GetTexture();
            return icon.IsLoaded;
        }
        
        private void DrawOverlayGUI()
        {
            GUILayout.Space(4f);

            var parameters = KineticToolsParameters.instance;
            var so = new SerializedObject(parameters);
            
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth += 20;
            
            EditorGUI.BeginChangeCheck();
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Gravity Tool", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(false));
                
                var dropShortcut = ShortcutManager.instance.GetShortcutBinding(KineticToolsCore.GravityShortcutID);
                DrawShortcut(new GUIContent($"({dropShortcut.ToString()})", "Hold to activate. You can rebind shortcut in Shortcut Manager"));
            }

            
            parameters.SimulationSpeed = EditorGUILayout.Slider("Simulation Speed", parameters.SimulationSpeed, 0.1f, 20f);
            parameters.Bounciness = EditorGUILayout.Slider("Bounciness", parameters.Bounciness, 0f, 1f);
            parameters.Friction = EditorGUILayout.Slider("Friction", parameters.Friction, 0f, 2f);

            
            EditorGUILayout.Separator();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Surface Snap Tool", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(false));
                
                var snapShortcutBind = parameters.EnterSnapWithModifiersKeys ? parameters.SnapShortcut.ToString().Replace(", ", "+") + "  or  " : "";
                var label = new GUIContent($"({snapShortcutBind}{ShortcutManager.instance.GetShortcutBinding(KineticToolsCore.SurfaceSnapShortcutID)})",
                    "Hold to activate.\nYou can rebind shortcut in Shortcut Manager\n\nModifier shortcut can be changed or disabled in Advanced settings");
                DrawShortcut(label);
            }

            EditorGUILayout.PropertyField(so.FindProperty(nameof(parameters.DepenetrationMode)));
            
            GUILayout.Space(20);

            EditorGUILayout.PropertyField(so.FindProperty(nameof(parameters.OrientWithSurfaceNormal)));

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                GUILayout.Label(new GUIContent("Object Axis", "Up axis to use as surface normal and yaw axis\n\nShortcut: Right Mouse Button while snap active"), GUILayout.Width(EditorGUIUtility.labelWidth - EditorGUIUtility.singleLineHeight));
                
                DrawAxisSelectionElement(new GUIContent("+x"), Axis.PlusX);
                DrawAxisSelectionElement(new GUIContent("-x"), Axis.MinusX);
                DrawAxisSelectionElement(new GUIContent("+y"), Axis.PlusY);
                DrawAxisSelectionElement(new GUIContent("-y"), Axis.MinusY);
                DrawAxisSelectionElement(new GUIContent("+z"), Axis.PlusZ);
                DrawAxisSelectionElement(new GUIContent("-z"), Axis.MinusZ);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(so.FindProperty(nameof(parameters.UseOriginalColliders)), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(so.FindProperty(nameof(parameters.IgnoreBackfaces)), GUILayout.ExpandWidth(false));
            
            using (new EditorGUILayout.HorizontalScope())
            {

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Advanced Settings", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    SettingsService.OpenUserPreferences("Preferences/Kinetic Tools");
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                parameters.SaveFile();
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
            return;

            void DrawAxisSelectionElement(GUIContent label, Axis axis)
            {
                GUI.color = parameters.ObjectUpAxis == axis ? Color.white : new Color(0.5f,0.5f,0.5f,1f);
                if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(25f), GUILayout.Height(25f)))
                {
                    parameters.ObjectUpAxis = axis;
                }
                GUI.color = Color.white;
            }
        }

        private static void DrawShortcut(GUIContent label)
        {
            GUI.color = Color.gray;
            GUILayout.Label(label, EditorStyles.miniBoldLabel);
            GUI.color = Color.white;
        }
    }
}
