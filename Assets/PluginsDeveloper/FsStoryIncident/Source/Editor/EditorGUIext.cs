using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SettingsProvider = UnityEditor.SettingsProvider;

namespace FsStoryIncident
{
	public static class GUILayoutx
    {
        public static bool ButtonGreen(string text, string tooltip = "", int width = -1)
        {
            return ButtonColor(Color.green, text, tooltip, width);
        }

        public static bool ButtonYellow(string text, string tooltip = "", int width = -1)
        {
            return ButtonColor(Color.yellow, text, tooltip, width);
        }

        public static bool ButtonRed(string text, string tooltip = "", int width = -1)
        {
            return ButtonColor(Color.red, text, tooltip, width);
        }

        public static bool ButtonColor(Color color, string text, string tooltip = "", int width = -1)
        {
            GUIUtilityx.PushTint(color);
            bool press;
            if (width <= 0)
                press = GUILayout.Button(new GUIContent(text, tooltip));
            else
                press = GUILayout.Button(new GUIContent(text, tooltip), GUILayout.Width(width));
            GUIUtilityx.PopTint();
            return press;
        }

        public static void Separator(float upSpace = 6f, float downSpace = 6f)
        {
            EditorGUILayout.Space(upSpace);

            Rect r = GUILayoutUtility.GetRect(new GUIContent(), GUIStylex.Get.Separator);

            if (Event.current.type == EventType.Repaint)
            {
                GUIStylex.Get.Separator.Draw(r, false, false, false, false);
            }

            EditorGUILayout.Space(downSpace);
        }

        #region Number Field Clamp
        public static int IntField(string head, string tooltip, int value)
        {
            return EditorGUILayout.DelayedIntField(new GUIContent(head, tooltip), value);
        }

        public static int IntField(string head, string tooltip, int value, int min = int.MinValue, int max = int.MaxValue)
        {
            return Mathf.Clamp(EditorGUILayout.DelayedIntField(new GUIContent(head, tooltip), value), min, max);
        }

        public static uint UIntField(string head, string tooltip, uint value, uint min = uint.MinValue, uint max = uint.MaxValue)
        {
            return (uint)Mathf.Clamp(EditorGUILayout.LongField(new GUIContent(head, tooltip), value), min, max);
        }

        public static short ShortField(string head, string tooltip, short value, short min = short.MinValue, short max = short.MaxValue)
        {
            return (short)Mathf.Clamp(EditorGUILayout.DelayedIntField(new GUIContent(head, tooltip), value), min, max);
        }

        public static ushort UShortField(string head, string tooltip, ushort value, ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        {
            return (ushort)Mathf.Clamp(EditorGUILayout.DelayedIntField(new GUIContent(head, tooltip), value), min, max);
        }
        #endregion

        public static string TextField(string head, string tooltip, string str)
		{
			return EditorGUILayout.DelayedTextField(new GUIContent(head, tooltip), str);
        }

        public static void TextFieldDisplayOnly(string head, string tooltip, string str)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.DelayedTextField(new GUIContent(head, tooltip), str);
            EditorGUI.EndDisabledGroup();
        }

        public static T EnumPopup<T>(string head, string tooltip, T enumVaule) where T : struct, System.Enum
		{
			return (T)EditorGUILayout.EnumPopup(new GUIContent(head, tooltip), enumVaule);
        }

		public static bool Foldout(string head, string tooltip, bool value, bool changedExclude = true)
		{
            bool changedCached = false;
            if (changedExclude)
                changedCached = GUI.changed;

            bool foldout = EditorGUILayout.Foldout(value, new GUIContent(head, tooltip));

            if (changedExclude && !changedCached) GUI.changed = changedCached;//开关FadeArea排除Changed判断

            return foldout;
        }

