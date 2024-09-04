using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 布娃娃2D 渲染器
/// Shader帧动画
/// </summary>
namespace Avatar2DRender
{
    //必要组件 MeshRenderer
    [RequireComponent(typeof(MeshRenderer))]
    public class Avatar2DRenderer : MonoBehaviour
    {
        /// <summary>
        /// 布娃娃 状态
        /// </summary>
        public enum EAvatarState
        {
            None,
            /// <summary>
            /// 空闲
            /// </summary>
            Idle,
            /// <summary>
            /// 走
            /// </summary>
            Walk
        }

        /// <summary>
        /// 布娃娃 部位
        /// </summary>
        public enum EAvatarPart
        {
            None,
            /// <summary>
            /// 身体 头部
            /// </summary>
            BodyHead,
            /// <summary>
            /// 身体 躯干
            /// </summary>
            BodyTrunk,
            /// <summary>
            /// 身体 眼睛
            /// </summary>
            BodyEye,
            /// <summary>
            /// 身体 眉毛
            /// </summary>
            BodyBrow,
            /// <summary>
            /// 身体 头发-前
            /// </summary>
            BodyHairFront,
            /// <summary>
            /// 身体 头发-后
            /// </summary>
            BodyHairBack,
            /// <summary>
            /// 身体 其他特征
            /// </summary>
            BodyOther,

            /// <summary>
            /// 装备 头部
            /// </summary>
            EquipHelmet,
            /// <summary>
            /// 装备 胸部
            /// </summary>
            EquipArmour,
            /// <summary>
            /// 装备 手部
            /// </summary>
            EquipGloves,
            /// <summary>
            /// 装备 腿部
            /// </summary>
            EquipLeggings,
            /// <summary>
            /// 装备 脚部
            /// </summary>
            EquipBoots,
            /// <summary>
            /// 装备 主手
            /// </summary>
            EquipMainhand,
            /// <summary>
            /// 装备 副手
            /// </summary>
            EquipOffhand,
            /// <summary>
            /// 装备 其他
            /// </summary>
            EquipOther,
        }

        /// <summary>
        /// 布娃娃 状态 对应的材质球
        /// </summary>
        public List<Material> m_ListAvatarStateMat = new List<Material>();
        /// <summary>
        /// 纹理贴图 角色目录
        /// </summary>
        public string m_TexCharacterDirectory = string.Empty;
        /// <summary>
        /// 纹理贴图 文件名格式
        /// </summary>
        public string m_TexFileNameFormat = string.Empty;

        /// <summary>
        /// 布娃娃 当前的状态
        /// </summary>
        public EAvatarState AvatarStateCurrent { get { return m_AvatarStateCurrent; } }
        private EAvatarState m_AvatarStateCurrent = EAvatarState.None;

        /// <summary>
        /// 布娃娃 当前的部位皮肤ID
        /// </summary>
        public Dictionary<EAvatarPart, int> AvatarPartSkinIdCurrent { get { return m_AvatarPartSkinIdCurrent; } }
        private Dictionary<EAvatarPart, int> m_AvatarPartSkinIdCurrent = new Dictionary<EAvatarPart, int>();

        private MeshRenderer m_MeshRender;
        private Texture2D m_TexDefault;

        private void Awake()
        {
            m_MeshRender = GetComponent<MeshRenderer>();

            //默认 所有部位的皮肤ID
            SetAvatarPartSkinId(EAvatarPart.BodyHead, 1);
            SetAvatarPartSkinId(EAvatarPart.BodyTrunk, 1);
            SetAvatarPartSkinId(EAvatarPart.BodyEye, 1);
            SetAvatarPartSkinId(EAvatarPart.BodyBrow, 1);
            SetAvatarPartSkinId(EAvatarPart.BodyHairFront, 1);
            SetAvatarPartSkinId(EAvatarPart.BodyHairBack, 1);
            SetAvatarPartSkinId(EAvatarPart.BodyOther, 1);

            SetAvatarPartSkinId(EAvatarPart.EquipHelmet, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipArmour, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipGloves, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipLeggings, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipBoots, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipMainhand, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipOffhand, 1);
            SetAvatarPartSkinId(EAvatarPart.EquipOther, 1);
        }

        /// <summary>
        /// 设置 布娃娃状态
        /// </summary>
        /// <param name="avatarState"></param>
        public void SetAvatarState(EAvatarState avatarState = EAvatarState.Idle)
        {
            //记录 当前布娃娃状态
            m_AvatarStateCurrent = avatarState;
            
            //转换为List下标
            int index = (int)m_AvatarStateCurrent - 1;
            if (index > m_ListAvatarStateMat.Count)
            {
                Debug.LogError($"Avatar2DRender.Avatar2DRenderer.SetAvatarState() Error! >> Invalid EAvatarState param! EAvatarState-{avatarState}");
                return;
            }
            if (index > m_ListAvatarStateMat.Count)
            {
                Debug.LogError($"Avatar2DRender.Avatar2DRenderer.SetAvatarState() Error! >> EAvatarState param not have Material! EAvatarState-{avatarState}");
                return;
            }

            //更换 对应状态的材质球
            m_MeshRender.material = m_ListAvatarStateMat[index];
            //设置 动画开始时间
            m_MeshRender.material.SetFloat("_TimeStart", Time.realtimeSinceStartup);
        }

        /// <summary>
        /// 设置布娃娃部位的 皮肤Id
        /// </summary>
        public void SetAvatarPartSkinId(EAvatarPart avatarPart, int skinId = 1)
        {
            //记录 头部 当前的皮肤ID
            if (AvatarPartSkinIdCurrent.ContainsKey(avatarPart))
            {
                AvatarPartSkinIdCurrent[avatarPart] = skinId;
            }
            else
            {
                AvatarPartSkinIdCurrent.Add(avatarPart, skinId);
            }
           
            string partName = avatarPart.ToString();
            //设置 所有状态的材质球 头部的帧动画图集
            for (int i = 0; i < m_ListAvatarStateMat.Count; i++)
            {
                var mat = m_ListAvatarStateMat[i];
                var roleState = (EAvatarState)(i + 1);
                //纹理贴图 Adress
                string skinName = string.Format(m_TexFileNameFormat, partName, skinId, roleState);
                string address = $"{m_TexCharacterDirectory}/{partName}/{skinName}";
                //异步加载 回调中设置材质球的纹理贴图
                var handle = Addressables.LoadAssetAsync<Texture>(address);
                handle.Completed += (resulthandle) =>
                {
                    var skinTex = resulthandle.Result as Texture;
                    if (skinTex == null)
                    {
                        skinTex = AssetSystem.Instance.Tex2DDefault;
                    }
                    mat.SetTexture($"_{partName}Tex", skinTex);
                };
            }
        }
    }
}


