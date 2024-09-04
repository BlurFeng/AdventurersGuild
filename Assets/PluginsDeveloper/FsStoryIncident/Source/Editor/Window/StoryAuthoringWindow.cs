using UnityEngine;
using UnityEditor;

namespace FsStoryIncident
{
    /// <summary>
    /// 故事编写工具窗口
    /// 用于生成和编辑故事配置资源文件
    /// </summary>
    public class StoryAuthoringWindow : EditorWindow
    {
        public enum TopMenu
        {
            /// <summary>
            /// 故事集合
            /// </summary>
            StoryGather,

            /// <summary>
            /// 事件包集合
            /// </summary>
            IncidentPackGather,

            /// <summary>
            /// 存档数据操作
            /// </summary>
            SaveDataModification,

            /// <summary>
            /// 其他
            /// </summary>
            Other,

            Count,
        }

        /// <summary>
        /// 创建的窗口自身
        /// </summary>
        public static EditorWindow window;

        TopMenu topMenuCur;

        [MenuItem("Tools/FsStoryIncident/StoryAuthoringWindow")]
        public static void ShowWindow()
        {
            window = EditorWindow.GetWindow(typeof(StoryAuthoringWindow));
            Vector2 size = new Vector2(500f, 600f);
            window.minSize = size;
            window.titleContent.text = "Story Authoring";
        }

        private void OnEnable()
        {
            OnEnableStoryGather();
            OnEnableIncidentPackGather();
        }

