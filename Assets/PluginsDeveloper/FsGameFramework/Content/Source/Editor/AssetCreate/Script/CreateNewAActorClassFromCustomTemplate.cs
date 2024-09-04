using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewAActorClassFromCustomTemplate
    {
        private const string scriptTemplatePath = "Assets/PluginsDeveloper/FsGameFramework/Content/Sources/Template/AActorScript.cs.txt";

        [MenuItem(itemName: "Assets/Create/FsGameFramework/Create New Actor Script", isValidateFunction: false, priority: 0)]
        public static void CreateScriptFromTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(scriptTemplatePath, "NewActor.cs");
        }
    }
}