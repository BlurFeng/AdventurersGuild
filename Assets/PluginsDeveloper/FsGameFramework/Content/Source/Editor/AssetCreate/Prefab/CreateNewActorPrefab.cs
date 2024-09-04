using UnityEngine;
using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewActorPrefab
    {
        [MenuItem("Assets/Create/FsGameFramework/CreateNew Actor Prefab", false, 11)]
        public static void CreatNewPrefab()
        {
            EditorUtility.CreatePrefab(GameobjectProcessor, "NewActorPrefab");
        }

        static bool GameobjectProcessor(GameObject obj)
        {
            obj.AddComponent<AActor>();

            return true;
        }
    }
}