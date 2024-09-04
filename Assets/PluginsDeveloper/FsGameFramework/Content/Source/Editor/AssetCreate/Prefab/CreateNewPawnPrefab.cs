using UnityEngine;
using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewPawnPrefab
    {
        [MenuItem("Assets/Create/FsGameFramework/CreateNew Pawn Prefab", false, 11)]
        public static void CreatNewPrefab()
        {
            EditorUtility.CreatePrefab(GameobjectProcessor, "NewPawnPrefab");
        }

        static bool GameobjectProcessor(GameObject obj)
        {
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            CapsuleCollider capsuleCollider = obj.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            obj.AddComponent<APawn>();

            return true;
        }
    }
}