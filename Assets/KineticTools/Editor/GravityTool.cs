using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

#if !UNITY_6000_0_OR_NEWER
// Stopped being laxative after Unity 6 
using PhysicsMaterial = UnityEngine.PhysicMaterial;
#endif

namespace Lachuga.KineticTool.Editor
{
    public class GravityTool : Tool
    {
        private const float SpringRigidbodyDrag = 2f;
        
        private const double SimulationTickTime = 1.0 / 60.0;
        private const double DeathSpiralLimit = 0.3;
        private double lastTickTime = -1f;
        
        private KineticToolsParameters parameters;
        private PhysicsMaterial physicsMaterial;

        private readonly EditorPhysicsScene physicsScene = new();
        private readonly List<(Transform, Transform)> simulatingObjects = new(32);
        private readonly string undoCommandName = "Gravity Tool";

        private bool isSpringActive;
        private SceneView springSceneView;
        private Transform mouseSpring;
        private float mouseSpringDistanceFromCamera;
        private int guiControlID;
        private System.Type cachedEditorTool;


        private readonly KineticToolsIcon originIcon = new("Icon_Pivot.psd");

        public override bool OnEnter()
        {
            parameters = KineticToolsParameters.instance;

            simulatingObjects.Clear();
            isSpringActive = false;

            var selectedTransforms = new List<Transform>(Selection.GetTransforms(SelectionMode.Unfiltered));
            if (selectedTransforms.Count == 0)
                return false;

            var fullHierarchy = Utility.GetFullHierarchyAndPresort(selectedTransforms);
            
            physicsMaterial = new PhysicsMaterial
            {
                bounciness = parameters.Bounciness,
                staticFriction = parameters.Friction,
                dynamicFriction = parameters.Friction
            };

            var cameraPos = SceneView.lastActiveSceneView.camera.transform.position;
            physicsScene.Create(selectedTransforms[0].position, cameraPos, fullHierarchy, physicsMaterial);

            var hierarchies = Utility.BuildSeparatedHierarchies(selectedTransforms, fullHierarchy);
            var isCenterPivot = Tools.pivotMode == PivotMode.Center;

            if (isCenterPivot)
            {
                simulatingObjects.Add((null, physicsScene.CreateEmptyRigidbody()));
            }
            
            for (var i = 0; i < hierarchies.Count; i++)
            {
                var hierarchy = hierarchies[i];

                var simulationRoot = Utility.CreateSimulationHierarchy(hierarchy, physicsScene, out _, !isCenterPivot);
                if (isCenterPivot)
                {
                    simulationRoot.parent = simulatingObjects[0].Item2;
                }
                simulatingObjects.Add((hierarchy[0].transform, simulationRoot));
            }
            
            cachedEditorTool = ToolManager.activeToolType;
            Tools.current = UnityEditor.Tool.None;
            
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            return true;
        }

        public override void OnExit()
        {
            DeactivateSpring();
            physicsScene.Destroy();
            Object.DestroyImmediate(physicsMaterial);
            ToolManager.SetActiveTool(cachedEditorTool);
            
            EditorApplication.update -= OnUpdate;
        }

