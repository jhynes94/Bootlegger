using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if !UNITY_6000_0_OR_NEWER
// Stopped being laxative after Unity 6 
using PhysicsMaterial = UnityEngine.PhysicMaterial;
#endif

namespace Lachuga.KineticTool.Editor
{
    public enum CopyScaleMode
    {
        None,
        LocalToLocal,
        LossyToLocal,
    }

    public struct SmartBounds
    {
        public Bounds Bounds;
        private bool isCreated;

        public Vector3 Center => Bounds.center;
        public Vector3 Extents => Bounds.extents;
        
        public void Encapsulate(Bounds bounds)
        {
            if (isCreated)
            {
                Bounds.Encapsulate(bounds);
                return;
            }

            Bounds = bounds;
            isCreated = true;
        }
    }

    public static class Utility
    {
        public static void CopyTransform(Transform src, Transform dst, CopyScaleMode copyScaleMode = CopyScaleMode.None)
        {
            src.GetPositionAndRotation(out var position, out var rotation);
            dst.SetPositionAndRotation(position, rotation);

            switch (copyScaleMode)
            {
                case CopyScaleMode.LocalToLocal:
                    dst.localScale = src.localScale;
                    break;
                case CopyScaleMode.LossyToLocal:
                    dst.localScale = src.lossyScale;
                    break;
            }
        }


        public static void GetFullHierarchyFromTransforms(List<Transform> selection, List<Transform> fullHierarchy)
        {
            foreach (var selectedTransform in selection)
            {
                var allTransforms = selectedTransform.GetComponentsInChildren<Transform>();
                foreach (var transform in allTransforms)
                {
                    if (fullHierarchy.Contains(transform))
                        continue;

                    fullHierarchy.Add(transform);
                }
            }
        }
        
        public static List<List<Transform>> BuildSeparatedHierarchies(List<Transform> selectedTransforms, List<Transform> fullHierarchy)
        {
            // Build hierarchy chains
            var hierarchies = new List<List<Transform>>(selectedTransforms.Count);
            for (var i = 0; i < selectedTransforms.Count; i++)
            {
                hierarchies.Add(new List<Transform>(8));
                hierarchies[i].Add(selectedTransforms[i]);
                fullHierarchy.Remove(selectedTransforms[i]);
            }

            for (var fi = fullHierarchy.Count - 1; fi >= 0; fi--)
            {
                var t = fullHierarchy[fi];
                for (var i = 0; i < selectedTransforms.Count; i++)
                {
                    if (t == selectedTransforms[i])
                    {
                        fullHierarchy.RemoveAt(fi);
                        break;
                    }

                    if (!t.IsChildOf(selectedTransforms[i]))
                    {
                        continue;
                    }

                    fullHierarchy.RemoveAt(fi);
                    hierarchies[i].Add(t);
                    break;
                }
            }
            
            for (int i = hierarchies.Count - 1; i >= 0; i--)
            {
                var hierarchy = hierarchies[i];
                // Sort in Parent > Child order
                hierarchy.Sort((a, b) => a.IsChildOf(b) ? 1 : -1);
            }

            return hierarchies;
        }

        public static List<Transform> GetFullHierarchyAndPresort(List<Transform> selectedTransforms)
        {
            // Sort in Child>Parent order
            selectedTransforms.Sort((a, b) => a.IsChildOf(b) ? -1 : 1);

            var fullHierarchy = new List<Transform>(64);
            GetFullHierarchyFromTransforms(selectedTransforms, fullHierarchy);
            return fullHierarchy;
        }
        
