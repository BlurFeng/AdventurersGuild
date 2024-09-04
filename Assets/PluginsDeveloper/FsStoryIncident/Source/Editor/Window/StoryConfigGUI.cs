using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// T为StoryIncidentConfigGUIBase子类，但是是这个GUI对应config.subConfig对应的GUI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StoryIncidentConfigGUIBase
    {
        protected EditorWindow window;
        
        public StoryIncidentConfigGUIBase ParentGUI { get; protected set; }
        public T GetParentGUI<T>() where T : StoryIncidentConfigGUIBase { return ParentGUI as T; }

        protected List<StoryIncidentConfigGUIBase> m_SubStoryConfigGUIs = new List<StoryIncidentConfigGUIBase>();
        public System.Object configData { get; private set; }
        protected StorySubConfigBase config;

        #region Get Paretn Config
        protected StoryConfig StoryConfigParent
        {
            get
            {
                if (storyConfigParent == null) storyConfigParent = FindConfigParent<StoryConfig>(ParentGUI);
                return storyConfigParent;
            }
        }
        private StoryConfig storyConfigParent;

        protected ChapterConfig ChapterConfigParent
        {
            get
            {
                if (chapterConfigParent == null) chapterConfigParent = FindConfigParent<ChapterConfig>(ParentGUI);
                return chapterConfigParent;
            }
        }
        private ChapterConfig chapterConfigParent;

        protected IncidentItemConfig IncidentItemConfigParent
        {
            get
            {
                if (incidentItemConfigParent == null) incidentItemConfigParent = FindConfigParent<IncidentItemConfig>(ParentGUI);
                return incidentItemConfigParent;
            }
        }
        private IncidentItemConfig incidentItemConfigParent;

        protected IncidentNodeConfig IncidentNodeConfigParent
        {
            get
            {
                if (incidentNodeConfigParent == null) incidentNodeConfigParent = FindConfigParent<IncidentNodeConfig>(ParentGUI);
                return incidentNodeConfigParent;
            }
        }
        private IncidentNodeConfig incidentNodeConfigParent;

        private T FindConfigParent<T>(StoryIncidentConfigGUIBase parentGUI) where T : class
        {
            if (parentGUI == null) return null;
            T config;
            if (parentGUI.configData is T) return parentGUI.configData as T;
            else config = FindConfigParent<T>(parentGUI.ParentGUI);
            return config;
        }
        #endregion

        public void SetInfo<TSubGUI>(StoryIncidentConfigGUIBase parentGUI, EditorWindow window, System.Object configData) where TSubGUI : StoryIncidentConfigGUIBase, new()
        {
            this.configData = configData;
            this.ParentGUI = parentGUI;
            this.window = window;
            this.config = configData as StorySubConfigBase;

            if (configData == null) return;
            SetInfoRewriteBrfore(configData);
            ResetSubConfigGUIs();
            if (config == null) return;
            InitDrawConfigCommonData(config.configCommonData);
        }

        //初始化
        protected virtual void SetInfoRewriteBrfore(System.Object configData) { }

        //当GUI自身被销毁时，这等同于对应的Config数据被销毁
        protected virtual void OnDestroySelf() { }

        //重置子GUI方法，子类需要调用在其中ResetSubConfigGUIs<SubGUI>()方法，告知具体的SubGUI类型
        protected virtual void ResetSubConfigGUIs() { }

        public virtual void OnEnable()
        {
            for (int i = 0; i < m_SubStoryConfigGUIs.Count; i++)
            {
                m_SubStoryConfigGUIs[i].OnEnable();
            }
        }

        //当窗口关闭时
        public virtual void OnDisable()
        {
            for (int i = 0; i < m_SubStoryConfigGUIs.Count; i++)
            {
                m_SubStoryConfigGUIs[i].OnDisable();
            }
        }

        //获取自身FadeArea组件的风格
        protected virtual bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = GUIStylex.Get.AreaStyle_1;
            labelStyle = GUIStylex.Get.LabelStyle_1;
            return true;
        }

        //过去FadeArea默认的开关状态，如果缓存了FadeArea的开关状态，可使用缓存值
        protected virtual bool GetFadeAreaOpenDefault() { return true; }

        //获取子Config数据列表
        protected virtual System.Object[] GetSubConfigDatas() { return null; }

        //设置子Config数据列表
        protected virtual void SetSubConfigDatas(System.Object[] configDatas) { }

        //绘制GUI
        public virtual void Draw(int index) { }

        //重新设置自GUI列表
        public void ResetSubConfigGUIs<TSubGUI>() where TSubGUI : StoryIncidentConfigGUIBase, new()
        {
            m_SubStoryConfigGUIs.Clear();
            System.Object[] configDatas = GetSubConfigDatas();
            if (configDatas == null) return;

            for (int i = 0; i < configDatas.Length; i++)
            {
                var config = configDatas[i];
                CreateSubConfigGUIs<TSubGUI>(config, i);
            }
        }

        //创建一个自GUI类
        protected void CreateSubConfigGUIs<TSubGUI>(System.Object configData, int index) where TSubGUI : StoryIncidentConfigGUIBase, new()
        {
            TSubGUI gui = new TSubGUI();
            gui.SetInfo<TSubGUI>(this, window, configData);
            m_SubStoryConfigGUIs.Add(gui);
        }

        //销毁一个自GUI类
        protected void DestroySubConfigGUI(int index)
        {
            m_SubStoryConfigGUIs.RemoveAt(index);
        }

        //由父类GUI点击新增子类config按钮时，调用创建一个子类配置
        protected virtual void CreateSubConfig<TSubConfig, TSubGUI>() where TSubConfig : IConfigCommonData, new() where TSubGUI : StoryIncidentConfigGUIBase, new()
        {
            System.Object[] configDatas = GetSubConfigDatas();
            if (configDatas == null) return;

            List<System.Object> list = new List<System.Object>(configDatas);
            TSubConfig config = new TSubConfig();

            list.Add(config); var array = list.ToArray();
            SetSubConfigDatas(array);
            OnCreateSubConfig(ref array[array.Length - 1]);

            //CheckSubConfigGUIs<TSubGUI>(GetSubConfigDatas());
            CreateSubConfigGUIs<TSubGUI>(config, list.Count - 1);
        }

        protected virtual void OnCreateSubConfig<TSubConfig>(ref TSubConfig subConfig) where TSubConfig : new() 
        {
            if (this.config == null) return;
            //StorySubConfigBase subConfigTemp = subConfig as StorySubConfigBase;
        }

        //由子类GUI上点击删除按钮时，通知父类从子配置中移除自身
        public virtual void DestroySubConfig<TSubGUI>(int index) where TSubGUI : StoryIncidentConfigGUIBase, new()
        {
            //删除需要Index告知删除哪个项目，实际上在for中直接操作了List会导致这一帧数的循环和index产生问题
            //但是会直接刷新到下一帧的Draw，所以不用额外记录要移除的Index在for后统一删除了

            System.Object[] configDatas = GetSubConfigDatas();
            if (configDatas == null) return;

            List<System.Object> list = new List<System.Object>(configDatas);
            var subConfig = list[index];
            list.RemoveAt(index);
            SetSubConfigDatas(list.ToArray());
            OnDestroySubConfig(subConfig);

            //CheckSubConfigGUIs<TSubGUI>(GetSubConfigDatas());
            DestroySubConfigGUI(index);
        }

        protected virtual void OnDestroySubConfig<TSubConfig>(TSubConfig subConfig) where TSubConfig : new() { }

        //数据数组转换方法，子类在重写GetSubConfigDatas后调用
        protected T[] ConvertConfigDatas<T>(object[] configDatas)
        {
            T[] toArray = new T[configDatas.Length];
            for (int i = 0; i < configDatas.Length; i++)
            {
                toArray[i] = (T)configDatas[i];
            }

            return toArray;
        }

        /// <summary>
        /// 绘制子配置GUI
        /// 具体绘制内容由子配置的GUI类Draw方法实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        protected void DrawSubGUIs<T>(T[] datas)
        {
            if (datas == null || datas.Length == 0) return;
            for (int i = 0; i < datas.Length; i++)
            {
                if (i >= m_SubStoryConfigGUIs.Count) break;
                m_SubStoryConfigGUIs[i].Draw(i);
            }
        }

        /// <summary>
        /// 绘制创建子配置GUI
        /// </summary>
        /// <typeparam name="TSubConfig"></typeparam>
        /// <typeparam name="TSubGUI"></typeparam>
        /// <param name="subName"></param>
        protected void DrawCreateSubConfig<TSubConfig, TSubGUI>(string subName) where TSubConfig : IConfigCommonData, new() where TSubGUI : StoryIncidentConfigGUIBase, new()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{subName}列表", GUIStylex.Get.TitleStyle_3);
            if (GUILayout.Button($"创建{subName}"))
            {
                CreateSubConfig<TSubConfig, TSubGUI>();
            }
            EditorGUILayout.EndHorizontal();
        }

        #region FadeArea Draw 绘制折叠空间
        private FadeArea fadeArea;
        private FadeArea FadeArea
        {
            get
            {
                if (fadeArea == null)
                {
                    if (GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle))
                        fadeArea = new FadeArea(window, GetFadeAreaOpenDefault(), areaStyle, labelStyle);
                }

                return fadeArea;
            }
        }

        /// <summary>
        /// 在折叠空间中绘制
        /// </summary>
        /// <param name="index"></param>
        /// <param name="header"></param>
        /// <param name="deleteItem"></param>
        /// <param name="open"></param>
        /// <param name="draw"></param>
        protected void FadeAreaDraw(int index, string header, string deleteItem, ref bool open, Action draw, Action headerDraw = null)
        {
            if (configData != null && configData is IConfigCommonData data)
            {
                header = $"{header} {(string.IsNullOrEmpty(data.NameEditorShow()) ? data.Guid() : data.NameEditorShow())}";
            }

            FadeArea.FadeAreaDraw(header, ref open, draw, () =>
            {
                if (headerDraw != null) headerDraw.Invoke();

                //删除事件
                if (GUILayout.Button("Delete", GUIStylex.Get.BtnStyle_DeleteBox))
                {
                    if (EditorUtility.DisplayDialog($"删除{deleteItem}", $"确认要删除{deleteItem}吗？", "删除", "取消"))
                    {
                        ParentGUI.DestroySubConfig<IncidentConfigGUI>(index);
                    }
                }
            });
        }
        #endregion

        #region DrawConfigCommonData 绘制通用数据
        ConfigCommonDataGUI configCommonDataGUI;

        protected void InitDrawConfigCommonData(ConfigCommonData config)
        {
            if (config == null) return;
            configCommonDataGUI = new ConfigCommonDataGUI(config);
        }

        /// <summary>
        /// 绘制通用配置UI
        /// </summary>
        protected bool DrawConfigCommonData()
        {
            if (configCommonDataGUI == null) return false;
            return configCommonDataGUI.Draw();
        }
        #endregion
    }

    public class StoryConfigGatherGUI : StoryIncidentConfigGUIBase
    {
        private StoryGatherConfig storyGatherConfig;
        private StoryGatherConfig storyGatherConfigCopy;

        private Vector2 storysScrollViewPos;
        public string path { get; private set; }

        private bool isDirty;//标脏，是否需要保存

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            storyGatherConfig = configData as StoryGatherConfig;
            storyGatherConfigCopy = ScriptableObject.Instantiate(storyGatherConfig);
            path = AssetDatabase.GetAssetPath(storyGatherConfig);

            StoryIncidentEditorStaticData.OnAutoSaveChange += OnAutoSaveChange;

            InitDrawConfigCommonData(storyGatherConfig.configCommonData);
        }

        protected override void ResetSubConfigGUIs()
        {
            ResetSubConfigGUIs<StoryConfigGUI>();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            StoryIncidentEditorStaticData.OnAutoSaveChange -= OnAutoSaveChange;

            SaveCheck();
        }

        protected override bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = null; labelStyle = null;
            return false;
        }

        protected override object[] GetSubConfigDatas()
        {
            return storyGatherConfig.storyConfigs != null ? storyGatherConfig.storyConfigs : new StoryConfig[0];
        }

        protected override void SetSubConfigDatas(object[] configDatas)
        {
            storyGatherConfig.storyConfigs = ConvertConfigDatas<StoryConfig>(configDatas);
        }

        public override void Draw(int index)
        {
            if (storyGatherConfig == null) return;

            base.Draw(index);

            //故事集合信息
            GUILayoutx.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("故事集合配置", GUIStylex.Get.TitleStyle_2);
            EditorGUILayout.LabelField(StoryIncidentEditorStaticData.AutoSave ? "自动保存模式" : "手动保存模式", GUIStylex.Get.TitleStyle_3, GUILayout.Width(80f));
            EditorGUI.BeginDisabledGroup(!isDirty);
            if (GUILayoutx.ButtonGreen("Save", width: 80)) Save();
            if (GUILayoutx.ButtonYellow("Revert", width: 80)) if (EditorUtility.DisplayDialog("回退修改", "确定要回退修改的内容吗？", "确定", "取消")) Revert();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (!StoryIncidentEditorStaticData.AutoSave) EditorGUI.BeginChangeCheck();
            DrawConfigCommonData();
            DrawCreateSubConfig<StoryConfig, StoryConfigGUI>("故事");
            if (!StoryIncidentEditorStaticData.AutoSave && EditorGUI.EndChangeCheck()) isDirty = true;

            storysScrollViewPos = EditorGUILayout.BeginScrollView(storysScrollViewPos);

            if (!StoryIncidentEditorStaticData.AutoSave) EditorGUI.BeginChangeCheck();
            DrawSubGUIs(storyGatherConfig.storyConfigs);
            if (!StoryIncidentEditorStaticData.AutoSave && EditorGUI.EndChangeCheck()) isDirty = true;

            EditorGUILayout.EndScrollView();

            
        }

        private void Save()
        {
            if (!isDirty) return;
            isDirty = false;

            storyGatherConfigCopy = ScriptableObject.Instantiate(storyGatherConfig);

            AssetDatabase.SaveAssets();
        }

        private void Revert()
        {
            if (!isDirty) return;
            isDirty = false;

            storyGatherConfig.Copy(storyGatherConfigCopy);
            storyGatherConfigCopy = ScriptableObject.Instantiate(storyGatherConfig);

            InitDrawConfigCommonData(storyGatherConfig.configCommonData);

            ResetSubConfigGUIs();
        }

        public void SaveCheck()
        {
            if (isDirty)
            {
                if (EditorUtility.DisplayDialog("保存故事集合资源", "故事集合资源修改未保存，是否保存？", "保存", "不保存")) Save();
                else Revert();
            }
        }

        private bool StoryGatherConfigContains(StoryConfig sc)
        {
            for (int i = 0; i < storyGatherConfig.storyConfigs.Length; i++)
            {
                if (storyGatherConfig.storyConfigs[i] == sc)
                    return true;
            }

            return false;
        }

        private bool StoryGatherConfigCopyContains(StoryConfig sc)
        {
            for (int i = 0; i < storyGatherConfigCopy.storyConfigs.Length; i++)
            {
                if (storyGatherConfigCopy.storyConfigs[i] == sc)
                    return true;
            }

            return false;
        }

        private void OnAutoSaveChange(bool autoSave)
        {
            if(autoSave)
            {
                Save();
            }
            else
            {
                //从自动保存转为非自动保存时，更新Copy文件
                storyGatherConfigCopy = ScriptableObject.Instantiate(storyGatherConfig);
            }
        }
    }

    public class StoryConfigGUI : StoryIncidentConfigGUIBase
    {
        public StoryConfig StoryConfig { get; private set; }

        public bool IsDirty { get; private set; }//标脏，是否需要保存

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            StoryConfig = configData as StoryConfig;
            StoryConfig.RefreshChaptersEditorData();

            GUILayoutxCondition.Init(ref StoryConfig.conditionConfig);
            GUILayoutxTask.Init(ref StoryConfig.taskConfig);
        }

        protected override void ResetSubConfigGUIs()
        {
            ResetSubConfigGUIs<ChapterConfigGUI>();
        }

        protected override bool GetFadeAreaOpenDefault()
        {
            return StoryConfig.fadeAreaOpenCached;
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        protected override object[] GetSubConfigDatas()
        {
            return StoryConfig.chapters != null ? StoryConfig.chapters : new ChapterConfig[0];
        }

        protected override void SetSubConfigDatas(object[] configDatas)
        {
            StoryConfig.chapters = ConvertConfigDatas<ChapterConfig>(configDatas);
        }

        protected override void OnCreateSubConfig<TSubConfig>(ref TSubConfig subConfig)
        {
            base.OnCreateSubConfig(ref subConfig);

            StoryConfig.RefreshChaptersEditorData();
        }

        protected override void OnDestroySubConfig<TSubConfig>(TSubConfig subConfig)
        {
            base.OnDestroySubConfig(subConfig);

            StoryConfig.RefreshChaptersEditorData();
        }

        public override void Draw(int index)
        {
            base.Draw(index);

            FadeAreaDraw(index, "Story", "故事", ref StoryConfig.fadeAreaOpenCached, () =>
            {
                if (!StoryIncidentEditorStaticData.AutoSave) EditorGUI.BeginChangeCheck();

                DrawConfigCommonData();

                if (StoryConfig.HaveChapters)
                {
                    StoryConfig.StartChapterIndex = EditorGUILayout.Popup("StartChapter", StoryConfig.StartChapterIndex, StoryConfig.chapterHeaders.ToArray());
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("StartChapter", GUILayout.Width(150f)); EditorGUILayout.LabelField("没有任何章");
                    EditorGUILayout.EndHorizontal();
                }

                GUILayoutxCondition.Draw(ref StoryConfig.conditionConfig);

                GUILayoutxTask.Draw(ref StoryConfig.taskConfig);

                DrawCreateSubConfig<ChapterConfig, ChapterConfigGUI>("章");

                DrawSubGUIs(StoryConfig.chapters);

                if (!StoryIncidentEditorStaticData.AutoSave && EditorGUI.EndChangeCheck()) IsDirty = true;
            });
        }
    }

    public class ChapterConfigGUI : StoryIncidentConfigGUIBase
    {
        ChapterConfig chapterConfig;

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            chapterConfig = configData as ChapterConfig;
            chapterConfig.storyConfig = ParentGUI.configData as StoryConfig;
            chapterConfig.storyConfig.OnRefreshChaptersEditorData += chapterConfig.OnRefreshChaptersEditorData;
            chapterConfig.RefreshIncidentsEditorData();

            GUILayoutxTask.Init(ref chapterConfig.taskConfig);

            if (chapterConfig.linkOtherConfig != null)
            {
                for (int i = 0; i < chapterConfig.linkOtherConfig.linkOtherItems.Count; i++)
                {
                    GUILayoutxCondition.Init(ref chapterConfig.linkOtherConfig.linkOtherItems[i].conditionConfig);
                }
            }
        }

        protected override void OnDestroySelf()
        {
            base.OnDestroySelf();

            chapterConfig.storyConfig.OnRefreshChaptersEditorData -= chapterConfig.OnRefreshChaptersEditorData;
        }

        protected override void ResetSubConfigGUIs()
        {
            ResetSubConfigGUIs<IncidentConfigGUI>();
        }

        protected override bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = GUIStylex.Get.AreaStyle_2;
            labelStyle = GUIStylex.Get.LabelStyle_2;
            return true;
        }

        protected override bool GetFadeAreaOpenDefault()
        {
            return chapterConfig.fadeAreaOpenCached;
        }

        protected override object[] GetSubConfigDatas()
        {
            return chapterConfig.incidents != null ? chapterConfig.incidents : new IncidentConfig[0];
        }

        protected override void SetSubConfigDatas(object[] configDatas)
        {
            chapterConfig.incidents = ConvertConfigDatas<IncidentConfig>(configDatas);
        }

        protected override void OnCreateSubConfig<TSubConfig>(ref TSubConfig subConfig)
        {
            base.OnCreateSubConfig(ref subConfig);
            chapterConfig.RefreshIncidentsEditorData();
        }

        protected override void OnDestroySubConfig<TSubConfig>(TSubConfig subConfig)
        {
            base.OnDestroySubConfig(subConfig);
            chapterConfig.RefreshIncidentsEditorData();
        }

        public override void Draw(int index)
        {
            base.Draw(index);

            FadeAreaDraw(index, "ChapterConfig", "章", ref chapterConfig.fadeAreaOpenCached, () =>
            {
                if (DrawConfigCommonData())
                {
                    StoryConfigParent.RefreshChaptersEditorData();
                }

                GUILayoutxTask.Draw(ref chapterConfig.taskConfig);

                GUILayoutxLinkOther.DrawLinkOtherConfigs(
                    ref chapterConfig.linkOtherConfig, StoryConfigParent.chapterHeaders.ToArray(), chapterConfig.nodeHeaders.ToArray(), chapterConfig.nodes,
                    chapterConfig.storyConfig.GetChaptersGuidByIndex, chapterConfig.GetNodeGuidByIndex);

                chapterConfig.type = GUILayoutx.EnumPopup("Type", ConfigConstData.chapterConfig_type_Tooltip, chapterConfig.type);

                //不同类型章的特定配置
                switch (chapterConfig.type)
                {
                    case ChapterType.Line:
                        EditorGUILayout.LabelField(ConfigConstData.chapterConfig_Tier_Header, GUIStylex.Get.TitleStyle_3);
                        chapterConfig.StartIncidentIndex = EditorGUILayout.Popup("StartIncident", chapterConfig.StartIncidentIndex, chapterConfig.incidentHeaders.ToArray());
                        EditorGUILayout.Space();
                        break;
                    case ChapterType.Tier:
                        EditorGUILayout.LabelField(ConfigConstData.chapterConfig_Tier_Header, GUIStylex.Get.TitleStyle_3);
                        chapterConfig.conditionNum = GUILayoutx.IntField("ConditionNum", ConfigConstData.chapterConfig_ConditionNum_Tooltip, chapterConfig.conditionNum);
                        EditorGUILayout.Space();
                        break;
                    case ChapterType.Prose:
                        break;
                    default:
                        break;
                }

                DrawCreateSubConfig<IncidentConfig, IncidentConfigGUI>("事件");

                DrawSubGUIs(chapterConfig.incidents);
            });
        }
    }

    public class IncidentConfigGUI : StoryIncidentConfigGUIBase
    {
        private IncidentConfig incidentConfig;

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            incidentConfig = configData as IncidentConfig;
            incidentConfig.chapterConfig = ParentGUI.configData as ChapterConfig;
            incidentConfig.chapterConfig.OnRefreshIncidentsEditorData += incidentConfig.OnRefreshIncidentsEditorData;
            GUILayoutxCondition.Init(ref incidentConfig.conditionConfig);
            GUILayoutxTask.Init(ref incidentConfig.taskConfig);

            if (incidentConfig.linkOtherConfig != null)
            {
                for (int i = 0; i < incidentConfig.linkOtherConfig.linkOtherItems.Count; i++)
                {
                    GUILayoutxCondition.Init(ref incidentConfig.linkOtherConfig.linkOtherItems[i].conditionConfig);
                }
            }
        }

        protected override void OnDestroySelf()
        {
            base.OnDestroySelf();

            incidentConfig.chapterConfig.OnRefreshIncidentsEditorData -= incidentConfig.OnRefreshIncidentsEditorData;
        }

        protected override void ResetSubConfigGUIs()
        {
            ResetSubConfigGUIs<IncidentItemConfigGUI>();
        }

        protected override bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = GUIStylex.Get.AreaStyle_2;
            labelStyle = GUIStylex.Get.LabelStyle_2;
            return true;
        }

        protected override bool GetFadeAreaOpenDefault()
        {
            return incidentConfig.fadeAreaOpenCached;
        }

        protected override object[] GetSubConfigDatas()
        {
            return incidentConfig.incidentItems != null ? incidentConfig.incidentItems : new IncidentItemConfig[0];
        }

        protected override void SetSubConfigDatas(object[] configDatas)
        {
            incidentConfig.incidentItems = ConvertConfigDatas<IncidentItemConfig>(configDatas);
        }

        public override void Draw(int index)
        {
            base.Draw(index);

            FadeAreaDraw(index, "Incident", "事件", ref incidentConfig.fadeAreaOpenCached, () =>
            {
                if (DrawConfigCommonData())
                {
                    ChapterConfigParent.RefreshIncidentsEditorData();
                }
                GUILayoutxRandomData.Draw(ref incidentConfig.randomData);
                incidentConfig.repetition = EditorGUILayout.Toggle(new GUIContent("Repetition", ConfigConstData.incidentConfig_repetition_Tooltip), incidentConfig.repetition);
                GUILayoutxCondition.Draw(ref incidentConfig.conditionConfig);

                //线型章所属的事件，才需要编辑链接到的其他事件信息
                if(incidentConfig.chapterConfig.type == ChapterType.Line)
                    GUILayoutxLinkOther.DrawLinkOtherConfigs(
                        ref incidentConfig.linkOtherConfig, ChapterConfigParent.incidentHeaders.ToArray(), incidentConfig.nodeHeaders.ToArray(), incidentConfig.nodes,
                        incidentConfig.chapterConfig.GetIncidentGuidByIndex, incidentConfig.GetNodeGuidByIndex);

                DrawCreateSubConfig<IncidentItemConfig, IncidentItemConfigGUI>("事件项目");

                DrawSubGUIs(incidentConfig.incidentItems);
            });
        }
    }

    public class IncidentItemConfigGUI : StoryIncidentConfigGUIBase
    {
        private IncidentItemConfig incidentItemConfig;

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            incidentItemConfig = configData as IncidentItemConfig;
            incidentItemConfig.incidentConfig = ParentGUI.configData as IncidentConfig;
            GUILayoutxCondition.Init(ref incidentItemConfig.conditionConfig);

            incidentItemConfig.RefreshNodeEditorData();
        }

        protected override void ResetSubConfigGUIs()
        {
            ResetSubConfigGUIs<IncidentNodeConfigGUI>();
        }

        protected override bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = GUIStylex.Get.AreaStyle_2;
            labelStyle = GUIStylex.Get.LabelStyle_2;
            return true;
        }

        protected override bool GetFadeAreaOpenDefault()
        {
            return incidentItemConfig.fadeAreaOpenCached;
        }

        protected override object[] GetSubConfigDatas()
        {
            return incidentItemConfig.nodes != null ? incidentItemConfig.nodes : new IncidentNodeConfig[0];
        }

        protected override void SetSubConfigDatas(object[] configDatas)
        {
            incidentItemConfig.nodes = ConvertConfigDatas<IncidentNodeConfig>(configDatas);
        }

        protected override void OnCreateSubConfig<TSubConfig>(ref TSubConfig subConfig)
        {
            base.OnCreateSubConfig(ref subConfig);

            incidentItemConfig.RefreshNodeEditorData();
        }

        protected override void OnDestroySubConfig<TSubConfig>(TSubConfig subConfig)
        {
            base.OnDestroySubConfig(subConfig);

            incidentItemConfig.RefreshNodeEditorData();
        }

        public override void Draw(int index)
        {
            base.Draw(index);

            FadeAreaDraw(index, "IncidentItem", "事件项目", ref incidentItemConfig.fadeAreaOpenCached, () =>
            {
                DrawConfigCommonData();
                GUILayoutxRandomData.Draw(ref incidentItemConfig.randomData, false);

                if (incidentItemConfig.HaveNodes)
                {
                    incidentItemConfig.StartNodeIndex = EditorGUILayout.Popup("StartNode", incidentItemConfig.StartNodeIndex, incidentItemConfig.nodeHeaders.ToArray());
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("StartNode", GUILayout.Width(150f)); EditorGUILayout.LabelField("没有任何事件");
                    EditorGUILayout.EndHorizontal();
                }

                GUILayoutxCondition.Draw(ref incidentItemConfig.conditionConfig);

                GUILayoutxTask.Draw(ref incidentItemConfig.taskConfig);

                DrawCreateSubConfig<IncidentNodeConfig, IncidentNodeConfigGUI>("事件节点");

                DrawSubGUIs(incidentItemConfig.nodes);
            });
        }
    }

    public class IncidentNodeConfigGUI : StoryIncidentConfigGUIBase
    {
        private IncidentNodeConfig incidentNodeConfig;

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            incidentNodeConfig = configData as IncidentNodeConfig;
            incidentNodeConfig.incidentItemConfig = ParentGUI.configData as IncidentItemConfig;

            GUILayoutxTask.Init(ref incidentNodeConfig.taskConfig);

            incidentNodeConfig.RefreshChooseEditorData();
        }

        protected override void ResetSubConfigGUIs()
        {
            ResetSubConfigGUIs<IncidentChooseConfigGUI>();
        }

        protected override bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = GUIStylex.Get.AreaStyle_2;
            labelStyle = GUIStylex.Get.LabelStyle_2;
            return true;
        }

        protected override bool GetFadeAreaOpenDefault()
        {
            return incidentNodeConfig.fadeAreaOpenCached;
        }

        protected override object[] GetSubConfigDatas()
        {
            return incidentNodeConfig.chooses != null ? incidentNodeConfig.chooses : new IncidentChooseConfig[0];
        }

        protected override void SetSubConfigDatas(object[] configDatas)
        {
            incidentNodeConfig.chooses = ConvertConfigDatas<IncidentChooseConfig>(configDatas);
        }

        protected override void OnCreateSubConfig<TSubConfig>(ref TSubConfig subConfig)
        {
            base.OnCreateSubConfig(ref subConfig);
            incidentNodeConfig.RefreshChooseEditorData();
        }

        protected override void OnDestroySubConfig<TSubConfig>(TSubConfig subConfig)
        {
            base.OnDestroySubConfig(subConfig);
            incidentNodeConfig.RefreshChooseEditorData();
        }

        public override void Draw(int index)
        {
            base.Draw(index);

            FadeAreaDraw(index, "Node", "事件节点", ref incidentNodeConfig.fadeAreaOpenCached, () =>
            {
                if (DrawConfigCommonData())
                {
                    IncidentItemConfigParent.RefreshNodeEditorData();
                }

                GUILayoutxTask.Draw(ref incidentNodeConfig.taskConfig);

                incidentNodeConfig.nodeType = GUILayoutx.EnumPopup("NodeType", ConfigConstData.incidentNodeConfig_nodeType_Tooltip, incidentNodeConfig.nodeType);
                incidentNodeConfig.scoreLimit = GUILayoutx.EnumPopup("ScoreLimit", ConfigConstData.scoreLimit_Tooltip, incidentNodeConfig.scoreLimit);
                if(incidentNodeConfig.scoreLimit != ComparisonOperators.None) 
                    incidentNodeConfig.scoreLimitNum = GUILayoutx.IntField("ScoreLimitNum", ConfigConstData.scoreLimitNum_Tooltip, incidentNodeConfig.scoreLimitNum);

                if (incidentNodeConfig.paragraphs != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    incidentNodeConfig.foldoutParagraphs = GUILayoutx.Foldout("Paragraphs", ConfigConstData.incidentNodeConfig_paragraphs_Tooltip, incidentNodeConfig.foldoutParagraphs);
                    if (GUILayout.Button("创建段落"))
                    {
                        List<Paragraph> paragraphs = new List<Paragraph>(incidentNodeConfig.paragraphs);
                        paragraphs.Add(new Paragraph());
                        incidentNodeConfig.paragraphs = paragraphs.ToArray();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (incidentNodeConfig.foldoutParagraphs)
                    {
                        int removeIndex = -1;
                        EditorGUI.indentLevel += 1;
                        EditorGUILayout.Space();
                        for (int i = 0; i < incidentNodeConfig.paragraphs.Length; i++)
                        {
                            var section = incidentNodeConfig.paragraphs[i];
                            EditorGUILayout.BeginHorizontal();
                            section.foldoutSelf = GUILayoutx.Foldout($"Section {i}", section.foldoutSelf);
                            if (GUILayout.Button("Delete", GUIStylex.Get.BtnStyle_DeleteBox))
                            {
                                removeIndex = i;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (section.foldoutSelf)
                            {
                                EditorGUI.indentLevel += 1;
                                section.actor = GUILayoutx.TextField("Actor", ConfigConstData.incidentNodeConfig_paragraphs_actor_Tooltip, section.actor);
                                section.describe = GUILayoutx.TextField("Describe", ConfigConstData.describe_Tooltip, section.describe);
                                EditorGUI.indentLevel -= 1;
                            }

                            incidentNodeConfig.paragraphs[i] = section;
                        }
                        if (removeIndex >= 0)
                        {
                            List<Paragraph> paragraphs = new List<Paragraph>(incidentNodeConfig.paragraphs);
                            paragraphs.RemoveAt(removeIndex);
                            incidentNodeConfig.paragraphs = paragraphs.ToArray();
                        }
                        EditorGUI.indentLevel -= 1;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Paragraphs");
                    if (GUILayout.Button("创建段落"))
                    {
                        incidentNodeConfig.paragraphs = new Paragraph[1];
                        incidentNodeConfig.paragraphs[0] = new Paragraph();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                DrawCreateSubConfig<IncidentChooseConfig, IncidentChooseConfigGUI>("节点选择");

                DrawSubGUIs(incidentNodeConfig.chooses);
            });
        }
    }

    public class IncidentChooseConfigGUI : StoryIncidentConfigGUIBase
    {
        private IncidentChooseConfig incidentChooseConfig;
        private readonly List<IncidentNodeConfig> nodesSelf = new List<IncidentNodeConfig>();
        private string[] nodeHeadersSelf;

        protected override void SetInfoRewriteBrfore(object configData)
        {
            base.SetInfoRewriteBrfore(configData);

            incidentChooseConfig = configData as IncidentChooseConfig;
            incidentChooseConfig.incidentNodeConfig = ParentGUI.configData as IncidentNodeConfig;
            GUILayoutxCondition.Init(ref incidentChooseConfig.conditionConfig);
            GUILayoutxTask.Init(ref incidentChooseConfig.taskConfig);

            nodesSelf.Add(IncidentNodeConfigParent);
            nodeHeadersSelf = new string[] 
            {
                $"{(string.IsNullOrEmpty(IncidentNodeConfigParent.configCommonData.NameEditorShow) ? $"Node {IncidentNodeConfigParent.Guid()}" : IncidentNodeConfigParent.configCommonData.NameEditorShow)}" 
            };

            if (incidentChooseConfig.linkOtherConfig != null)
            {
                for (int i = 0; i < incidentChooseConfig.linkOtherConfig.linkOtherItems.Count; i++)
                {
                    GUILayoutxCondition.Init(ref incidentChooseConfig.linkOtherConfig.linkOtherItems[i].conditionConfig);
                }
            }
        }

        protected override bool GetSubFadeAreaStyle(out UnityEngine.GUIStyle areaStyle, out UnityEngine.GUIStyle labelStyle)
        {
            areaStyle = GUIStylex.Get.AreaStyle_2;
            labelStyle = GUIStylex.Get.LabelStyle_2;
            return true;
        }

        protected override bool GetFadeAreaOpenDefault()
        {
            return incidentChooseConfig.fadeAreaOpenCached;
        }

        public override void Draw(int index)
        {
            base.Draw(index);

            FadeAreaDraw(index, "Choose", "节点选择", ref incidentChooseConfig.fadeAreaOpenCached, () =>
            {
                if (DrawConfigCommonData())
                {
                    IncidentNodeConfigParent.RefreshChooseEditorData();
                }
                incidentChooseConfig.scoreUseType = GUILayoutx.EnumPopup("ScoreUseType", ConfigConstData.incidentChooseConfig_scoreUseType_Tooltip, incidentChooseConfig.scoreUseType);
                incidentChooseConfig.score = GUILayoutx.IntField("Score", ConfigConstData.incidentChooseConfig_score_Tooltip, incidentChooseConfig.score);

                GUILayoutxLinkOther.DrawLinkOtherConfigs(
                    ref incidentChooseConfig.linkOtherConfig, IncidentItemConfigParent.nodeHeaders.ToArray(), nodeHeadersSelf, nodesSelf,
                    IncidentItemConfigParent.GetNodeGuidByIndex, IncidentItemConfigParent.GetNodeGuidByIndex);

                GUILayoutxCondition.Draw(ref incidentChooseConfig.conditionConfig);

                GUILayoutxTask.Draw(ref incidentChooseConfig.taskConfig);
            });
        }
    }
}