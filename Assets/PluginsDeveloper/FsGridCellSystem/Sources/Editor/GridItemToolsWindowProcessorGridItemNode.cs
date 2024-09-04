using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FsGridCellSystem
{
    /// <summary>
    /// 项目继承此类型，介入到创建GridItem的工作流中。
    /// 当你创建了一个GridItem类(有GridItemComponent成员字段的项目定义的GridItem基类)的子类，你想在工具自动化创建GridItem预制体并添加相应脚本时介入到工作流中。
    /// 那么你应当继承此类型并重写GetTargetGridItemType()方法来指定你要处理的类型。以及重写你需要的介入工作流方法。
    /// </summary>
    public class GridItemToolsWindowProcessorGridItemNode
    {
        public virtual void Init()
        {

        }

        /// <summary>
        /// 子类必须实现此方法，告知此GridItemToolsWindowProcessorNode是为了处理哪类GridItem的处理节点脚本
        /// </summary>
        /// <returns></returns>
        public virtual Type GetTargetGridItemType()
        {
            return null;
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
            OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot);
            return true;
        }

        /// <summary>
        /// 当更新一个网格物品预制体时
        /// 工作流后期，工具已经处理完毕
        /// </summary>
        /// <param name="gridItem">网格物品。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <param name="u3dComponent">挂在GridItem脚本</param>
        /// <param name="viewRoot">显示层结构根节点</param>
        /// <param name="colliderRoot">碰撞器根节点</param>
        /// <returns></returns>
        public virtual bool OnUpdateGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
        {
            OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot);
            return true;
        }

        /// <summary>
        /// 当改变一个网格物品预制体时
        /// 此事件在OnCreateGridItemAfter和OnUpdateGridItemPrefab中被调用
        /// 请注意不要重复实现导致内容重复执行多次
        /// 工作流后期，工具已经处理完毕
        /// </summary>
        /// <param name="gridItem">网格物品。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <param name="u3dComponent">挂在GridItem脚本</param>
        /// <param name="viewRoot">显示层结构根节点</param>
        /// <param name="colliderRoot">碰撞器根节点</param>
        /// <returns></returns>
        public virtual bool OnChangeGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
        {
            return true;
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
            return true;
        }

        /// <summary>
        /// 当创建一个预制件，预制件是一个或多个GridItem的组合
        /// 工作流后期，工具已经处理完毕
        /// </summary>
        /// <param name="preformedUnit">预制件。这个GObj是临时的，之后会创建对应预制体并删除</param>
        /// <returns></returns>
        public virtual bool OnCreatePreformedUnitAfter(GameObject preformedUnit)
        {
            return true;
        }

        /// <summary>
        /// 当创建一个预制件的预制体
        /// 预制件是一个或多个GridItem的组合
        /// </summary>
        /// <param name="preformedUnitPrefab"></param>
        /// <returns></returns>
        public virtual bool OnCreatePreformedUnitPrefab(GameObject preformedUnitPrefab)
        {
            return true;
        }
    }
}