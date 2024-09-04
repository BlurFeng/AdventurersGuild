
namespace FsStoryIncident
{
    public class ConfigConstData
    {
        //public const string localId_Tooltip = "局部Id，由故事配置编辑工具在编辑时分配。";
        public const string guid_Tooltip = "Guid，全局唯一标识符。";
        public const string configCommonData_Tooltip = "配置通常数据，故事配置中，所有的配置类都需要配置的数据。";
        public const string conditionConfig_Tooltip = "条件配置。不配置时默认通过。";
        public const string taskConfig_Tooltip = "工作配置。当目标被完成时，执行配置的工作。比如进行一个choose时，减少玩家的金币。";
        public const string name_Tooltip = "名称。可以是具体内容或多语言Code。这取决于项目如何使用。";
        public const string describe_Tooltip = "描述。可以是具体内容或多语言Code。这取决于项目如何使用。";
        public const string tags_Tooltip = "标签，用户可自定义添加标签，标签根据用户需求可用于不同的用途。";
        public const string commentName_Tooltip = "备注名称信息，此数据不会被打包，仅在Editor可用。";
        public const string comment_Tooltip = "备注信息，此数据不会被打包，仅在Editor可用。";
        public const string randomData_Tooltip = "用于随机功能的数据。也可以按需用于其他功能。";
        public const string probability_Tooltip = "发生概率。独立计算的发生概率。0-10000百分比精确到小数点后两位。";
        public const string priority_Tooltip = "优先级，当进行选取时，会优先选择优先级更高的。范围-32,768到32,767。";
        public const string weight_Tooltip = "权重，当有多个对象满足条件时，根据权重进行随机。范围0到65,535。";
        public const string scoreLimit_Tooltip = "分数限制，对比方式。None则无限制。当通过分数限制后此链接事件才算满足分数条件。";
        public const string scoreLimitNum_Tooltip = "分数限制的对比数字，根据选择的对比方式进行对比。";
        public const string param_Tooltip =
            "条件参数。规范分隔符为“| ; , -”。" +
            "例：p1|p21;p22|p311,p312;p32|p41;p421,p422;p4311-p4312,p432;p44" +
            "使用规范分隔符后，可以使用StoryIncidentLibrary的ParamParser或ParamsCut方法进行解析获取数据结构ParamData。" +
            "实际情况可以根据项目自行调整，因为参数的解析是在项目实现IStoryIncidentCondition接口的类中进行的。";


        public const string linkOtherConfig_Tooltip = "链接其他配置。在故事中，一个章结束时链接至下一章，直到结束。在线型章中，一个事件需要链接到其他事件，知道没有链接时自动结束。";
        public const string linkOtherConfig_targetPId_Tooltip = "指向的目标PId。";
        public const string linkOtherConfig_nodeChooseLimit_Tooltip = "节点选择限制。要求在某个阶段选择了某个选项才选满足条件。必须满足配置的所有节点选择要求。不配置则无限制。";
        public const string nodeChooseLimitConfig_type_Tooltip = "限制类型，选择了或未选择节点。符合要求则为通过。";
        public const string nodeChooseLimitConfig_nodeId_Tooltip = "节点Id";
        public const string nodeChooseLimitConfig_chooseId_Tooltip = "选择Id";


        public const string storyIncidentInitConfig_storyGatherConfigs = "故事集合列表。";
        public const string storyIncidentInitConfig_incidentPackGatherConfigs = "事件包集合列表。";


        public const string storyGatherConfig_Tooltip = "故事集合配置，包含多个故事。";
        public const string storyGatherConfig_storyConfigs_Tooltip = "故事列表";


        public const string storyConfig_Tooltip = "故事配置，一个故事中包含多个章。";
        public const string storyConfig_startChapterPId_Tooltip = "开始章节PId。为空则默认从第一个节点开始。";
        public const string storyConfig_chapters_Tooltip = "章配置数组。";


