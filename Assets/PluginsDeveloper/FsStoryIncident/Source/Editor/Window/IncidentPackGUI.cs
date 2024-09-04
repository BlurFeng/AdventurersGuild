using FsStoryIncident;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class IncidentPackGatherGUI : StoryIncidentConfigGUIBase
{
    #region StaticData

    public static List<string> AllIncidentHeaders { get; private set; } = new List<string>();
    public static List<string> AllIncidentGuids { get; private set; } = new List<string>();

    public static void RefreshAllIncidentHeaders()
    {
        AllIncidentGuids.Clear();
        AllIncidentHeaders.Clear();

        StoryGatherConfig[] sgConfigs = StoryIncidentEditorLibrary.GetAssets<StoryGatherConfig>();
        if (sgConfigs != null && sgConfigs.Length > 0)
        {
            foreach (StoryGatherConfig sg in sgConfigs)
            {
                foreach (StoryConfig s in sg.storyConfigs)
                {
                    foreach (ChapterConfig c in s.chapters)
                    {
                        if (c.HaveIncidents)
                        {
                            for (int i = c.incidents.Length - 1; i >= 0; i--)
                            {
                                var iConfig = c.incidents[i];
                                if (iConfig == null)
                                {
                                    Debug.LogWarning("RefreshAllIncidentHeaders have a none incidentConfig ?");
                                    continue;
                                }
                                AllIncidentHeaders.Add(
                                    $"{(string.IsNullOrEmpty(iConfig.configCommonData.NameEditorShow) ? iConfig.Guid() : $" {iConfig.configCommonData.NameEditorShow}")} from {(string.IsNullOrEmpty(s.configCommonData.NameEditorShow) ? s.Guid() : $" {s.configCommonData.NameEditorShow}")}");
                                AllIncidentGuids.Add(iConfig.configCommonData.GuidStr);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            AllIncidentGuids.Add("None");
            AllIncidentHeaders.Add("None");
        }
    }

    public static bool ContainsIGuid(Guid iGuid)
    {
        return AllIncidentGuids.Contains(iGuid.ToString());
    }

    public static bool GetIndexByIGuid(string iGuidStr, out int index)
    {
        for (int i = 0; i < AllIncidentGuids.Count; i++)
        {
            if (AllIncidentGuids[i] == iGuidStr)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }
    #endregion

    private IncidentPackGatherConfig incidentPackGatherConfig;
    private IncidentPackGatherConfig incidentPackGatherConfigCopy;
    public bool isDirty;//标脏，是否需要保存

    protected override void SetInfoRewriteBrfore(object configData)
    {
        base.SetInfoRewriteBrfore(configData);

        incidentPackGatherConfig = configData as IncidentPackGatherConfig;
        incidentPackGatherConfig.guidStr = StoryIncidentEditorLibrary.GetGUIDFromAsset(incidentPackGatherConfig).ToString();
        incidentPackGatherConfigCopy = ScriptableObject.Instantiate(incidentPackGatherConfig);

        InitDrawConfigCommonData(incidentPackGatherConfig.configCommonData);

        StoryIncidentEditorStaticData.OnAutoSaveChange += OnAutoSaveChange;
    }

    protected override void ResetSubConfigGUIs()
    {
        ResetSubConfigGUIs<IncidentPackGUI>();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SaveCheck();
        StoryIncidentEditorStaticData.OnAutoSaveChange -= OnAutoSaveChange;
    }

    protected override object[] GetSubConfigDatas()
    {
        return incidentPackGatherConfig.incidentPackConfigs;
    }

    protected override void SetSubConfigDatas(object[] configDatas)
    {
        incidentPackGatherConfig.incidentPackConfigs = ConvertConfigDatas<IncidentPackConfig>(configDatas);
    }

    public override void Draw(int index)
    {
        base.Draw(index);

        if (incidentPackGatherConfig == null) return;

        GUILayoutx.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("事件包集合配置", GUIStylex.Get.TitleStyle_2);
        EditorGUILayout.LabelField(StoryIncidentEditorStaticData.AutoSave ? "自动保存模式" : "手动保存模式", GUIStylex.Get.TitleStyle_3, GUILayout.Width(80f));
        EditorGUI.BeginDisabledGroup(!isDirty);
        if (GUILayoutx.ButtonGreen("Save", width: 80)) Save();
        if (GUILayoutx.ButtonYellow("Revert", width: 80)) if (EditorUtility.DisplayDialog("回退修改", "确定要回退修改的内容吗？", "确定", "取消")) Revert();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //自身配置
        if (!StoryIncidentEditorStaticData.AutoSave) EditorGUI.BeginChangeCheck();

        DrawConfigCommonData();

        //事件包列表
        DrawCreateSubConfig<IncidentPackConfig, IncidentPackGUI>("事件包");
        DrawSubGUIs(incidentPackGatherConfig.incidentPackConfigs);

        if (!StoryIncidentEditorStaticData.AutoSave && EditorGUI.EndChangeCheck()) isDirty = true;
    }

    private void Save()
    {
        if (!isDirty) return;
        isDirty = false;

        //重新记录数据备份
        incidentPackGatherConfigCopy = ScriptableObject.Instantiate(incidentPackGatherConfig);

        AssetDatabase.SaveAssets();
    }

    private void Revert()
    {
        if (!isDirty) return;
        isDirty = false;

        //当前数据回退到之前保存的备份数据
        incidentPackGatherConfig.Copy(incidentPackGatherConfigCopy);
        incidentPackGatherConfigCopy = ScriptableObject.Instantiate(incidentPackGatherConfig);

        InitDrawConfigCommonData(incidentPackGatherConfig.configCommonData);

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

    private void OnAutoSaveChange(bool autoSave)
    {
        if (autoSave)
        {
            Save();
        }
        else
        {
            //从自动保存转为非自动保存时，更新Copy文件
            incidentPackGatherConfigCopy = ScriptableObject.Instantiate(incidentPackGatherConfig);
        }
    }
}

public class IncidentPackGUI : StoryIncidentConfigGUIBase
{
    private IncidentPackConfig incidentPackConfig;
    private ReorderableList reorderableList;

    protected override void SetInfoRewriteBrfore(object configData)
    {
        base.SetInfoRewriteBrfore(configData);

        incidentPackConfig = configData as IncidentPackConfig;
        incidentPackConfig.incidentPackGatherConfig = ParentGUI.configData as IncidentPackGatherConfig;
        GUILayoutxCondition.Init(ref incidentPackConfig.conditionConfig);
         
        reorderableList = new ReorderableList(incidentPackConfig.incidentGuidStrs, typeof(ulong), true, true, true, true)
        {
            drawHeaderCallback = DrawIncidentIdsHeader,
            drawElementCallback = DrawIncidentIdsElement,
            onAddCallback = OnAddIncidentIdsCallback,
            onRemoveCallback = OnRemoveIncidentIdsCallback,
            onMouseDragCallback = OnMouseDragIncidentIdsCallback,
            elementHeight = 20f,
        };
    }

    protected override bool GetFadeAreaOpenDefault()
    {
        return incidentPackConfig.fadeAreaOpenCached;
    }

    public override void Draw(int index)
    {
        base.Draw(index);

        if (incidentPackConfig == null) return;

        FadeAreaDraw(index, "IncidentPack", "事件包", ref incidentPackConfig.fadeAreaOpenCached, () =>
        {
            DrawConfigCommonData();

            GUILayoutxRandomData.Draw(ref incidentPackConfig.randomData);

            GUILayoutxCondition.Draw(ref incidentPackConfig.conditionConfig);

            reorderableList.DoLayoutList();
        });
    }

    private void DrawIncidentIdsHeader(Rect rect)
    {
        var titleRect = rect;
        GUI.Label(titleRect, new GUIContent("IncidentIds", ConfigConstData.tags_Tooltip));
    }

    private void DrawIncidentIdsElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index >= incidentPackConfig.incidentGuidStrs.Count) return;

        //通过输入修改
        string guidStr = incidentPackConfig.incidentGuidStrs[index];
        float widthLeft = 280f;
        string newStr = EditorGUI.DelayedTextField(new Rect(rect.position, new Vector2(widthLeft, rect.size.y)), guidStr);
        if (!newStr.Equals(guidStr))
        {
            if (IncidentPackGatherGUI.GetIndexByIGuid(guidStr, out int selectIndex))
            {
                incidentPackConfig.incidentGuidStrs[index] = guidStr;
                //更新当前选中事件对应的selectIndex
                incidentPackConfig.incidentSelectIndexs[index] = selectIndex;
            }
        }

        //通过选择下拉表进行修改
        int selectIndexOld = incidentPackConfig.incidentSelectIndexs[index];
        if(selectIndexOld >= IncidentPackGatherGUI.AllIncidentGuids.Count - 1)
        {
            selectIndexOld = 0;
        }

        int selectIndexNew = EditorGUI.Popup(
            new Rect(new Vector2(rect.position.x + widthLeft + 20f, rect.position.y), 
            new Vector2(rect.size.x - widthLeft - 20f, rect.size.y)), 
            selectIndexOld, 
            IncidentPackGatherGUI.AllIncidentHeaders.ToArray());

        if (selectIndexNew != selectIndexOld)
        {
            incidentPackConfig.incidentSelectIndexs[index] = selectIndexNew;
            incidentPackConfig.incidentGuidStrs[index] = IncidentPackGatherGUI.AllIncidentGuids[selectIndexNew];
        }
    }

    private void OnAddIncidentIdsCallback(ReorderableList list)
    {
        ReorderableList.defaultBehaviours.DoAddButton(list);
        incidentPackConfig.incidentGuidStrs[list.count - 1] = IncidentPackGatherGUI.AllIncidentGuids[0];
        incidentPackConfig.incidentGuidStrsIsDirty = true;
        RefreshIncidentSelectIndexs();
    }

    private void OnRemoveIncidentIdsCallback(ReorderableList list)
    {
        ReorderableList.defaultBehaviours.DoRemoveButton(list);
        incidentPackConfig.incidentGuidStrsIsDirty = true;
        RefreshIncidentSelectIndexs();
    }

    private void OnMouseDragIncidentIdsCallback(ReorderableList list)
    {
        RefreshIncidentSelectIndexs();
    }

    private void RefreshIncidentSelectIndexs()
    {
        incidentPackConfig.incidentSelectIndexs.Clear();
        for (int i = 0; i < incidentPackConfig.incidentGuidStrs.Count; i++)
        {
            string guidStr = incidentPackConfig.incidentGuidStrs[i];
            IncidentPackGatherGUI.GetIndexByIGuid(guidStr, out int selectIndex);
            incidentPackConfig.incidentSelectIndexs.Add(selectIndex);
        }
    }
}