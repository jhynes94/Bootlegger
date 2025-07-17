using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if !UNITY_6000_0_OR_NEWER
// Stopped being laxative after Unity 6 
using PhysicsMaterial = UnityEngine.PhysicMaterial;
#endif

namespace Lachuga.KineticTool.Editor
{
    [System.Serializable]
    public class PhysicsFilter
    {
        public LayerMask LayerMask = ~0;
        public List<Shader> ExcludeShaders = new();
        public List<string> ExcludeGameObjectNames = new();
        
        [System.NonSerialized] public System.Predicate<GameObject> ShouldIncludeObjectCustomPass;

        public bool ShouldIncludeObject(GameObject gameObject)
        {
            if (((1 << gameObject.layer) & LayerMask.value) == 0)
            {
                return false;
            }

            var objName = gameObject.name;
            if (ExcludeGameObjectNames.Any(t => objName.Contains(t)))
            {
                return false;
            }


            if (!ReferenceEquals(ShouldIncludeObjectCustomPass, null))
            {
                return ShouldIncludeObjectCustomPass(gameObject);
            }
            
            return true;
        }


        public bool ShouldIncludeObject(Renderer renderer)
        {
            if (!ShouldIncludeObject(renderer.gameObject))
            {
                return false;
            }
            
            var mat = renderer.sharedMaterial;
            if (mat == null || ExcludeShaders.Contains(mat.shader))
            {
                return false;
            }
            
            return true;
        }
    }
    
    
    public class EditorPhysicsScene
    {
        public Scene Scene;
        public PhysicsScene Physics;
        public PhysicsMaterial DefaultMaterial;
        public int PhysicsLayer;
        
        private readonly RaycastHit[] sceneHits = new RaycastHit[64];
        private PhysicsFilter filter;
        
        private bool useBounds;
        private Bounds worldBounds;
        private Vector3 cameraPosition;
        private float cameraDistanceMaxSqr;
        private bool queryBackfaces;


        
        public void Create(Vector3 boundsCenter, Vector3 currentCameraPosition, List<Transform> excludeTransforms, PhysicsMaterial material)
        {
            if (Scene.isLoaded)
            {
                Debug.LogError("Trying to create second physics scene");
                return;
            }

            var parameters = KineticToolsParameters.instance;
            filter = parameters.Filter;
            
            Scene = EditorSceneManager.NewPreviewScene();
            Physics = Scene.GetPhysicsScene();
            DefaultMaterial = material;
            useBounds = KineticToolsParameters.instance.LimitDistanceOfMeshSearch;
            PhysicsLayer = parameters.PhysicsSimulationLayer;
            queryBackfaces = UnityEngine.Physics.queriesHitBackfaces;
            UnityEngine.Physics.queriesHitBackfaces = !parameters.IgnoreBackfaces;
            
            var limitDistance = parameters.DistanceOfMeshSearch;
            worldBounds = new Bounds(boundsCenter, new Vector3(limitDistance,limitDistance,limitDistance));
            cameraDistanceMaxSqr = limitDistance * limitDistance;
            cameraPosition = currentCameraPosition;
            
            
            var excludeRenderers = BuildExcludeSet(excludeTransforms);
            GetSceneObjects(out var sceneRenderers, out var terrainColliders);
            TryAddRenderersToScene(sceneRenderers, excludeRenderers);
            TryAddTerrainsToScene(terrainColliders);
        }


        private static HashSet<Renderer> BuildExcludeSet(List<Transform> excludeList)
        {
            var lodGroups = Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var excludeSet = new HashSet<Renderer>(4096);
            for (var i = 0; i < excludeList.Count; i++)
            {
                var excludeTransform = excludeList[i];
                var renderer = excludeTransform.GetComponent<Renderer>();
                if (ReferenceEquals(renderer, null))
                    continue;

                excludeSet.Add(renderer);
            }

            for (var lodGroupIdx = 0; lodGroupIdx < lodGroups.Length; lodGroupIdx++)
            {
                var lodGroup = lodGroups[lodGroupIdx];
                var lods = lodGroup.GetLODs();
                if (lods.Length < 2)
                    continue;

                for (int i = 1; i < lods.Length; i++)
                {
                    var renderers = lods[i].renderers;
                    foreach (var renderer in renderers)
                    {
                        excludeSet.Add(renderer);
                    }
                }
            }

            return excludeSet;
        }
        
