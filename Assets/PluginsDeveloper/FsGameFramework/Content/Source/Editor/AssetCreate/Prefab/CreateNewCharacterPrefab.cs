using UnityEngine;
using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewCharacterPrefab
    {
        [MenuItem("Assets/Create/FsGameFramework/CreateNew Character Prefab", false, 11)]
        public static void CreatNewPrefab()
        {
            EditorUtility.CreatePrefab(GameobjectProcessor, "NewCharacterPrefab");
        }

        static bool GameobjectProcessor(GameObject obj)
        {
            //obj.AddComponent<MeshFilter>();
            //obj.AddComponent<MeshRenderer>();
            CapsuleCollider capsuleCollider = obj.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            obj.AddComponent<ACharacter>();

            return true;
        }
    }
}