        public static Transform CreateSimulationHierarchy(List<Transform> sourceObjects, EditorPhysicsScene scene, out SmartBounds bounds, bool needRigidbody = true)
        {
            var simulationRoot = new GameObject
            {
                layer = scene.PhysicsLayer
            };
            SceneManager.MoveGameObjectToScene(simulationRoot, scene.Scene);
            var simulationRootTransform = simulationRoot.transform;
            CopyTransform(sourceObjects[0], simulationRootTransform, CopyScaleMode.LossyToLocal);

            bounds = new SmartBounds();

            var foundCollider = false;
            var childCount = 0;
            
            if (KineticToolsParameters.instance.UseOriginalColliders)
            {
                foreach (var sourceObject in sourceObjects)
                {
                    var sourceColliders = sourceObject.GetComponents<Collider>();
                    if(sourceColliders.Length == 0)
                        continue;

                    foundCollider = true;
                    
                    var childGo = new GameObject();
                    childGo.layer = scene.PhysicsLayer;
                    var childTr = childGo.transform;
                    CopyTransform(sourceObject, childTr, CopyScaleMode.LossyToLocal);
                    childTr.parent = simulationRootTransform;

                    foreach (var sourceCollider in sourceColliders)
                    {
                        var collider = childGo.AddComponent(sourceCollider.GetType()) as Collider;
                        EditorUtility.CopySerialized(sourceCollider, collider);
                        if (collider is MeshCollider meshCollider)
                        {
                            meshCollider.convex = true;
                        }
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }

            if (!foundCollider)
            {
                for (int i = 1; i < sourceObjects.Count; i++)
                {
                    if (!TryAddChild(sourceObjects[i], scene, ref bounds, simulationRootTransform)) continue;
                    childCount++;
                }

                var mesh = TryGetMeshFromGameObject(sourceObjects[0]);
                if (mesh != null)
                {
                    var collider = AddColliderToSimulationObject(simulationRoot, mesh, scene.DefaultMaterial);
                    bounds.Encapsulate(collider.bounds);
                }
                else if (childCount == 0)
                {
                    // Add fallback collider
                    var collider = simulationRoot.AddComponent<SphereCollider>();
                    collider.radius = 0.05f;
                    collider.sharedMaterial = scene.DefaultMaterial;
                    bounds.Encapsulate(new Bounds(simulationRootTransform.position, Vector3.zero));
                }
            }

            if (needRigidbody)
            {
                var rigidbody = simulationRoot.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            return simulationRootTransform;
        }

        private static bool TryAddChild(Transform sourceObject, EditorPhysicsScene scene, ref SmartBounds bounds, Transform simulationRootTransform)
        {
            var inputMesh = TryGetMeshFromGameObject(sourceObject);

            if(ReferenceEquals(inputMesh, null))
            {
                return false;
            }            
            
            var childGo = new GameObject();
            childGo.layer = scene.PhysicsLayer;
            var childTr = childGo.transform;
            CopyTransform(sourceObject, childTr, CopyScaleMode.LossyToLocal);
            childTr.parent = simulationRootTransform;

            var col = AddColliderToSimulationObject(childGo, inputMesh, scene.DefaultMaterial);
            bounds.Encapsulate(col.bounds);
            return true;
        }

        private static Mesh TryGetMeshFromGameObject(Transform sourceObject)
        {
            Mesh inputMesh = null;

            if (sourceObject.TryGetComponent(out MeshFilter mf))
            {
                inputMesh = mf.sharedMesh;
            }
            else if(sourceObject.TryGetComponent(out SkinnedMeshRenderer smr))
            {
                inputMesh = smr.sharedMesh;
            }

            return inputMesh;
        }

        public static Ray GetSceneViewCameraRay(SceneView sceneView, Vector2 eventMousePosition)
        {
            var camera = sceneView.camera;
            var mPos = eventMousePosition;
            mPos.y = camera.pixelHeight - mPos.y;
            return camera.ScreenPointToRay(mPos); 
        }
        
        public static bool IsMouseInsideSceneView(SceneView sceneView)
        {
            var mousePosition = Event.current.mousePosition;
            var sceneViewRect = sceneView.position;
            
            var absoluteMousePosition = sceneViewRect.position + mousePosition;
            return sceneViewRect.Contains(absoluteMousePosition);
        }

        private static Collider AddColliderToSimulationObject(GameObject simulationObject, Mesh sourceMesh, PhysicsMaterial material)
        {
            var collider = simulationObject.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.sharedMaterial = material;
            collider.sharedMesh = sourceMesh;
            return collider;
        }
        
        public static float GetScrollAxis(Event e)
        {
            var scrollAxis = Mathf.Abs(e.delta.x) > Mathf.Abs(e.delta.y) ? e.delta.x : e.delta.y;
            if (Mathf.Abs(scrollAxis) > 1f)
            {
                scrollAxis = Mathf.Sign(scrollAxis);
            }

            return scrollAxis;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRigidbodyDrag(Rigidbody body, float linearDrag, float angularDrag)
        {
            #if UNITY_6000_0_OR_NEWER
                body.linearDamping = linearDrag;
                body.angularDamping = angularDrag;
            #else
                body.drag = linearDrag;
                body.angularDrag = angularDrag;
            #endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRigidbodyLinearVelocity(Rigidbody body, Vector3 linearVelocity)
        {
            #if UNITY_6000_0_OR_NEWER
                body.linearVelocity = linearVelocity;
            #else
                body.velocity = linearVelocity;
            #endif
        }
    }
}