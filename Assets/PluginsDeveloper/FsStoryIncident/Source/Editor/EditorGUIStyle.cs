using UnityEditor;
using UnityEngine;

namespace FsStoryIncident
{
    public class GUIStylex
    {
		private static int editorSkinCached = -1;
        public static bool isDarkSkin => editorSkinCached == 1;
        private static GUIStylex style = null;
        public static GUIStylex Get 
        { 
            get
            { 
                if (style == null || editorSkinCached != (EditorGUIUtility.isProSkin ? 1 : 2))
                {
                    editorSkinCached = EditorGUIUtility.isProSkin ? 1 : 2;
                    style = new GUIStylex();
                }

                return style; 
            } 
        }

        /// <summary>
        /// 风格-菜单按钮
        /// </summary>
        public GUIStyle MenuButtonStyle_Normal { get; private set; }
        
        /// <summary>
        /// 风格-菜单按钮选中
        /// </summary>
        public GUIStyle MenuButtonStyle_Selected { get; private set; }

        /// <summary>
        /// 标题风格1级
        /// </summary>
        public GUIStyle TitleStyle_1 { get; private set; }

        /// <summary>
        /// 标题风格2级
        /// </summary>
        public GUIStyle TitleStyle_2 { get; private set; }

        /// <summary>
        /// 标题风格3级
        /// </summary>
        public GUIStyle TitleStyle_3 { get; private set; }

        /// <summary>
        /// 标题风格4级
        /// </summary>
        public GUIStyle TitleStyle_4 { get; private set; }

        /// <summary>
        /// 区域风格1级
        /// </summary>
        public GUIStyle AreaStyle_1 { get; private set; }

        /// <summary>
        /// 区域风格2级
        /// </summary>
        public GUIStyle AreaStyle_2 { get; private set; }

        /// <summary>
        /// 标签风格1级
        /// </summary>
        public GUIStyle LabelStyle_1 { get; private set; }

        /// <summary>
        /// 标签风格2级
        /// </summary>
        public GUIStyle LabelStyle_2 { get; private set; }

        /// <summary>
        /// 删除按钮_方盒型
        /// </summary>
        public GUIStyle BtnStyle_DeleteBox { get; private set; }

        /// <summary>
        /// 信息按钮_方盒型
        /// </summary>
        public GUIStyle BtnStyle_InfoBox { get; private set; }

        /// <summary>
        /// 分隔图形
        /// </summary>
        public GUIStyle Separator { get; private set; }

        //GUI皮肤资源文件
        public GUISkin Skin { get; private set; }

		public GUIStylex()
        {
            Color colorDark = new(0.1f, 0.1f, 0.1f);
            Color colorLightGray = new(0.8784f, 0.8784f, 0.8784f);

            //加载GUISkin
            Skin = AssetDatabase.LoadAssetAtPath(StoryIncidentEditorLibrary.GetAssetsPathBySelfFolder(isDarkSkin ? "EditorSkinDark" : "EditorSkinLight", false), typeof(GUISkin)) as GUISkin;
            if (Skin == null) return;

            //菜单按钮
            MenuButtonStyle_Normal = new GUIStyle(EditorStyles.toolbarButton);
            MenuButtonStyle_Normal.fontStyle = FontStyle.Normal;
            MenuButtonStyle_Normal.fontSize = 14;
            MenuButtonStyle_Normal.fixedHeight = 24;
            //菜单按钮选中
            MenuButtonStyle_Selected = new GUIStyle(MenuButtonStyle_Normal);
            MenuButtonStyle_Selected.fontStyle = FontStyle.Bold;

            //一级标题风格
            TitleStyle_1 = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            //二级标题风格
            TitleStyle_2 = new GUIStyle
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            //三级标题风格
            TitleStyle_3 = new GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            //四级标题风格
            TitleStyle_4 = new GUIStyle
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = isDarkSkin ? colorLightGray : colorDark
                }
            };

            AreaStyle_1 = Skin.FindStyle("Box1_1");
            AreaStyle_2 = Skin.FindStyle("Box1_2");
            LabelStyle_1 = Skin.FindStyle("Label1");
            LabelStyle_2 = Skin.FindStyle("Label2");
            BtnStyle_DeleteBox = Skin.FindStyle("Btn_DeleteBox");
            BtnStyle_InfoBox = Skin.FindStyle("Btn_InfoBox");
            Separator = Skin.FindStyle("BoxSeparator");
        }
	}
}