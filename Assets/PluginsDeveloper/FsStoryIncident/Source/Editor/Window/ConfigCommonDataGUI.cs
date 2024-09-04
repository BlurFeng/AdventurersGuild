using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FsStoryIncident
{
    public class ConfigCommonDataGUI
    {
        private readonly ReorderableList reorderableList;
        private readonly ConfigCommonData config;

        public ConfigCommonDataGUI(ConfigCommonData config)
        {
            this.config = config;
            reorderableList = new ReorderableList(config.tags, typeof(string), true, true, true, true)
            {
                drawHeaderCallback = DrawTagsHeader,
                drawElementCallback = DrawTagsElement,
                elementHeight = 20f,
            };
        }

        /// <summary>
        /// 子类调用，绘制配置通常数据
        /// </summary>
        /// <param name="configCommonData"></param>
        public bool Draw()
        {
            GUILayoutx.TextFieldDisplayOnly("Guid:", ConfigConstData.guid_Tooltip, config.GuidStr);
            config.foldout = GUILayoutx.Foldout("CommonData", ConfigConstData.configCommonData_Tooltip, config.foldout);
            if (config.foldout)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;

                config.name = GUILayoutx.TextField("Name", ConfigConstData.name_Tooltip, config.name);
                config.describe = GUILayoutx.TextField("Describe", ConfigConstData.describe_Tooltip, config.describe);
                config.commentName = GUILayoutx.TextField("CommentName", ConfigConstData.commentName_Tooltip, config.commentName);
                config.comment = GUILayoutx.TextField("Comment", ConfigConstData.comment_Tooltip, config.comment);
                reorderableList.DoLayoutList();

                EditorGUI.indentLevel--;
                return EditorGUI.EndChangeCheck();
            }
            return false;
        }

        private void DrawTagsHeader(Rect rect)
        {
            var titleRect = rect;
            GUI.Label(titleRect, new GUIContent("Tags", ConfigConstData.tags_Tooltip));
        }

        private void DrawTagsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= config.tags.Count) return;
            config.tags[index] = EditorGUI.DelayedTextField(rect, config.tags[index]);
        }
    }
}