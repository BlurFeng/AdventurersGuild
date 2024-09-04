using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewAPawnClassFromCustomTemplate
    {
        private const string scriptTemplatePath = "Assets/PluginsDeveloper/FsGameFramework/Content/Sources/Template/APawnScript.cs.txt";

        [MenuItem(itemName: "Assets/Create/FsGameFramework/Create New Pawn Script", isValidateFunction: false, priority: 0)]
        public static void CreateScriptFromTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(scriptTemplatePath, "NewPawn.cs");
        }
    }
}