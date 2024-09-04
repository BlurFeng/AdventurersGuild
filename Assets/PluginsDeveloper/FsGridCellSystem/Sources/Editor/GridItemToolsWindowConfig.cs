using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FsGridCellSystem
{
    /// <summary>
    /// ������Ʒ���ߴ�������
    /// �ɻ���ʹ洢�û��ڴ��ڽ��е�����
    /// </summary>
    [Serializable]
    public class GridItemToolsWindowConfig : ScriptableObject
    {
        //������Ʒ����KeyVaule
        [Serializable]
        public struct GridItemScriptSetting
        {
            /// <summary>
            /// �ű�Tag����Aseprite�б༭��Դʱ���ã�ͨ������������Text��ȡ��������֤
            /// </summary>
            public string scriptTag;

            /// <summary>
            /// ��Ҫ������GridItemԤ�����ϵĽű�
            /// </summary>
            public MonoScript gridItemScript;

            public GridItemScriptSetting(string scriptTag, MonoScript gridItemScript)
            {
                this.scriptTag = scriptTag;
                this.gridItemScript = gridItemScript;
            }
            public static bool operator ==(GridItemScriptSetting a, GridItemScriptSetting b)
            {
                if (a.scriptTag != b.scriptTag) return false;
                if (a.gridItemScript != b.gridItemScript) return false;

                return true;
            }

            public static bool operator !=(GridItemScriptSetting a, GridItemScriptSetting b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj.GetType() != GetType()) return false;

                GridItemScriptSetting objData = (GridItemScriptSetting)obj;
                return objData == this;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public MonoScript processorScript;//�������������ű�GridItemToolsWindowProcessor

        //����Ԥ�Ƽ�������������Ʒ
        public TextAsset gridItemsDataTxt_CreatePrefab;//������Ʒ�����ı�
        public string imagesFolderPath_CreatePrefab;//������ƷͼƬ����ļ���·��
        public string gridItemsExportFolder_CreatePrefab;//������ƷԤ���嵼���ļ���·��
        public MonoScript gridItemMonoScript_CreatePrefab;//������Ʒ�ű�����
        [SerializeField]
        public List<GridItemScriptSetting> gridItemScriptSettings = new List<GridItemScriptSetting>();//������Ʒ�ű���ϸ����
        public bool useExistedPrefab_CreatePrefab = true;//ʹ���Ѿ����ڵ�Ԥ����
        public string gridItemPrefabSearchPath_CreatePrefab;//������ƷԤ��������·��
        public bool gridItemPrefabUpdate_CreatePrefab = true;//��������Ԥ���������
        public bool gridItemPrefabUpdate_GridItemMonoScript = true;//����Mono�ű�
        public bool gridItemPrefabUpdate_GridItemComponent_CreatePrefab = true;//����GridItem�ϵ�GridItemComponent����
        public bool gridItemPrefabUpdate_ViewRoot_CreatePrefab = true;//����GridItem��ViewRoot��ʾ�ýڵ�
        public bool gridItemPrefabUpdate_ColliderRoot_CreatePrefab = false;//������ײ���ڵ㣬Ĭ�Ϲرա���Ϊ���ܴ���ĸ����˾����ֹ���������ײ����

        //���µ���������ƷԤ����
        public GameObject singleGridItemPrefabUpdate_Prefab;//������ƷԤ����
        public TextAsset singleGridItemPrefabUpdate_DataText;//������Ʒ�����ı�
        public bool singleGridItemPrefabUpdate_GridItemMonoScript = true;//����Mono�ű�
        public bool singleGridItemPrefabUpdate_GridItemComponent = true;//����GridItem�ϵ�GridItemComponent����
        public bool singleGridItemPrefabUpdate_ViewRoot = true;//����GridItem��ViewRoot��ʾ�ýڵ�
        public bool singleGridItemPrefabUpdate_ColliderRoot = true;//������ײ���ڵ㣬Ĭ�Ϲرա���Ϊ���ܴ���ĸ����˾����ֹ���������ײ����

        //������Ʒ������װ������
        public GameObject gridItemPreformedUnitPrefab_Update_Prefab;//������ƷԤ�Ƽ���Ԥ����
        public TextAsset gridItemsDataTxt_Update_DataText;//������Ʒ�����ı�

        //��������������Ʒ��װ��������������Ʒ��ViewRoot
        public GameObject gridItemPreformedUnitPrefab_ViewRootSet;//������ƷԤ�Ƽ���Ԥ����
        public TextAsset gridItemsDataTxt_ViewRootSet;//������Ʒ�����ı�
    }
}