        public static bool Foldout(string head, bool value, bool changedExclude = true)
        {
            bool changedCached = false;
            if (changedExclude)
                changedCached = GUI.changed;

            bool foldout = EditorGUILayout.Foldout(value, head);

            if (changedExclude && !changedCached) GUI.changed = changedCached;//开关FadeArea排除Changed判断

            return foldout;
        }
    }

	//GUI扩展工具
	//有从AstarPathfindingProject插件拿过来的GUI绘制类

	public static class GUIUtilityx
	{
		static Stack<Color> colors = new Stack<Color>();

		public static void PushTint(Color tint)
		{
			colors.Push(GUI.color);
			GUI.color *= tint;
		}

		public static void PopTint()
		{
			GUI.color = colors.Pop();
		}
	}

    public static class GUILayoutxCondition
    {
        /// <summary>
        /// 子类如果有ConditionConfig需要配置，在SetInfo时调用此方法初始化
        /// </summary>
        /// <param name="conditionConfig"></param>
        public static void Init(ref ConditionConfig conditionConfig)
        {
            //获取所有实现了IStoryCondition接口的类型
            if (ConditionConfig.conditionTypes == null)
            {
                StoryIncidentEditorLibrary.GetTypes(typeof(IStoryCondition), out System.Type[] types, out ConditionConfig.conditionTypes, "None");
                ConditionConfig.conditionParamsComments = new string[ConditionConfig.conditionTypes.Length];
                if (ConditionConfig.conditionTypes.Length > 0)
                {
                    ConditionConfig.conditionParamsComments[0] = "";
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (StoryCondition.GetStoryConditionDefault(types[i], out IStoryCondition iStoryCondition))
                        {
                            ConditionConfig.conditionParamsComments[i + 1] = iStoryCondition.GetParamsComment();
                        }
                    }
                }
            }

            if (conditionConfig.HaveConditionItems)
            {
                for (int i = 0; i < conditionConfig.conditionItems.Count; i++)
                {
                    //更新时重新确认条件对应的Index，因为可能新增了条件并重新编译走到这
                    var item = conditionConfig.conditionItems[i];
                    item.ConditionTypePopupIndex = ConditionConfig.GetIndexByType(conditionConfig.conditionItems[i].typeStr);
                    conditionConfig.conditionItems[i] = item;
                }
            }
        }