        private static void GetSceneObjects(out List<Renderer> renderers, out List<TerrainCollider> terrainColliders)
        {
            renderers = new List<Renderer>(32768);
            terrainColliders = new List<TerrainCollider>(16);
            
            var shouldGetSceneMeshes = true;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                switch (prefabStage.mode)
                {
                    case PrefabStage.Mode.InIsolation:
                        shouldGetSceneMeshes = false;
                        break;
                    case PrefabStage.Mode.InContext:
                        shouldGetSceneMeshes = PrefabInContextUtility.GetRenderMode() != InContextRenderMode.Hidden;
                        break;
                }

                var prefabRoot = prefabStage.prefabContentsRoot;
                renderers.AddRange(prefabRoot.GetComponentsInChildren<Renderer>());
                terrainColliders.AddRange(prefabRoot.GetComponentsInChildren<TerrainCollider>());
            }

            if (shouldGetSceneMeshes)
            {
                renderers.AddRange(Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));  
                terrainColliders.AddRange(Object.FindObjectsByType<TerrainCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
            }
        }
        
        private void TryAddRenderersToScene(List<Renderer> renderers, HashSet<Renderer> excludeSet)
        {
            var rendererCount = renderers.Count;
            for (int i = 0; i < rendererCount; ++i)
            {
                var renderer = renderers[i];
                
                if (useBounds && !worldBounds.Intersects(renderer.bounds) && cameraDistanceMaxSqr < renderer.bounds.SqrDistance(cameraPosition))
                    continue;
            
                if(excludeSet.Contains(renderer))
                    continue;
            
                if(!filter.ShouldIncludeObject(renderer))
                    continue;
            
                Mesh collisionMesh = null;

                switch (renderer)
                {
                    case MeshRenderer:
                    {
                        var meshFilter = renderer.GetComponent<MeshFilter>();
                        if(meshFilter == null)
                            continue;
                        collisionMesh = meshFilter.sharedMesh;
                        break;
                    }
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        collisionMesh = skinnedMeshRenderer.sharedMesh;
                        break;
                }

                if (ReferenceEquals(collisionMesh, null))
                    continue;
            
                var cloneGo = new GameObject(string.Empty);
                var cloneTr = cloneGo.transform;
                cloneGo.layer = PhysicsLayer;
                SceneManager.MoveGameObjectToScene(cloneGo, Scene);

                Utility.CopyTransform(renderer.transform, cloneTr, CopyScaleMode.LossyToLocal);
                var collider = cloneGo.AddComponent<MeshCollider>();
                collider.convex = false;
                collider.sharedMesh = collisionMesh;
                collider.sharedMaterial = DefaultMaterial;
            }
        }

        private void TryAddTerrainsToScene(List<TerrainCollider> terrainColliders)
        {
            foreach (var terrainCollider in terrainColliders)
            {
                var isTerrainOutOfBounds = !worldBounds.Intersects(terrainCollider.bounds) && cameraDistanceMaxSqr < terrainCollider.bounds.SqrDistance(cameraPosition);
                if (useBounds && isTerrainOutOfBounds)
                {
                    continue;
                }
                    
                var cloneGo = new GameObject(string.Empty);
                var cloneTr = cloneGo.transform;
                cloneGo.layer = PhysicsLayer;
                SceneManager.MoveGameObjectToScene(cloneGo, Scene);
                
                Utility.CopyTransform(terrainCollider.transform, cloneTr, CopyScaleMode.LossyToLocal);
                var collider = (Collider)cloneGo.AddComponent(terrainCollider.GetType());
                EditorUtility.CopySerialized(terrainCollider, collider);
                collider.sharedMaterial = DefaultMaterial;

                // Avoid tree calculation on far terrains even then bounds aren't used
                if (isTerrainOutOfBounds)
                {
                    continue;
                }

                var protoRenderers = new List<Renderer>(64);
                
                var data = terrainCollider.terrainData;
                var trees = data.treeInstances;
                var treePrototypes = data.treePrototypes;

                    
                // Build Tree Prototypes to use in Tree instances
                var prototypes = new List<Transform>(treePrototypes.Length);
                var prototypeRotatable = new List<bool>(treePrototypes.Length);
                var prototypeWithRenderers = new List<bool>(treePrototypes.Length);

                for (var protoIndex = 0; protoIndex < treePrototypes.Length; protoIndex++)
                {
                    var treePrototype = treePrototypes[protoIndex];
                    var protoPrefab = treePrototype.prefab;
                    var prototypeGo = new GameObject(string.Empty)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        layer = PhysicsLayer
                    };

                    var prototypeTr = prototypeGo.transform;
                    prototypes.Add(prototypeTr);

                    Utility.CopyTransform(protoPrefab.transform, prototypeTr, CopyScaleMode.LocalToLocal);
                    protoRenderers.Clear();
                    var hasLods = GetRenderersFromTreePrefab(protoPrefab.transform, protoRenderers);
                    prototypeRotatable.Add(hasLods);

                    if (!hasLods) 
                    {
                        // Tree with single mesh
                        var mf = protoPrefab.GetComponent<MeshFilter>();
                        if (mf == null || mf.sharedMesh == null)
                        {
                            prototypeWithRenderers.Add(false);
                            continue;
                        }

                        var mc = prototypeGo.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        terrainCollider.sharedMaterial = DefaultMaterial;
                        prototypeWithRenderers.Add(true);
                        continue;
                    }

                    var hasRenderers = false;
                    for (var i = 0; i < protoRenderers.Count; i++)
                    {
                        var renderer = protoRenderers[i];
                        if (!filter.ShouldIncludeObject(renderer))
                            continue;

                        hasRenderers = true;
                        var mf = renderer.GetComponent<MeshFilter>();
                        if (mf == null || mf.sharedMesh == null)
                            continue;

                        var treeChild = new GameObject(string.Empty)
                        {
                            layer = PhysicsLayer
                        };
                        var treeChildTr = treeChild.transform;
                        Utility.CopyTransform(renderer.transform, treeChildTr, CopyScaleMode.LossyToLocal);
                        treeChildTr.parent = prototypeTr;

                        var mc = treeChild.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        terrainCollider.sharedMaterial = DefaultMaterial;
                    }

                    prototypeWithRenderers.Add(hasRenderers);
                }


                // Spawn Instances
                var terrainSize = data.size;
                var terrainPos = terrainCollider.transform.position;

                for (var i = 0; i < trees.Length; i++)
                {
                    var treeInstance = trees[i];
                    var protoIdx = treeInstance.prototypeIndex;

                    if (!prototypeWithRenderers[protoIdx])
                        continue;

                    var p = treeInstance.position;
                    p.Scale(terrainSize);
                    var treeWorldPosition = p + terrainPos;

                    if (!worldBounds.Contains(treeWorldPosition))
                        continue;

                    var proto = prototypes[protoIdx];
                    var isRotatable = prototypeRotatable[protoIdx];

                    var treeTr = Object.Instantiate(proto, treeWorldPosition, isRotatable ? Quaternion.Euler(0f, treeInstance.rotation * Mathf.Rad2Deg, 0f) : Quaternion.identity);
                    SceneManager.MoveGameObjectToScene(treeTr.gameObject, Scene);

                    var scale = new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale);
                    scale.Scale(proto.localScale);
                    treeTr.localScale = scale;
                }

