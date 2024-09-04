using UnityEditor;

namespace FsStoryIncident
{
    public class UnityEditorAction
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            EditorApplication.wantsToQuit += Quit;

            StoryIncidentEditorConfig.InitializeOnLoadMethod();
        }

        static bool Quit()
        {
            EditorApplication.wantsToQuit -= Quit;

            StoryIncidentEditorConfig.Quit();

            return true;
        }
    }
}