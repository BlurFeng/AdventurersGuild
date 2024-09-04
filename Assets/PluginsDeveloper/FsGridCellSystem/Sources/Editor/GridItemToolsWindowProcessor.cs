using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace FsGridCellSystem
{
    /// <summary>
    /// 网格物品工具窗口处理器类
    /// 此类能接受网格物品工具工作流程中的事件，允许项目重写此类来接入到工作流程中，进行额外的处理。
    /// 这是对整个工作流的介入类，他是最基本的介入类的中介和管理。
    /// 如果你想介入GridItem类(有GridItemComponent成员字段的项目定义的GridItem基类)创建工作流中。建议为所有GridItem子类编辑相应的GridItemToolsWindowProcessorGridItemNode。这会更清晰和易于管理。
    /// </summary>
    public class GridItemToolsWindowProcessor
    {
        Dictionary<Type, GridItemToolsWindowProcessorGridItemNode> m_GridItemToolsWindowProcessorNodeDic;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            //获取和生成所有GridItemToolsWindowProcessorNode
            m_GridItemToolsWindowProcessorNodeDic = new Dictionary<Type, GridItemToolsWindowProcessorGridItemNode>();
            System.Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            System.Type[] pNodeTypes = (from System.Type type in types where type.IsSubclassOf(typeof(GridItemToolsWindowProcessorGridItemNode)) select type).ToArray();
            for (int i = 0; i < pNodeTypes.Length; i++)
            {
                var type = pNodeTypes[i];

                var pNode = Activator.CreateInstance(type) as GridItemToolsWindowProcessorGridItemNode;
                Type gridItemType = pNode.GetTargetGridItemType();
                if (gridItemType == null) continue;
                pNode.Init();
                m_GridItemToolsWindowProcessorNodeDic.Add(gridItemType, pNode);
            }
        }

        /// <summary>
        /// 当创建一个网格物品
        /// 工作流后期，工具已经处理完毕
        /// </summary>
        /// <param name="gridItem">网格物品。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <param name="u3dComponent">挂在GridItem脚本</param>
        /// <param name="viewRoot">显示层结构根节点</param>
        /// <param name="colliderRoot">碰撞器根节点</param>
        /// <returns></returns>
        public virtual bool OnCreateGridItemAfter(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
        {
            if (gridItem == null || u3dComponent == null) return false;

            Type type = u3dComponent.GetType();
            if (!m_GridItemToolsWindowProcessorNodeDic.ContainsKey(type)) return true;

            return m_GridItemToolsWindowProcessorNodeDic[type].OnCreateGridItemAfter(gridItem, u3dComponent, viewRoot, colliderRoot);
        }

        /// <summary>
        /// 当更新一个网格物品预制体
        /// 工作流后期，工具已经处理完毕
        /// </summary>
        /// <param name="gridItem">网格物品。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <param name="u3dComponent">挂在GridItem脚本</param>
        /// <param name="viewRoot">显示层结构根节点</param>
        /// <param name="colliderRoot">碰撞器根节点</param>
        /// <returns></returns>
        public virtual bool OnUpdateGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
        {
            if (gridItem == null || u3dComponent == null) return false;

            Type type = u3dComponent.GetType();
            if (!m_GridItemToolsWindowProcessorNodeDic.ContainsKey(type)) return true;

            return m_GridItemToolsWindowProcessorNodeDic[type].OnUpdateGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot);
        }

        /// <summary>
        /// 当创建一个网格物品预制体
        /// </summary>
        /// <param name="gridItem">网格物品。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <param name="u3dComponent">挂在GridItem脚本</param>
        /// <param name="viewRoot">显示层结构根节点</param>
        /// <param name="colliderRoot">碰撞器根节点</param>
        /// <returns></returns>
        public virtual bool OnCreateGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
        {
            if (gridItem == null || u3dComponent == null) return false;

            Type type = u3dComponent.GetType();
            if (!m_GridItemToolsWindowProcessorNodeDic.ContainsKey(type)) return true;

            return m_GridItemToolsWindowProcessorNodeDic[type].OnCreateGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot);
        }

        /// <summary>
        /// 当创建一个预制件，预制件是一个或多个GridItem的组合
        /// 工作流后期，工具已经处理完毕
        /// </summary>
        /// <param name="preformedUnit">预制件。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <returns></returns>
        public virtual bool OnCreatePreformedUnitAfter(GameObject preformedUnit)
        {
            if (preformedUnit == null) return false;

            bool succeed = true;
            if(m_GridItemToolsWindowProcessorNodeDic.Count > 0)
            {
                foreach (var item in m_GridItemToolsWindowProcessorNodeDic)
                {
                    bool succeedTemp = item.Value.OnCreatePreformedUnitAfter(preformedUnit);
                    if (!succeedTemp) succeed = false;
                }
            }

            return succeed;
        }

        /// <summary>
        /// 当创建一个预制件的预制体
        /// 预制件是一个或多个GridItem的组合
        /// </summary>
        /// <param name="preformedUnitPrefab"></param>
        /// <returns></returns>
        public virtual bool OnCreatePreformedUnitPrefab(GameObject preformedUnitPrefab)
        {
            if (preformedUnitPrefab == null) return false;

            bool succeed = true;
            if (m_GridItemToolsWindowProcessorNodeDic.Count > 0)
            {
                foreach (var item in m_GridItemToolsWindowProcessorNodeDic)
                {
                    bool succeedTemp = item.Value.OnCreatePreformedUnitPrefab(preformedUnitPrefab);
                    if (!succeedTemp) succeed = false;
                }
            }

            return succeed;
        }
    }
}