        /// <summary>
        /// 绘制条件配置GUI
        /// </summary>
        /// <param name="conditionConfig"></param>
        public static void Draw(ref ConditionConfig conditionConfig)
        {
            if (!ConditionConfig.HaveConditionTypes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("ConditionConfig", ConfigConstData.conditionConfig_Tooltip), GUILayout.Width(150));
                EditorGUILayout.LabelField("没有任何可用的条件接口类");
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            conditionConfig.foldout = GUILayoutx.Foldout("ConditionConfig", ConfigConstData.conditionConfig_Tooltip, conditionConfig.foldout);
            if (GUILayout.Button("添加条件项目"))
            {
                if (conditionConfig.conditionItems == null)
                    conditionConfig.conditionItems = new List<ConditionItemConfig>();

                var newItem = new ConditionItemConfig();
                newItem.foldout = true;
                conditionConfig.conditionItems.Add(newItem);
                conditionConfig.foldout = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (!conditionConfig.foldout) return;

            EditorGUI.indentLevel += 1;

            //条件逻辑表达式
            if (conditionConfig.logicExpressionNotValid)
            {
                EditorGUILayout.HelpBox("逻辑表达式不合法，在运行时将直接通过条件。逻辑表达式必须由数字和逻辑运算符 && || ! == != 组成。（支持单个写法 & | =）", MessageType.Warning);
            }

            string logicExpressionCachedNew = GUILayoutx.TextField("LogicExpression", ConfigConstData.conditionConfig_logicExpression_Tooltip, conditionConfig.logicExpressionCached);
            if (logicExpressionCachedNew != conditionConfig.logicExpressionCached)
            {
                conditionConfig.logicExpressionNotValid = !StoryCondition.CheckExpressionValid(ref logicExpressionCachedNew, conditionConfig.conditionItems);
                conditionConfig.logicExpressionCached = logicExpressionCachedNew;
                conditionConfig.logicExpression = conditionConfig.logicExpressionNotValid ? String.Empty : conditionConfig.logicExpressionCached;
            }

            //显示当前条件项目
            if (conditionConfig.HaveConditionItems)
            {
                for (int i = 0; i < conditionConfig.conditionItems.Count; i++)
                {
                    ConditionItemConfig item = conditionConfig.conditionItems[i];

                    EditorGUILayout.BeginHorizontal();
                    item.foldout = GUILayoutx.Foldout($"{(string.IsNullOrEmpty(item.typeStr) ? "ConditionItem" : item.typeStr)} {i}", item.foldout);
                    bool isd = false;
                    if (GUILayout.Button("Delete", GUIStylex.Get.BtnStyle_DeleteBox))
                    {
                        conditionConfig.conditionItems.RemoveAt(i);
                        i--;
                        isd = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (isd) continue;

                    if (item.foldout)
                    {
                        EditorGUI.indentLevel += 1;
                        item.ConditionTypePopupIndex = EditorGUILayout.Popup(
                            new GUIContent("ConditionType", ConditionConfig.conditionParamsComments[item.ConditionTypePopupIndex]), item.ConditionTypePopupIndex, ConditionConfig.conditionTypes);
                        string type = ConditionConfig.conditionTypes[item.ConditionTypePopupIndex];
                        if (!string.IsNullOrEmpty(type))
                        {
                            item.param = GUILayoutx.TextField("Param", ConfigConstData.param_Tooltip, item.param);
                        }
                        EditorGUI.indentLevel -= 1;
                    }

                    conditionConfig.conditionItems[i] = item;
                }
            }

            EditorGUI.indentLevel -= 1;
        }
    }

    public static class GUILayoutxTask
    {
        /// <summary>
        /// 子类如果有TaskConfig需要配置，在SetInfo时调用此方法初始化
        /// </summary>
        /// <param name="taskConfig"></param>
        public static void Init(ref TaskConfig taskConfig)
        {
            //获取所有实现了IStoryTask接口的类型
            if (TaskConfig.taskTypes == null)
            {
                StoryIncidentEditorLibrary.GetTypes(typeof(IStoryTask), out System.Type[] types, out TaskConfig.taskTypes, "None");
                TaskConfig.taskParamsComments = new string[TaskConfig.taskTypes.Length];
                if (TaskConfig.taskTypes.Length > 0)
                {
                    TaskConfig.taskParamsComments[0] = "";
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (StoryTask.GetStoryTaskDefault(types[i], out IStoryTask iStoryTask))
                        {
                            TaskConfig.taskParamsComments[i + 1] = iStoryTask.GetParamsComment();
                        }
                    }
                }
            }

            if (taskConfig.HaveTaskItems)
            {
                for (int i = 0; i < taskConfig.taskItems.Count; i++)
                {
                    //更新时重新确认工作对应的Index，因为可能新增了Task并重新编译走到这
                    var item = taskConfig.taskItems[i];
                    item.TaskTypePopupIndex = TaskConfig.GetIndexByType(taskConfig.taskItems[i].typeStr);
                    taskConfig.taskItems[i] = item;
                }
            }
        }

        /// <summary>
        /// 绘制工作配置GUI
        /// </summary>
        /// <param name="taskConfig"></param>
        public static void Draw(ref TaskConfig taskConfig)
        {
            if (!TaskConfig.HaveTaskTypes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("TaskConfig", ConfigConstData.taskConfig_Tooltip), GUILayout.Width(150));
                EditorGUILayout.LabelField("没有任何可用的工作接口类");
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            taskConfig.foldout = GUILayoutx.Foldout("TaskConfig", ConfigConstData.taskConfig_Tooltip, taskConfig.foldout);
            if (GUILayout.Button("添加工作项目"))
            {
                if (taskConfig.taskItems == null)
                    taskConfig.taskItems = new List<TaskItemConfig>();

                var newItem = new TaskItemConfig();
                newItem.foldout = true;
                taskConfig.taskItems.Add(newItem);
                taskConfig.foldout = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (!taskConfig.foldout) return;

            EditorGUI.indentLevel += 1;

            //显示当前工作项目
            if (taskConfig.HaveTaskItems)
            {
                for (int i = 0; i < taskConfig.taskItems.Count; i++)
                {
                    TaskItemConfig item = taskConfig.taskItems[i];

                    EditorGUILayout.BeginHorizontal();
                    item.foldout = GUILayoutx.Foldout($"{(string.IsNullOrEmpty(item.typeStr) ? "TaskItem" : item.typeStr)} {i}", item.foldout);
                    bool isd = false;
                    if (GUILayout.Button("Delete", GUIStylex.Get.BtnStyle_DeleteBox))
                    {
                        taskConfig.taskItems.RemoveAt(i);
                        i--;
                        isd = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (isd) continue;

                    if (item.foldout)
                    {
                        EditorGUI.indentLevel += 1;
                        item.TaskTypePopupIndex = EditorGUILayout.Popup(
                            new GUIContent("TaskType", TaskConfig.taskParamsComments[item.TaskTypePopupIndex]), item.TaskTypePopupIndex, TaskConfig.taskTypes);
                        string type = TaskConfig.taskTypes[item.TaskTypePopupIndex];
                        if (!string.IsNullOrEmpty(type))
                        {
                            item.param = GUILayoutx.TextField("Param", ConfigConstData.param_Tooltip, item.param);
                        }
                        EditorGUI.indentLevel -= 1;
                    }

                    taskConfig.taskItems[i] = item;
                }
            }

            EditorGUI.indentLevel -= 1;
        }
    }

    public static class GUILayoutxLinkOther
    {
        public static void DrawLinkOtherConfigs(ref LinkOtherConfig linkOtherConfig, string[] selectTargetHeaders, string[] selectNodeHeaders, List<IncidentNodeConfig> selectNodes,
            Func<int, Guid> getGuidByIndexFuncForTarget, Func<int, Guid> getGuidByIndexFuncForNode)
        {
            if (linkOtherConfig == null) return;

            EditorGUILayout.BeginHorizontal();
            linkOtherConfig.foldoutLinkItems = GUILayoutx.Foldout("LinkOtherConfig", ConfigConstData.linkOtherConfig_Tooltip, linkOtherConfig.foldoutLinkItems);
            if (GUILayout.Button("创建链接项目"))
            {
                linkOtherConfig.linkOtherItems.Add(new LinkOtherItem()); 
            }
            EditorGUILayout.EndHorizontal();

            if (linkOtherConfig.foldoutLinkItems)
            {
                int itemsRemoveIndex = -1;
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < linkOtherConfig.linkOtherItems.Count; i++)
                {
                    LinkOtherItem item = linkOtherConfig.linkOtherItems[i];
                    EditorGUILayout.BeginHorizontal();
                    item.foldoutSelf = GUILayoutx.Foldout($"Item {i}", item.foldoutSelf);
                    if (GUILayout.Button("Delete", GUIStylex.Get.BtnStyle_DeleteBox))
                    {
                        itemsRemoveIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    if (item.foldoutSelf)
                    {
                        EditorGUI.indentLevel += 1;

                        int targetIndex = EditorGUILayout.Popup("TargetGuid", item.TargetIndex, selectTargetHeaders);
                        if(targetIndex != item.TargetIndex)
                        {
                            item.targetSGuid.GuidStr = string.Empty;
                            if (getGuidByIndexFuncForTarget != null) item.targetSGuid.GuidStr = getGuidByIndexFuncForTarget(targetIndex).ToString();
                            if (item.targetSGuid.GuidStr != string.Empty) item.TargetIndex = targetIndex;
                        }
                        GUILayoutxRandomData.Draw(ref item.randomData, false);
                        item.scoreLimit = GUILayoutx.EnumPopup("ScoreLimit", ConfigConstData.scoreLimit_Tooltip, item.scoreLimit);
                        if(item.scoreLimit != ComparisonOperators.None)
                            item.scoreLimitNum = GUILayoutx.IntField("ScoreLimitNum", ConfigConstData.scoreLimitNum_Tooltip, item.scoreLimitNum);

                        //节点选择限制
                        EditorGUILayout.BeginHorizontal();
                        item.foldoutNodeChooseLimit = GUILayoutx.Foldout("NodeChooseLimit", ConfigConstData.linkOtherConfig_nodeChooseLimit_Tooltip, item.foldoutNodeChooseLimit);
                        if (GUILayout.Button("创建节点选择限制"))
                        {
                            item.nodeChooseLimit.Add(new NodeChooseLimitConfig());
                        }
                        EditorGUILayout.EndHorizontal();
                        if (item.foldoutNodeChooseLimit)
                        {
                            int nodeChooseLimitRemoveIndex = -1;
                            EditorGUI.indentLevel += 1;
                            for (int j = 0; j < item.nodeChooseLimit.Count; j++)
                            {
                                var nodeChooseLimitConfig = item.nodeChooseLimit[j];
                                EditorGUILayout.BeginHorizontal();
                                nodeChooseLimitConfig.foldoutSelf = GUILayoutx.Foldout($"NodeChooseLimit {j}", nodeChooseLimitConfig.foldoutSelf);
                                if (GUILayout.Button("Delete", GUIStylex.Get.BtnStyle_DeleteBox))
                                {
                                    nodeChooseLimitRemoveIndex = j;
                                }
                                EditorGUILayout.EndHorizontal();
                                if (nodeChooseLimitConfig.foldoutSelf)
                                {
                                    EditorGUI.indentLevel += 1;

                                    nodeChooseLimitConfig.type = GUILayoutx.EnumPopup("LimitType", ConfigConstData.nodeChooseLimitConfig_type_Tooltip, nodeChooseLimitConfig.type);
                                    int nodeIndex = EditorGUILayout.Popup("Node", nodeChooseLimitConfig.NodeIndex, selectNodeHeaders);
                                    IncidentNodeConfig node = selectNodes != null && nodeIndex <= selectNodes.Count ? selectNodes[nodeIndex] : null;
                                    if(nodeChooseLimitConfig.nodeConfig == null) nodeChooseLimitConfig.SetNodeConfig(node);
                                    if (nodeChooseLimitConfig.NodeIndex != nodeIndex)
                                    {
                                        nodeChooseLimitConfig .nodeGuid = System.Guid.Empty;
                                        if (getGuidByIndexFuncForNode != null) nodeChooseLimitConfig.nodeGuid = getGuidByIndexFuncForNode(nodeIndex);
                                        if (nodeChooseLimitConfig.nodeGuid != System.Guid.Empty)
                                        {
                                            nodeChooseLimitConfig.NodeIndex = nodeIndex;
                                            nodeChooseLimitConfig.SetNodeConfig(node);
                                        }
                                    }
                                    if (node != null)
                                        nodeChooseLimitConfig.ChooseIndex = EditorGUILayout.Popup("Choose", nodeChooseLimitConfig.ChooseIndex, node.chooseHeaders.ToArray());

                                    EditorGUI.indentLevel -= 1;
                                }
                                item.nodeChooseLimit[j] = nodeChooseLimitConfig;
                            }
                            if (nodeChooseLimitRemoveIndex >= 0)
                            {
                                item.nodeChooseLimit.RemoveAt(nodeChooseLimitRemoveIndex);
                            }
                            EditorGUI.indentLevel -= 1;
                        }

                        GUILayoutxCondition.Draw(ref item.conditionConfig);

                        EditorGUI.indentLevel -= 1;
                    }
                }

                if (itemsRemoveIndex >= 0)
                {
                    linkOtherConfig.linkOtherItems.RemoveAt(itemsRemoveIndex);
                }

                EditorGUI.indentLevel -= 1;
            }
        }
    }

    public static class GUILayoutxRandomData
    {
        public static void Draw(ref RandomData randomData, bool drawProbability = true, bool drawPriority = true, bool drawWeight = true)
        {
            if(drawProbability) randomData.probability = GUILayoutx.UShortField("Probability", ConfigConstData.probability_Tooltip, randomData.probability, 0, 10000);
            if (drawPriority) randomData.priority = GUILayoutx.ShortField("Priority", ConfigConstData.priority_Tooltip, randomData.priority);
            if (drawWeight) randomData.weight = GUILayoutx.UShortField("Weight", ConfigConstData.weight_Tooltip, randomData.weight);
        }
    }

    /// <summary>
    /// 可开关隐藏区域GUI
    /// 调用顺序:
    /// - Begin
    /// - Header
    /// - if(BeginFade)
    /// - { 自定义内容 }
    /// - End
    /// </summary>
    public class FadeArea
    {
        Rect lastRect;
        float value;
        float lastUpdate;
        readonly GUIStyle labelStyle;
        readonly GUIStyle areaStyle;
        bool visible;
        readonly EditorWindow editorWindow;
        readonly SettingsProvider settingsProvider;
        readonly bool immediately;
        bool changedCached;

        public float beginSpace = 1.5f;
        public bool changedExcludeHeraderClick = true;//将点击Header排除出GUI.changed

        /// <summary>
        /// Is this area open.
        /// This is not the same as if any contents are visible, use <see cref="BeginFade"/> for that.
        /// </summary>
        public bool open;

        /// <summary>Animate dropdowns when they open and close</summary>
        public static bool fancyEffects = true;
        const float animationSpeed = 100f;

        public FadeArea(EditorWindow editor, bool open, GUIStyle areaStyle, GUIStyle labelStyle = null, float beginSpace = 1.5f, bool immediately = false, bool changedExcludeHeraderClick = true)
        {
            this.editorWindow = editor;

            this.areaStyle = areaStyle;
            this.labelStyle = labelStyle;
            visible = this.open = open;
            value = open ? 1 : 0;
            this.beginSpace = beginSpace;
            this.immediately = immediately;
            this.changedExcludeHeraderClick = changedExcludeHeraderClick;
        }

        public FadeArea(SettingsProvider settingsProvider, bool open, GUIStyle areaStyle, GUIStyle labelStyle = null, float beginSpace = 1.5f, bool immediately = false, bool changedExcludeHeraderClick = true)
        {
            this.settingsProvider = settingsProvider;

            this.areaStyle = areaStyle;
            this.labelStyle = labelStyle;
            visible = this.open = open;
            value = open ? 1 : 0;
            this.beginSpace = beginSpace;
            this.immediately = immediately;
            this.changedExcludeHeraderClick = changedExcludeHeraderClick;
        }

        void Tick()
        {
            if (Event.current.type == EventType.Repaint)
            {
                float deltaTime = Time.realtimeSinceStartup - lastUpdate;

                // Right at the start of a transition the deltaTime will
                // not be reliable, so use a very small value instead
                // until the next repaint
                if (value == 0f || value == 1f) deltaTime = 0.001f;
                deltaTime = Mathf.Clamp(deltaTime, 0.00001F, 0.1F);

                // Larger regions fade slightly slower
                deltaTime /= Mathf.Sqrt(Mathf.Max(lastRect.height, 100));

                lastUpdate = Time.realtimeSinceStartup;


                float targetValue = open ? 1F : 0F;
                if (!Mathf.Approximately(targetValue, value))
                {
                    value += deltaTime * animationSpeed * Mathf.Sign(targetValue - value);
                    value = Mathf.Clamp01(value);

                    settingsProvider?.Repaint();
                    editorWindow?.Repaint();

                    if (!fancyEffects)
                    {
                        value = targetValue;
                    }
                }
                else
                {
                    value = targetValue;
                }
            }
        }

        public void Begin()
        {
            if (areaStyle != null)
            {
                lastRect = EditorGUILayout.BeginVertical(areaStyle);
            }
            else
            {
                lastRect = EditorGUILayout.BeginVertical();
            }

            EditorGUILayout.Space(beginSpace);
        }

        public void Header(string label, string tooltip = "")
        {
            Header(label, ref open, tooltip);
        }

        public void Header(string label, string tooltip, int width)
        {
            Header(label, ref open, tooltip, width);
        }

        public void Header(string label, int width)
        {
            Header(label, ref open, "", width);
        }

        public void Header(string label, ref bool open, string tooltip = "", int width = -1)
        {
            if (changedExcludeHeraderClick)
                changedCached = GUI.changed;

            bool press;
            if (width > 0)
            {
                press = GUILayout.Button(new GUIContent(label, tooltip), labelStyle, GUILayout.Width(width));
            }
            else
            {
                press = GUILayout.Button(new GUIContent(label, tooltip), labelStyle);
            }

            if (press)
            {
                open = !open;
                settingsProvider?.Repaint();
                editorWindow?.Repaint();
            }
            this.open = open;
            if (immediately) value = open ? 1f : 0f;

            if (changedExcludeHeraderClick && !changedCached) GUI.changed = changedCached;//开关FadeArea排除Changed判断
        }

        /// <summary>Hermite spline interpolation</summary>
        static float Hermite(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }

        public bool BeginFade()
        {
            var hermite = Hermite(0, 1, value);

            visible = EditorGUILayout.BeginFadeGroup(hermite);
            GUIUtilityx.PushTint(new Color(1, 1, 1, hermite));
            Tick();

            // Another vertical group is necessary to work around
            // a kink of the BeginFadeGroup implementation which
            // causes the padding to change when value!=0 && value!=1
            EditorGUILayout.BeginVertical();

            return visible;
        }

        public void End()
        {
            EditorGUILayout.EndVertical();

            if (visible)
            {
                // Some space that cannot be placed in the GUIStyle unfortunately
                GUILayout.Space(4);
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndVertical();
            GUIUtilityx.PopTint();
        }

        /// <summary>
        /// 在折叠空间中绘制
        /// </summary>
        /// <param name="headerLabel">头部标签</param>
        /// <param name="open">折叠空间是否打开</param>
        /// <param name="drawContent">折叠空间绘制内容</param>
        /// <param name="drawHeader">在头部绘制内容</param>
        public void FadeAreaDraw(string headerLabel, ref bool open, Action drawContent, Action drawHeader = null)
        {
            DrawFadeAreaHeader(headerLabel, drawHeader);
            FadeAreaBeginFade(ref open, drawContent);
            End();
        }

        /// <summary>
        /// 绘制折叠空间头部GUI
        /// </summary>
        /// <param name="headerLabel"></param>
        /// <param name="draw"></param>
        public void DrawFadeAreaHeader(string headerLabel, Action draw = null)
        {
            Begin();
            EditorGUILayout.BeginHorizontal();
            Header(headerLabel);
            if (draw != null) draw.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 折叠空间GUI，开始。
        /// </summary>
        /// <param name="open"></param>
        /// <param name="draw"></param>
        /// <returns>折叠空间是否打开</returns>
        public bool FadeAreaBeginFade(ref bool open, Action draw)
        {
            open = BeginFade();
            if (open && draw != null) draw.Invoke();
            return open;
        }
    }
}