        public override void OnSceneViewGUI(SceneView sceneView)
        {
            var e = Event.current;
            guiControlID = GUIUtility.GetControlID(FocusType.Passive);
            var isActiveSpringView = isSpringActive && sceneView == springSceneView;
            

            switch (e.GetTypeForControl(guiControlID))
            {
                case EventType.MouseUp:
                    if (isActiveSpringView)
                    {
                        DeactivateSpring();
                        e.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        TryActivateSpring(EditorGUIUtility.PointsToPixels(e.mousePosition), sceneView);
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (isActiveSpringView)
                    {
                        UpdateSpringPosition(sceneView, e);
                        e.Use();
                        // sceneView.Focus();
                    }
                    break;
                case EventType.ScrollWheel:
                    if (isActiveSpringView)
                    {
                        mouseSpringDistanceFromCamera += Utility.GetScrollAxis(e) * -0.25f;
                        mouseSpringDistanceFromCamera = Mathf.Max(mouseSpringDistanceFromCamera, 0.1f);
                        UpdateSpringPosition(sceneView, e);

                        e.Use();
                    }
                    break;
                
                case EventType.Repaint:
                    if (isActiveSpringView)
                    {
                        var springJoint = mouseSpring.GetComponent<SpringJoint>();
                        var p0 = mouseSpring.position;
                        var p1 = springJoint.connectedBody.transform.TransformPoint(springJoint.connectedAnchor);
                        Handles.color = new Color(1.0f, 0.5f, 0.25f);
                        Handles.DrawLine(p0, p1, 5f);
                    }

                    var iconSize = 16;
                    var halfSize = iconSize / 2;

                    var iconTexture = originIcon.GetTexture();
                    
                    if (Tools.pivotMode == PivotMode.Center)
                    {
                        var guiPoint = HandleUtility.WorldToGUIPoint(Tools.handlePosition);
                        Handles.BeginGUI();
                        GUI.DrawTexture(new Rect(guiPoint.x - halfSize, guiPoint.y - halfSize, iconSize, iconSize), iconTexture);
                        Handles.EndGUI();
                        break;    
                    }
                    
                    foreach (var simulatingObject in simulatingObjects)
                    {
                        var guiPoint = HandleUtility.WorldToGUIPoint(simulatingObject.Item1.position);
                        Handles.BeginGUI();
                        GUI.DrawTexture(new Rect(guiPoint.x - halfSize, guiPoint.y - halfSize, iconSize, iconSize), iconTexture);
                        Handles.EndGUI();
                    }
                    break;
            }

        }

        private void UpdateSpringPosition(SceneView sceneView, Event e)
        {
            var cameraRay = Utility.GetSceneViewCameraRay(sceneView, EditorGUIUtility.PointsToPixels(e.mousePosition));
            var foundTarget = physicsScene.Raycast(cameraRay, out var hit, ShouldExcludeHit);
                        
            var newPosition = cameraRay.origin + cameraRay.direction * mouseSpringDistanceFromCamera;
            if (foundTarget && (hit.distance < mouseSpringDistanceFromCamera))
            {
                newPosition = hit.point;
            }
                        
            mouseSpring.position = newPosition;
            return;
            
            bool ShouldExcludeHit(RaycastHit raycastHit) => raycastHit.rigidbody == mouseSpring.GetComponent<SpringJoint>().connectedBody;
        }


        private void TryActivateSpring(Vector2 mousePosition, SceneView sceneView)
        {
            DeactivateSpring();
            // Get object under mouse
            var cameraRay = Utility.GetSceneViewCameraRay(sceneView, mousePosition);
            bool ShouldExcludeHit(RaycastHit raycastHit) => raycastHit.rigidbody == null;
            var foundTarget = physicsScene.Raycast(cameraRay, out var hit, ShouldExcludeHit);
            if (!foundTarget)
            {
                return;
            }
                
            // Spawn Mouse Spring
            mouseSpring = physicsScene.CreateEmptyRigidbody();
            var springGo = mouseSpring.gameObject;
            mouseSpring.position = hit.point;
            mouseSpringDistanceFromCamera = hit.distance;
                
            var springBody = springGo.GetComponent<Rigidbody>();
            springBody.isKinematic = true;

            var springJoint = springGo.AddComponent<SpringJoint>();
            var target = hit.rigidbody;
            springJoint.connectedBody = target;
            springJoint.spring = 500f;
            springJoint.damper = 200f; 
            Utility.SetRigidbodyDrag(target, SpringRigidbodyDrag, SpringRigidbodyDrag);

            GUIUtility.hotControl = guiControlID;

            springSceneView = sceneView;
            isSpringActive = true;
        }


        
        private void DeactivateSpring()
        {
            if(!isSpringActive)
                return;
            
            if (mouseSpring != null)
            {
                var body = mouseSpring.GetComponent<SpringJoint>().connectedBody;
                Utility.SetRigidbodyDrag(body, 0.05f, 0.0f);
                    
                Object.DestroyImmediate(mouseSpring.gameObject);
            }

            if (GUIUtility.hotControl == guiControlID)
                GUIUtility.hotControl = 0;
            
            isSpringActive = false;
            springSceneView = null;
        }


        private void OnUpdate()
        {
            var currentTime = EditorApplication.timeSinceStartup;
            var passedTime = currentTime - lastTickTime;
            if (passedTime < SimulationTickTime || currentTime < lastTickTime)
            {
                return;
            }
            
            lastTickTime = currentTime;
            
            if (passedTime > DeathSpiralLimit)
            {
                return;
            }
            
            OnSimulate();
            SceneView.RepaintAll();
        }

        private void OnSimulate()
        {
            var originalSimMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            
            var tickCount = Mathf.CeilToInt(parameters.SimulationSpeed);
            var stepTime = (float)(SimulationTickTime/tickCount) * parameters.SimulationSpeed;
            for (int i = 0; i < tickCount; i++)
            {
                physicsScene.Physics.Simulate(stepTime);
            }

            for (var i = simulatingObjects.Count - 1; i >= 0; i--)
            {
                var simulatingObject = simulatingObjects[i];
                if(simulatingObject.Item1 == null) 
                    continue;
                
                Undo.RecordObject(simulatingObject.Item1, undoCommandName);
                Utility.CopyTransform(simulatingObject.Item2, simulatingObject.Item1);

            }

            Physics.simulationMode = originalSimMode;
        }
    }
}