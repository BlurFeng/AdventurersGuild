using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewAPlayerControllerClassFromCustomTemplate
    {
        private const string scriptTemplatePath = "Assets/PluginsDeveloper/FsGameFramework/Content/Sources/Template/APlayerControllerScript.cs.txt";

        [MenuItem(itemName: "Assets/Create/FsGameFramework/Create New PlayerController Script", isValidateFunction: false, priority: 0)]
        public static void CreateScriptFromTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(scriptTemplatePath, "NewPlayerController.cs");
        }
    }
}