using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Avatar2DRender
{
    public class CharacterController : MonoBehaviour
    {
        public float m_Speed = 0.01f;
        public int m_SkinIdMax = 2;

        private Transform m_Trans;
        private Vector3 m_TransScaleOri; //原始缩放尺寸
        private Avatar2DRenderer m_Avatar2DRenderer; //布娃娃2D渲染器

        private Avatar2DRenderer.EAvatarState m_AvatarStateCurrent = Avatar2DRenderer.EAvatarState.None;

        private void Awake()
        {
            m_Trans = transform;
            m_TransScaleOri = m_Trans.localScale;
            m_Avatar2DRenderer = GetComponent<Avatar2DRenderer>();
        }

        private void Update()
        {
            //布娃娃2D 换皮肤功能
            if (Input.GetKeyDown(KeyCode.H)) //更换头部皮肤
            {
                var skinIdCur = m_Avatar2DRenderer.AvatarPartSkinIdCurrent[Avatar2DRenderer.EAvatarPart.BodyHead];
                skinIdCur++;
                if (skinIdCur > 2)
                {
                    skinIdCur = 1;
                }

                Debug.LogError($"HeadSkinId-{skinIdCur}");

                m_Avatar2DRenderer.SetAvatarPartSkinId(Avatar2DRenderer.EAvatarPart.BodyHead, skinIdCur);
            }
            else if (Input.GetKeyDown(KeyCode.B)) //更换身体皮肤
            {
                var skinIdCur = m_Avatar2DRenderer.AvatarPartSkinIdCurrent[Avatar2DRenderer.EAvatarPart.BodyTrunk];
                skinIdCur++;
                if (skinIdCur > 2)
                {
                    skinIdCur = 1;
                }

                Debug.LogError($"BodySkinId-{skinIdCur}");

                m_Avatar2DRenderer.SetAvatarPartSkinId(Avatar2DRenderer.EAvatarPart.BodyTrunk, skinIdCur);
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        //移动
        private void Move()
        {
            Avatar2DRenderer.EAvatarState avatarStateNew = m_AvatarStateCurrent;

            if (Input.GetKey(KeyCode.A))
            {
                avatarStateNew = Avatar2DRenderer.EAvatarState.Walk;
                m_Trans.localScale = new Vector3(-m_TransScaleOri.x, m_TransScaleOri.y, m_TransScaleOri.z);
                m_Trans.position += new Vector3(-m_Speed, 0, 0);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                avatarStateNew = Avatar2DRenderer.EAvatarState.Walk;
                m_Trans.localScale = m_TransScaleOri;
                m_Trans.position += new Vector3(m_Speed, 0, 0);
            }
            else
            {
                avatarStateNew = Avatar2DRenderer.EAvatarState.Idle;
            }

            //改变 布娃娃渲染器状态
            if (m_AvatarStateCurrent != avatarStateNew)
            {
                m_AvatarStateCurrent = avatarStateNew;
                m_Avatar2DRenderer.SetAvatarState(m_AvatarStateCurrent);
            }
        }
    }
}