        private void OnDisable()
        {
            OnDisableStoryGather();
            OnDisableIncidentPackGather();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < (int)TopMenu.Count; i++)
            {
                TopMenu menu = (TopMenu)i;
                UnityEngine.GUIStyle topMenuStyle = i == 0 ? EditorStyles.miniButtonLeft : (i == ((int)TopMenu.Count - 1) ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid);
                if (GUILayout.Toggle(topMenuCur == menu, new GUIContent(menu.ToString()), topMenuStyle))
                {
                    topMenuCur = menu;

                    switch (topMenuCur)
                    {
                        case TopMenu.StoryGather:
                            break;
                        case TopMenu.IncidentPackGather:
                            OnSwitchIncidentPackGather();
                            break;
                        case TopMenu.SaveDataModification:
                            break;
                        case TopMenu.Other:
                            break;
                        default:
                            break;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            switch (topMenuCur)
            {
                case TopMenu.StoryGather:
                    DrawStoryGather();
                    break;
                case TopMenu.IncidentPackGather:
                    DrawIncidentPackGather();
                    break;
                case TopMenu.SaveDataModification:
                    break;
                case TopMenu.Other:
                    DrawOther();
                    break;
                default:
                    break;
            }

            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        #region StoryGather

        private StoryConfigGatherGUI storyConfigGatherGUI;
        private StoryGatherConfig storyGatherConfigOld;

        private void OnEnableStoryGather()
        {
            storyConfigGatherGUI = new StoryConfigGatherGUI();

            storyGatherConfigOld = null;
            storyConfigGatherGUI.SetInfo<StoryConfigGUI>(null, this, StoryIncidentEditorStaticData.StoryGatherConfig);

            storyConfigGatherGUI.OnEnable();
        }

        private void OnDisableStoryGather()
        {
            storyConfigGatherGUI.OnDisable();

            StoryGatherAssetDataSave(StoryIncidentEditorStaticData.StoryGatherConfig);
        }

        public void StoryGatherAssetDataSave(StoryGatherConfig storyGatherConfig)
        {
            if (storyGatherConfig == null) return;

            //保存一次配置文件，和用户修改内容的保存确认不同，主要保存一些隐藏的字段
            //比如GUI的开关状态等

            //故事集合配置文件
            EditorUtility.SetDirty(storyGatherConfig);
            AssetDatabase.SaveAssetIfDirty(storyGatherConfig);
        }

        private void DrawStoryGather()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("故事集合编辑", GUIStylex.Get.TitleStyle_1);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            StoryIncidentEditorStaticData.StoryGatherConfig = EditorGUILayout.ObjectField( 
               new GUIContent("故事集合配置文件", "故事集合配置文件，集合中包含多个故事配置。"),
               StoryIncidentEditorStaticData.StoryGatherConfig, typeof(StoryGatherConfig), false, GUILayout.MinWidth(450f)) as StoryGatherConfig;

            EditorGUI.BeginDisabledGroup(StoryIncidentEditorStaticData.StoryGatherConfig == null);
            if (GUILayoutx.ButtonRed("删除故事集合", width: 100))
            {
                if (EditorUtility.DisplayDialog("删除故事集合", "确认要删除故事集合配置资源？", "删除", "取消"))
                {
                    AssetDatabase.DeleteAsset(storyConfigGatherGUI.path);
                    AssetDatabase.Refresh();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            //保存一下之前选中的配置文件，主要是为了保存FadeAreaOpenCached等隐藏的自动修改的成员数据
            if (storyGatherConfigOld != StoryIncidentEditorStaticData.StoryGatherConfig)
            {
                storyConfigGatherGUI.SaveCheck();//确认是否需要保存修改
                StoryGatherAssetDataSave(storyGatherConfigOld);
                storyGatherConfigOld = StoryIncidentEditorStaticData.StoryGatherConfig;
                
                storyConfigGatherGUI.SetInfo<StoryConfigGUI>(null, this, StoryIncidentEditorStaticData.StoryGatherConfig);
            }

            if (StoryIncidentEditorStaticData.StoryGatherConfig == null) return;

            storyConfigGatherGUI.Draw(0);
        }
        #endregion

        #region IncidentPackGather

        private IncidentPackGatherGUI incidentPackGatherGUI;
        private IncidentPackGatherConfig incidentPackGatherConfigOld;

        private void OnSwitchIncidentPackGather()
        {
            //更新所有事件列表数据
            IncidentPackGatherGUI.RefreshAllIncidentHeaders();
        }

        private void OnEnableIncidentPackGather()
        {
            incidentPackGatherGUI = new IncidentPackGatherGUI();

            incidentPackGatherConfigOld = null;

            incidentPackGatherGUI.SetInfo<IncidentPackGatherGUI>(null, this, StoryIncidentEditorStaticData.IncidentPackGatherConfig);

            incidentPackGatherGUI.OnEnable();
        }

        private void OnDisableIncidentPackGather()
        {
            incidentPackGatherGUI.OnDisable();

            IncidentPackGatherAssetDataSave(StoryIncidentEditorStaticData.IncidentPackGatherConfig);
        }

        public void IncidentPackGatherAssetDataSave(IncidentPackGatherConfig incidentPackGatherConfig)
        {
            if (incidentPackGatherConfig == null) return;

            //保存一次配置文件，和用户修改内容的保存确认不同，主要保存一些隐藏的字段
            //比如GUI的开关状态等

            //故事集合配置文件
            EditorUtility.SetDirty(incidentPackGatherConfig);
            AssetDatabase.SaveAssetIfDirty(incidentPackGatherConfig);
        }

        private void DrawIncidentPackGather()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("事件包集合编辑", GUIStylex.Get.TitleStyle_1);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            StoryIncidentEditorStaticData.IncidentPackGatherConfig = EditorGUILayout.ObjectField(
               new GUIContent("事件包集合配置文件", "事件包集合配置文件中包含多个事件包配置。事件包是事件信息的集合，可以用于随机发生事件等操作。"),
               StoryIncidentEditorStaticData.IncidentPackGatherConfig, typeof(IncidentPackGatherConfig), false, GUILayout.MinWidth(450f)) as IncidentPackGatherConfig;

            EditorGUI.BeginDisabledGroup(StoryIncidentEditorStaticData.IncidentPackGatherConfig == null);
            if (GUILayoutx.ButtonRed("删除事件包集合", width: 100))
            {
                if (EditorUtility.DisplayDialog("删除事件包集合", "确认要删除事件包集合配置资源？", "删除", "取消"))
                {
                    AssetDatabase.DeleteAsset(storyConfigGatherGUI.path);
                    AssetDatabase.Refresh();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            //保存一下之前选中的配置文件，主要是为了保存FadeAreaOpenCached等隐藏的自动修改的成员数据
            if (incidentPackGatherConfigOld != StoryIncidentEditorStaticData.IncidentPackGatherConfig)
            {
                incidentPackGatherGUI.SaveCheck();//确认是否需要保存修改
                IncidentPackGatherAssetDataSave(incidentPackGatherConfigOld);
                incidentPackGatherConfigOld = StoryIncidentEditorStaticData.IncidentPackGatherConfig;

                incidentPackGatherGUI.SetInfo<IncidentPackGatherGUI>(null, this, StoryIncidentEditorStaticData.IncidentPackGatherConfig);
            }

            if (StoryIncidentEditorStaticData.IncidentPackGatherConfig == null) return;

            incidentPackGatherGUI.Draw(0);
        }

        #endregion

        #region Other

        public void DrawOther()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("其他", GUIStylex.Get.TitleStyle_1);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("编辑工具配置", GUIStylex.Get.TitleStyle_2);
            StoryIncidentEditorStaticData.AutoSave = EditorGUILayout.Toggle(new GUIContent("自动保存", ""), StoryIncidentEditorStaticData.AutoSave);
            EditorGUILayout.Space();

            //EditorGUILayout.LabelField("配置文件操作：", GUIStylex.Get.TitleStyle_2);
        }

        #endregion
    }
}