        public const string chapterConfig_Tooltip = "章配置，包含多个事件配置。";
        public const string chapterConfig_type_Tooltip = "章结构类型。1 = 线型，完成一个事件后移到下一个事件直到结束。2 = 层型，完成一章指定数量的事件后，移动到下一章直到结束。3 = 零散，一般用于存放可以反复触发的事件。不会主动结束。";
        public const string chapterConfig_Line_Header = "线型故事数据";
        public const string chapterConfig_StartIncidentPId_Tooltip = "起始的事件，如果没有配置，默认使用可用的第一个事件。";
        public const string chapterConfig_Tier_Header = "层型故事数据";
        public const string chapterConfig_ConditionNum_Tooltip = "每层后移动的条件数组。及必须完成这一层的几个事件后才能移动到下一层。-1为完成所有事件。";
        public const string chapterConfig_incidents_Tooltip = "事件Id数组。顺序不重要，因为单个事件中可以配置这个事件结束时指向的下一个事件。";


        public const string incidentConfig_Tooltip = "事件配置。事件中包含对个事件项目，根据不同情况使用不同事件项目。达到“不同人遇到相同事件时，发生的情况不同”等业务需求";
        public const string incidentConfig_repetition_Tooltip = "允许重复发生。";
        public const string incidentConfig_incidentItems_Tooltip = "故事项目数组。一个有效的事件必须包含起码一个事件项目。";

        public const string incidentItemConfig_Tooltip = "事件项目配置，一个事件会有多个事件项目，根据情况不同使用不同的事件项目。事件项目中包含事件节点。";
        public const string incidentItemConfig_startNodeId_Tooltip = "开始节点Id。为空则默认从第一个节点开始。";
        public const string incidentItemConfig_nodes_Tooltip = "节点数组。一个有效的事件项目包含起码一个节点。";


        public const string incidentNodeConfig_Tooltip = "事件节点配置。一个事件节点由段落和选择构成。一般用于剧情过程描述，以及关键节点让玩家进行选择。";
        public const string incidentNodeConfig_nodeType_Tooltip = "节点类型。定义节点的使用方式。更多具体演出内容由项目实现。";
        public const string incidentNodeConfig_paragraphs_Tooltip = "段落。在节点选择前，用于讲述的内容。可以是具体内容或多语言Code。这取决于项目如何使用。";
        public const string incidentNodeConfig_paragraphs_actor_Tooltip = "演员，讲述者。";
        public const string incidentNodeConfig_chooses_Tooltip = "节点提供的选择。Choice类型的节点必须配置选择节点。";

        public const string incidentChooseConfig_Tooltip = "事件选择配置。一个选择可以配置多个指向的节点，根据玩家情况可能指向不同的节点。";
        public const string incidentChooseConfig_scoreUseType_Tooltip = "分数使用方式。决定在此次节点中，此选择的分数如何进行使用。比如多个选择分数相加。";
        public const string incidentChooseConfig_score_Tooltip = "此选择的分数。";
        public const string incidentChooseConfig_linkNodes_Tooltip = "链接节点数组。当有多个链接节点时，会根据链接节点配置进行选择。";

        public const string linkNodeConfig_targetNodeId_Tooltip = "链接到的目标节点Id。";

        public const string incidentPackGatherConfig_Tooltip = "事件包集合配置。";
        public const string incidentPackGatherConfig_incidentPackConfigs_Tooltip = "事件包列表。";
        public const string incidentPackConfig_incidentIds_Tooltip = "事件包包含的事件Id列表。";


        public const string conditionConfig_conditionItems_Tooltip = "条件项目数组。";
        public const string conditionConfig_logicExpression_Tooltip =
            "条件逻辑表达式。由条件项目数组下标和逻辑符号组成的表达式。" +
            "使用数字和逻辑运算符 && || ! == != （支持单个写法 & | =）,逻辑表达式为空或非法时默认条件通过。" +
            "任何在条件项目数组中不存在的下标，都视为true。（例：0&&(1||2)||3&&!0||1==2&&3!=4）（使用负值直接表示true或false，-1为true，其他为false。）";
        public const string conditionConfig_OwnerType_Tooltip = "所有者类型，表示是谁在使用这个条件配置。";
        public const string conditionItemConfig_type_Tooltip = "实现条件接口的类名称。需要包含完整的命名空间等信息。";

        public const string taskConfig_taskItems_Tooltip = "工作项目数组，所有配置的工作项目都会执行一次。";
        public const string taskConfig_OwnerType_Tooltip = "所有者类型，表示是谁在使用这个工作配置。";
        public const string taskItemConfig_type_Tooltip = "实现工作接口的类名称。需要包含完整的命名空间等信息。";
    }
}