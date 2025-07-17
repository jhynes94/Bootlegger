using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Lachuga.KineticTool.Editor
{
    public enum Axis
    {
        PlusX,
        MinusX,
        PlusY,
        MinusY,
        PlusZ,
        MinusZ
    }
    
    public class SurfaceSnapTool : Tool
    {
        private class SnapObjectData
        {
            public Transform SimulationRoot;
            public List<(Transform, Transform)> Targets;

            public Rigidbody Rigidbody;
            public Quaternion OriginalRotation;
            public Vector3 OriginalPosition;

            public Vector3 PositionPivotSpace;
            public Vector3 BoundsExtents;

            public Quaternion YawDelta;
            public Vector3 SimulationTargetPoint;
        }
        
        private readonly EditorPhysicsScene physicsScene = new();
        private readonly List<SnapObjectData> snapObjects = new(32);
        private bool shouldRecalculate;
        private Vector2 lastMousePosition;
        private float customYaw;
        
        private readonly KineticToolsIcon iconSnap = new("Icon_Snap.psd");
        private readonly KineticToolsIcon iconSnapNormal = new("Icon_Snap_Normal.psd");
        private KineticToolsParameters parameters;
        private const string UndoID = "Surface Snap Tool";
        private Vector3 gizmoPosition;
        private bool isCustomYaw;
        private readonly List<Transform> selectedTransforms = new(32);
        private List<List<Transform>> hierarchies = new();
        private bool isCenterPivot;
        private System.Type cachedEditorTool;

        private static readonly Quaternion[] AxisToRotation = 
        {
            Quaternion.Euler(0, 0, 90), 
            Quaternion.Euler(0, 0, -90),
            
            Quaternion.Euler(0, 0, 0), 
            Quaternion.Euler(180, 180, 0), 
            
            Quaternion.Euler(-90, 0, 0), 
            Quaternion.Euler(90, 180, 0), 
        };
        
        private static readonly int[] AxisIndex = 
        {
            0,0,1,1,2,2,
        };
        
        private static readonly Vector3[] AxisToYawAxis = 
        {
            new( 1,  0,  0),
            new(-1,  0,  0),
            new( 0,  1,  0),
            new( 0, -1,  0),
            new( 0,  0,  1),
            new( 0,  0, -1)
        };

        public override bool OnEnter()
        {
            selectedTransforms.Clear();
            selectedTransforms.AddRange(Selection.GetTransforms(SelectionMode.Unfiltered));
            if (selectedTransforms.Count == 0)
                return false;

            parameters = KineticToolsParameters.instance;
            
            var fullHierarchy = Utility.GetFullHierarchyAndPresort(selectedTransforms);
            
            var sceneView = SceneView.lastActiveSceneView;
            var camera = sceneView.camera;
            var cameraPos = camera.transform.position;
            isCenterPivot = Tools.pivotMode == PivotMode.Center;

            physicsScene.Create(selectedTransforms[0].position, cameraPos, fullHierarchy, null);
            hierarchies = Utility.BuildSeparatedHierarchies(selectedTransforms, fullHierarchy);
            
            customYaw = 0f;
            isCustomYaw = false;
            snapObjects.Clear();
            
            BuildSnapObjects();

            cachedEditorTool = ToolManager.activeToolType;
            Tools.current = UnityEditor.Tool.None;

            return true;
        }

        private void BuildSnapObjects()
        {
            var orientWithSurfaceNormal = parameters.OrientWithSurfaceNormal;

            // Apply Yaw if already have snap objects
            if (snapObjects.Count > 0)
            {
                var yawAxis = AxisToYawAxis[(int)parameters.ObjectUpAxis];// orientWithSurfaceNormal ? AxisToYawAxis[(int)parameters.ObjectUpAxis] : Vector3.up;
                var yawRotation = isCustomYaw ? Quaternion.Euler(yawAxis * customYaw) : Quaternion.identity;

                if (isCenterPivot)
                {
                    var snapRoot = snapObjects[0];
                    ApplySnapObject(snapRoot, snapRoot.OriginalPosition, snapRoot.OriginalRotation * yawRotation);
                }
                else
                {
                    for (var i = snapObjects.Count - 1; i >= 0; i--)
                    {
                        var snapObject = snapObjects[i];
                        var root = snapObject.SimulationRoot;
                        var target = snapObject.Targets[0];

                        root.rotation = snapObject.OriginalRotation * yawRotation;

                        Undo.RecordObject(target.Item1, UndoID);
                        target.Item1.SetPositionAndRotation(snapObject.OriginalPosition, target.Item2.rotation);
                    }
                }
            }
            customYaw = 0f;
            isCustomYaw = false;

            foreach (var snapObject in snapObjects)
            {
                if(snapObject.SimulationRoot == null)
                    continue;
                
                Object.DestroyImmediate(snapObject.SimulationRoot.gameObject);
            }
            snapObjects.Clear();
            
            var yawAxisIdx = AxisIndex[(int)parameters.ObjectUpAxis];
            var sign = (int)parameters.ObjectUpAxis % 2 == 0 ? 1f : -1f;

            var totalBounds = new SmartBounds();
            
            var handlePosition = Tools.handlePosition;
            var handleRotation = Tools.handleRotation;
            var handleInvRotation = Quaternion.Inverse(handleRotation);
            var isGlobalPivot = Tools.pivotRotation == PivotRotation.Global;
            
            // Create root for center mode
            if (isCenterPivot)
            { 
                CreateSnapBase(handlePosition, handleRotation, yawAxisIdx, sign);
            }
            
            for (var i = 0; i < hierarchies.Count; i++)
            {
                var hierarchy = hierarchies[i];
                var target = hierarchy[0];
                target.GetPositionAndRotation(out var originalPosition, out var originalRotation);

                if (orientWithSurfaceNormal)
                {
                    if (isCenterPivot)
                    {
                        // Make object local to center
                        Undo.RecordObject(target, UndoID);
                        RotateAroundPoint(target, handlePosition, handleInvRotation);
                    }
                    else if (!isGlobalPivot)
                    {
                        // Align object to world for correct bounds calculation
                        Undo.RecordObject(target, UndoID);
                        target.rotation = Quaternion.identity;
                    }
                }
 
                var simulationTransform = Utility.CreateSimulationHierarchy(hierarchy, physicsScene, out var bounds, needRigidbody: false);
                totalBounds.Encapsulate(bounds.Bounds);

                if (isCenterPivot)
                {
                    snapObjects[0].Targets.Add((target ,simulationTransform));
                    simulationTransform.parent =  snapObjects[0].SimulationRoot;
                    continue;
                }

                var midPoint = bounds.Center;
                if (parameters.DepenetrationMode == DepenetrationMode.Off)
                {
                    midPoint = originalPosition;
                }
                var snapObject = CreateSnapBase(midPoint, isGlobalPivot ? Quaternion.identity : originalRotation, yawAxisIdx, sign);
                snapObject.BoundsExtents = bounds.Extents;
                snapObject.OriginalPosition = originalPosition;
                simulationTransform.parent = snapObject.SimulationRoot;
                snapObject.Targets.Add((target, simulationTransform));
            }
            shouldRecalculate = true;

            if (isCenterPivot)
            {
                var posDiff = handlePosition - totalBounds.Center;
                snapObjects[0].BoundsExtents = totalBounds.Extents;

                foreach (var target in snapObjects[0].Targets)
                {
                    target.Item2.position += posDiff;
                }
            }

            var center = parameters.DepenetrationMode == DepenetrationMode.Off ? handlePosition : totalBounds.Center;
            for (int i = 0; i < snapObjects.Count; i++)
            {
                snapObjects[i].PositionPivotSpace = snapObjects[i].SimulationRoot.position - center;
            }
            
        }

        private bool ShouldExcludeHit(RaycastHit hit)
        {
            var body = hit.rigidbody;
            if (body == null) return false;
            
            for (var i = 0; i < snapObjects.Count; i++)
            {
                if (body == snapObjects[i].Rigidbody)
                {
                    return true;
                }
            }

            return false;
        }

        private SnapObjectData CreateSnapBase(Vector3 position, Quaternion rotation, int yawAxisIdx, float sign)
        {
            var simulationRoot = physicsScene.CreateEmptyRigidbody();
            var orientWithSurfaceNormal = parameters.OrientWithSurfaceNormal;
            simulationRoot.SetPositionAndRotation(position, orientWithSurfaceNormal ? Quaternion.identity : rotation);

            Vector3 upAxis;
            Vector3 rightAxis;
            switch (yawAxisIdx)
            {
                case 0:   // X up
                    upAxis = rotation * Vector3.right;
                    rightAxis = rotation * Vector3.down;
                    break;
                case 1:   // Y up
                    upAxis = rotation * Vector3.up;
                    rightAxis = rotation * Vector3.right;
                    break;
                default:  // Z up
                    upAxis = rotation * Vector3.forward;
                    rightAxis = rotation * Vector3.right;
                    break;
            }

            upAxis *= sign; 
            rightAxis *= sign; 
                
            var planeRotation = GetStableRotationFromNormal(upAxis);
            var objectRotationOnPlane = GetStableRotationFromNormal(upAxis, rightAxis);
            var invPlaneRotation = Quaternion.Inverse(planeRotation);
            var yawDelta = invPlaneRotation * objectRotationOnPlane;

            snapObjects.Add(new SnapObjectData()
            {
                SimulationRoot = simulationRoot,
                Targets = new (1),
                Rigidbody = simulationRoot.GetComponent<Rigidbody>(),
                OriginalPosition = position,
                OriginalRotation = rotation,
                YawDelta = yawDelta,
            });

            return snapObjects[^1];
        }

        private static void RotateAroundPoint(Transform target, Vector3 point, Quaternion rotation)
        {
            var dir = target.position - point;
            dir = rotation * dir;
            var newPosition = point + dir;
            var newRotation = rotation * target.rotation;
            target.SetPositionAndRotation(newPosition, newRotation);
        }

        private static void ApplySnapObject(SnapObjectData snapObject, Vector3 position, Quaternion rotation)
        {
            snapObject.SimulationRoot.SetPositionAndRotation(position, rotation);

            for (var i = snapObject.Targets.Count - 1; i >= 0; i--)
            {
                var snapTarget = snapObject.Targets[i];
                Undo.RecordObject(snapTarget.Item1, UndoID);
                Utility.CopyTransform(snapTarget.Item2, snapTarget.Item1);
            }
        }

        
        private void Recalculate()
        {
            shouldRecalculate = false;
            var depenetrationMode = parameters.DepenetrationMode;
            
            var mPos = lastMousePosition;
            var cam = SceneView.lastActiveSceneView.camera;
            var camPosition = cam.transform.position;
            mPos.y = cam.pixelHeight - mPos.y;
            var cameraRay = cam.ScreenPointToRay(mPos);
            var axisRotation = AxisToRotation[(int)parameters.ObjectUpAxis];
            
            if(!physicsScene.Raycast(cameraRay, out var cameraHit, ShouldExcludeHit)) 
                return;
            
            gizmoPosition = cameraHit.point;
            if (depenetrationMode == DepenetrationMode.PhysicsCastWithSelfCollision)
            {
                foreach (var snapObject in snapObjects)
                {
                    snapObject.Rigidbody.detectCollisions = false;
                }
            }

            var pivotToWorldRotation = Quaternion.identity;
            var pivotToWorld = Matrix4x4.TRS(cameraHit.point, pivotToWorldRotation, Vector3.one);
            
            var depenetrationAxis = AxisIndex[(int)parameters.ObjectUpAxis];

            for (var si = snapObjects.Count - 1; si >= 0; si--)
            {
                var snapObject = snapObjects[si];

                var rayTargetPoint = pivotToWorld.MultiplyPoint(snapObject.PositionPivotSpace);

                var rayOrigin = camPosition;// + snapObject.PositionLocalToPivot;
                var cameraToPoint = rayTargetPoint - rayOrigin;
                var cameraToPointMag = cameraToPoint.magnitude;
                cameraToPoint /= cameraToPointMag;

                var sceneRay = new Ray(rayOrigin, cameraToPoint);
                if (!physicsScene.Raycast(sceneRay, out var sceneHit, ShouldExcludeHit))
                {
                    ApplySnapObject(snapObject, sceneRay.origin + sceneRay.direction * cameraToPointMag, snapObject.OriginalRotation);
                    continue;
                }
                

                var planeRotation = GetStableRotationFromNormal(sceneHit.normal);
                Quaternion desiredObjectRotation;

                if (parameters.OrientWithSurfaceNormal)
                {
                    desiredObjectRotation = planeRotation * snapObject.YawDelta * axisRotation;
                }
                else
                {
                    desiredObjectRotation =  snapObject.OriginalRotation;
                }


                var snapRigidbody = snapObject.Rigidbody;
                snapRigidbody.rotation = desiredObjectRotation;


                switch (parameters.DepenetrationMode)
                {
                    case DepenetrationMode.PhysicsCast:
                    case DepenetrationMode.PhysicsCastWithSelfCollision:
                    {
                        var depenetration = parameters.OrientWithSurfaceNormal ? snapObject.BoundsExtents[depenetrationAxis] : ColliderDot(sceneHit.normal, snapObject.BoundsExtents);
                        var surfacePoint = sceneHit.point + sceneHit.normal * depenetration;

                        var castDir = (surfacePoint - camPosition).normalized;
                        
                        snapObject.Rigidbody.detectCollisions = true;
                        snapRigidbody.position = camPosition;
                        snapRigidbody.rotation = desiredObjectRotation;

                        var hits = snapRigidbody.SweepTestAll(castDir);
                        var maxHitIdx = -1;
                        var maxHitDist = float.MaxValue;
                        for (int i = 0; i < hits.Length; i++)
                        {
                            if(depenetrationMode != DepenetrationMode.PhysicsCastWithSelfCollision && hits[i].rigidbody != null)
                                continue;
                        
                            if (hits[i].distance < maxHitDist)
                            {
                                maxHitIdx = i;
                                maxHitDist = hits[i].distance;
                            }
                        }

                        if (maxHitIdx == -1)
                        {
                            ApplySnapObject(snapObject, camPosition + castDir * cameraToPointMag, desiredObjectRotation);
                            continue;
                        }

                        ApplySnapObject(snapObject, camPosition + castDir * maxHitDist, desiredObjectRotation);

                        if (depenetrationMode == DepenetrationMode.PhysicsCastWithSelfCollision && !isCenterPivot)
                        {
                            snapRigidbody.position = snapObject.SimulationRoot.position;
                            snapRigidbody.rotation = snapObject.SimulationRoot.rotation;
                        }
                        break;
                    }
                    case DepenetrationMode.PhysicsSimulation:
                    {
                        var depenetration = parameters.OrientWithSurfaceNormal ? snapObject.BoundsExtents[depenetrationAxis] : ColliderDot(sceneHit.normal, snapObject.BoundsExtents);
                        var surfacePoint = sceneHit.point + sceneHit.normal * depenetration;


                        snapRigidbody.position = surfacePoint;
                        snapObject.SimulationTargetPoint = sceneHit.point;
                        snapRigidbody.rotation = desiredObjectRotation;
                        snapRigidbody.useGravity = false;     
                        snapRigidbody.detectCollisions = true;
                        Utility.SetRigidbodyDrag(snapRigidbody, 0.3f, 0.3f);
                        snapRigidbody.sleepThreshold = 0f;
                        snapObject.SimulationRoot.SetPositionAndRotation(snapRigidbody.position, desiredObjectRotation);
                        snapRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                        Utility.SetRigidbodyLinearVelocity(snapRigidbody, Vector3.zero);
                        snapRigidbody.angularVelocity = Vector3.zero;
                        snapRigidbody.solverIterations = 16;
                        
                        break;
                    }
                    case DepenetrationMode.BoundsOffset:
                    {
                        var depenetration = parameters.OrientWithSurfaceNormal ? snapObject.BoundsExtents[depenetrationAxis] : ColliderDot(sceneHit.normal, snapObject.BoundsExtents);
                        var surfacePoint = sceneHit.point + sceneHit.normal * depenetration;
                        
                        ApplySnapObject(snapObject, surfacePoint, desiredObjectRotation);
                        break;
                    }
                    case DepenetrationMode.Off:
                    {
                        ApplySnapObject(snapObject, sceneHit.point, desiredObjectRotation);
                        break;
                    }
                }
            }

            if (depenetrationMode == DepenetrationMode.PhysicsSimulation)
            {
                for (int i = 0; i < 4; i++)
                {
                    foreach (var snapObject in snapObjects)
                    {
                        var diff = snapObject.SimulationTargetPoint - snapObject.Rigidbody.position;
                        Utility.SetRigidbodyLinearVelocity(snapObject.Rigidbody, diff);
                    }
                    physicsScene.Physics.Simulate(1f/4);
                }

                for (var i = snapObjects.Count - 1; i >= 0; i--)
                {
                    var snapObject = snapObjects[i];
                    ApplySnapObject(snapObject, snapObject.Rigidbody.position, snapObject.Rigidbody.rotation);
                }
            }
        }
        
        public override void OnSceneViewGUI(SceneView sceneView)
        {
            var e = Event.current;
            
            switch (e.type)
            {
                case EventType.MouseDown:
                {
                    switch (e.button)
                    {
                        case 0:
                            parameters.OrientWithSurfaceNormal = !parameters.OrientWithSurfaceNormal;
                            BuildSnapObjects();

                            e.Use();
                            break;
                        case 1:
                            parameters.ObjectUpAxis = (Axis)Mathf.Repeat((int)parameters.ObjectUpAxis+1, 6);
                            
                            BuildSnapObjects();
                            e.Use();
                            break;
                    }
                    break;
                }
                case EventType.MouseUp:
                {
                    switch (e.button)
                    {
                        case 0:
                            e.Use();
                            shouldRecalculate = true;
                            break;
                        case 1:
                            e.Use();
                            shouldRecalculate = true;
                            break;
                    }
                    break;
                }
            }

            if (e.isMouse)
            {
                shouldRecalculate = true;
                e.Use();
            }

            if (e.isScrollWheel)
            {
                var wheelDir =  Utility.GetScrollAxis(e);
                
                customYaw += (wheelDir) * 15f;
                customYaw = Mathf.Repeat(customYaw, 360f);
                isCustomYaw = true;
                
                BuildSnapObjects();
                e.Use();
            }
            lastMousePosition = EditorGUIUtility.PointsToPixels(e.mousePosition);


            if (shouldRecalculate)
            {
                Recalculate();
                SceneView.RepaintAll();
            }
            

            if (e.type == EventType.Repaint)
            {
                DrawSnapGizmos();
            }
        }



        private static void DrawGizmoIcon(Vector3 position, Texture2D icon)
        {
            var guiPoint = HandleUtility.WorldToGUIPoint(position);
            Handles.BeginGUI();
            var iconSize = 64 ;
            var halfSize = iconSize / 2;
            var guiOffset = new Vector2(iconSize , -iconSize);
            GUI.DrawTexture(new Rect(guiPoint.x - halfSize + guiOffset.x, guiPoint.y - halfSize + guiOffset.y, iconSize, iconSize), icon);
            Handles.EndGUI();
        }
        
        
        private void DrawSnapGizmos()
        {
            DrawGizmoIcon(gizmoPosition, parameters.OrientWithSurfaceNormal ? iconSnapNormal.GetTexture() : iconSnap.GetTexture());
            
            foreach (var snapObject in snapObjects)
            {
                snapObject.SimulationRoot.GetPositionAndRotation(out var handlePosition, out var handleRotation);
                Handles.PositionHandle(handlePosition, handleRotation);
            }
        }

        public override void OnExit()
        {
            ToolManager.SetActiveTool(cachedEditorTool);
            physicsScene.Destroy();
        }

        private const float HalfSqrtOfTwo = 0.707106781f;
        
        private static Quaternion GetStableRotationFromNormal(Vector3 normal)
        {
            var crossAxis = Vector3.up;
            if (Mathf.Abs(normal.y) > HalfSqrtOfTwo)
            {
                crossAxis = Vector3.right;
            }
            var tangent = Vector3.Cross(normal, crossAxis);
            if (tangent.sqrMagnitude < 0.001f)
            {
                tangent = Vector3.Cross(normal, Vector3.forward);
            }
            tangent.Normalize();
            var bitangent = Vector3.Cross(tangent, normal);
            return Quaternion.LookRotation(bitangent, normal);
        }

        private static Quaternion GetStableRotationFromNormal(Vector3 normal, Vector3 tangent)
        {
            var bitangent = Vector3.Cross(tangent, normal);
            return Quaternion.LookRotation(bitangent, normal);
        }

        private static float ColliderDot(Vector3 normal, Vector3 collider)
        {
            return Mathf.Abs(normal.x) * Mathf.Abs(collider.x) + Mathf.Abs(normal.y) * Mathf.Abs(collider.y) + Mathf.Abs(normal.z) * Mathf.Abs(collider.z);
        }
    }
}