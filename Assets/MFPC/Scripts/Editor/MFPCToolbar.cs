using UnityEditor;
using UnityEngine;
using System.IO;
using MFPC.Input.PlayerInput;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

#endif

namespace MFPC.EditorScripts
{
    public class MFPCToolbar : EditorWindow
    {
        private const string advancedCharacterControllerPath = "/Prefabs/AdvancedControl.prefab";
        private const string simpleCharacterControllerPath = "/Prefabs/SimpleControl.prefab";

        private GameObject selectedCharacter;

        [MenuItem("Tools/MFPC/Create Character Controller (Advanced)")]
        public static void CreateAdvancedCharacterController()
        {
            InstantiateCharacterController(FindFolderPathContaining("MFPC", advancedCharacterControllerPath));
        }

        [MenuItem("Tools/MFPC/Create Character Controller (Simple)")]
        public static void CreateSimpleCharacterController()
        {
            InstantiateCharacterController(FindFolderPathContaining("MFPC", simpleCharacterControllerPath));
        }

        [MenuItem("Tools/MFPC/Refresh Character Controller Input")]
        public static void RefreshCharacterControllerInput()
        {
            GetWindow(typeof(MFPCToolbar));
        }

        private void OnEnable()
        {
            selectedCharacter = GameObject.FindObjectOfType<Player>().gameObject;
        }

        private void OnGUI()
        {
            GUILayout.Label("Select character object:", EditorStyles.boldLabel);

            selectedCharacter = EditorGUILayout.ObjectField(selectedCharacter, typeof(GameObject), true) as GameObject;

            if (GUILayout.Button("Refresh Input"))
            {
                if (selectedCharacter != null)
                {
                    UpdateNewPlayerInputHandler(selectedCharacter);
                    UpdateInputModule();
                }
                else Debug.LogError("Character not selected!");
            }
        }

        private static void InstantiateCharacterController(string prefabPath)
        {
            if (GameObject.FindObjectOfType<Player>() != null)
            {
                Debug.LogWarning("There cannot be more than one character on scene");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                GameObject instantiatedObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instantiatedObject != null)
                {
                    Selection.activeGameObject = instantiatedObject;
                    GameObject player = GameObject.FindObjectOfType<Player>().gameObject;
                    
                    UpdateNewPlayerInputHandler(player);
                    UpdateInputModule();
                }
            }
            else
                Debug.LogError(
                    "Player prefab not found. Make sure it's in the Resources folder and that it's in the correct path.");
        }
        
        private static void UpdateNewPlayerInputHandler(GameObject player)
        {
            if (player.TryGetComponent(out NewPlayerInputHandler newPlayerInputHandler))
                DestroyImmediate(newPlayerInputHandler);
            
#if ENABLE_INPUT_SYSTEM
            player.AddComponent<NewPlayerInputHandler>();
#endif
        }

        private static string FindFolderPathContaining(string folderNameToFind, string pathToTarget)
        {
            DirectoryInfo[] directories =
                new DirectoryInfo(Application.dataPath).GetDirectories("*", SearchOption.AllDirectories);

            foreach (DirectoryInfo dir in directories)
            {
                if (dir.Name == folderNameToFind)
                {
                    return "Assets" + dir.FullName.Substring(Application.dataPath.Length) + pathToTarget;
                }
            }

            return null;
        }

        private static void UpdateInputModule()
        {
            EventSystem eventSystem = FindObjectOfType<EventSystem>();

            if (eventSystem != null) DestroyImmediate(eventSystem.gameObject);

            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemGO.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}