                // Destroy prototypes
                foreach (var simulationPrototype in prototypes)
                {
                    Object.DestroyImmediate(simulationPrototype.gameObject);
                }
            }
        }


        /// <returns>Presence of LodGroup</returns>
        private bool GetRenderersFromTreePrefab(Transform root, List<Renderer> outputs)
        {
            var lodGroups = root.GetComponentsInChildren<LODGroup>();
            if (lodGroups.Length == 0)
            {
                outputs.AddRange(root.GetComponentsInChildren<Renderer>());

                return false;
            }

            foreach (var lodGroup in lodGroups)
            {
                if (lodGroup.lodCount == 0) continue;

                var lod0 = lodGroup.GetLODs()[0];
                outputs.AddRange(lod0.renderers);
            }

            return true;
        }
        

        public void Destroy()
        {
            if(!Scene.isLoaded)
                return;
           
            // Restore Global Physics setting
            UnityEngine.Physics.queriesHitBackfaces = queryBackfaces;
            
            EditorSceneManager.ClosePreviewScene(Scene);
        }
        
        public bool Raycast(Ray ray, out RaycastHit sceneHit, System.Predicate<RaycastHit> shouldExcludeHit = null, float maxDistance = float.PositiveInfinity)
        {
            sceneHit = default;
            var hitCount = Physics.Raycast(ray.origin, ray.direction, sceneHits, maxDistance);
            if (hitCount == 0)
            {
                return false;
            }
            var minHitIdx = -1;
            var minDist = float.MaxValue;
            for (int i = 0; i < hitCount; i++)
            {
                var h = sceneHits[i];
                
                if (shouldExcludeHit != null && shouldExcludeHit.Invoke(h))
                {
                    continue;
                }

                if (minDist < h.distance) continue;
                
                minDist = h.distance;
                minHitIdx = i;
            }
            if (minHitIdx == -1)
            {
                return false;
            }
            sceneHit = sceneHits[minHitIdx];
            if (Vector3.Dot(sceneHit.normal, ray.direction) > 0)
            {
                sceneHit.normal *= -1;
            }
            return true;
        }

        public Transform CreateEmptyRigidbody()
        {
            var gameObject = new GameObject(string.Empty)
            {
                layer = PhysicsLayer
            };
            SceneManager.MoveGameObjectToScene(gameObject, Scene);
            var rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            return gameObject.transform;
        }
    }
}