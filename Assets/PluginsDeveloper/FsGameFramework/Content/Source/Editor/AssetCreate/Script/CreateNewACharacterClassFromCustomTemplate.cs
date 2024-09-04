using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewACharacterClassFromCustomTemplate
    {
        private const string scriptTemplatePath = "Assets/PluginsDeveloper/FsGameFramework/Content/Sources/Template/ACharacterScript.cs.txt";

        [MenuItem(itemName: "Assets/Create/FsGameFramework/Create New Character Script", isValidateFunction: false, priority: 0)]
        public static void CreateScriptFromTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(scriptTemplatePath, "NewCharacter.cs");
        }
    }
}