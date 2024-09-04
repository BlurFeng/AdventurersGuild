using UnityEditor;

namespace FsGameFramework
{
    public class CreateNewUActorComponentClassFromCustomTemplate
    {
        private const string scriptTemplatePath = "Assets/PluginsDeveloper/FsGameFramework/Content/Sources/Template/UActorComponentScript.cs.txt";

        [MenuItem(itemName: "Assets/Create/FsGameFramework/Create New ActorComponent Script", isValidateFunction: false, priority: 0)]
        public static void CreateScriptFromTemplate()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(scriptTemplatePath, "NewActorComponent.cs");
        }
    }
}