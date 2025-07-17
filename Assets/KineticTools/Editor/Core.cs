using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Lachuga.KineticTool.Editor
{
    [InitializeOnLoad]
    public static class KineticToolsCore
    {
        private static readonly GravityTool gravityTool = new();
        private static readonly SurfaceSnapTool surfaceSnapTool = new();
        
        public const string GravityShortcutID = "KineticTools/Drop";
        public const string SurfaceSnapShortcutID = "KineticTools/Surface Snap";
        
        private static Tool activeTool;
        private static bool lastModifierShortcutState;
        
        #if UNITY_6000_0_OR_NEWER
            private static bool needOverlayMenuReload = true;
        #endif


        static KineticToolsCore()
        {
            EditorApplication.focusChanged += EditorApplicationOnFocusChanged;
            SceneView.duringSceneGui += OnSceneGUI;
        }


        private static void EditorApplicationOnFocusChanged(bool isFocused)
        {
            if (!isFocused)
            {
                ExitActiveTool();
                lastModifierShortcutState = false;
            }                                                                                           
        }

        
        private static void OnSceneGUI(SceneView sceneView)
        {
            EnsureOverlayMenuReloaded();
            
            var e = Event.current;
            var parameters = KineticToolsParameters.instance;     
            
            var isModifierShortcutPressed = e.modifiers == parameters.SnapShortcut;

            if (isModifierShortcutPressed != lastModifierShortcutState)
            {
                if (isModifierShortcutPressed)
                {
                    if (Utility.IsMouseInsideSceneView(sceneView) && DragAndDrop.objectReferences.Length == 0)
                    {
                        sceneView.Focus();
                        ConsumeToolShortcut(surfaceSnapTool, true);
                        lastModifierShortcutState = true;
                    }
                }
                else
                {
                    ConsumeToolShortcut(surfaceSnapTool, false);
                    lastModifierShortcutState = false;
                }
            }
            
            activeTool?.OnSceneViewGUI(sceneView);
        }

        private static void TryEnterTool(Tool tool)
        {
            ExitActiveTool();
            
            if (tool.OnEnter())
            {
                activeTool = tool;
            }
            else
            {
                tool.OnExit();
            }
        }

        private static void ExitActiveTool()
        {
            activeTool?.OnExit();
            activeTool = null; 
        }

        
        [ClutchShortcut(GravityShortcutID, defaultKeyCode: KeyCode.G, defaultShortcutModifiers: ShortcutModifiers.Shift)]        
        public static void GravityToolShortcut(ShortcutArguments shortcutArguments)
        {               
            ConsumeToolShortcut(gravityTool, shortcutArguments.stage == ShortcutStage.Begin);
        }
        
        [ClutchShortcut(SurfaceSnapShortcutID, defaultKeyCode: KeyCode.X, defaultShortcutModifiers: ShortcutModifiers.Shift)]        
        public static void SnapToNormalToolShortcut(ShortcutArguments shortcutArguments)
        {
            ConsumeToolShortcut(surfaceSnapTool, shortcutArguments.stage == ShortcutStage.Begin);
        }
        

        private static void ConsumeToolShortcut(Tool tool, bool shortcutPressed)
        {
            if (shortcutPressed)
            {
                TryEnterTool(tool);    
            }
            else if(activeTool == tool)
            {
                ExitActiveTool();
            }
        }
        
        // Try to reload OverlayMenu after first import of package.
        // This needed because Overlay icon isn't imported when Overlay created, and OverlayMenu caches missing icon
        private static void EnsureOverlayMenuReloaded()
        {
            #if UNITY_6000_0_OR_NEWER
                if(!needOverlayMenuReload) 
                    return;

                var parameters = KineticToolsParameters.instance;
                if (parameters.IsReloadedOverlayMenuAfterFirstImport)
                {
                    needOverlayMenuReload = false;
                    return;
                }
                
                var sws = SceneView.sceneViews;
                var overlayMenuList = new List<Overlay>();
                var isLoaded = false;
                foreach (SceneView sceneView in sws)
                {
                    var overlayWindow = sceneView.overlayCanvas;
                    var overlays = overlayWindow?.overlays;
                    if (overlays == null) continue;
                    
                    foreach (var overlay in overlays)
                    {
                        if (overlay is KineticToolsOverlay kto)
                        {
                            isLoaded = kto.ReloadIcon();
                            continue;
                        }

                        if (overlay.id != "Overlays/OverlayMenu") continue;
                        if(!overlay.displayed) continue;
                           
                        overlayMenuList.Add(overlay);
                        break;
                    }
                }
                
                if(!isLoaded)
                    return;

                foreach (var overlayMenu in overlayMenuList)
                {
                    overlayMenu.displayed = false;
                    overlayMenu.displayed = true;
                }
                
                needOverlayMenuReload = false;
                parameters.IsReloadedOverlayMenuAfterFirstImport = true;
                parameters.SaveFile();
            #endif
        }
    }
}