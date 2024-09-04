using UnityEngine;
using UnityEditor;

namespace FsGameFramework
{
    /// <summary>
    /// 在场景中创建一个PlayerStart的GameObject
    /// </summary>
    public class CreateNewPlayerStart
    {
        [MenuItem(itemName: "GameObject/FGameFramework/PlayerStart", isValidateFunction: false, priority: 1)]
        public static void CreateScriptFromTemplate()
        {
            //新建GameObject
            var obj = new GameObject();
            obj.AddComponent<FPlayerStart>();
            obj.name = "New PlayerStart";

            //如果当前选择了一个GameObject 设置位置到当前选中GameObject下
            if(UnityEditor.Selection.activeTransform != null)
            {
                obj.transform.SetParent(UnityEditor.Selection.activeTransform);
            }
            //否则 设置位置到当前Scene窗口的根节点下 (包括Prefab的预览场景)
            else
            {
                var currentStage = UnityEditor.SceneManagement.StageUtility.GetCurrentStage();//获取当前的窗口包括Prefab的预览窗口

                //如果是预制体预览Scene 设置到根节点下
                if (currentStage is UnityEditor.SceneManagement.PreviewSceneStage)
                {
                    obj.transform.SetParent((currentStage as UnityEditor.SceneManagement.PreviewSceneStage).scene.GetRootGameObjects()[0].transform);
                }
                //如果是主场景 不做处理 默认节点就是主场景的根节点
                //else if(currentStage is UnityEditor.SceneManagement.MainStage){}
            }

            //设置当前选中GameObject为新建的obj
            UnityEditor.Selection.activeGameObject = obj;
        }
    }
}