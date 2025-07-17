using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lachuga.KineticTool.Editor
{
    [FilePath("UserSettings/KineticToolsParameters.data", FilePathAttribute.Location.ProjectFolder)]
    public class KineticToolsParameters : ScriptableSingleton<KineticToolsParameters>
    {
        public PhysicsFilter Filter;
        
        public bool EnterSnapWithModifiersKeys;
        public EventModifiers SnapShortcut = EventModifiers.Alt | EventModifiers.Control;
        
        public float SimulationSpeed;
        public float Bounciness;
        public float Friction;
        
        public DepenetrationMode DepenetrationMode = DepenetrationMode.BoundsOffset;
        
        [Tooltip("Orient selected objects to surface.\n\nShortcut: Left Mouse Button while snap active")]
        public bool OrientWithSurfaceNormal; 
        public Axis ObjectUpAxis = Axis.PlusY;

        public int PhysicsSimulationLayer;
        public bool LimitDistanceOfMeshSearch;
        public float DistanceOfMeshSearch;
        [Tooltip("(Only for selected objects)\nIf checked, both Tools will try to find colliders on selected objects. If no colliders found, will fallback to default collision model")]
        public bool UseOriginalColliders;
        [Tooltip("If checked, all raycasts will ignore backfaces.\nTurn this option off when snapping to double-sided meshes")]
        public bool IgnoreBackfaces;

        public bool IsReloadedOverlayMenuAfterFirstImport = false;

        public void Reset()
        {
            PhysicsSimulationLayer = 0;
            UseOriginalColliders = false;
            IgnoreBackfaces = true;
            EnterSnapWithModifiersKeys = true;
            
            SimulationSpeed = 2.0f;
            Bounciness = 0.0f;
            Friction = 0.5f;
            
            DepenetrationMode = DepenetrationMode.BoundsOffset;
            
            OrientWithSurfaceNormal = false;
            ObjectUpAxis = Axis.PlusY;
            Filter = new PhysicsFilter();

            LimitDistanceOfMeshSearch = false;
            DistanceOfMeshSearch = 50.0f;
            
            
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                SnapShortcut = EventModifiers.Alt | EventModifiers.Command;
                return;
            }
            SnapShortcut = SnapShortcut = EventModifiers.Alt | EventModifiers.Control;
        }

        public void SaveFile()
        {
            Save(true);
        }
    }

    [System.Serializable]
    public enum DepenetrationMode
    {
        Off = 0,
        BoundsOffset = 1,
        PhysicsCast = 2,
        PhysicsCastWithSelfCollision = 4,
        PhysicsSimulation = 8,
    }
    
    static class KineticToolParametersRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Kinetic Tools", SettingsScope.User)
            {
                label = "Kinetic Tools",
                guiHandler = (searchContext) =>
                {
                    var instance = KineticToolsParameters.instance;
                    var so = new SerializedObject(instance);

                    var originalLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth += 50;
                    
                    EditorGUI.BeginChangeCheck();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                        {
                            var shouldReset = EditorUtility.DisplayDialog("Reset to Defaults", "Do you want to reset all parameters to default values?", "Yes", "No");
                            if (shouldReset)
                            {
                                instance.Reset();
                                instance.SaveFile();
                            }
                        }
                    }

                    GUILayout.Label("Shortcuts", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(so.FindProperty(nameof(instance.EnterSnapWithModifiersKeys)));
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(!instance.EnterSnapWithModifiersKeys);
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(instance.SnapShortcut)));
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;

                    GUILayout.Space(30f);
                    
                    var filterProp = so.FindProperty(nameof(instance.Filter));
                    EditorGUILayout.Separator();
                    GUILayout.Label("Filter Settings", EditorStyles.boldLabel);
                    EditorGUILayout.Separator();

                    EditorGUILayout.PropertyField(filterProp.FindPropertyRelative(nameof(instance.Filter.LayerMask)));
                    EditorGUILayout.Space(2f);

                    EditorGUILayout.PropertyField(filterProp.FindPropertyRelative(nameof(instance.Filter.ExcludeShaders)));
                    EditorGUILayout.Space(2f);

                    EditorGUILayout.PropertyField(filterProp.FindPropertyRelative(nameof(instance.Filter.ExcludeGameObjectNames)));
                    GUILayout.Label("GameObjects with names containing any entry will be excluded\n(Case sensitive)\n\nYou can add your own filter logic via C# callback (see the Documentation)", EditorStyles.helpBox);

                    GUILayout.Space(30f);
                    GUILayout.Label("Other", EditorStyles.boldLabel);

                    instance.PhysicsSimulationLayer = EditorGUILayout.LayerField(new GUIContent("Physics Simulation Layer", "Layer to use in all physics simulations. It can be any layer that can collide with itself"), instance.PhysicsSimulationLayer);
                    
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(instance.LimitDistanceOfMeshSearch)));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(instance.DistanceOfMeshSearch)));
                    EditorGUI.indentLevel--;

                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        so.ApplyModifiedPropertiesWithoutUndo();
                        instance.SaveFile();
                    }
                },

                keywords = new HashSet<string>(new[] { "Kinetic", "Filter" })
            };

            return provider;
        }
    }
    
    

}